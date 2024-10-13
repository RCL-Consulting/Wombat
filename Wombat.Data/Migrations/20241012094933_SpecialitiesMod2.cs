using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Data.Migrations
{
    /// <inheritdoc />
    public partial class SpecialitiesMod2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CanDeleteFromList",
                table: "SubSpecialities",
                newName: "CanEditAndDelete");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "19A3D40C-9852-43B9-9BEC-B2552FA715F7",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "aa38f72d-8790-484c-a0fe-adced0ce4897", "AQAAAAIAAYagAAAAELxapYfmclQv4m5sKD4bynnVCfN+Ru2Zzou+NcSIzNvoZMgUqJlgi6WDH6hlmDeqSw==", "b9d4a99b-c4ad-4e1f-8ef8-241773f23280" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "64348419-1486-40bf-97cf-139211630c31", "AQAAAAIAAYagAAAAEI4nv/kwXDmuT3n+FapFk3xek9osUEEz4s5bklI7e3j0BzR88AzhYL8L7bcaGznU/A==", "99c9c11a-1094-48f0-9b30-101855b403fa" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "ec28083f-4ea5-4f61-af17-4466625e8c2e", "AQAAAAIAAYagAAAAEK1BPLxDFUgmcrDPY40yAI5xdX4i/pWVQk2hYWEpX/mFLzf+fODm9xgFxvp/IgG/qg==", "55e08371-0289-48de-9a58-fdc34342a534" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CanEditAndDelete",
                table: "SubSpecialities",
                newName: "CanDeleteFromList");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "19A3D40C-9852-43B9-9BEC-B2552FA715F7",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "9e697d4e-74f8-4803-8a1e-3dff49461fbb", "AQAAAAIAAYagAAAAEAZgvfZnQzpJ8EDMMYnHCJM0MMo1nTdiSawOpmg1GEDAuPJ7SGRoIcVdVqAuF7KtmA==", "d2c33ae6-89f9-4365-a237-facbf5fed131" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "fd56aae3-a5fe-4e8f-a0ce-06b68e17e8f3", "AQAAAAIAAYagAAAAEOGrx9nsiUqEgK4lE24u9W3G2/MAFpiqdWNe8kNzfjOvHcHUgGvsztIs+2zC4nKQRg==", "16776fe2-5868-47fa-b19a-24d2572c54d6" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "4dc1956d-9ddb-44cb-82b1-49f3fae3522b", "AQAAAAIAAYagAAAAEIK3J7Yt4ZLZXv54+x71VdpUC09qNocW6hu+M2Y6P048rwf1RGnzEkGZ7FbmQQ45iw==", "d5ff6159-bf14-46bb-8bba-09dcbcb14974" });
        }
    }
}
