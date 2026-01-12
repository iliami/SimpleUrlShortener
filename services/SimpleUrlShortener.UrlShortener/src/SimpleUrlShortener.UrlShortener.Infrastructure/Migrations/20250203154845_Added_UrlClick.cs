using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleUrlShortener.UrlShortener.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Added_UrlClick : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UrlClicks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClickedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UrlId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrlClicks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UrlClicks_Urls_UrlId",
                        column: x => x.UrlId,
                        principalTable: "Urls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UrlClicks_UrlId",
                table: "UrlClicks",
                column: "UrlId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UrlClicks");
        }
    }
}
