using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OttooGennie.API.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Admins",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AdminKey = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Admins",
                columns: new[] { "Id", "AdminKey", "CreatedAt", "IsActive", "Name", "Phone" },
                values: new object[,]
                {
                    { new Guid("11111111-0000-0000-0000-000000000001"), "RRADM001", new DateTime(2026, 4, 20, 17, 2, 7, 574, DateTimeKind.Utc).AddTicks(7515), true, "Admin One", "9000000001" },
                    { new Guid("11111111-0000-0000-0000-000000000002"), "RRADM002", new DateTime(2026, 4, 20, 17, 2, 7, 574, DateTimeKind.Utc).AddTicks(7546), true, "Admin Two", "9000000002" },
                    { new Guid("11111111-0000-0000-0000-000000000003"), "RRADM003", new DateTime(2026, 4, 20, 17, 2, 7, 574, DateTimeKind.Utc).AddTicks(7553), true, "Admin Three", "9000000003" },
                    { new Guid("11111111-0000-0000-0000-000000000004"), "RRADM004", new DateTime(2026, 4, 20, 17, 2, 7, 574, DateTimeKind.Utc).AddTicks(7559), true, "Admin Four", "9000000004" },
                    { new Guid("11111111-0000-0000-0000-000000000005"), "RRADM005", new DateTime(2026, 4, 20, 17, 2, 7, 574, DateTimeKind.Utc).AddTicks(7563), true, "Admin Five", "9000000005" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Admins_AdminKey",
                table: "Admins",
                column: "AdminKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Admins");
        }
    }
}
