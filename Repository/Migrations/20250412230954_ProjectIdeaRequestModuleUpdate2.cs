using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class ProjectIdeaRequestModuleUpdate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectIdeaRequest_ProjectIdeas_ProjectIdeaId",
                table: "ProjectIdeaRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectIdeaRequest_Supervisors_SupervisorId",
                table: "ProjectIdeaRequest");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProjectIdeaRequest",
                table: "ProjectIdeaRequest");

            migrationBuilder.RenameTable(
                name: "ProjectIdeaRequest",
                newName: "ProjectIdeasRequest");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectIdeaRequest_SupervisorId",
                table: "ProjectIdeasRequest",
                newName: "IX_ProjectIdeasRequest_SupervisorId");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectIdeaRequest_ProjectIdeaId",
                table: "ProjectIdeasRequest",
                newName: "IX_ProjectIdeasRequest_ProjectIdeaId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProjectIdeasRequest",
                table: "ProjectIdeasRequest",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectIdeasRequest_ProjectIdeas_ProjectIdeaId",
                table: "ProjectIdeasRequest",
                column: "ProjectIdeaId",
                principalTable: "ProjectIdeas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectIdeasRequest_Supervisors_SupervisorId",
                table: "ProjectIdeasRequest",
                column: "SupervisorId",
                principalTable: "Supervisors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectIdeasRequest_ProjectIdeas_ProjectIdeaId",
                table: "ProjectIdeasRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectIdeasRequest_Supervisors_SupervisorId",
                table: "ProjectIdeasRequest");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProjectIdeasRequest",
                table: "ProjectIdeasRequest");

            migrationBuilder.RenameTable(
                name: "ProjectIdeasRequest",
                newName: "ProjectIdeaRequest");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectIdeasRequest_SupervisorId",
                table: "ProjectIdeaRequest",
                newName: "IX_ProjectIdeaRequest_SupervisorId");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectIdeasRequest_ProjectIdeaId",
                table: "ProjectIdeaRequest",
                newName: "IX_ProjectIdeaRequest_ProjectIdeaId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProjectIdeaRequest",
                table: "ProjectIdeaRequest",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectIdeaRequest_ProjectIdeas_ProjectIdeaId",
                table: "ProjectIdeaRequest",
                column: "ProjectIdeaId",
                principalTable: "ProjectIdeas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectIdeaRequest_Supervisors_SupervisorId",
                table: "ProjectIdeaRequest",
                column: "SupervisorId",
                principalTable: "Supervisors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
