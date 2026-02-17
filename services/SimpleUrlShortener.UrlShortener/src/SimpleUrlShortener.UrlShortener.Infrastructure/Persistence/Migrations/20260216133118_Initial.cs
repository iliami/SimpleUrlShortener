using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleUrlShortener.UrlShortener.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UrlMapping",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    Original = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrlMapping", x => x.Code);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UrlMapping_Code",
                table: "UrlMapping",
                column: "Code")
                .Annotation("Npgsql:IndexMethod", "hash");

            migrationBuilder.CreateIndex(
                name: "IX_UrlMapping_Original",
                table: "UrlMapping",
                column: "Original")
                .Annotation("Npgsql:IndexMethod", "hash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UrlMapping");
        }
    }
}
