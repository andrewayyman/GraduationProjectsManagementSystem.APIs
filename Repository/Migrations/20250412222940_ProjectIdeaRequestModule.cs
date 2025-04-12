using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class ProjectIdeaRequestModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectIdeas_Supervisors_SupervisorId",
                table: "ProjectIdeas");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ProjectIdeas");

            migrationBuilder.AlterColumn<int>(
                name: "SupervisorId",
                table: "ProjectIdeas",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "ProjectIdeaRequest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectIdeaId = table.Column<int>(type: "int", nullable: false),
                    SupervisorId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectIdeaRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectIdeaRequest_ProjectIdeas_ProjectIdeaId",
                        column: x => x.ProjectIdeaId,
                        principalTable: "ProjectIdeas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectIdeaRequest_Supervisors_SupervisorId",
                        column: x => x.SupervisorId,
                        principalTable: "Supervisors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectIdeaRequest_ProjectIdeaId",
                table: "ProjectIdeaRequest",
                column: "ProjectIdeaId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectIdeaRequest_SupervisorId",
                table: "ProjectIdeaRequest",
                column: "SupervisorId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectIdeas_Supervisors_SupervisorId",
                table: "ProjectIdeas",
                column: "SupervisorId",
                principalTable: "Supervisors",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectIdeas_Supervisors_SupervisorId",
                table: "ProjectIdeas");

            migrationBuilder.DropTable(
                name: "ProjectIdeaRequest");

            migrationBuilder.AlterColumn<int>(
                name: "SupervisorId",
                table: "ProjectIdeas",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "ProjectIdeas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectIdeas_Supervisors_SupervisorId",
                table: "ProjectIdeas",
                column: "SupervisorId",
                principalTable: "Supervisors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
