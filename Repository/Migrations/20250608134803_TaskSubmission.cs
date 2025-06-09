using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class TaskSubmission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskSubmission_Tasks_TaskId",
                table: "TaskSubmission");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TaskSubmission",
                table: "TaskSubmission");

            migrationBuilder.DropColumn(
                name: "FileUrl",
                table: "TaskSubmission");

            migrationBuilder.RenameTable(
                name: "TaskSubmission",
                newName: "TaskSubmissions");

            migrationBuilder.RenameIndex(
                name: "IX_TaskSubmission_TaskId",
                table: "TaskSubmissions",
                newName: "IX_TaskSubmissions_TaskId");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "TaskSubmissions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RepoLink",
                table: "TaskSubmissions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TaskSubmissions",
                table: "TaskSubmissions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskSubmissions_Tasks_TaskId",
                table: "TaskSubmissions",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskSubmissions_Tasks_TaskId",
                table: "TaskSubmissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TaskSubmissions",
                table: "TaskSubmissions");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "TaskSubmissions");

            migrationBuilder.DropColumn(
                name: "RepoLink",
                table: "TaskSubmissions");

            migrationBuilder.RenameTable(
                name: "TaskSubmissions",
                newName: "TaskSubmission");

            migrationBuilder.RenameIndex(
                name: "IX_TaskSubmissions_TaskId",
                table: "TaskSubmission",
                newName: "IX_TaskSubmission_TaskId");

            migrationBuilder.AddColumn<string>(
                name: "FileUrl",
                table: "TaskSubmission",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TaskSubmission",
                table: "TaskSubmission",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskSubmission_Tasks_TaskId",
                table: "TaskSubmission",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
