using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealtimeChatBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddTimestampToMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SentAt",
                table: "Messages",
                newName: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "Messages",
                newName: "SentAt");
        }
    }
}
