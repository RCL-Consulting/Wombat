namespace Wombat.Application.Common;

/// <summary>
/// Marks a MediatR command that intentionally has no FluentValidation validator.
/// Commands marked with this attribute are excluded from the architecture rule that
/// requires every command to have a corresponding validator.
/// </summary>
/// <remarks>
/// Use this attribute only when validation is provably unnecessary — for example,
/// when the command carries a single non-nullable ID (structural guarantee), has no
/// parameters, or when domain-level guards make a separate validator redundant.
/// Add an XML comment on the command explaining the reasoning.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class NoValidatorAttribute : Attribute;
