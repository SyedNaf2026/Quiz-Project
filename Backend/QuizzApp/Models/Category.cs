namespace QuizzApp.Models
{
    // Represents a quiz category (e.g. "Science", "History")
    public class Category
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        // Navigation: quizzes under this category
        public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
    }
}
