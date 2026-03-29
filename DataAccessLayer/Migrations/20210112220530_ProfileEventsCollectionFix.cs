using Microsoft.EntityFrameworkCore.Migrations;

namespace DataAccessLayer.Migrations
{
    public partial class ProfileEventsCollectionFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Profile_ProfileEvents_ProfileEventsId",
                table: "Profile");

            migrationBuilder.DropTable(
                name: "EventProfileEvents");

            migrationBuilder.DropTable(
                name: "ProfileEvents");

            migrationBuilder.DropIndex(
                name: "IX_Profile_ProfileEventsId",
                table: "Profile");

            migrationBuilder.DropColumn(
                name: "ProfileEventsId",
                table: "Profile");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProfileEventsId",
                table: "Profile",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProfileEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    ProfileId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventProfileEvents",
                columns: table => new
                {
                    EventsId = table.Column<int>(type: "int", nullable: false),
                    ProfileEventsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventProfileEvents", x => new { x.EventsId, x.ProfileEventsId });
                    table.ForeignKey(
                        name: "FK_EventProfileEvents_Event_EventsId",
                        column: x => x.EventsId,
                        principalTable: "Event",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventProfileEvents_ProfileEvents_ProfileEventsId",
                        column: x => x.ProfileEventsId,
                        principalTable: "ProfileEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Profile_ProfileEventsId",
                table: "Profile",
                column: "ProfileEventsId");

            migrationBuilder.CreateIndex(
                name: "IX_EventProfileEvents_ProfileEventsId",
                table: "EventProfileEvents",
                column: "ProfileEventsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Profile_ProfileEvents_ProfileEventsId",
                table: "Profile",
                column: "ProfileEventsId",
                principalTable: "ProfileEvents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
