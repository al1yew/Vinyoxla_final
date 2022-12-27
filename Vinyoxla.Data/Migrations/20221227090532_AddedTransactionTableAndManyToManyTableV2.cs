using Microsoft.EntityFrameworkCore.Migrations;

namespace Vinyoxla.Data.Migrations
{
    public partial class AddedTransactionTableAndManyToManyTableV2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "VinCodes",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "FileName",
                table: "VinCodes",
                type: "int",
                nullable: false,
                oldClrType: typeof(string));
        }
    }
}
