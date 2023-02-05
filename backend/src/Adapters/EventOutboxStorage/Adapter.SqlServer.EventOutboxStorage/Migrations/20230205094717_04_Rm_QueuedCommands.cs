using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Adapter.SqlServer.EventOutboxStorage.Migrations
{
    public partial class _04_Rm_QueuedCommands : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommandContext_HttpQueued",
                table: "OutboxItems");

            migrationBuilder.DropColumn(
                name: "CommandContext_WSQueued",
                table: "OutboxItems");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CommandContext_HttpQueued",
                table: "OutboxItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CommandContext_WSQueued",
                table: "OutboxItems",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
