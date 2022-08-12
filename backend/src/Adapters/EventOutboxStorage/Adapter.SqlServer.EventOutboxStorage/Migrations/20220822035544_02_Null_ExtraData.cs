using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Adapter.SqlServer.EventOutboxStorage.Migrations
{
    public partial class _02_Null_ExtraData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CommandContext_ExtraData",
                table: "OutboxItems",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CommandContext_ExtraData",
                table: "OutboxItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
