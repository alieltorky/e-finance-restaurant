using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Online_Restaurant.Migrations
{
    /// <inheritdoc />
    public partial class menuItemIdForshadow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenuIngredients_MenuItems_Menu_ItemMenuItemId",
                table: "MenuIngredients");

            migrationBuilder.DropColumn(
                name: "MenuItemId",
                table: "MenuIngredients");

            migrationBuilder.RenameColumn(
                name: "Menu_ItemMenuItemId",
                table: "MenuIngredients",
                newName: "Menu_ItemId");

            migrationBuilder.RenameIndex(
                name: "IX_MenuIngredients_Menu_ItemMenuItemId",
                table: "MenuIngredients",
                newName: "IX_MenuIngredients_Menu_ItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_MenuIngredients_MenuItems_Menu_ItemId",
                table: "MenuIngredients",
                column: "Menu_ItemId",
                principalTable: "MenuItems",
                principalColumn: "MenuItemId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenuIngredients_MenuItems_Menu_ItemId",
                table: "MenuIngredients");

            migrationBuilder.RenameColumn(
                name: "Menu_ItemId",
                table: "MenuIngredients",
                newName: "Menu_ItemMenuItemId");

            migrationBuilder.RenameIndex(
                name: "IX_MenuIngredients_Menu_ItemId",
                table: "MenuIngredients",
                newName: "IX_MenuIngredients_Menu_ItemMenuItemId");

            migrationBuilder.AddColumn<int>(
                name: "MenuItemId",
                table: "MenuIngredients",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_MenuIngredients_MenuItems_Menu_ItemMenuItemId",
                table: "MenuIngredients",
                column: "Menu_ItemMenuItemId",
                principalTable: "MenuItems",
                principalColumn: "MenuItemId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
