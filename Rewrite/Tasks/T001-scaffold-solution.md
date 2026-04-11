# T001 — Scaffold the solution

**Phase:** 0 — Ground truth
**Depends on:** nothing
**Blocks:** T002, T010

## Goal

Create an empty but buildable five-project solution matching `ARCHITECTURE.md`, with NuGet centrally managed, warnings-as-errors in Release, and a green `dotnet build` from a fresh clone.

## What to do

1. Delete (or move aside) the current Wombat projects: `Wombat.Common`, `Wombat.Data`, `Wombat.Application`, `Wombat.Web`, `Wombat.sln`. Keep the reference folder and the `Rewrite/` folder intact. Commit this as a single "remove old tree" commit so git history is clean.
2. Create a new solution at the repo root: `dotnet new sln -n Wombat`.
3. Under `src/`, create five projects:
   - `Wombat.Domain` (`classlib`)
   - `Wombat.Application` (`classlib`)
   - `Wombat.Infrastructure` (`classlib`)
   - `Wombat.Api` (`webapi` — minimal API style)
   - `Wombat.Web` (`blazor` — Interactive Server)
4. Under `tests/`, create four projects:
   - `Wombat.Domain.Tests` (xunit)
   - `Wombat.Application.Tests` (xunit)
   - `Wombat.Architecture.Tests` (xunit)
   - `Wombat.Integration.Tests` (xunit)
5. Wire project references per ARCHITECTURE.md layer rules.
6. Copy `Directory.Build.props`, `Directory.Packages.props`, `.editorconfig` from `ClinicAssist.NET_ref_DO_NOT_COMMIT/`. Change the `RootNamespace` prefix if needed. Keep the same NuGet versions.
7. Copy `.gitignore` from the reference or use the standard `dotnet new gitignore`. Add `Rewrite/` patterns if any plan files should be ignored (none by default — all plan files are committed).
8. Add a minimal `Program.cs` to `Wombat.Web` with just `builder.Build().Run()` — enough to boot an empty Blazor shell. The real configuration goes in later tasks.
9. Add a minimal `Program.cs` to `Wombat.Api` — an empty minimal API with just `app.MapGet("/health", () => "ok")`.
10. Commit.

## Files created

- `Wombat.sln`
- `Directory.Build.props`
- `Directory.Packages.props`
- `.editorconfig`
- `.gitignore`
- `src/Wombat.Domain/Wombat.Domain.csproj`
- `src/Wombat.Application/Wombat.Application.csproj`
- `src/Wombat.Infrastructure/Wombat.Infrastructure.csproj`
- `src/Wombat.Api/Wombat.Api.csproj`, `Program.cs`, `appsettings.json`
- `src/Wombat.Web/Wombat.Web.csproj`, `Program.cs`, `App.razor`, `Components/App.razor`, `Components/Routes.razor`, `appsettings.json`
- `tests/Wombat.Domain.Tests/Wombat.Domain.Tests.csproj`
- `tests/Wombat.Application.Tests/Wombat.Application.Tests.csproj`
- `tests/Wombat.Architecture.Tests/Wombat.Architecture.Tests.csproj`
- `tests/Wombat.Integration.Tests/Wombat.Integration.Tests.csproj`

## Verification

- [ ] `dotnet build -c Release` exits 0 with zero warnings.
- [ ] `dotnet test` exits 0 (there are no real tests yet; the projects should compile and the runners should report "no tests").
- [ ] `dotnet run --project src/Wombat.Web` starts, binds to a port, and `curl http://localhost:5080/` returns an HTML shell.
- [ ] `dotnet run --project src/Wombat.Api` starts and `curl http://localhost:5090/health` returns `ok`.

## Notes & gotchas

- The old Wombat tree must be fully removed before scaffolding. Don't try to shoehorn the new structure on top of the old one.
- Blazor Interactive Server was added in .NET 8; make sure the csproj `TargetFramework` matches what ClinicAssist uses (copy exactly — do not upgrade to a newer preview).
- Do not enable nullable warnings as errors yet; that comes after first real code lands and we know the patterns. Keep `<Nullable>enable</Nullable>` but not `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` for Phase 0.
