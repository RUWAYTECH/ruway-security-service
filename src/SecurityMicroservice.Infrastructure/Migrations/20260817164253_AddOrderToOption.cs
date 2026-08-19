using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecurityMicroservice.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderToOption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Options",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Backfill: numera 1..n por módulo respetando el orden por Name,
            // que es el que ya usan los listados de opciones.
            migrationBuilder.Sql(@"
                UPDATE o
                SET o.[Order] = x.rn
                FROM Options o
                INNER JOIN (
                    SELECT OptionId,
                           ROW_NUMBER() OVER (PARTITION BY ModuleId ORDER BY Name) AS rn
                    FROM Options
                ) x ON x.OptionId = o.OptionId;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Order",
                table: "Options");
        }
    }
}
