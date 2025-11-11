using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qffect.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ADM");

            migrationBuilder.CreateTable(
                name: "Tasks",
                schema: "ADM",
                columns: table => new
                {
                    TaskID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TaskDescription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    RefModuleCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    RefEntity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RefId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    OwnerEmpId = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    PriorityId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ADM_Tasks", x => x.TaskID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ADM_Tasks_Ref",
                schema: "ADM",
                table: "Tasks",
                columns: new[] { "RefModuleCode", "RefEntity", "RefId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tasks",
                schema: "ADM");
        }
    }
}
