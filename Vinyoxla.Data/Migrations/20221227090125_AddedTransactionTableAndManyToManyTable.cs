using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Vinyoxla.Data.Migrations
{
    public partial class AddedTransactionTableAndManyToManyTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VinCodes_AspNetUsers_AppUserId",
                table: "VinCodes");

            migrationBuilder.DropIndex(
                name: "IX_VinCodes_AppUserId",
                table: "VinCodes");

            migrationBuilder.DropColumn(
                name: "AppUserId",
                table: "VinCodes");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "VinCodes",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFromAPI",
                table: "VinCodes",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "AppUserToVincodes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppUserId1 = table.Column<string>(nullable: true),
                    AppUserId = table.Column<int>(nullable: false),
                    VinCodeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUserToVincodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppUserToVincodes_AspNetUsers_AppUserId1",
                        column: x => x.AppUserId1,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppUserToVincodes_VinCodes_VinCodeId",
                        column: x => x.VinCodeId,
                        principalTable: "VinCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    Code = table.Column<string>(nullable: false),
                    IsConfirmed = table.Column<bool>(nullable: false),
                    AppUserId1 = table.Column<string>(nullable: true),
                    AppUserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_AspNetUsers_AppUserId1",
                        column: x => x.AppUserId1,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppUserToVincodes_AppUserId1",
                table: "AppUserToVincodes",
                column: "AppUserId1");

            migrationBuilder.CreateIndex(
                name: "IX_AppUserToVincodes_VinCodeId",
                table: "AppUserToVincodes",
                column: "VinCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_AppUserId1",
                table: "Transactions",
                column: "AppUserId1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppUserToVincodes");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropColumn(
                name: "IsFromAPI",
                table: "VinCodes");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "VinCodes",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime));

            migrationBuilder.AddColumn<string>(
                name: "AppUserId",
                table: "VinCodes",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VinCodes_AppUserId",
                table: "VinCodes",
                column: "AppUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_VinCodes_AspNetUsers_AppUserId",
                table: "VinCodes",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
