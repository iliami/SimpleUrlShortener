using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleUrlShortener.AnalyticsCollector.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LazyEvaluationOfRedirectionCoordinates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Longitude",
                schema: "analytics-collector",
                table: "UrlMappingRedirection",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<double>(
                name: "Latitude",
                schema: "analytics-collector",
                table: "UrlMappingRedirection",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AddColumn<string>(
                name: "IpKind",
                schema: "analytics-collector",
                table: "UrlMappingRedirection",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IpKind",
                schema: "analytics-collector",
                table: "UrlMappingRedirection");

            migrationBuilder.AlterColumn<double>(
                name: "Longitude",
                schema: "analytics-collector",
                table: "UrlMappingRedirection",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Latitude",
                schema: "analytics-collector",
                table: "UrlMappingRedirection",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);
        }
    }
}
