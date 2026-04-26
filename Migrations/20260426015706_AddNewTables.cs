using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OttooGennie.API.Migrations
{
    /// <inheritdoc />
    public partial class AddNewTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlockedSlots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<string>(type: "text", nullable: false),
                    SlotTime = table.Column<string>(type: "text", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockedSlots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reviews_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reviews_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 4, 26, 1, 57, 3, 307, DateTimeKind.Utc).AddTicks(7265));

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 4, 26, 1, 57, 3, 307, DateTimeKind.Utc).AddTicks(7293));

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000003"),
                column: "CreatedAt",
                value: new DateTime(2026, 4, 26, 1, 57, 3, 307, DateTimeKind.Utc).AddTicks(7299));

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000004"),
                column: "CreatedAt",
                value: new DateTime(2026, 4, 26, 1, 57, 3, 307, DateTimeKind.Utc).AddTicks(7304));

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000005"),
                column: "CreatedAt",
                value: new DateTime(2026, 4, 26, 1, 57, 3, 307, DateTimeKind.Utc).AddTicks(7333));

            migrationBuilder.CreateIndex(
                name: "IX_BlockedSlots_Date_SlotTime",
                table: "BlockedSlots",
                columns: new[] { "Date", "SlotTime" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_BookingId",
                table: "Reviews",
                column: "BookingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UserId",
                table: "Reviews",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlockedSlots");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 4, 20, 17, 2, 7, 574, DateTimeKind.Utc).AddTicks(7515));

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 4, 20, 17, 2, 7, 574, DateTimeKind.Utc).AddTicks(7546));

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000003"),
                column: "CreatedAt",
                value: new DateTime(2026, 4, 20, 17, 2, 7, 574, DateTimeKind.Utc).AddTicks(7553));

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000004"),
                column: "CreatedAt",
                value: new DateTime(2026, 4, 20, 17, 2, 7, 574, DateTimeKind.Utc).AddTicks(7559));

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000005"),
                column: "CreatedAt",
                value: new DateTime(2026, 4, 20, 17, 2, 7, 574, DateTimeKind.Utc).AddTicks(7563));
        }
    }
}
