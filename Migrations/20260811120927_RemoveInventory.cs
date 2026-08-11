using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Online_Restaurant.Migrations
{
    /// <inheritdoc />
    public partial class RemoveInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ingredients_Inventory_InventoryId",
                table: "Ingredients");

            migrationBuilder.DropForeignKey(
                name: "FK_SupplyDeliveries_Inventory_InventoryId",
                table: "SupplyDeliveries");

            migrationBuilder.DropTable(
                name: "Inventory");

            migrationBuilder.DropIndex(
                name: "IX_Ingredients_InventoryId",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "InventoryId",
                table: "Ingredients");

            migrationBuilder.RenameColumn(
                name: "InventoryId",
                table: "SupplyDeliveries",
                newName: "IngredientId");

            migrationBuilder.RenameIndex(
                name: "IX_SupplyDeliveries_InventoryId",
                table: "SupplyDeliveries",
                newName: "IX_SupplyDeliveries_IngredientId");

            migrationBuilder.AddForeignKey(
                name: "FK_SupplyDeliveries_Ingredients_IngredientId",
                table: "SupplyDeliveries",
                column: "IngredientId",
                principalTable: "Ingredients",
                principalColumn: "IngredientId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupplyDeliveries_Ingredients_IngredientId",
                table: "SupplyDeliveries");

            migrationBuilder.RenameColumn(
                name: "IngredientId",
                table: "SupplyDeliveries",
                newName: "InventoryId");

            migrationBuilder.RenameIndex(
                name: "IX_SupplyDeliveries_IngredientId",
                table: "SupplyDeliveries",
                newName: "IX_SupplyDeliveries_InventoryId");

            migrationBuilder.AddColumn<int>(
                name: "InventoryId",
                table: "Ingredients",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Inventory",
                columns: table => new
                {
                    InventoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CurrentQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InventoryName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventory", x => x.InventoryId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ingredients_InventoryId",
                table: "Ingredients",
                column: "InventoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ingredients_Inventory_InventoryId",
                table: "Ingredients",
                column: "InventoryId",
                principalTable: "Inventory",
                principalColumn: "InventoryId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SupplyDeliveries_Inventory_InventoryId",
                table: "SupplyDeliveries",
                column: "InventoryId",
                principalTable: "Inventory",
                principalColumn: "InventoryId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
