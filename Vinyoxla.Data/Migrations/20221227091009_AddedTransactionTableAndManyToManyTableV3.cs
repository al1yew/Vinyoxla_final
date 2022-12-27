using Microsoft.EntityFrameworkCore.Migrations;

namespace Vinyoxla.Data.Migrations
{
    public partial class AddedTransactionTableAndManyToManyTableV3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppUserToVincodes_AspNetUsers_AppUserId1",
                table: "AppUserToVincodes");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_AspNetUsers_AppUserId1",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_AppUserId1",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_AppUserToVincodes_AppUserId1",
                table: "AppUserToVincodes");

            migrationBuilder.DropColumn(
                name: "AppUserId1",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "AppUserId1",
                table: "AppUserToVincodes");

            migrationBuilder.AlterColumn<string>(
                name: "AppUserId",
                table: "Transactions",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "AppUserId",
                table: "AppUserToVincodes",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_AppUserId",
                table: "Transactions",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppUserToVincodes_AppUserId",
                table: "AppUserToVincodes",
                column: "AppUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppUserToVincodes_AspNetUsers_AppUserId",
                table: "AppUserToVincodes",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_AspNetUsers_AppUserId",
                table: "Transactions",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppUserToVincodes_AspNetUsers_AppUserId",
                table: "AppUserToVincodes");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_AspNetUsers_AppUserId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_AppUserId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_AppUserToVincodes_AppUserId",
                table: "AppUserToVincodes");

            migrationBuilder.AlterColumn<int>(
                name: "AppUserId",
                table: "Transactions",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AppUserId1",
                table: "Transactions",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AppUserId",
                table: "AppUserToVincodes",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AppUserId1",
                table: "AppUserToVincodes",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_AppUserId1",
                table: "Transactions",
                column: "AppUserId1");

            migrationBuilder.CreateIndex(
                name: "IX_AppUserToVincodes_AppUserId1",
                table: "AppUserToVincodes",
                column: "AppUserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_AppUserToVincodes_AspNetUsers_AppUserId1",
                table: "AppUserToVincodes",
                column: "AppUserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_AspNetUsers_AppUserId1",
                table: "Transactions",
                column: "AppUserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
