using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseAPI.Data;

namespace WarehouseAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly WarehouseDbContext _context;

        public CategoriesController(WarehouseDbContext context)
        {
            _context = context;
        }

        // GET: api/Categories
        // wyświetla wszystkie kategorie
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            return await _context.Categories.ToListAsync();
        }

        // GET: api/Categories/5
        // wyświetla kategorię o podanym id
        [ActionName("ByID")]
        [HttpGet("{id:range(1,250)}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
                throw new Exception($"Category with id {id} does not exist!");

            return category;
        }
        // GET: api/Categories/ListWithProducts
        // wyświetla kategorie wraz z produktami
        [ActionName("ListWithProducts")]
        [HttpGet]
        public async Task<ActionResult<List<object>>> GetListWithProducts()
        {
            var category = await (from c in _db.Categories
                                  orderby c.Name ascending
                                  select new
                                  {
                                      c.Id,
                                      c.Name,
                                      Products = from p in c.Products
                                                 orderby p.Name ascending
                                                 select new
                                                 {
                                                     p.Id,
                                                     p.Name,
                                                     p.Description,
                                                     p.Price,
                                                     p.Status
                                                 }
                                  }).ToListAsync();

            return Ok(category);
        }
        // GET: api/Categories/ProductByID/...
        //wyswietla produkt o podanym id z przydzieloną kategorią
        [ActionName("ProductByID")] 
        [HttpGet("{id:range(1,250)}")]
        public async Task<ActionResult<Product>> GetProductByID(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                throw new Exception($"Product with id {id} does not exist!");

            return product;
        }
        
        // PUT: api/Categories/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        // aktualizuje kategorię o podanym id
        
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(int id, Category category)
        {
            if (id != category.Id)
            {
                return BadRequest();
            }

            _context.Entry(category).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Categories
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        // dodaje nową kategorię
        [ActionName("AddNewCategory")]
        [HttpPost]
        public async Task<ActionResult<int>> PostAdd(Category newCategory)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            var categoryDB = await _db.Categories.FirstOrDefaultAsync(c => c.Name == newCategory.Name);
            if (categoryDB != null)
                return Conflict($"Category with name {newCategory.Name} already exists");

            _db.Categories.Add(newCategory); 
            await _db.SaveChangesAsync(); 

            return Ok(newCategory.Id);
        }

        // dodaje nowy produkt do listy produktów
        [ActionName("AddNewProduct")]
        [HttpPost]
        public async Task<ActionResult<int>> PostAdd(Product newProduct)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            var productDB = await _db.Products.FirstOrDefaultAsync(p => p.Name == newProduct.Name);
            if (productDB != null)
                return Conflict($"Product with name {newProduct.Name} already exists");

            _db.Products.Add(newProduct); 
            await _db.SaveChangesAsync(); 

            return Ok(newProduct.Id);
        }

        // DELETE: api/Categories/5
        // usuwa kategorię o podanym id
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }

        // usuwa produkt o podanym id
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
