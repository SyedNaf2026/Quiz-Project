using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizzApp.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionTypeAndDifficulty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add QuestionType column to Questions table
            migrationBuilder.AddColumn<string>(
                name: "QuestionType",
                table: "Questions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "MultipleChoice");

            // Add Difficulty column to Quizzes table
            migrationBuilder.AddColumn<string>(
                name: "Difficulty",
                table: "Quizzes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuestionType",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "Difficulty",
                table: "Quizzes");
        }
    }
}
