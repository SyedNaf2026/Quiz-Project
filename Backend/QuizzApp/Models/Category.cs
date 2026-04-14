namespace QuizzApp.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Who created this category (nullable — existing categories have no owner)
        public int? CreatedBy { get; set; }
        public User? Creator { get; set; }

        public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
    }
}
