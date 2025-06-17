using Warehouse.DTO;
using WarehouseAPI.DTO;
using Xunit;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Testy.Integration
{
    public class TestyDto
    {
        [Fact]
        public void CanSerializeAndDeserializeCategoryDetailsDTO()
        {
            var dto = new CategoryDetailsDTO
            {
                Id = 1,
                Name = "Electronics",
                Description = "All kinds of electronic devices",
                Products = new List<ProductDTO>
                {
                    new ProductDTO { Id = 101, Name = "Phone", Price = 599.99M, Status = 1, CategoryId = 1 },
                    new ProductDTO { Id = 102, Name = "Laptop", Price = 1299.99M, Status = 1, CategoryId = 1 }
                }
            };

            string json = JsonConvert.SerializeObject(dto);
            var deserializedDto = JsonConvert.DeserializeObject<CategoryDetailsDTO>(json);

            Assert.NotNull(deserializedDto);
            Assert.Equal(dto.Id, deserializedDto.Id);
            Assert.Equal(dto.Name, deserializedDto.Name);
            Assert.Equal(dto.Description, deserializedDto.Description);
            Assert.NotNull(deserializedDto.Products);
            Assert.Equal(2, deserializedDto.Products.Count);
        }

        [Fact]
        public void CanSerializeAndDeserializeCategoryDTO()
        {
            var dto = new CategoryDTO
            {
                Id = 10,
                Name = "Furniture",
                Description = "All kinds of furniture"
            };

            string json = JsonConvert.SerializeObject(dto);
            var deserializedDto = JsonConvert.DeserializeObject<CategoryDTO>(json);

            Assert.NotNull(deserializedDto);
            Assert.Equal(dto.Id, deserializedDto.Id);
            Assert.Equal(dto.Name, deserializedDto.Name);
            Assert.Equal(dto.Description, deserializedDto.Description);
        }

        [Fact]
        public void CanSerializeAndDeserializeLoginDto()
        {
            var dto = new LoginDto
            {
                Username = "testuser",
                Password = "securepassword123"
            };

            string json = JsonConvert.SerializeObject(dto);
            var deserializedDto = JsonConvert.DeserializeObject<LoginDto>(json);

            Assert.NotNull(deserializedDto);
            Assert.Equal(dto.Username, deserializedDto.Username);
            Assert.Equal(dto.Password, deserializedDto.Password);
        }

        [Fact]
        public void CanSerializeAndDeserializeProductCreateDTO()
        {
            var dto = new ProductCreateDTO
            {
                Name = "Desk Lamp",
                Description = "LED lamp",
                Price = 89.99m,
                CategoryId = 3
            };

            string json = JsonConvert.SerializeObject(dto);
            var deserializedDto = JsonConvert.DeserializeObject<ProductCreateDTO>(json);

            Assert.NotNull(deserializedDto);
            Assert.Equal(dto.Name, deserializedDto.Name);
            Assert.Equal(dto.Description, deserializedDto.Description);
            Assert.Equal(dto.Price, deserializedDto.Price);
            Assert.Equal(dto.CategoryId, deserializedDto.CategoryId);
        }

        [Fact]
        public void CanSerializeAndDeserializeProductDTO()
        {
            var dto = new ProductDTO
            {
                Id = 501,
                Name = "Smartphone",
                Description = "Latest model with 5G",
                Price = 2999.99m,
                Status = 1,
                CategoryId = 2
            };

            string json = JsonConvert.SerializeObject(dto);
            var deserializedDto = JsonConvert.DeserializeObject<ProductDTO>(json);

            Assert.NotNull(deserializedDto);
            Assert.Equal(dto.Id, deserializedDto.Id);
            Assert.Equal(dto.Name, deserializedDto.Name);
            Assert.Equal(dto.Description, deserializedDto.Description);
            Assert.Equal(dto.Price, deserializedDto.Price);
            Assert.Equal(dto.Status, deserializedDto.Status);
            Assert.Equal(dto.CategoryId, deserializedDto.CategoryId);
        }

        [Fact]
        public void CanSerializeAndDeserializeRegisterDto()
        {
            var dto = new RegisterDto
            {
                Username = "newuser123",
                Email = "newuser@example.com",
                Password = "VerySecurePassword!"
            };

            string json = JsonConvert.SerializeObject(dto);
            var deserializedDto = JsonConvert.DeserializeObject<RegisterDto>(json);

            Assert.NotNull(deserializedDto);
            Assert.Equal(dto.Username, deserializedDto.Username);
            Assert.Equal(dto.Email, deserializedDto.Email);
            Assert.Equal(dto.Password, deserializedDto.Password);
        }
    }
}