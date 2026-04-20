using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameStarReflection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE \"ActivityTypes\" " +
                "SET \"Key\" = 'reflective_note', \"Title\" = 'Reflective Note' " +
                "WHERE \"Key\" = 'star_reflection';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE \"ActivityTypes\" " +
                "SET \"Key\" = 'star_reflection', \"Title\" = 'STAR Reflection' " +
                "WHERE \"Key\" = 'reflective_note';");
        }
    }
}
