using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WarehouseAPI.Controllers;
using WarehouseAPI.Data;
using WarehouseAPI.Model;
using WarehouseAPI.DTO;
using WarehouseAPI.Profiles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace Categories.Tests.Unit
{
    public class CategoriesControllerTests
    {
        private (CategoriesController controller, WarehouseDbContext context) GetControllerWithData(List<Category> categories = null)
        {
            var options = new DbContextOptionsBuilder<WarehouseDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            var context = new WarehouseDbContext(options);

            if (categories != null)
            {
                context.Categories.AddRange(categories);
                context.SaveChanges();
            }

            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            var mapper = config.CreateMapper();
            var loggerMock = new Mock<ILogger<CategoriesController>>();

            var controller = new CategoriesController(context, mapper, loggerMock.Object);
            return (controller, context);
        }

        // Pomocnicza metoda ustawiająca fałszywego użytkownika
        private void SetFakeUser(ControllerBase controller)
        {
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.ControllerContext.HttpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[]
                {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "TestUser")
                })
            );
        }

        [Fact]

        // Sprawdza, czy metoda GetListAsync zwraca wszystkie kategorie
        public async Task GetListAsync_ReturnsAllCategories()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { Name = "Test1", Description = "Desc1" },
                new Category { Name = "Test2", Description = "Desc2" }
            };
            var (controller, _) = GetControllerWithData(categories);

            SetFakeUser(controller);

            // Act
            var result = await controller.GetListAsync();

            // Assert
            var objectResult = Assert.IsAssignableFrom<ObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<List<CategoryDTO>>(objectResult.Value);
            Assert.Equal(2, list.Count);
        }

        [Fact]

        // Sprawdza, czy metoda GetByID zwraca kategorię o podanym ID
        public async Task GetByID_ExistingId_ReturnsCategory()
        {
            // Arrange
            var categories = new List<Category> { new Category { Name = "TestCat", Description = "Desc" } };
            var (controller, context) = GetControllerWithData(categories);
            var categoryFromDb = context.Categories.First();

            SetFakeUser(controller);

            // Act
            var result = await controller.GetByID(categoryFromDb.Id);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result.Result);
            var catDto = Assert.IsType<CategoryDTO>(objectResult.Value);
            Assert.Equal(categoryFromDb.Name, catDto.Name);
        }

        // Sprawdza, czy PostAdd poprawnie dodaje nową kategorię i zwraca jej Id

        [Fact]
        public async Task PostAdd_Valid_AddsCategory()
        {
            // Arrange
            var (controller, context) = GetControllerWithData();
            SetFakeUser(controller);

            var dto = new CategoryDTO
            {
                Name = "NewCat",
                Description = "Opis"
            };

            // Act
            var result = await controller.PostAdd(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            int newCategoryId = Assert.IsType<int>(okResult.Value);
            Assert.True(context.Categories.Any(c => c.Id == newCategoryId));
        }

        // Sprawdza, czy PutUpdate aktualizuje istniejącą kategorię w bazie

        [Fact]
        public async Task PutUpdate_ExistingCategory_UpdatesCategory()
        {
            // Arrange
            var categories = new List<Category> { new Category { Name = "EditCat", Description = "OldDesc" } };
            var (controller, context) = GetControllerWithData(categories);
            var categoryFromDb = context.Categories.First();

            SetFakeUser(controller);

            var dto = new CategoryDTO
            {
                Id = categoryFromDb.Id,
                Name = "EditCat",
                Description = "NewDesc"
            };

            // Act
            var result = await controller.PutUpdate(dto);

            // Assert
            Assert.IsType<OkResult>(result);
            var updated = context.Categories.Find(categoryFromDb.Id);
            Assert.Equal("NewDesc", updated.Description);
        }

        //Sprawdza, czy Delete usuwa istniejącą kategorię z bazy

        [Fact]
        public async Task Delete_ExistingId_RemovesCategory()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<WarehouseDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            using var context = new WarehouseDbContext(options);
            context.Categories.Add(new Category { Name = "DelCat", Description = "DelDesc" });
            context.SaveChanges();

            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            var mapper = config.CreateMapper();
            var loggerMock = new Mock<ILogger<CategoriesController>>();
            var controller = new CategoriesController(context, mapper, loggerMock.Object);

            SetFakeUser(controller);

            var categoryFromDb = context.Categories.First();

            // Act
            var result = await controller.Delete(categoryFromDb.Id);

            // Assert
            var okResult = Assert.IsType<OkResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.False(context.Categories.Any(c => c.Id == categoryFromDb.Id));
        }
    }
}

