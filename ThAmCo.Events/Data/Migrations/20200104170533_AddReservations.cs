using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ThAmCo.Events.Data.Migrations
{
    public partial class AddReservations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "reservations",
                schema: "thamco.events",
                table: "Events",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Reservation",
                schema: "thamco.events",
                columns: table => new
                {
                    Reference = table.Column<string>(maxLength: 13, nullable: false),
                    EventDate = table.Column<DateTime>(nullable: false),
                    VenueCode = table.Column<string>(nullable: false),
                    WhenMade = table.Column<DateTime>(nullable: false),
                    StaffId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservation", x => x.Reference);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reservation",
                schema: "thamco.events");

            migrationBuilder.DropColumn(
                name: "reservations",
                schema: "thamco.events",
                table: "Events");
        }
    }
}
