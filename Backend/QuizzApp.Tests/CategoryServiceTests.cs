using Moq;
using QuizzApp.DTOs;
using QuizzApp.Interfaces;
using QuizzApp.Models;
using QuizzApp.Services;
using Xunit;

namespace QuizzApp.Tests
{
    public class CategoryServiceTests
    {
        private readonly Mock<IGenericRepository<Category>> _categoryRepoMock;
        private readonly CategoryService _categoryService;

        public CategoryServiceTests()
        {
            _categoryRepoMock = new Mock<IGenericRepository<Category>>();
            _categoryService = new CategoryService(_categoryRepoMock.Object);
        }

        // ── CreateCategory Tests ────────────────────────────────

        [Fact]
        public async Task CreateCategory_ReturnsCorrectDTO()
        {
            // Arrange: prepare a DTO with name and description
            var dto = new CreateCategoryDTO { Name = "Science", Description = "Science questions" };
            _categoryRepoMock.Setup(r => r.AddAsync(It.IsAny<Category>())).Returns(Task.CompletedTask);

            // Act: call the service
            var result = await _categoryService.CreateCategoryAsync(dto);

            // Assert: returned DTO should match what we sent
            Assert.Equal("Science", result.Name);
            Assert.Equal("Science questions", result.Description);
        }

        [Fact]
        public async Task CreateCategory_CallsAddAsync_Once()
        {
            // Arrange
            var dto = new CreateCategoryDTO { Name = "History", Description = "History questions" };
            _categoryRepoMock.Setup(r => r.AddAsync(It.IsAny<Category>())).Returns(Task.CompletedTask);

            // Act
            await _categoryService.CreateCategoryAsync(dto);

            // Assert: AddAsync must be called exactly once
            _categoryRepoMock.Verify(r => r.AddAsync(It.IsAny<Category>()), Times.Once);
        }

        // ── GetAllCategories Tests ──────────────────────────────

        [Fact]
        public async Task GetAllCategories_ReturnsAllMappedCorrectly()
        {
            // Arrange: fake DB has 2 categories
            var fakeCategories = new List<Category>
            {
                new Category { Id = 1, Name = "Science",  Description = "Science questions" },
                new Category { Id = 2, Name = "History",  Description = "History questions" }
            };
            _categoryRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(fakeCategories);

            // Act
            var result = (await _categoryService.GetAllCategoriesAsync()).ToList();

            // Assert: should return 2 items with correct data
            Assert.Equal(2, result.Count);
            Assert.Equal("Science", result[0].Name);
            Assert.Equal("History", result[1].Name);
        }

        [Fact]
        public async Task GetAllCategories_WhenEmpty_ReturnsEmptyList()
        {
            // Arrange: fake DB has no categories
            _categoryRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Category>());

            // Act
            var result = await _categoryService.GetAllCategoriesAsync();

            // Assert: should return empty, not null
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllCategories_MapsIdCorrectly()
        {
            // Arrange
            var fakeCategories = new List<Category>
            {
                new Category { Id = 5, Name = "Maths", Description = "Maths questions" }
            };
            _categoryRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(fakeCategories);

            // Act
            var result = (await _categoryService.GetAllCategoriesAsync()).ToList();

            // Assert: Id must be mapped correctly
            Assert.Equal(5, result[0].Id);
        }
    }
}
