using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleUrlShortener.AnalyticsCollector.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class IntroduceUrlMappingDeletion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UrlMappingRedirection_UrlMapping_UrlMappingEntityCode",
                schema: "analytics-collector",
                table: "UrlMappingRedirection");

            migrationBuilder.DropIndex(
                name: "IX_UrlMappingRedirection_UrlMappingEntityCode",
                schema: "analytics-collector",
                table: "UrlMappingRedirection");

            migrationBuilder.DropColumn(
                name: "UrlMappingEntityCode",
                schema: "analytics-collector",
                table: "UrlMappingRedirection");

            migrationBuilder.DropColumn(
                name: "IsRevoked",
                schema: "analytics-collector",
                table: "UrlMapping");

            migrationBuilder.AddColumn<Guid>(
                name: "UrlMappingDeletionId",
                schema: "analytics-collector",
                table: "UrlMappingRedirection",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UrlMappingId",
                schema: "analytics-collector",
                table: "UrlMappingRedirection",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UrlMappingDeletion",
                schema: "analytics-collector",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Original = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrlMappingDeletion", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UrlMappingRedirection_UrlMappingDeletionId",
                schema: "analytics-collector",
                table: "UrlMappingRedirection",
                column: "UrlMappingDeletionId");

            migrationBuilder.CreateIndex(
                name: "IX_UrlMappingRedirection_UrlMappingId",
                schema: "analytics-collector",
                table: "UrlMappingRedirection",
                column: "UrlMappingId");

            migrationBuilder.AddForeignKey(
                name: "FK_UrlMappingRedirection_UrlMappingDeletion_UrlMappingDeletion~",
                schema: "analytics-collector",
                table: "UrlMappingRedirection",
                column: "UrlMappingDeletionId",
                principalSchema: "analytics-collector",
                principalTable: "UrlMappingDeletion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UrlMappingRedirection_UrlMapping_UrlMappingId",
                schema: "analytics-collector",
                table: "UrlMappingRedirection",
                column: "UrlMappingId",
                principalSchema: "analytics-collector",
                principalTable: "UrlMapping",
                principalColumn: "Code",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UrlMappingRedirection_UrlMappingDeletion_UrlMappingDeletion~",
                schema: "analytics-collector",
                table: "UrlMappingRedirection");

            migrationBuilder.DropForeignKey(
                name: "FK_UrlMappingRedirection_UrlMapping_UrlMappingId",
                schema: "analytics-collector",
                table: "UrlMappingRedirection");

            migrationBuilder.DropTable(
                name: "UrlMappingDeletion",
                schema: "analytics-collector");

            migrationBuilder.DropIndex(
                name: "IX_UrlMappingRedirection_UrlMappingDeletionId",
                schema: "analytics-collector",
                table: "UrlMappingRedirection");

            migrationBuilder.DropIndex(
                name: "IX_UrlMappingRedirection_UrlMappingId",
                schema: "analytics-collector",
                table: "UrlMappingRedirection");

            migrationBuilder.DropColumn(
                name: "UrlMappingDeletionId",
                schema: "analytics-collector",
                table: "UrlMappingRedirection");

            migrationBuilder.DropColumn(
                name: "UrlMappingId",
                schema: "analytics-collector",
                table: "UrlMappingRedirection");

            migrationBuilder.AddColumn<string>(
                name: "UrlMappingEntityCode",
                schema: "analytics-collector",
                table: "UrlMappingRedirection",
                type: "character varying(32)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRevoked",
                schema: "analytics-collector",
                table: "UrlMapping",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_UrlMappingRedirection_UrlMappingEntityCode",
                schema: "analytics-collector",
                table: "UrlMappingRedirection",
                column: "UrlMappingEntityCode");

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
    }
}
