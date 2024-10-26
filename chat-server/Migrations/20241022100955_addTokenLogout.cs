using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chat_server.Migrations
{
    /// <inheritdoc />
    public partial class addTokenLogout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Dislike",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "Fullname",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "Like",
                table: "Profiles");

            migrationBuilder.CreateTable(
                name: "TokenLogouts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    expireDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenLogouts", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TokenLogouts");

            migrationBuilder.AddColumn<string>(
                name: "Dislike",
                table: "Profiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Fullname",
                table: "Profiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Like",
                table: "Profiles",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
