using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductService.Migrations
{
    /// <inheritdoc />
    public partial class FixProductImageForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
          

            migrationBuilder.CreateIndex(
                name: "IX_ProductImage_ProductId",
                schema: "SellShop.Product",
                table: "ProductImage",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImage_Products_ProductId",
                schema: "SellShop.Product",
                table: "ProductImage",
                column: "ProductId",
                principalSchema: "SellShop.Product",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductImage_Products_ProductId",
                schema: "SellShop.Product",
                table: "ProductImage");

            migrationBuilder.DropIndex(
                name: "IX_ProductImage_ProductId",
                schema: "SellShop.Product",
                table: "ProductImage");

            migrationBuilder.AddColumn<Guid>(
                name: "ProductsId",
                schema: "SellShop.Product",
                table: "ProductImage",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_ProductImage_ProductsId",
                schema: "SellShop.Product",
                table: "ProductImage",
                column: "ProductsId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImage_Products_ProductsId",
                schema: "SellShop.Product",
                table: "ProductImage",
                column: "ProductsId",
                principalSchema: "SellShop.Product",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
