using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Online_Restaurant.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderIdINInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrderId",
                table: "Inventories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_OrderId",
                table: "Inventories",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_Orders_OrderId",
                table: "Inventories",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "OrderId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_Orders_OrderId",
                table: "Inventories");

            migrationBuilder.DropIndex(
                name: "IX_Inventories_OrderId",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "Inventories");
        }
    }
}
