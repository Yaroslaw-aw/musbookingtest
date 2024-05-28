using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MUSbooking.Migrations
{
    /// <inheritdoc />
    public partial class AddQuantityToOrderEquipments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "OrderEquipments",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "OrderEquipments");
        }
    }
}
