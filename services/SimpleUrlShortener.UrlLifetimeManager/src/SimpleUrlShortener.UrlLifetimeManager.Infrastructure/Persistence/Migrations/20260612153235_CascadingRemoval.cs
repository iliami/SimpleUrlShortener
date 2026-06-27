using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleUrlShortener.UrlLifetimeManager.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CascadingRemoval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UrlMappingRedirection_UrlMapping_UrlMappingEntityCode",
                schema: "url-lifetime-manager",
                table: "UrlMappingRedirection");

            migrationBuilder.AddForeignKey(
                name: "FK_UrlMappingRedirection_UrlMapping_UrlMappingEntityCode",
                schema: "url-lifetime-manager",
                table: "UrlMappingRedirection",
                column: "UrlMappingEntityCode",
                principalSchema: "url-lifetime-manager",
                principalTable: "UrlMapping",
                principalColumn: "Code",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UrlMappingRedirection_UrlMapping_UrlMappingEntityCode",
                schema: "url-lifetime-manager",
                table: "UrlMappingRedirection");

            migrationBuilder.AddForeignKey(
                name: "FK_UrlMappingRedirection_UrlMapping_UrlMappingEntityCode",
                schema: "url-lifetime-manager",
                table: "UrlMappingRedirection",
                column: "UrlMappingEntityCode",
                principalSchema: "url-lifetime-manager",
                principalTable: "UrlMapping",
                principalColumn: "Code");
        }
    }
}
