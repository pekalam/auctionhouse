using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Adapter.SqlServer.EventOutboxStorage.Migrations
{
    public partial class _03_Rm_ReadModelNotifications : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReadModelNotifications",
                table: "OutboxItems");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReadModelNotifications",
                table: "OutboxItems",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
