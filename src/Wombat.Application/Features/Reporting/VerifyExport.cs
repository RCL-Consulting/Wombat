using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Reporting;

namespace Wombat.Application.Features.Reporting;

/// <summary>
/// Whether Wombat produced a PDF, from its SHA-256 as the holder computed it (T379, flow 13, S10; T313). The hash is read
/// as the tools print it: every whitespace character is dropped, so certutil's older byte-by-byte form and a pasted line
/// break verify, and it is lower-cased, so Get-FileHash's upper case verifies (the stored hash is lower case,
/// <c>Convert.ToHexStringLower</c>). What cannot be a SHA-256 hash is refused with its reason before anything is looked up.
/// </summary>
public sealed record VerifyExportQuery(string ContentHash) : IRequest<VerifyExportAnswer>;

/// <summary>Why a typed hash is not one, so the page can say so in its own words (never from a message's text).</summary>
public enum VerifyExportRefusal
{
    /// <summary>Nothing but whitespace.</summary>
    Blank,

    /// <summary>A character that is no hexadecimal digit, named in <see cref="VerifyExportAnswer.Character" />.</summary>
    NotHexadecimal,

    /// <summary>
    /// Twelve hexadecimal digits: the start of the hash that a file's name carries (<c>portfolio-aba6ce7943c2.pdf</c>), not
    /// the hash (V06).
    /// </summary>
    FileNamePrefix,

    /// <summary>Hexadecimal, but not 64 digits; the count is <see cref="VerifyExportAnswer.Length" />.</summary>
    WrongLength
}

/// <summary>
/// The check's answer: a refusal, or the export the hash belongs to, or neither (a well-formed hash no export has).
/// </summary>
public sealed record VerifyExportAnswer
{
    public VerifyExportRefusal? Refusal { get; init; }

    /// <summary>The characters typed, whitespace dropped (<see cref="VerifyExportRefusal.WrongLength" />).</summary>
    public int Length { get; init; }

    /// <summary>The first character that is no hexadecimal digit, as typed (<see cref="VerifyExportRefusal.NotHexadecimal" />).</summary>
    public string? Character { get; init; }

    /// <summary>The export this hash is, when it is a hash and an export has it.</summary>
    public PortfolioExportRecordDto? Export { get; init; }
}

public sealed class VerifyExportQueryHandler : IRequestHandler<VerifyExportQuery, VerifyExportAnswer>
{
    /// <summary>A SHA-256 hash in hexadecimal.</summary>
    public const int HashLength = 64;

    /// <summary>How much of the hash a portfolio's file name carries (<c>PortfolioPdfService</c>).</summary>
    public const int FileNamePrefixLength = 12;

    private readonly IApplicationDbContext _dbContext;

    public VerifyExportQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<VerifyExportAnswer> Handle(VerifyExportQuery request, CancellationToken cancellationToken)
    {
        var typed = WithoutWhitespace(request.ContentHash ?? string.Empty);
        if (typed.Length == 0)
        {
            return new VerifyExportAnswer { Refusal = VerifyExportRefusal.Blank };
        }

        // A character that is no digit is named before any length is counted: "O" for a zero leaves the length right and
        // the hash wrong, and a pasted "SHA256 hash of …" line is told by its first word (T379, S10).
        foreach (var rune in typed.EnumerateRunes())
        {
            if (!(rune.IsAscii && char.IsAsciiHexDigit((char)rune.Value)))
            {
                return new VerifyExportAnswer { Refusal = VerifyExportRefusal.NotHexadecimal, Character = rune.ToString() };
            }
        }

        if (typed.Length != HashLength)
        {
            return new VerifyExportAnswer
            {
                Refusal = typed.Length == FileNamePrefixLength ? VerifyExportRefusal.FileNamePrefix : VerifyExportRefusal.WrongLength,
                Length = typed.Length
            };
        }

        var hash = typed.ToLowerInvariant();

        // The same dates exported again give the same file, so one hash may have several rows (step 5.9's three): the
        // answer is the first generation and its exporter, the count, and the last (T379, S10). Rows are never deleted by
        // age (T313), so a file verifies as long as its trainee's record is kept.
        var exports = await _dbContext.Set<PortfolioExport>()
            .AsNoTracking()
            .Where(export => export.ContentHash == hash)
            .OrderBy(export => export.ExportedOn)
            .ThenBy(export => export.Id)
            .ToListAsync(cancellationToken);

        if (exports.Count == 0)
        {
            return new VerifyExportAnswer();
        }

        var first = exports[0];
        return new VerifyExportAnswer
        {
            Export = new PortfolioExportRecordDto(
                first.TraineeUserId,
                first.ExportedByUserId,
                first.ExportedOn,
                exports[^1].ExportedOn,
                exports.Count,
                first.FilterFromDate,
                first.FilterToDate,
                first.ContentHash,
                first.FileName)
        };
    }

    private static string WithoutWhitespace(string text)
    {
        var kept = new StringBuilder(text.Length);
        foreach (var rune in text.EnumerateRunes())
        {
            // A zero-width space (U+200B) or a byte-order mark (U+FEFF) is no whitespace to .NET, but pasted it is as
            // invisible as one, and a refusal could only name an invisible character (T379, B6).
            if (!Rune.IsWhiteSpace(rune) && rune.Value is not (0x200B or 0xFEFF))
            {
                kept.Append(rune.ToString());
            }
        }

        return kept.ToString();
    }
}
