using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chat_server.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Friendships",
                newName: "FriendShipId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FriendShipId",
                table: "Friendships",
                newName: "Id");
        }
    }
}
