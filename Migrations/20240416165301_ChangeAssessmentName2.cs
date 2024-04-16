using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Migrations
{
    /// <inheritdoc />
    public partial class ChangeAssessmentName2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assessments_AssessmentCategories_AssessmentCategoryId",
                table: "Assessments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Assessments",
                table: "Assessments");

            migrationBuilder.RenameTable(
                name: "Assessments",
                newName: "AssessmentContexts");

            migrationBuilder.RenameIndex(
                name: "IX_Assessments_AssessmentCategoryId",
                table: "AssessmentContexts",
                newName: "IX_AssessmentContexts_AssessmentCategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AssessmentContexts",
                table: "AssessmentContexts",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "f358f740-a26e-4016-89c5-69e27f84683a", "AQAAAAIAAYagAAAAEPt0O2ojBm+NKq11aGj5USrDCh3EJFkoiD/WjOLYj3dIrscArnRODO0F24TDPJD9oQ==", "2950989d-ddd3-4717-a02e-1b5ffa2b49c1" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "e7a1d997-433a-4de5-aacc-48bc8c54ba52", "AQAAAAIAAYagAAAAEJKz85xEgOekWpk86eddhsvN0JWYbsoYk4VAogf9uk1LftdkpMpqsFi5Xo/3c7n0PA==", "6e1bf4d1-96f8-4785-9ebd-56d4c1897135" });

            migrationBuilder.AddForeignKey(
                name: "FK_AssessmentContexts_AssessmentCategories_AssessmentCategoryId",
                table: "AssessmentContexts",
                column: "AssessmentCategoryId",
                principalTable: "AssessmentCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssessmentContexts_AssessmentCategories_AssessmentCategoryId",
                table: "AssessmentContexts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AssessmentContexts",
                table: "AssessmentContexts");

            migrationBuilder.RenameTable(
                name: "AssessmentContexts",
                newName: "Assessments");

            migrationBuilder.RenameIndex(
                name: "IX_AssessmentContexts_AssessmentCategoryId",
                table: "Assessments",
                newName: "IX_Assessments_AssessmentCategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Assessments",
                table: "Assessments",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "52ac863d-409f-481b-a654-9c068377d0cc", "AQAAAAIAAYagAAAAEKL2yostPhir2oBNCaR5dt6Cd8roKjNyPSf/rLZmAuCiekfNX6HZSQscWFm26i4xeA==", "efddbd20-4e6a-4273-b922-9aafc43a18c9" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "649ce986-ce19-484e-be56-dbaf2cd3daee", "AQAAAAIAAYagAAAAEEogWsbfFfrFB5NVCaIoe7HPkSYaAANVyPQ+J0Isnz2uUhPrvFadhfiqwIPaTxshqA==", "a698602a-6272-49f6-be30-4810f6fa3a24" });

            migrationBuilder.AddForeignKey(
                name: "FK_Assessments_AssessmentCategories_AssessmentCategoryId",
                table: "Assessments",
                column: "AssessmentCategoryId",
                principalTable: "AssessmentCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
