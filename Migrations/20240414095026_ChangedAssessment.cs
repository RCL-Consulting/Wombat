using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Migrations
{
    /// <inheritdoc />
    public partial class ChangedAssessment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssessorId",
                table: "Assessments");

            migrationBuilder.RenameColumn(
                name: "TraineeId",
                table: "Assessments",
                newName: "Description");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "7361f54f-29e1-4be2-bd38-101034bd5c17", "AQAAAAIAAYagAAAAEPtqeBmKjavFWhvIzY13ttDp1XG0T0o51VcwNzrU9u5yvKZ7eNvJdXB59CsfBQ7Y2Q==", "5356149e-3388-4920-9f49-4aa628274cbe" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "fda66001-d85d-4e15-9829-237b937a0dd0", "AQAAAAIAAYagAAAAEA+4PCUfnfxT3uIJjdpL7FtSz04kqc5PViAO1Z6xI9uXj0mt5+nD/54BZP0K3Lm/Og==", "1685029d-4ebf-4395-a814-e2b2c7e4c2fd" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Assessments",
                newName: "TraineeId");

            migrationBuilder.AddColumn<string>(
                name: "AssessorId",
                table: "Assessments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "3387d3bb-a996-4940-a2f4-f8ab67b17aba", "AQAAAAIAAYagAAAAEDWsFx57ZnrLbcM7G/70fyGngCHmvB7pHvbqkhazavoXmm5+21lJFm0dJVygg5DUFA==", "e1ce16c5-76c9-4e89-97e8-1a279b65e10d" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "04cdaf60-a613-4003-9a2b-ae077b52c269", "AQAAAAIAAYagAAAAEJQ09hKsLHIHDLt4x2+sk4vUCTnNfwTO7RrgXOC+gCmgEDB3/B63fJrgXNdmgRd+Rw==", "6a6aec25-ff36-4430-9b0e-ddab018628c9" });
        }
    }
}
