using System.Reflection;
using System.Text.RegularExpressions;
using FluentAssertions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Wombat.Api.Endpoints;
using Wombat.Web.Services;

namespace Wombat.Architecture.Tests;

/// <summary>
/// The forcing function for T282: no log line carries a person's email address (ARCHITECTURE.md § Logging &amp;
/// diagnostics: "User IDs yes, email addresses no").
/// </summary>
/// <remarks>
/// <para>
/// Until T282 the mail worker logged "Email to {To}" on every send, retry and drop, beside the mail's tags, which carry
/// <c>campaign:N</c> on an MSF mail. For as long as logs were kept they named every respondent of every campaign, which
/// undoes T207.
/// </para>
/// <para>
/// <b>Why the compiled IL, not the source.</b> The scan reads every call to a logging API in the five <c>src</c>
/// assemblies and finds the message template that call passes, as the logger will receive it. The compiler has already
/// folded a constant or a concatenation of literals into one string, and a Razor component's <c>@code</c>, an async
/// method's state machine, a lambda and a <c>[LoggerMessage]</c> method's generated code are all in the assembly, where a
/// scan of <c>.cs</c> files sees none of the Razor code and would have to parse C# to join a split literal. Every call
/// site is judged, not only those a pattern happened to match: a template the scan cannot read, because it is built at
/// run time (an interpolated string, a <c>static readonly</c> field) or because the code calls <c>ILogger.Log</c> itself,
/// fails the test, since what such a line logs is exactly what no reader can see.
/// </para>
/// <para>
/// <b>What it refuses.</b> A template that names an address placeholder (<see cref="NamesAnAddress" />: <c>{To}</c>,
/// <c>{Email}</c>, <c>{RespondentEmail}</c>, <c>{UserName}</c>, which Wombat sets to the address) or quotes an address,
/// in every arm of a template a conditional chooses. Then, since renaming <c>{To}</c> to <c>{Who}</c> would pass a
/// template-only check, what the call is given: the instructions from its first argument, the logger, to the call are
/// exactly the evaluation of its arguments, the exception and the event id as well as the values, so any address-named
/// property, field, parameter or local read there, a method whose name says it returns an address
/// (<c>GetEmailAsync</c>), <c>IIdentity.Name</c> or an address claim (which hold the user name, and so the address), an
/// address literal, or a whole object that has an address property (an <c>EmailMessage</c>, an Identity user, whose
/// <c>ToString</c> is its user name) is refused too. So is a call whose values come in an array built before it, which
/// no reader of the call can see into.
/// </para>
/// <para>
/// <b>Which calls.</b> <c>LoggerExtensions.Log*</c> and <c>BeginScope</c>, and <c>LoggerMessage.Define*</c>, whose template
/// is an argument; a call to a <c>[LoggerMessage]</c> method, or to the delegate a <c>Define*</c> returned, whose template
/// is judged where it is declared and whose arguments are judged at the call (T282 review: the first scan judged only
/// their templates, so a clean template handed <c>message.To</c> passed); and <c>ILogger.Log</c> called directly, which
/// is refused.
/// </para>
/// <para>
/// A value computed out of sight, in another method or before an <c>await</c> inside the call's arguments, is not seen;
/// the template check and review cover that.
/// </para>
/// </remarks>
public class NoAddressInLogsTests
{
    private static readonly Assembly[] Assemblies =
    [
        typeof(Wombat.Domain.Institutions.Institution).Assembly,
        typeof(Wombat.Application.DependencyInjection).Assembly,
        typeof(Wombat.Infrastructure.DependencyInjection).Assembly,
        typeof(MsfRespondEndpoint).Assembly,
        typeof(IScopedSender).Assembly
    ];

    [Fact]
    public void No_log_call_in_src_names_an_address()
    {
        var problems = Assemblies
            .SelectMany(assembly => CallSitesIn(assembly))
            .SelectMany(site => site.Problems.Select(problem => $"{site.Where} {problem}"))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToList();

        // Every offender is named in the reason, because the assertion's own message shows only the first.
        problems.Should().BeEmpty(
            "a log line outlives what it names, and ARCHITECTURE.md allows user ids in logs but never an email address: " +
            "name a mail by its tags and EmailLog's reference, a person by their id (T282). Found: {0}",
            string.Join("; ", problems));
    }

    [Fact]
    public void The_scan_reads_every_line_the_mail_worker_writes()
    {
        // The guard for the test above: a scan that found no call sites, or could not read the worker's, would pass for
        // the wrong reason. The worker writes seven lines about a mail, each naming it by its reference.
        var sites = Assemblies.SelectMany(assembly => CallSitesIn(assembly)).ToList();

        sites.Should().HaveCountGreaterThan(50, "guard: src makes far more log calls than this, so the scan is blind");
        sites.Where(site => site.TopLevelType == "Wombat.Infrastructure.Email.EmailWorker")
            .Select(site => site.Template)
            .Should().HaveCount(7)
            .And.AllSatisfy(template => template.Should().Contain("{Reference}").And.Contain("{Tags}"));
    }

    [Fact]
    public void The_scan_catches_each_way_a_specimen_logs_an_address()
    {
        // Each specimen below logs an address one way; each check in the scan must catch its own, and only its own.
        var found = CallSitesIn(typeof(LogAddressSpecimens).Assembly, typeof(LogAddressSpecimens).FullName)
            .Where(site => site.Problems.Count > 0)
            .GroupBy(site => SpecimenOf(site.Where), StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.SelectMany(site => site.Problems).Order(StringComparer.Ordinal).ToList(),
                StringComparer.Ordinal);

        found.Should().BeEquivalentTo(new Dictionary<string, List<string>>
        {
            ["Placeholder"] = ["names the placeholder {To}"],
            ["AlignedPlaceholder"] = ["names the placeholder {RespondentEmail}"],
            ["UserNamePlaceholder"] = ["names the placeholder {UserName}"],
            ["QuotedInTemplate"] = ["quotes an address in its message"],
            ["Getter"] = ["passes EmailMessage.To"],
            ["Parameter"] = ["passes the parameter respondentEmail"],
            ["StaticField"] = ["passes the field adminEmail"],
            ["Local"] = ["passes the local recipientEmail"],
            ["Literal"] = ["passes the address literal \"admin@example.test\""],
            ["WholeMessage"] = ["passes a whole EmailMessage, whose To is an address"],
            ["Interpolated"] = ["has a message that is not a constant string, so no reader can see what it logs"],
            ["Define"] = ["names the placeholder {Email}"],
            ["Scope"] = ["names the placeholder {Recipient}", "passes the parameter email"],
            ["Hoisted"] = ["passes the field email"],
            ["Captured"] = ["passes the field toAddress"],
            ["Direct"] = ["calls ILogger.Log itself, so no reader can see what it logs"],
            ["GeneratedMessage"] = ["names the placeholder {Email}"],
            // The generator's Define for GeneratedMessage, in the class's static constructor.
            [".cctor"] = ["names the placeholder {Email}"],
            // Seven values: the generator calls ILogger.Log itself, which the scan leaves to the attribute's template.
            ["SevenValues"] = ["names the placeholder {Email}"],
            // The T282 review's probes, each of which passed the first scan.
            ["GeneratedCalled"] = ["passes EmailMessage.To"],
            ["DefinedInvoked"] = ["passes EmailMessage.To"],
            ["IdentityName"] = ["passes IIdentity.Name, the user name, which Wombat sets to the address"],
            ["EmailClaim"] = [$"reads the claim \"{System.Security.Claims.ClaimTypes.Email}\", which holds an address"],
            ["LookedUp"] = ["passes what IAddressBook.GetUserName returns"],
            ["InException"] = ["passes EmailMessage.To"],
            ["ValuesBuiltBeforehand"] = ["passes its values in an array built beforehand, so no reader can see what it logs"],
            ["ConditionalTemplate"] = ["names the placeholder {To}"]
        });
    }

    [Fact]
    public void The_scan_passes_a_line_that_counts_or_dates_what_it_does_not_name()
    {
        // The other half of the specimens' test: "{NoEmailCount}", "{ToDate}", a DateOnly named "to", a count for each
        // reason named "skippedRecipients" (as the coordinator digest logs), the mail's subject and tags, an exception,
        // a claim's type logged as a value, a template chosen between two clean ones, and a [LoggerMessage] method and a
        // Define's delegate given a reference are not addresses, and a scan that refused them would be ignored.
        var sites = CallSitesIn(typeof(LogAddressSpecimens).Assembly, typeof(LogAddressSpecimens).FullName)
            .Where(site => SpecimenOf(site.Where) is "Counts" or "Tagged" or "Failed" or "ClaimNamed" or "Chosen"
                or "GeneratedClean")
            .ToList();

        sites.Should().HaveCount(7, "guard: each clean specimen is a call site the scan read");
        sites.Should().AllSatisfy(site => site.Problems.Should().BeEmpty());
    }

    // ─── The scan ────────────────────────────────────────────────────────────

    private sealed record LogCallSite(string TopLevelType, string Where, string? Template, List<string> Problems);

    /// <summary>Every log call site in <paramref name="assembly" />, or only under the type <paramref name="within" />.</summary>
    private static List<LogCallSite> CallSitesIn(Assembly assembly, string? within = null)
    {
        using var resolver = new DefaultAssemblyResolver();

        // The shared framework's assemblies (Identity, logging) are not copied beside the test's, but this process has
        // loaded them, so their folders are where an object's base types are found.
        foreach (var folder in AppDomain.CurrentDomain.GetAssemblies()
                     .Where(loaded => !loaded.IsDynamic && !string.IsNullOrEmpty(loaded.Location))
                     .Select(loaded => Path.GetDirectoryName(loaded.Location)!)
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            resolver.AddSearchDirectory(folder);
        }

        // Symbols give locals their names; a build writes the portable PDB beside every assembly.
        using var module = ModuleDefinition.ReadModule(
            assembly.Location,
            new ReaderParameters { ReadSymbols = true, AssemblyResolver = resolver });

        // Materialised before the module is disposed. Nested types too: an async method's body lives in its state
        // machine, and a lambda's in a display class.
        var sites = new List<LogCallSite>();
        foreach (var topLevel in module.Types)
        {
            foreach (var type in Flatten(topLevel).Where(type => within is null || IsWithin(type, within)))
            {
                foreach (var method in type.Methods)
                {
                    foreach (var template in LoggerMessageTemplates(method))
                    {
                        sites.Add(new LogCallSite(topLevel.FullName, Where(method), template, TemplateProblems(template)));
                    }

                    // The logging generator's own code is judged by its [LoggerMessage] template, read above.
                    if (method.HasBody && !IsGeneratedLogging(method) && !IsGeneratedLogging(type))
                    {
                        sites.AddRange(CallSites(method)
                            .Select(site => new LogCallSite(topLevel.FullName, Where(method), site.Template, site.Problems)));
                    }
                }
            }
        }

        return sites;
    }

    /// <summary>
    /// Each logging call in <paramref name="method" />, with the template it passes and what is wrong with it, found by
    /// following which instruction pushed each value on the evaluation stack.
    /// </summary>
    private static List<(string? Template, List<string> Problems)> CallSites(MethodDefinition method)
    {
        var body = method.Body;
        var stateAt = new Dictionary<Instruction, Instruction?[]>();
        foreach (var handler in body.ExceptionHandlers)
        {
            // A catch or filter block starts with the exception on the stack; a finally or fault block with nothing.
            stateAt[handler.HandlerStart] = handler.HandlerType is ExceptionHandlerType.Catch or ExceptionHandlerType.Filter
                ? [null]
                : [];
            if (handler.FilterStart is not null)
            {
                stateAt[handler.FilterStart] = [null];
            }
        }

        var calls = new List<LogCall>();
        var stored = new Dictionary<Instruction, Instruction?>();
        var alternatives = new Dictionary<Instruction, HashSet<Instruction?>>();
        var stack = new List<Instruction?>();
        var reachable = true;

        foreach (var instruction in body.Instructions)
        {
            if (!reachable)
            {
                // Reached only by a branch: take the stack the branch recorded. A target reached only by a backward
                // branch (a loop's body) has none recorded, and C# leaves the stack empty there.
                stack = stateAt.TryGetValue(instruction, out var recorded) ? [.. recorded] : [];
            }
            else if (stateAt.TryGetValue(instruction, out var joined))
            {
                // Reached both by falling through and by a branch: a conditional's two arms meet here.
                Join(stack, joined, alternatives);
            }

            if (instruction.OpCode.Code is Code.Stelem_Ref or Code.Stelem_Any)
            {
                stored[instruction] = Peek(stack, 0);
            }

            if (instruction.OpCode.Code is Code.Call or Code.Callvirt && instruction.Operand is MethodReference target &&
                LogCallOf(instruction, target, stack) is { } call)
            {
                calls.Add(call);
            }

            Apply(instruction, method, stack);

            foreach (var branchTarget in BranchTargets(instruction))
            {
                if (!stateAt.TryAdd(branchTarget, [.. stack]))
                {
                    Join(stateAt[branchTarget], stack, alternatives);
                }
            }

            reachable = instruction.OpCode.FlowControl is not (FlowControl.Branch or FlowControl.Return or FlowControl.Throw);
        }

        return calls.Select(call => Judge(method, call, stored, alternatives)).ToList();
    }

    /// <summary>
    /// A call to a logging API. <see cref="First" /> pushed its first argument, the logger (or <c>this</c>, for a
    /// <c>[LoggerMessage]</c> instance method), so everything from there to <see cref="Call" /> is the evaluation of its
    /// arguments: the exception and the event id as well as the template and the values. <see cref="Template" /> pushed
    /// the template, where the API takes one, and <see cref="Values" /> the <c>params object[]</c>, where it takes that.
    /// </summary>
    private sealed record LogCall(
        Instruction Call,
        Instruction? First,
        bool TakesTemplate,
        Instruction? Template,
        bool TakesValues,
        Instruction? Values,
        bool Direct);

    /// <summary>
    /// The log call <paramref name="call" /> makes, if it is one: <c>LoggerExtensions.Log*</c>, <c>BeginScope</c> and
    /// <c>LoggerMessage.Define*</c>, which take a template; a <c>[LoggerMessage]</c> method, or the delegate a
    /// <c>Define</c> returned, which take only values, their template being judged where it is declared; or
    /// <c>ILogger.Log</c> or <c>BeginScope</c> called directly.
    /// </summary>
    private static LogCall? LogCallOf(Instruction call, MethodReference target, List<Instruction?> stack)
    {
        var count = target.Parameters.Count;
        if (TemplateParameter(target) is { } index)
        {
            var takesValues = count > 0 &&
                              target.Parameters[^1].ParameterType is ArrayType { ElementType.MetadataType: MetadataType.Object };
            return new LogCall(
                call,
                Peek(stack, count - 1),
                TakesTemplate: true,
                Peek(stack, count - 1 - index),
                takesValues,
                takesValues ? Peek(stack, 0) : null,
                Direct: false);
        }

        if (IsLoggingDelegate(target))
        {
            // The delegate lies beneath the logger on the stack, and is not an argument.
            return new LogCall(call, Peek(stack, count - 1), false, null, false, null, false);
        }

        if (IsLoggerMessageMethod(target))
        {
            var depth = count - 1 + (target.HasThis ? 1 : 0);
            return depth < 0 ? null : new LogCall(call, Peek(stack, depth), false, null, false, null, false);
        }

        if (target.DeclaringType.FullName == "Microsoft.Extensions.Logging.ILogger" && target.Name is "Log" or "BeginScope")
        {
            return new LogCall(call, null, false, null, false, null, Direct: true);
        }

        return null;
    }

    /// <summary>
    /// Where two paths meet, the value one left in a slot of the stack may stand in for the value the other left there:
    /// the arm of a conditional that falls through, and the arm that branched here. So a template chosen by a conditional
    /// is judged in each of its arms, not only the one that happens to fall through to the call.
    /// </summary>
    private static void Join(
        IReadOnlyList<Instruction?> kept,
        IReadOnlyList<Instruction?> other,
        Dictionary<Instruction, HashSet<Instruction?>> alternatives)
    {
        if (kept.Count != other.Count)
        {
            return;
        }

        for (var slot = 0; slot < kept.Count; slot++)
        {
            if (kept[slot] is { } push && push != other[slot])
            {
                if (!alternatives.TryGetValue(push, out var set))
                {
                    alternatives[push] = set = [];
                }

                set.Add(other[slot]);
            }
        }
    }

    /// <summary>
    /// <paramref name="push" /> and every value that may stand in its slot instead (<see cref="Join" />), a null among them
    /// for one the scan does not know.
    /// </summary>
    private static List<Instruction?> WithAlternatives(
        Instruction push, Dictionary<Instruction, HashSet<Instruction?>> alternatives)
    {
        var found = new List<Instruction?> { push };
        var seen = new HashSet<Instruction> { push };
        for (var next = 0; next < found.Count; next++)
        {
            if (found[next] is not { } current || !alternatives.TryGetValue(current, out var set))
            {
                continue;
            }

            foreach (var alternative in set)
            {
                if (alternative is null ? !found.Contains(null) : seen.Add(alternative))
                {
                    found.Add(alternative);
                }
            }
        }

        return found;
    }

    private static (string? Template, List<string> Problems) Judge(
        MethodDefinition method,
        LogCall call,
        Dictionary<Instruction, Instruction?> stored,
        Dictionary<Instruction, HashSet<Instruction?>> alternatives)
    {
        if (call.Direct)
        {
            return (null, ["calls ILogger.Log itself, so no reader can see what it logs"]);
        }

        if (call.First is null)
        {
            return (null, ["passes arguments the scan could not find, so no reader can see what it logs"]);
        }

        string? shown = null;
        var problems = new List<string>();
        var templates = new HashSet<Instruction>();
        if (call.TakesTemplate)
        {
            // Every arm of a conditional template is judged, and one the scan cannot read refuses the call.
            foreach (var candidate in call.Template is { } push ? WithAlternatives(push, alternatives) : [null])
            {
                if (candidate is not { OpCode.Code: Code.Ldstr, Operand: string template })
                {
                    return (null, ["has a message that is not a constant string, so no reader can see what it logs"]);
                }

                shown ??= template;
                templates.Add(candidate);
                problems.AddRange(TemplateProblems(template));
            }
        }

        if (call.TakesValues && !IsParamsArray(call.Values))
        {
            // The compiler builds a params array in place; one built beforehand holds what this scan never reads.
            problems.Add("passes its values in an array built beforehand, so no reader can see what it logs");
        }

        // The arguments are evaluated in order from the first, so what lies between it and the call is their evaluation:
        // the exception and the event id, which come before the template, as well as the values after it.
        for (var instruction = call.First; instruction is not null && instruction != call.Call; instruction = instruction.Next)
        {
            if (!templates.Contains(instruction))
            {
                problems.AddRange(ValueProblems(method, instruction, stored, call.Call));
            }
        }

        return (shown, problems);
    }

    /// <summary>Whether <paramref name="push" /> is a <c>params</c> array the compiler built at the call: new, or empty.</summary>
    private static bool IsParamsArray(Instruction? push)
        => push is { OpCode.Code: Code.Newarr } or
        {
            OpCode.Code: Code.Call,
            Operand: MethodReference { Name: "Empty", DeclaringType.FullName: "System.Array" }
        };

    private static List<string> TemplateProblems(string template)
    {
        var problems = Placeholders(template)
            .Where(placeholder => NamesAnAddress(placeholder, null))
            .Select(placeholder => $"names the placeholder {{{placeholder}}}")
            .ToList();
        if (AddressLiteral.IsMatch(template))
        {
            problems.Add("quotes an address in its message");
        }

        return problems;
    }

    /// <summary>What an instruction among a log call's arguments reads that is, or holds, an address.</summary>
    private static IEnumerable<string> ValueProblems(
        MethodDefinition method, Instruction instruction, Dictionary<Instruction, Instruction?> stored, Instruction call)
    {
        switch (instruction.OpCode.Code)
        {
            case Code.Ldstr when AddressLiteral.IsMatch((string)instruction.Operand):
                yield return $"passes the address literal \"{instruction.Operand}\"";
                break;

            case Code.Ldstr when AddressClaimTypes.Contains((string)instruction.Operand) &&
                                 instruction.Next is { OpCode.Code: Code.Call or Code.Callvirt } lookUp &&
                                 lookUp != call:
                // A claim looked up by its type, principal.FindFirstValue(ClaimTypes.Email); a claim type logged as a
                // value, as in "no {Claim} claim", goes into the values array instead.
                yield return $"reads the claim \"{instruction.Operand}\", which holds an address";
                break;

            case Code.Call or Code.Callvirt
                when instruction.Operand is MethodReference { Name: var name } getter &&
                     name.StartsWith("get_", StringComparison.Ordinal) &&
                     NamesAnAddress(name, getter.ReturnType):
                yield return $"passes {getter.DeclaringType.Name}.{name[4..]}";
                break;

            case Code.Call or Code.Callvirt
                when instruction.Operand is MethodReference { Name: "get_Name" } getter &&
                     IdentityTypes.Contains(getter.DeclaringType.FullName):
                yield return $"passes {getter.DeclaringType.Name}.Name, the user name, which Wombat sets to the address";
                break;

            case Code.Call or Code.Callvirt
                when instruction.Operand is MethodReference { Name: var name } callee &&
                     !name.Contains('_', StringComparison.Ordinal) && !name.StartsWith('.') &&
                     NamesAnAddress(LookUpName(name), callee.ReturnType):
                // A method that looks an address up: userManager.GetEmailAsync(user), GetUserName(principal).
                yield return $"passes what {callee.DeclaringType.Name}.{name} returns";
                break;

            case Code.Ldfld or Code.Ldsfld or Code.Ldflda or Code.Ldsflda
                when instruction.Operand is FieldReference field && NamesAnAddress(field.Name, field.FieldType):
                // By the name it was written with: a hoisted local's field is <email>5__2.
                yield return $"passes the field {Undecorated(field.Name)}";
                break;
        }

        if (ArgumentOf(method, instruction) is { } argument && NamesAnAddress(argument.Name, argument.Type))
        {
            yield return $"passes the parameter {argument.Name}";
        }

        if (LocalOf(method, instruction) is { } local &&
            method.DebugInformation.TryGetName(local, out var localName) &&
            NamesAnAddress(localName, local.VariableType))
        {
            yield return $"passes the local {localName}";
        }

        // A value stored into the arguments array: an object logged whole is rendered by its ToString, or destructured.
        if (stored.TryGetValue(instruction, out var value) && value is not null &&
            PushedType(method, value) is { } type && AddressPropertyOf(type) is { } property)
        {
            yield return $"passes a whole {type.Name}, whose {property} is an address";
        }
    }

    // ─── What names an address ───────────────────────────────────────────────

    /// <summary>The last word of a name that makes it an address: <c>respondentEmail</c>, <c>ToAddress</c>, <c>Recipient</c>.</summary>
    private static readonly HashSet<string> AddressWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "Email", "Emails", "Address", "Addresses", "Recipient", "Recipients", "Mailbox", "Mailboxes"
    };

    /// <summary>
    /// Whole names that are an address: a mail's <c>To</c> and copies, and Identity's user name, which Wombat sets to the
    /// address (<c>InvitedUserProvisioner</c>, <c>ExternalLoginHandler</c>).
    /// </summary>
    private static readonly HashSet<string> AddressNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "To", "Cc", "Bcc", "UserName", "NormalizedUserName"
    };

    /// <summary>
    /// The claims that hold an address: Identity's name claim, which carries the user name and so the address, and the
    /// email, UPN and preferred-user-name claims an SSO provider sends.
    /// </summary>
    private static readonly HashSet<string> AddressClaimTypes = new(StringComparer.Ordinal)
    {
        System.Security.Claims.ClaimTypes.Name,
        System.Security.Claims.ClaimTypes.Email,
        System.Security.Claims.ClaimTypes.Upn,
        "email",
        "preferred_username",
        "upn",
        "unique_name"
    };

    /// <summary>The identities whose <c>Name</c> is the signed-in user's user name, which Wombat sets to the address.</summary>
    private static readonly HashSet<string> IdentityTypes = new(StringComparer.Ordinal)
    {
        "System.Security.Principal.IIdentity",
        "System.Security.Claims.ClaimsIdentity",
        "System.Security.Principal.GenericIdentity"
    };

    /// <summary>What a method that looks a value up returns, by its name: <c>GetEmailAsync</c> as <c>Email</c>.</summary>
    private static string LookUpName(string method)
        => Regex.Replace(Regex.Replace(method, "^Get(?=[A-Z])", string.Empty), "Async$", string.Empty);

    private static readonly Regex AddressLiteral = new(
        @"[A-Za-z0-9._%+-]+@[A-Za-z0-9-]+(?:\.[A-Za-z0-9-]+)+",
        RegexOptions.CultureInvariant);

    /// <summary>
    /// Whether <paramref name="name" /> (a placeholder, property, field, parameter or local) names an address. The last
    /// word decides, so a count of addresses (<c>{NoEmailCount}</c>) or a date (<c>{ToDate}</c>) does not; nor does a
    /// value whose type cannot hold one (<see cref="CanHoldAnAddress" />): a <c>DateOnly to</c>, or the digest's
    /// <c>SkippedRecipients</c>, a count for each reason. A placeholder has no type, so <c>{To}</c> always names one.
    /// </summary>
    private static bool NamesAnAddress(string name, TypeReference? type)
    {
        var bare = Undecorated(name);
        if (bare.Length == 0 || (type is not null && !CanHoldAnAddress(type)))
        {
            return false;
        }

        if (AddressNames.Contains(bare))
        {
            return true;
        }

        var words = Regex.Matches(bare, "[A-Z]?[a-z0-9]+|[A-Z]+(?![a-z])");
        return words.Count > 0 && AddressWords.Contains(words[^1].Value);
    }

    /// <summary>
    /// Whether a value of <paramref name="type" /> can be or carry an address's text: a string, any class, or an array or
    /// generic type over one (a <c>List&lt;string&gt;</c>), but not a number, a date, an enum or a collection of those.
    /// </summary>
    private static bool CanHoldAnAddress(TypeReference type)
        => type switch
        {
            ArrayType array => CanHoldAnAddress(array.ElementType),
            GenericInstanceType generic => generic.GenericArguments.Any(CanHoldAnAddress),
            ByReferenceType reference => CanHoldAnAddress(reference.ElementType),
            _ => !type.IsValueType
        };

    /// <summary>
    /// A name as written: <c>&lt;email&gt;5__2</c> (a hoisted local) and <c>&lt;To&gt;k__BackingField</c> as
    /// <c>email</c> and <c>To</c>, <c>get_To</c> as <c>To</c>, and <c>_email</c> or <c>s_adminEmail</c> without the prefix.
    /// </summary>
    private static string Undecorated(string name)
    {
        var hoisted = Regex.Match(name, "^<([^>]*)>");
        if (hoisted.Success)
        {
            name = hoisted.Groups[1].Value;
        }

        if (name.StartsWith("get_", StringComparison.Ordinal))
        {
            name = name[4..];
        }

        return Regex.Replace(name, "^(?:[sm]_|_+)", string.Empty);
    }

    /// <summary>The holes of a message template: <c>{Name}</c>, <c>{Name:format}</c>, <c>{Name,-10}</c>, <c>{@Name}</c>.</summary>
    private static IEnumerable<string> Placeholders(string template)
        => Regex.Matches(template, @"(?<!\{)\{(?!\{)[@$]?([^{}:,]+)(?:[,:][^{}]*)?\}")
            .Select(match => match.Groups[1].Value.Trim());

    /// <summary>The first address-named public property of <paramref name="type" /> or a base type, if it has one.</summary>
    private static string? AddressPropertyOf(TypeReference type)
    {
        if (type.IsPrimitive || type.MetadataType is MetadataType.String or MetadataType.Object)
        {
            return null;
        }

        TypeDefinition? current;
        try
        {
            current = type.Resolve();
        }
        catch (AssemblyResolutionException)
        {
            return null;
        }

        while (current is not null)
        {
            var property = current.Properties.FirstOrDefault(candidate =>
                candidate.GetMethod is { IsPublic: true } && NamesAnAddress(candidate.Name, candidate.PropertyType));
            if (property is not null)
            {
                return property.Name;
            }

            try
            {
                current = current.BaseType?.Resolve();
            }
            catch (AssemblyResolutionException)
            {
                return null;
            }
        }

        return null;
    }

    // ─── Reading IL ──────────────────────────────────────────────────────────

    /// <summary>
    /// The template's position among <paramref name="target" />'s parameters, when it is a logging API that takes one:
    /// <c>LoggerExtensions.Log*</c> and <c>BeginScope</c>, and <c>LoggerMessage.Define*</c>. Each has exactly one string
    /// parameter, and it is the template (a member reference carries no parameter names to ask by).
    /// </summary>
    private static int? TemplateParameter(MethodReference target)
    {
        if (target.DeclaringType.FullName is not ("Microsoft.Extensions.Logging.LoggerExtensions"
            or "Microsoft.Extensions.Logging.LoggerMessage"))
        {
            return null;
        }

        var strings = target.Parameters
            .Select((parameter, index) => (parameter, index))
            .Where(pair => pair.parameter.ParameterType.MetadataType == MetadataType.String)
            .ToList();
        return strings.Count == 1 ? strings[0].index : null;
    }

    /// <summary>The templates of a <c>[LoggerMessage]</c> method, from the constructor or its <c>Message</c>.</summary>
    private static IEnumerable<string> LoggerMessageTemplates(MethodDefinition method)
        => method.CustomAttributes
            .Where(attribute => attribute.AttributeType.FullName == "Microsoft.Extensions.Logging.LoggerMessageAttribute")
            .SelectMany(attribute => attribute.ConstructorArguments.Select(argument => argument.Value)
                .Concat(attribute.Properties.Where(property => property.Name == "Message")
                    .Select(property => property.Argument.Value)))
            .OfType<string>();

    private static bool IsGeneratedLogging(Mono.Cecil.ICustomAttributeProvider member)
        => member.CustomAttributes.Any(attribute =>
            attribute.AttributeType.FullName == "System.CodeDom.Compiler.GeneratedCodeAttribute" &&
            attribute.ConstructorArguments.FirstOrDefault().Value is "Microsoft.Extensions.Logging.Generators");

    /// <summary>
    /// Whether <paramref name="target" /> invokes a delegate that takes a logger first, as what <c>LoggerMessage.Define*</c>
    /// and <c>DefineScope</c> return does (<c>Action&lt;ILogger, …&gt;</c>, <c>Func&lt;ILogger, …&gt;</c>).
    /// </summary>
    private static bool IsLoggingDelegate(MethodReference target)
        => target.Name == "Invoke" &&
           target.DeclaringType is GenericInstanceType { ElementType.Namespace: "System" } delegateType &&
           (delegateType.ElementType.Name.StartsWith("Action`", StringComparison.Ordinal) ||
            delegateType.ElementType.Name.StartsWith("Func`", StringComparison.Ordinal)) &&
           delegateType.GenericArguments[0].FullName == "Microsoft.Extensions.Logging.ILogger";

    /// <summary>Whether <paramref name="target" /> is a <c>[LoggerMessage]</c> method of Wombat's own.</summary>
    private static bool IsLoggerMessageMethod(MethodReference target)
    {
        // A framework's generated logging methods are internal to it, and resolving every call into the framework would
        // be slow, so only a call into a Wombat assembly is resolved.
        var inWombat = target.DeclaringType.Scope switch
        {
            ModuleDefinition => true,
            AssemblyNameReference reference => reference.Name.StartsWith("Wombat.", StringComparison.Ordinal),
            _ => false
        };
        if (!inWombat)
        {
            return false;
        }

        try
        {
            return target.Resolve()?.CustomAttributes.Any(attribute =>
                attribute.AttributeType.FullName == "Microsoft.Extensions.Logging.LoggerMessageAttribute") == true;
        }
        catch (AssemblyResolutionException)
        {
            return false;
        }
    }

    private static void Apply(Instruction instruction, MethodDefinition method, List<Instruction?> stack)
    {
        if (instruction.OpCode.Code == Code.Dup)
        {
            stack.Add(Peek(stack, 0));
            return;
        }

        if (instruction.OpCode.StackBehaviourPop == StackBehaviour.PopAll)
        {
            stack.Clear();
        }
        else
        {
            for (var popped = PopCount(instruction, method); popped > 0 && stack.Count > 0; popped--)
            {
                stack.RemoveAt(stack.Count - 1);
            }
        }

        for (var pushed = PushCount(instruction); pushed > 0; pushed--)
        {
            stack.Add(instruction);
        }
    }

    private static int PopCount(Instruction instruction, MethodDefinition method)
    {
        var behaviour = instruction.OpCode.StackBehaviourPop;
        if (behaviour != StackBehaviour.Varpop)
        {
            // Pop0, Pop1, Popi_popi, Popref_popi_popref: one value per segment of the name.
            var name = behaviour.ToString();
            return name == nameof(StackBehaviour.Pop0) ? 0 : name.Split('_').Length;
        }

        return instruction.Operand switch
        {
            MethodReference constructor when instruction.OpCode.Code == Code.Newobj => constructor.Parameters.Count,
            MethodReference callee => callee.Parameters.Count + (callee.HasThis && !callee.ExplicitThis ? 1 : 0),
            CallSite site => site.Parameters.Count + (site.HasThis ? 1 : 0) + 1,
            _ => method.ReturnType.MetadataType == MetadataType.Void ? 0 : 1
        };
    }

    private static int PushCount(Instruction instruction)
        => instruction.OpCode.StackBehaviourPush switch
        {
            StackBehaviour.Push0 => 0,
            StackBehaviour.Push1_push1 => 2,
            StackBehaviour.Varpush => instruction.Operand switch
            {
                MethodReference callee => callee.ReturnType.MetadataType == MetadataType.Void ? 0 : 1,
                CallSite site => site.ReturnType.MetadataType == MetadataType.Void ? 0 : 1,
                _ => 0
            },
            _ => 1
        };

    private static IEnumerable<Instruction> BranchTargets(Instruction instruction)
        => instruction.Operand switch
        {
            Instruction target => [target],
            Instruction[] targets => targets,
            _ => []
        };

    private static Instruction? Peek(List<Instruction?> stack, int depth)
        => depth >= 0 && stack.Count > depth ? stack[stack.Count - 1 - depth] : null;

    /// <summary>The type of the value <paramref name="push" /> put on the stack, where the instruction says.</summary>
    private static TypeReference? PushedType(MethodDefinition method, Instruction push)
        => push.OpCode.Code switch
        {
            Code.Box or Code.Castclass or Code.Isinst => (TypeReference)push.Operand,
            Code.Ldfld or Code.Ldsfld => ((FieldReference)push.Operand).FieldType,
            Code.Call or Code.Callvirt => ((MethodReference)push.Operand).ReturnType,
            Code.Newobj => ((MethodReference)push.Operand).DeclaringType,
            _ => ArgumentOf(method, push)?.Type ?? LocalOf(method, push)?.VariableType
        };

    /// <summary>The parameter an <c>ldarg</c> or <c>ldarga</c> loads, with <c>this</c> as the declaring type.</summary>
    private static (string Name, TypeReference Type)? ArgumentOf(MethodDefinition method, Instruction instruction)
    {
        ParameterDefinition? parameter;
        switch (instruction.OpCode.Code)
        {
            case Code.Ldarg_0 or Code.Ldarg_1 or Code.Ldarg_2 or Code.Ldarg_3:
                // The short forms count this first.
                var index = instruction.OpCode.Code switch
                {
                    Code.Ldarg_0 => 0,
                    Code.Ldarg_1 => 1,
                    Code.Ldarg_2 => 2,
                    _ => 3
                };
                if (method.HasThis && index == 0)
                {
                    return ("this", method.DeclaringType);
                }

                var position = method.HasThis ? index - 1 : index;
                parameter = position < method.Parameters.Count ? method.Parameters[position] : null;
                break;

            case Code.Ldarg_S or Code.Ldarg or Code.Ldarga_S or Code.Ldarga:
                parameter = instruction.Operand as ParameterDefinition;
                break;

            default:
                return null;
        }

        if (parameter is null)
        {
            return null;
        }

        return parameter == method.Body.ThisParameter
            ? ("this", method.DeclaringType)
            : (parameter.Name, parameter.ParameterType);
    }

    private static VariableDefinition? LocalOf(MethodDefinition method, Instruction instruction)
    {
        var variables = method.Body.Variables;
        return instruction.OpCode.Code switch
        {
            Code.Ldloc_0 => variables[0],
            Code.Ldloc_1 => variables[1],
            Code.Ldloc_2 => variables[2],
            Code.Ldloc_3 => variables[3],
            Code.Ldloc_S or Code.Ldloc or Code.Ldloca_S or Code.Ldloca => instruction.Operand as VariableDefinition,
            _ => null
        };
    }

    private static bool IsWithin(TypeDefinition type, string within)
        => type.FullName == within || type.FullName.StartsWith(within + "/", StringComparison.Ordinal);

    private static string Where(MethodDefinition method) => $"{method.DeclaringType.FullName}::{method.Name}";

    /// <summary>
    /// The specimen a call site belongs to: its method, or the method whose state machine (<c>&lt;Hoisted&gt;d__3</c>) or
    /// lambda (<c>&lt;Captured&gt;b__0</c>) it is.
    /// </summary>
    private static string SpecimenOf(string where)
    {
        var separator = where.IndexOf("::", StringComparison.Ordinal);
        var type = where[..separator];
        var method = where[(separator + 2)..];

        var lambda = Regex.Match(method, "^<([^>]+)>b__");
        if (lambda.Success)
        {
            return lambda.Groups[1].Value;
        }

        var stateMachine = Regex.Match(type[(type.LastIndexOf('/') + 1)..], "^<([^>]+)>d__");
        return stateMachine.Success ? stateMachine.Groups[1].Value : method;
    }

    private static IEnumerable<TypeDefinition> Flatten(TypeDefinition type)
        => new[] { type }.Concat(type.NestedTypes.SelectMany(Flatten));
}
