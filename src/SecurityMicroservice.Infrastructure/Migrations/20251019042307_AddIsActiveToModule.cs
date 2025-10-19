using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecurityMicroservice.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Modules",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Modules");
        }
    }
}
