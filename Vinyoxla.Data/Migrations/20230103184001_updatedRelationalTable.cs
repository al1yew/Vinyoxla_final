﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Vinyoxla.Data.Migrations
{
    public partial class updatedRelationalTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AppUserToVincodes",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AppUserToVincodes");
        }
    }
}
