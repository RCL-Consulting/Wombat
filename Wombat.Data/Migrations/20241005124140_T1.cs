using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Data.Migrations
{
    /// <inheritdoc />
    public partial class T1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Options_OptionSets_OptionSetId",
                table: "Options");

            migrationBuilder.AlterColumn<int>(
                name: "EPAId",
                table: "AssessmentTemplates",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "19A3D40C-9852-43B9-9BEC-B2552FA715F7",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "7553e95c-49f1-4d1d-bbeb-741d42e2f97e", "AQAAAAIAAYagAAAAEFk5OHD+r48WsxsR+bFoHriPL+kXqw0adurkZlpyc2ewjgBiuQmnsAhl8uu7HQonvQ==", "b1b550a2-d898-4faa-aca5-1e87a94eb995" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "72150f2c-f7ed-41bd-a61c-08833157438d", "AQAAAAIAAYagAAAAEMm4+xyaNyyRMaPAWlMPd9/XSttJHQH+VBsNc7alzrxbUa8Je2/qq8ow86onEWRPsw==", "3d13e857-144f-48d3-b979-63ddba52a8d0" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "485c83ea-6d42-477b-b45b-d68a664fb754", "AQAAAAIAAYagAAAAEKaFzJm4busW73vjtHC/Ceyf//zZd3SmlnUoaCBpXVKqmr2vcWY5E6g6UypfdfCsWw==", "3663d175-0c3c-4f90-9f9a-d35f1a9fedf9" });

            migrationBuilder.AddForeignKey(
                name: "FK_Options_OptionSets_OptionSetId",
                table: "Options",
                column: "OptionSetId",
                principalTable: "OptionSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Options_OptionSets_OptionSetId",
                table: "Options");

            migrationBuilder.AlterColumn<int>(
                name: "EPAId",
                table: "AssessmentTemplates",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "19A3D40C-9852-43B9-9BEC-B2552FA715F7",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "d5205226-282c-4a73-b538-f176b3b096a9", "AQAAAAIAAYagAAAAEKDbhtTFP15f5NjmfwcPib2v6IQGnJu7dBJdrCrEv7Ox7UdZOFEQOJIYPqZQo0zbfA==", "eb54c48c-37e8-42a9-945b-7317bc4c658f" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "0eeace9d-510f-4ef4-a50c-a2fbc86ac401", "AQAAAAIAAYagAAAAEOW3ylzQr/WWcvEDiYs9cwyKx38CWwvb1KeMt6sk1qmDhkAO5lYGY67a3aNw7Whm6w==", "adef23fe-beeb-411a-b87e-34ab31884136" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "7cfd1991-f9b5-44a7-8512-73cca149abc1", "AQAAAAIAAYagAAAAEJYXPqGQyn67espaRUkvYbBOKI4k6r/wQVJMNBdDXfhqGz9o8r+pqO2eimg6VvgnzA==", "02ff9362-34d4-49f4-8a83-122882ce56c2" });

            migrationBuilder.AddForeignKey(
                name: "FK_Options_OptionSets_OptionSetId",
                table: "Options",
                column: "OptionSetId",
                principalTable: "OptionSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
