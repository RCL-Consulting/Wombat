using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Wombat.Infrastructure.Reporting;

internal static class CoverPageComponent
{
    public static void Compose(IContainer container, PortfolioData data)
    {
        container.Column(column =>
        {
            column.Item().Height(60);

            column.Item().AlignCenter().Text(text =>
            {
                text.Span(data.InstitutionName).Bold().FontSize(18).FontColor(Colors.Grey.Darken3);
            });

            column.Item().Height(30);

            column.Item().AlignCenter().Text(text =>
            {
                text.Span("Portfolio Export").Bold().FontSize(22).FontColor(Colors.Blue.Darken3);
            });

            column.Item().Height(20);

            column.Item().AlignCenter().Text(text =>
            {
                text.Span(data.TraineeName).FontSize(14);
            });

            column.Item().Height(8);

            if (data.ProgrammeName is not null)
            {
                column.Item().AlignCenter().Text(text =>
                {
                    text.Span(data.ProgrammeName).FontSize(11).FontColor(Colors.Grey.Darken1);
                });
            }

            if (data.SpecialityName is not null)
            {
                column.Item().Height(4);
                column.Item().AlignCenter().Text(text =>
                {
                    text.Span(data.SpecialityName).FontSize(10).FontColor(Colors.Grey.Darken1);
                    if (data.SubSpecialityName is not null)
                    {
                        text.Span($" — {data.SubSpecialityName}").FontSize(10).FontColor(Colors.Grey.Darken1);
                    }
                });
            }

            column.Item().Height(20);

            var dateRange = FormatDateRange(data.FromDate, data.ToDate);
            if (dateRange is not null)
            {
                column.Item().AlignCenter().Text(text =>
                {
                    text.Span("Period: ").FontSize(10);
                    text.Span(dateRange).FontSize(10).Bold();
                });
            }

            column.Item().Height(40);

            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
        });
    }

    private static string? FormatDateRange(DateOnly? from, DateOnly? to)
    {
        if (from.HasValue && to.HasValue)
        {
            return $"{from.Value:yyyy-MM-dd} to {to.Value:yyyy-MM-dd}";
        }

        if (from.HasValue)
        {
            return $"From {from.Value:yyyy-MM-dd}";
        }

        if (to.HasValue)
        {
            return $"Up to {to.Value:yyyy-MM-dd}";
        }

        return null;
    }
}
