using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Online_Restaurant.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InventoryName",
                table: "Inventory",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InventoryName",
                table: "Inventory");
        }
    }
}
