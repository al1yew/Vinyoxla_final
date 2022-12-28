using Microsoft.EntityFrameworkCore.Migrations;

namespace Vinyoxla.Data.Migrations
{
    public partial class UpdatedVIncodeTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAutoCheck",
                table: "VinCodes");

            migrationBuilder.DropColumn(
                name: "IsCarfax",
                table: "VinCodes");

            migrationBuilder.DropColumn(
                name: "IsFromAPI",
                table: "VinCodes");

            migrationBuilder.AddColumn<int>(
                name: "PurchasedTimes",
                table: "VinCodes",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PurchasedTimes",
                table: "VinCodes");

            migrationBuilder.AddColumn<bool>(
                name: "IsAutoCheck",
                table: "VinCodes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCarfax",
                table: "VinCodes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFromAPI",
                table: "VinCodes",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
