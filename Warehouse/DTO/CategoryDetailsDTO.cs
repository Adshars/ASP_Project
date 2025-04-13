using WarehouseAPI.DTO;

namespace Warehouse.DTO
{
    public class CategoryDetailsDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public List<ProductDTO>? Products { get; set; }
    }
}