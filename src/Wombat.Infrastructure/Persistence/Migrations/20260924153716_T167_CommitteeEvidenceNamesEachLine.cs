using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// T167: a committee evidence line records its EPA, instrument, rating, encounter date and state, frozen at Start.
    /// Every column is nullable and none is backfilled: a line frozen before this existed recorded none of it, and
    /// guessing it now from the activity as it stands would rewrite what the committee saw. The review page lists such
    /// lines as they were frozen; a review started afresh records them (W-007).
    /// </summary>
    public partial class T167_CommitteeEvidenceNamesEachLine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EpaCode",
                table: "CommitteeEvidenceItems",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EpaId",
                table: "CommitteeEvidenceItems",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EpaTitle",
                table: "CommitteeEvidenceItems",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstrumentKey",
                table: "CommitteeEvidenceItems",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstrumentName",
                table: "CommitteeEvidenceItems",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRatedInstrument",
                table: "CommitteeEvidenceItems",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ObservedOn",
                table: "CommitteeEvidenceItems",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ObservedOnSource",
                table: "CommitteeEvidenceItems",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RatingLabel",
                table: "CommitteeEvidenceItems",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RatingOrder",
                table: "CommitteeEvidenceItems",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceState",
                table: "CommitteeEvidenceItems",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EpaCode",
                table: "CommitteeEvidenceItems");

            migrationBuilder.DropColumn(
                name: "EpaId",
                table: "CommitteeEvidenceItems");

            migrationBuilder.DropColumn(
                name: "EpaTitle",
                table: "CommitteeEvidenceItems");

            migrationBuilder.DropColumn(
                name: "InstrumentKey",
                table: "CommitteeEvidenceItems");

            migrationBuilder.DropColumn(
                name: "InstrumentName",
                table: "CommitteeEvidenceItems");

            migrationBuilder.DropColumn(
                name: "IsRatedInstrument",
                table: "CommitteeEvidenceItems");

            migrationBuilder.DropColumn(
                name: "ObservedOn",
                table: "CommitteeEvidenceItems");

            migrationBuilder.DropColumn(
                name: "ObservedOnSource",
                table: "CommitteeEvidenceItems");

            migrationBuilder.DropColumn(
                name: "RatingLabel",
                table: "CommitteeEvidenceItems");

            migrationBuilder.DropColumn(
                name: "RatingOrder",
                table: "CommitteeEvidenceItems");

            migrationBuilder.DropColumn(
                name: "SourceState",
                table: "CommitteeEvidenceItems");
        }
    }
}
