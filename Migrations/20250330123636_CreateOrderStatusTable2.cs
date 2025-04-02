using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BanHang.Migrations
{
    /// <inheritdoc />
    public partial class CreateOrderStatusTable2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_OrderStatus_OrderStatusId",
                table: "Orders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderStatus",
                table: "OrderStatus");

            migrationBuilder.RenameTable(
                name: "OrderStatus",
                newName: "OrderStatuses");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderStatuses",
                table: "OrderStatuses",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_OrderStatuses_OrderStatusId",
                table: "Orders",
                column: "OrderStatusId",
                principalTable: "OrderStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_OrderStatuses_OrderStatusId",
                table: "Orders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderStatuses",
                table: "OrderStatuses");

            migrationBuilder.RenameTable(
                name: "OrderStatuses",
                newName: "OrderStatus");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderStatus",
                table: "OrderStatus",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_OrderStatus_OrderStatusId",
                table: "Orders",
                column: "OrderStatusId",
                principalTable: "OrderStatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
