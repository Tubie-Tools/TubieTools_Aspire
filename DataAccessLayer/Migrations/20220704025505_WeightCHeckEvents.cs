using Microsoft.EntityFrameworkCore.Migrations;

namespace DataAccessLayer.Migrations
{
    public partial class WeightCHeckEvents : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Kind"); 

            migrationBuilder.AddColumn<int>(
                name: "WeightOunces",
                table: "Event",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WeightPounds",
                table: "Event",
                nullable: true);
             
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProfileAspnetUsers");

            migrationBuilder.DropColumn(
                name: "WeightOunces",
                table: "Event");

            migrationBuilder.DropColumn(
                name: "WeightPounds",
                table: "Event");

            migrationBuilder.AddColumn<int>(
                name: "GenderId",
                table: "Profile",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Profile",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EventId",
                table: "EventThings",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ThingId",
                table: "EventThings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Thing",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventThingsId = table.Column<int>(type: "int", nullable: true),
                    KindId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Thing", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Thing_EventThings_EventThingsId",
                        column: x => x.EventThingsId,
                        principalTable: "EventThings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Things",
                columns: table => new
                {
                    EventThingsId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK_Things_EventThings_EventThingsId",
                        column: x => x.EventThingsId,
                        principalTable: "EventThings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Kind",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThingId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kind", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Kind_Thing_ThingId",
                        column: x => x.ThingId,
                        principalTable: "Thing",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Kinds",
                columns: table => new
                {
                    ThingId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK_Kinds_Thing_ThingId",
                        column: x => x.ThingId,
                        principalTable: "Thing",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Kind_ThingId",
                table: "Kind",
                column: "ThingId");

            migrationBuilder.CreateIndex(
                name: "IX_Thing_EventThingsId",
                table: "Thing",
                column: "EventThingsId");
        }
    }
}
