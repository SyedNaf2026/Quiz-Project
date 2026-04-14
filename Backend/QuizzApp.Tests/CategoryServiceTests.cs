using Microsoft.EntityFrameworkCore;
using Moq;
using QuizzApp.Context;
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

        public CategoryServiceTests()
        {
            _categoryRepoMock = new Mock<IGenericRepository<Category>>();
        }

        private AppDbContext CreateDb(string name)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(name)
                .Options;
            return new AppDbContext(options);
        }

        private CategoryService CreateService(AppDbContext db) =>
            new CategoryService(_categoryRepoMock.Object, db);

        // ── CreateCategory Tests ────────────────────────────────

        [Fact]
        public async Task CreateCategory_ReturnsCorrectDTO()
        {
            using var db = CreateDb("Cat_Create_Valid");
            _categoryRepoMock.Setup(r => r.AddAsync(It.IsAny<Category>())).Returns(Task.CompletedTask);
            var service = CreateService(db);

            var result = await service.CreateCategoryAsync(
                new CreateCategoryDTO { Name = "Science", Description = "Science questions" }, createdBy: 1);

            Assert.Equal("Science", result.Name);
            Assert.Equal("Science questions", result.Description);
        }

        [Fact]
        public async Task CreateCategory_CallsAddAsync_Once()
        {
            using var db = CreateDb("Cat_Create_Once");
            _categoryRepoMock.Setup(r => r.AddAsync(It.IsAny<Category>())).Returns(Task.CompletedTask);
            var service = CreateService(db);

            await service.CreateCategoryAsync(
                new CreateCategoryDTO { Name = "History", Description = "History questions" }, createdBy: 1);

            _categoryRepoMock.Verify(r => r.AddAsync(It.IsAny<Category>()), Times.Once);
        }

        // ── GetAllCategories Tests ──────────────────────────────

        [Fact]
        public async Task GetAllCategories_ReturnsAllMappedCorrectly()
        {
            using var db = CreateDb("Cat_GetAll");
            var fakeCategories = new List<Category>
            {
                new Category { Id = 1, Name = "Science",  Description = "Science questions" },
                new Category { Id = 2, Name = "History",  Description = "History questions" }
            };
            _categoryRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(fakeCategories);
            var service = CreateService(db);

            var result = (await service.GetAllCategoriesAsync()).ToList();

            Assert.Equal(2, result.Count);
            Assert.Equal("Science", result[0].Name);
            Assert.Equal("History", result[1].Name);
        }

        [Fact]
        public async Task GetAllCategories_WhenEmpty_ReturnsEmptyList()
        {
            using var db = CreateDb("Cat_GetAll_Empty");
            _categoryRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Category>());
            var service = CreateService(db);

            var result = await service.GetAllCategoriesAsync();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllCategories_MapsIdCorrectly()
        {
            using var db = CreateDb("Cat_GetAll_Id");
            _categoryRepoMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Category> { new Category { Id = 5, Name = "Maths", Description = "Maths" } });
            var service = CreateService(db);

            var result = (await service.GetAllCategoriesAsync()).ToList();

            Assert.Equal(5, result[0].Id);
        }

        // ── UpdateCategory Tests ────────────────────────────────

        [Fact]
        public async Task UpdateCategory_WrongOwner_ReturnsFail()
        {
            using var db = CreateDb("Cat_Update_WrongOwner");
            var cat = new Category { Id = 1, Name = "Old", CreatedBy = 1 };
            _categoryRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(cat);
            var service = CreateService(db);

            var (success, message, _) = await service.UpdateCategoryAsync(1,
                new CreateCategoryDTO { Name = "New" }, userId: 2);

            Assert.False(success);
            Assert.Equal("You can only edit your own categories.", message);
        }

        [Fact]
        public async Task UpdateCategory_ValidOwner_ReturnsSuccess()
        {
            using var db = CreateDb("Cat_Update_Valid");
            var cat = new Category { Id = 1, Name = "Old", CreatedBy = 1 };
            _categoryRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(cat);
            _categoryRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Category>())).Returns(Task.CompletedTask);
            var service = CreateService(db);

            var (success, _, data) = await service.UpdateCategoryAsync(1,
                new CreateCategoryDTO { Name = "New Name" }, userId: 1);

            Assert.True(success);
            Assert.Equal("New Name", data!.Name);
        }

        // ── DeleteCategory Tests ────────────────────────────────

        [Fact]
        public async Task DeleteCategory_WrongOwner_ReturnsFail()
        {
            using var db = CreateDb("Cat_Delete_WrongOwner");
            var cat = new Category { Id = 1, Name = "Cat", CreatedBy = 1 };
            _categoryRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(cat);
            var service = CreateService(db);

            var (success, message) = await service.DeleteCategoryAsync(1, userId: 2);

            Assert.False(success);
            Assert.Equal("You can only delete your own categories.", message);
        }

        [Fact]
        public async Task DeleteCategory_InUse_ReturnsFail()
        {
            using var db = CreateDb("Cat_Delete_InUse");
            var cat = new Category { Id = 1, Name = "Cat", CreatedBy = 1 };
            _categoryRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(cat);
            // Seed a quiz using this category
            db.Quizzes.Add(new Quiz { Title = "Q", CategoryId = 1, CreatedBy = 1 });
            await db.SaveChangesAsync();
            var service = CreateService(db);

            var (success, message) = await service.DeleteCategoryAsync(1, userId: 1);

            Assert.False(success);
            Assert.Contains("Cannot delete", message);
        }

        [Fact]
        public async Task DeleteCategory_Valid_ReturnsSuccess()
        {
            using var db = CreateDb("Cat_Delete_Valid");
            var cat = new Category { Id = 1, Name = "Cat", CreatedBy = 1 };
            _categoryRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(cat);
            _categoryRepoMock.Setup(r => r.DeleteAsync(It.IsAny<Category>())).Returns(Task.CompletedTask);
            var service = CreateService(db);

            var (success, _) = await service.DeleteCategoryAsync(1, userId: 1);

            Assert.True(success);
        }
    }
}
