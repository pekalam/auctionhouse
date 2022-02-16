using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Adapter.SqlServer.EventOutboxStorage.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OutboxItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Event = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CommandContext_CommandId_Id = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CommandContext_CorrelationId_Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CommandContext_User = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CommandContext_HttpQueued = table.Column<bool>(type: "bit", nullable: false),
                    CommandContext_WSQueued = table.Column<bool>(type: "bit", nullable: false),
                    CommandContext_Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReadModelNotifications = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<long>(type: "bigint", nullable: false),
                    Processed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxItems", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OutboxItems");
        }
    }
}
