using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chat_server.Migrations
{
    /// <inheritdoc />
    public partial class dropConTbl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Users",
                type: "nvarchar(max)",
                nullable:true,
                oldClrType: typeof(bool),
                oldType: "bit");

            // Drop index on ConversationId in Messages table if it exists
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_Messages_ConversationId ON Messages");

            // Drop the foreign key constraint on ConversationId in Messages table
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Conversations_ConversationId",
                table: "Messages");

            // Drop the ConversationId column from the Messages table
            migrationBuilder.DropColumn(
                name: "ConversationId",
                table: "Messages");

            // Drop the MessageType column from the Messages table
            migrationBuilder.DropColumn(
                name: "MessageType",
                table: "Messages");

            // Drop the Conversations table
            migrationBuilder.DropTable(
                name: "Conversations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "Status",
                table: "Users",
                type: "bit",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            // Recreate the Conversations table with properties matching the Conversation model
            migrationBuilder.CreateTable(
                name: "Conversations",
                columns: table => new
                {
                    ConversationId = table.Column<string>(nullable: false),
                    ConversationName = table.Column<string>(nullable: true),
                    CreateAt = table.Column<DateTime>(nullable: false),
                    CreateBy = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversations", x => x.ConversationId);
                });

            // Add the ConversationId column back to the Messages table
            migrationBuilder.AddColumn<string>(
                name: "ConversationId",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: true);

            // Add the MessageType column back to the Messages table
            migrationBuilder.AddColumn<string>(
                name: "MessageType",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: true);

            // Re-create the foreign key constraint for ConversationId in Messages table
            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Conversations_ConversationId",
                table: "Messages",
                column: "ConversationId",
                principalTable: "Conversations",
                principalColumn: "ConversationId",
                onDelete: ReferentialAction.Cascade);

            // Re-create the index on ConversationId in Messages table
            migrationBuilder.CreateIndex(
                name: "IX_Messages_ConversationId",
                table: "Messages",
                column: "ConversationId");
        }
    }
}
