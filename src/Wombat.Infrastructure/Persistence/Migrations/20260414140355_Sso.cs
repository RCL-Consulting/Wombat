using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Sso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowLocalPassword",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // All existing users are invitation-provisioned with local passwords.
            migrationBuilder.Sql("UPDATE \"AspNetUsers\" SET \"AllowLocalPassword\" = true;");

            migrationBuilder.CreateTable(
                name: "SsoGroupRoleMappings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProviderKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ExternalGroupId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ExternalGroupDisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    WombatRole = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    InstitutionId = table.Column<int>(type: "integer", nullable: false),
                    SpecialityId = table.Column<int>(type: "integer", nullable: true),
                    SubSpecialityId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SsoGroupRoleMappings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserRoleAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Role = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ProviderKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AssignedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoleAssignments", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SsoGroupRoleMappings_ProviderKey_ExternalGroupId_WombatRole",
                table: "SsoGroupRoleMappings",
                columns: new[] { "ProviderKey", "ExternalGroupId", "WombatRole" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleAssignments_UserId",
                table: "UserRoleAssignments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleAssignments_UserId_Role_Source",
                table: "UserRoleAssignments",
                columns: new[] { "UserId", "Role", "Source" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SsoGroupRoleMappings");

            migrationBuilder.DropTable(
                name: "UserRoleAssignments");

            migrationBuilder.DropColumn(
                name: "AllowLocalPassword",
                table: "AspNetUsers");
        }
    }
}
