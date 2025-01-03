using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WarehouseAPI.Model;

namespace WarehouseAPI.Data
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
