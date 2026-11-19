using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeteriaApi.Migrations
{
    /// <inheritdoc />
    public partial class AddProductColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Products",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Products",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "Products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "Price",
                table: "Products",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "Size",
                table: "Products",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "Products");
        }
    }
}
