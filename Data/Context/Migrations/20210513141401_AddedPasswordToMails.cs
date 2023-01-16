using Microsoft.EntityFrameworkCore.Migrations;

namespace Lyralabs.TempMailServer.Data.Context.Migrations
{
    public partial class AddedPasswordToMails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "Mails",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Password",
                table: "Mails");
        }
    }
}
