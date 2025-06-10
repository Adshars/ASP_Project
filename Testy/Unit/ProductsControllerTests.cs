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

namespace Products.Tests.Unit
{
    public class ProductsControllerTests
    {
        private (ProductsController controller, WarehouseDbContext context) GetControllerWithData(List<Product> products = null)
        {
            var options = new DbContextOptionsBuilder<WarehouseDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            var context = new WarehouseDbContext(options);

            // Dodaj domyślną kategorię, bo produkt musi ją mieć
            var category = new Category { Name = "DefaultCat", Description = "Default", Id = 1 };
            context.Categories.Add(category);

            if (products != null)
            {
                foreach (var p in products)
                    if (p.CategoryId == 0) p.CategoryId = category.Id;
                context.Products.AddRange(products);
                context.SaveChanges();
            }
            else
            {
                context.SaveChanges();
            }

            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            var mapper = config.CreateMapper();
            var loggerMock = new Mock<ILogger<ProductsController>>();

            var controller = new ProductsController(context, mapper, loggerMock.Object);
            return (controller, context);
        }

        // Pomocnicza metoda do ustawiania użytkownika (do User.Identity.Name)
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

        // Sprawdza, czy GetProducts zwraca listę wszystkich produktów.

        [Fact]
        public async Task GetProducts_ReturnsAllProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Name = "Product1", Description = "Desc1", Price = 10, CategoryId = 1 },
                new Product { Name = "Product2", Description = "Desc2", Price = 20, CategoryId = 1 }
            };
            var (controller, _) = GetControllerWithData(products);
            SetFakeUser(controller);

            // Act
            var result = await controller.GetProducts();

            // Assert
            var objectResult = Assert.IsAssignableFrom<ObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<List<ProductDTO>>(objectResult.Value);
            Assert.Equal(2, list.Count);
        }

        // Sprawdza, czy GetByID zwraca poprawny produkt dla istniejącego Id.

        [Fact]
        public void GetByID_ExistingId_ReturnsProduct()
        {
            // Arrange
            var products = new List<Product> { new Product { Name = "ProductA", Description = "A", Price = 5, CategoryId = 1 } };
            var (controller, context) = GetControllerWithData(products);
            SetFakeUser(controller);

            var productFromDb = context.Products.First();

            // Act
            var result = controller.GetByID(productFromDb.Id);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result.Result);
            var prodDto = Assert.IsType<ProductDTO>(objectResult.Value);
            Assert.Equal(productFromDb.Name, prodDto.Name);
        }

        // Sprawdza, czy PostAdd poprawnie dodaje nowy produkt i zwraca jego Id.

        [Fact]
        public async Task PostAdd_Valid_AddsProduct()
        {
            // Arrange
            var (controller, context) = GetControllerWithData();
            SetFakeUser(controller);

            var dto = new ProductCreateDTO
            {
                Name = "NowyProdukt",
                Description = "Testowy produkt",
                Price = 42,
                CategoryId = 1
            };

            // Act
            var result = await controller.PostAdd(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            int newProductId = Assert.IsType<int>(okResult.Value);
            Assert.True(context.Products.Any(p => p.Id == newProductId));
        }

        // Sprawdza, czy PutUpdate aktualizuje istniejący produkt.

        [Fact]
        public async Task PutUpdate_ExistingProduct_UpdatesProduct()
        {
            // Arrange
            var products = new List<Product> { new Product { Name = "ToEdit", Description = "Old", Price = 1, CategoryId = 1 } };
            var (controller, context) = GetControllerWithData(products);
            SetFakeUser(controller);

            var productFromDb = context.Products.First();

            var dto = new ProductDTO
            {
                Id = productFromDb.Id,
                Name = "ToEdit",
                Description = "Nowy opis",
                Price = 77,
                CategoryId = 1
            };

            // Act
            var result = await controller.PutUpdate(dto);

            // Assert
            Assert.IsType<OkResult>(result);
            var updated = context.Products.Find(productFromDb.Id);
            Assert.Equal("Nowy opis", updated.Description);
            Assert.Equal(77, updated.Price);
        }

        // Sprawdza, czy Delete usuwa istniejący produkt z bazy.

        [Fact]
        public async Task Delete_ExistingId_RemovesProduct()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<WarehouseDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            using var context = new WarehouseDbContext(options);
            var category = new Category { Name = "DelCat", Description = "DelDesc", Id = 1 };
            context.Categories.Add(category);

            var product = new Product { Name = "ToDelete", Description = "Del", Price = 15, CategoryId = 1 };
            context.Products.Add(product);
            context.SaveChanges();

            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            var mapper = config.CreateMapper();
            var loggerMock = new Mock<ILogger<ProductsController>>();
            var controller = new ProductsController(context, mapper, loggerMock.Object);
            SetFakeUser(controller);

            var productFromDb = context.Products.First();

            // Act
            var result = await controller.Delete(productFromDb.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Product deleted", okResult.Value);
            Assert.False(context.Products.Any(p => p.Id == productFromDb.Id));
        }
    }
}
