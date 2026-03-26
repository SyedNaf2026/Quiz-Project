using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizzApp.Migrations
{
    /// <inheritdoc />
    public partial class QuizzApp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Difficulty",
                table: "Quizzes");

            migrationBuilder.AlterColumn<string>(
                name: "QuestionType",
                table: "Questions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldDefaultValue: "MultipleChoice");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Difficulty",
                table: "Quizzes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "QuestionType",
                table: "Questions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "MultipleChoice",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
