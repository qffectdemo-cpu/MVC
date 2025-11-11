using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qffect.Infrastructure.Migrations.Settings
{
    /// <inheritdoc />
    public partial class InitialSettingsDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ADM");

            migrationBuilder.CreateTable(
                name: "Tenants",
                schema: "ADM",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Edition = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.TenantId);
                });

            migrationBuilder.CreateTable(
                name: "TenantModuleSettings",
                schema: "ADM",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModuleCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SettingsJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantModuleSettings", x => new { x.TenantId, x.ModuleCode });
                    table.ForeignKey(
                        name: "FK_TenantModuleSettings_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "ADM",
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Slug",
                schema: "ADM",
                table: "Tenants",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TenantModuleSettings",
                schema: "ADM");

            migrationBuilder.DropTable(
                name: "Tenants",
                schema: "ADM");
        }
    }
}
