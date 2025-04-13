using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WarehouseAPI.Data;

namespace WarehouseAPI.Model
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }
        public int Status { get; set; } // 0: Not avalible, 1: Available
        [ForeignKey("CategoryId")]
        public int CategoryId { get; set; }
    }
}
