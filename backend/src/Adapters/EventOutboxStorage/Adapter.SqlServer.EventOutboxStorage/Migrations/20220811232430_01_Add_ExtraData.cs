using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Adapter.SqlServer.EventOutboxStorage.Migrations
{
    public partial class _01_Add_ExtraData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommandContext_CommandId_Id",
                table: "OutboxItems");

            migrationBuilder.RenameColumn(
                name: "CommandContext_CorrelationId_Value",
                table: "OutboxItems",
                newName: "CommandContext_ExtraData");

            migrationBuilder.AddColumn<string>(
                name: "CommandContext_CommandId",
                table: "OutboxItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CommandContext_CorrelationId",
                table: "OutboxItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommandContext_CommandId",
                table: "OutboxItems");

            migrationBuilder.DropColumn(
                name: "CommandContext_CorrelationId",
                table: "OutboxItems");

            migrationBuilder.RenameColumn(
                name: "CommandContext_ExtraData",
                table: "OutboxItems",
                newName: "CommandContext_CorrelationId_Value");

            migrationBuilder.AddColumn<string>(
                name: "CommandContext_CommandId_Id",
                table: "OutboxItems",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
