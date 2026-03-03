using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SecurityMicroservice.Infrastructure.Data;

#nullable disable

namespace SecurityMicroservice.Infrastructure.Migrations
{
    [DbContext(typeof(SecurityDbContext))]
    [Migration("20260303123500_AddIsExternalToUsers")]
    public partial class AddIsExternalToUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsExternal",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsExternal",
                table: "Users");
        }
    }
}
