using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizzApp.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupQuizRequiresValidation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RequiresValidation",
                table: "GroupQuizzes",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequiresValidation",
                table: "GroupQuizzes");
        }
    }
}
