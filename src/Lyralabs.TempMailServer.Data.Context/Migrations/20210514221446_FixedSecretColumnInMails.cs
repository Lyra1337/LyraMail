using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Lyralabs.TempMailServer.Data.Context.Migrations
{
    public partial class FixedSecretColumnInMails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Secret",
                table: "Mails",
                type: "TEXT",
                nullable: false,
                defaultValue: Guid.Parse("00000000-0000-0000-0000-000000000000"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Secret",
                table: "Mails");
        }
    }
}
