using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// T137: an activity knows its EPA. Indexes <c>Activities.EpaId</c>, and on an existing database gives every stored
    /// schema whose EPA field is determined the <c>evidence_epa_field</c> pointer, then stamps <c>EpaId</c> on every
    /// existing activity through its PINNED version's pointer.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Why the stored schemas are touched, not only the activities. <c>ActivityService</c> re-stamps <c>EpaId</c> on
    /// every transition from the pinned version's pointer. Program.cs migrates BEFORE the seed refresher runs, and the
    /// refresher only ever adds a NEW version, so every activity already in flight stays pinned to a version with no
    /// pointer. A backfill of the column alone would therefore be erased by the next move on every in-flight activity:
    /// the EPA would appear on the list and vanish when the assessor completed it. Giving those versions the pointer
    /// makes the migration and the app's one resolver agree on every row, now and after any later transition. It adds
    /// only a pointer at a field each version already has, of type <c>epa</c>, whose credit (if any) already reads it;
    /// nothing the version renders, validates or credits changes. W-007 permits it; the alternative was re-seeding.
    /// </para>
    /// <para>
    /// A side effect, and the intended one: the refresher then finds the seeded types already in sync with their seed
    /// folders (which gained the same pointer), so this release republishes nothing and bumps no version.
    /// </para>
    /// <para>
    /// Which field gets the pointer mirrors <c>EvidenceEpa.EnsureCreditAgrees</c>, so the migration writes only schemas
    /// that rule accepts, and never guesses:
    /// <list type="bullet">
    ///   <item>No directive may target an item (a literal <c>curriculum_item_id</c> or a <c>curriculum_item_field</c>):
    ///   the rule refuses a pointer beside one, and such a type needs none.</item>
    ///   <item>If the credit reads its EPA from exactly one field, that field, provided it is an <c>epa</c> field. It is
    ///   the only value the rule would accept, so it is determined, not guessed, whatever its key.</item>
    ///   <item>If the credit reads no EPA field (it credits nothing), the form's only <c>epa</c> field, when it has
    ///   exactly one: the same invariant every seed holds (14 of 19 carry one, all keyed <c>epa_id</c>).</item>
    /// </list>
    /// Anything else (two EPA fields read by credit, an EPA field that is not an <c>epa</c> field, two EPA fields and no
    /// credit) is left alone; the rule makes its author choose at the next save.
    /// </para>
    /// <para>
    /// The id is read as the resolver reads it (<c>int.TryParse</c> with <c>NumberStyles.Integer</c>): a JSON number,
    /// or a string of digits with an optional <c>+</c> and ASCII whitespace either side. The digits are captured by
    /// <c>substring</c> rather than cast whole, so the cast only ever sees at most nine digits and cannot fail the
    /// migration, and the id is kept only when an EPA with it exists. A negative id or one past nine significant digits
    /// is left null; the resolver parses those but no EPA has them, so it stamps null too.
    /// </para>
    /// </remarks>
    public partial class T137_ActivityEvidenceEpa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Activities_EpaId",
                table: "Activities",
                column: "EpaId");

            // HAND-EDIT 1. The pointer, on every stored schema that qualifies: the published versions activities pin
            // to, the type's live copy of its newest version, and a draft in flight.
            migrationBuilder.Sql(AddPointer("ActivityTypeVersions", "SchemaJson", "CreditRulesJson"));
            migrationBuilder.Sql(AddPointer("ActivityTypes", "SchemaJson", "CreditRulesJson"));
            migrationBuilder.Sql(AddPointer("ActivityTypes", "StagingSchemaJson", "StagingCreditRulesJson"));

            // HAND-EDIT 2. The stamp, through the pinned version's pointer. substring() yields the digits or NULL, so
            // the cast never sees anything but one to nine digits.
            migrationBuilder.Sql("""
                UPDATE "Activities" AS a
                SET "EpaId" = e."Id"
                FROM "ActivityTypeVersions" AS v, "Epas" AS e
                WHERE v."ActivityTypeId" = a."ActivityTypeId"
                  AND v."Version" = a."SchemaVersion"
                  AND v."SchemaJson" ->> 'evidence_epa_field' IS NOT NULL
                  AND jsonb_typeof(a."DataJson" -> (v."SchemaJson" ->> 'evidence_epa_field')) IN ('number', 'string')
                  AND e."Id" = substring(
                      a."DataJson" ->> (v."SchemaJson" ->> 'evidence_epa_field')
                      FROM '^[ \t\n\r\f\v]*\+?0*([0-9]{1,9})[ \t\n\r\f\v]*$')::integer;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // The pointer comes off EVERY schema, including those the seeders and the refresher wrote after Up: the
            // parser before T137 refuses an unknown root property, so leaving it would make every such type unloadable.
            migrationBuilder.Sql("""
                UPDATE "ActivityTypeVersions" SET "SchemaJson" = "SchemaJson" - 'evidence_epa_field'
                WHERE "SchemaJson" -> 'evidence_epa_field' IS NOT NULL;

                UPDATE "ActivityTypes" SET "SchemaJson" = "SchemaJson" - 'evidence_epa_field'
                WHERE "SchemaJson" -> 'evidence_epa_field' IS NOT NULL;

                UPDATE "ActivityTypes" SET "StagingSchemaJson" = "StagingSchemaJson" - 'evidence_epa_field'
                WHERE "StagingSchemaJson" -> 'evidence_epa_field' IS NOT NULL;

                UPDATE "Activities" SET "EpaId" = NULL WHERE "EpaId" IS NOT NULL;
                """);

            migrationBuilder.DropIndex(
                name: "IX_Activities_EpaId",
                table: "Activities");
        }

        /// <summary>
        /// Gives a schema column the <c>evidence_epa_field</c> pointer where it has none yet and its EPA field is
        /// determined (see the class remarks), so every schema it touches passes <c>EvidenceEpa.EnsureCreditAgrees</c>.
        /// </summary>
        /// <remarks>
        /// The target is computed in a CTE because an <c>UPDATE</c>'s <c>FROM</c> cannot refer to the target table
        /// laterally. <c>credit</c> is the distinct EPA fields the credit reads; <c>epa_fields</c> the form's
        /// <c>epa</c> fields. The final <c>jsonb_path_exists</c> is what makes a credit-named field qualify only when it
        /// is an <c>epa</c> field.
        /// </remarks>
        private static string AddPointer(string table, string schemaColumn, string creditColumn) => $$"""
            WITH target AS (
                SELECT src."Id",
                       CASE
                           WHEN credit.n = 1 THEN credit.only_key
                           WHEN credit.n = 0 AND epa_fields.n = 1 THEN epa_fields.only_key
                       END AS field_key
                FROM "{{table}}" AS src
                CROSS JOIN LATERAL (
                    SELECT COUNT(DISTINCT btrim(m ->> 'epa_field')) AS n, MIN(btrim(m ->> 'epa_field')) AS only_key
                    FROM jsonb_path_query(COALESCE(src."{{creditColumn}}", '{}'::jsonb), '$.counts_for[*].curriculum_item_match') AS m
                    WHERE jsonb_typeof(m -> 'epa_field') = 'string' AND btrim(m ->> 'epa_field') <> ''
                ) AS credit
                CROSS JOIN LATERAL (
                    SELECT COUNT(*) AS n, MIN(f ->> 'key') AS only_key
                    FROM jsonb_path_query(src."{{schemaColumn}}", '$.sections[*].fields[*] ? (@.type == "epa")') AS f
                ) AS epa_fields
                WHERE src."{{schemaColumn}}" IS NOT NULL
                  AND src."{{schemaColumn}}" -> 'evidence_epa_field' IS NULL
                  AND NOT EXISTS (
                      SELECT 1
                      FROM jsonb_path_query(COALESCE(src."{{creditColumn}}", '{}'::jsonb), '$.counts_for[*].curriculum_item_match') AS m
                      WHERE jsonb_typeof(m -> 'curriculum_item_id') = 'number'
                         OR btrim(COALESCE(m ->> 'curriculum_item_field', '')) <> '')
            )
            UPDATE "{{table}}" AS t
            SET "{{schemaColumn}}" = jsonb_set(t."{{schemaColumn}}", '{evidence_epa_field}', to_jsonb(target.field_key))
            FROM target
            WHERE t."Id" = target."Id"
              AND target.field_key IS NOT NULL
              AND jsonb_path_exists(
                  t."{{schemaColumn}}",
                  '$.sections[*].fields[*] ? (@.key == $k && @.type == "epa")',
                  jsonb_build_object('k', target.field_key));
            """;
    }
}
