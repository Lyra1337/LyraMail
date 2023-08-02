using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lyralabs.TempMailServer.Data.Context.Migrations
{
    /// <inheritdoc />
    public partial class AddedIsReadFlagToMails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRead",
                table: "Mails",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRead",
                table: "Mails");
        }
    }
}
