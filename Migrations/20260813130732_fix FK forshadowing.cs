using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Online_Restaurant.Migrations
{
    /// <inheritdoc />
    public partial class fixFKforshadowing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_MenuItems_Menu_ItemMenuItemId",
                table: "OrderDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_Orders_OrdersOrderId",
                table: "OrderDetails");

            migrationBuilder.DropIndex(
                name: "IX_OrderDetails_Menu_ItemMenuItemId",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "MenuItemId",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "Menu_ItemMenuItemId",
                table: "OrderDetails");

            migrationBuilder.RenameColumn(
                name: "OrdersOrderId",
                table: "OrderDetails",
                newName: "Menu_ItemId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderDetails_OrdersOrderId",
                table: "OrderDetails",
                newName: "IX_OrderDetails_Menu_ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_OrderId",
                table: "OrderDetails",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_MenuItems_Menu_ItemId",
                table: "OrderDetails",
                column: "Menu_ItemId",
                principalTable: "MenuItems",
                principalColumn: "MenuItemId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_Orders_OrderId",
                table: "OrderDetails",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "OrderId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_MenuItems_Menu_ItemId",
                table: "OrderDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_Orders_OrderId",
                table: "OrderDetails");

            migrationBuilder.DropIndex(
                name: "IX_OrderDetails_OrderId",
                table: "OrderDetails");

            migrationBuilder.RenameColumn(
                name: "Menu_ItemId",
                table: "OrderDetails",
                newName: "OrdersOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderDetails_Menu_ItemId",
                table: "OrderDetails",
                newName: "IX_OrderDetails_OrdersOrderId");

            migrationBuilder.AddColumn<int>(
                name: "MenuItemId",
                table: "OrderDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Menu_ItemMenuItemId",
                table: "OrderDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_Menu_ItemMenuItemId",
                table: "OrderDetails",
                column: "Menu_ItemMenuItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_MenuItems_Menu_ItemMenuItemId",
                table: "OrderDetails",
                column: "Menu_ItemMenuItemId",
                principalTable: "MenuItems",
                principalColumn: "MenuItemId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_Orders_OrdersOrderId",
                table: "OrderDetails",
                column: "OrdersOrderId",
                principalTable: "Orders",
                principalColumn: "OrderId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
