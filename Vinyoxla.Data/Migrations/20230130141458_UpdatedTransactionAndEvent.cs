using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Vinyoxla.Data.Migrations
{
    public partial class UpdatedTransactionAndEvent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "Transactions");

            migrationBuilder.AddColumn<string>(
                name: "OrderId",
                table: "Transactions",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SessionId",
                table: "Transactions",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsFromAdminArea",
                table: "Events",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AspNetUsers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "IsFromAdminArea",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
