using Microsoft.EntityFrameworkCore.Migrations;

namespace Vinyoxla.Data.Migrations
{
    public partial class UpdatedRelationTableAndTransactionTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsConfirmed",
                table: "Transactions");

            migrationBuilder.AddColumn<int>(
                name: "Amount",
                table: "Transactions",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsFromBalance",
                table: "Transactions",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsTopUp",
                table: "Transactions",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PaymentIsSuccessful",
                table: "Transactions",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DidRefundToBalance",
                table: "AppUserToVincodes",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsApiError",
                table: "AppUserToVincodes",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFromApi",
                table: "AppUserToVincodes",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRenewedDueToAbsence",
                table: "AppUserToVincodes",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRenewedDueToExpire",
                table: "AppUserToVincodes",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Amount",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "IsFromBalance",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "IsTopUp",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "PaymentIsSuccessful",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "DidRefundToBalance",
                table: "AppUserToVincodes");

            migrationBuilder.DropColumn(
                name: "IsApiError",
                table: "AppUserToVincodes");

            migrationBuilder.DropColumn(
                name: "IsFromApi",
                table: "AppUserToVincodes");

            migrationBuilder.DropColumn(
                name: "IsRenewedDueToAbsence",
                table: "AppUserToVincodes");

            migrationBuilder.DropColumn(
                name: "IsRenewedDueToExpire",
                table: "AppUserToVincodes");

            migrationBuilder.AddColumn<bool>(
                name: "IsConfirmed",
                table: "Transactions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
