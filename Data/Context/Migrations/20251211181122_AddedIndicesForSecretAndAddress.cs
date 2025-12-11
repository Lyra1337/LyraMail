using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lyralabs.TempMailServer.Data.Context.Migrations
{
    /// <inheritdoc />
    public partial class AddedIndicesForSecretAndAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Mails_Secret",
                table: "Mails",
                column: "Secret");

            migrationBuilder.CreateIndex(
                name: "IX_Mailboxes_Address",
                table: "Mailboxes",
                column: "Address");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Mails_Secret",
                table: "Mails");

            migrationBuilder.DropIndex(
                name: "IX_Mailboxes_Address",
                table: "Mailboxes");
        }
    }
}
