using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleUrlShortener.AnalyticsCollector.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "analytics-collector");

            migrationBuilder.CreateTable(
                name: "UrlMapping",
                schema: "analytics-collector",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Original = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrlMapping", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "UrlMappingRedirection",
                schema: "analytics-collector",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OccuredOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Ip = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    UrlMappingEntityCode = table.Column<string>(type: "character varying(32)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrlMappingRedirection", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UrlMappingRedirection_UrlMapping_UrlMappingEntityCode",
                        column: x => x.UrlMappingEntityCode,
                        principalSchema: "analytics-collector",
                        principalTable: "UrlMapping",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateIndex(
                name: "IX_UrlMappingRedirection_UrlMappingEntityCode",
                schema: "analytics-collector",
                table: "UrlMappingRedirection",
                column: "UrlMappingEntityCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UrlMappingRedirection",
                schema: "analytics-collector");

            migrationBuilder.DropTable(
                name: "UrlMapping",
                schema: "analytics-collector");
        }
    }
}
