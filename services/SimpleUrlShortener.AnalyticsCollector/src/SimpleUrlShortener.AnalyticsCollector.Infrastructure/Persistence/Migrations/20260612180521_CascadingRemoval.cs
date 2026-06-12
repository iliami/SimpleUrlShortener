using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleUrlShortener.AnalyticsCollector.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CascadingRemoval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UrlMappingRedirection_UrlMapping_UrlMappingEntityCode",
                schema: "analytics-collector",
                table: "UrlMappingRedirection");

            migrationBuilder.AddForeignKey(
                name: "FK_UrlMappingRedirection_UrlMapping_UrlMappingEntityCode",
                schema: "analytics-collector",
                table: "UrlMappingRedirection",
                column: "UrlMappingEntityCode",
                principalSchema: "analytics-collector",
                principalTable: "UrlMapping",
                principalColumn: "Code",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UrlMappingRedirection_UrlMapping_UrlMappingEntityCode",
                schema: "analytics-collector",
                table: "UrlMappingRedirection");

            migrationBuilder.AddForeignKey(
                name: "FK_UrlMappingRedirection_UrlMapping_UrlMappingEntityCode",
                schema: "analytics-collector",
                table: "UrlMappingRedirection",
                column: "UrlMappingEntityCode",
                principalSchema: "analytics-collector",
                principalTable: "UrlMapping",
                principalColumn: "Code");
        }
    }
}
