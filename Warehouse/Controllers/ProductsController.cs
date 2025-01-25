using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseAPI.Data;
using WarehouseAPI.Model;

namespace WarehouseAPI.Controllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly WarehouseDbContext _context;

        public ProductsController(WarehouseDbContext context)
        {
            _context = context;
        }

        // GET: api/Products/List
        [ActionName("List")]
        [HttpGet]
        public async Task<ActionResult<List<Product>>> GetProducts()
       {
            try
            {
                return Ok(await _context.Products.ToListAsync()); //5
            }
            catch (Exception ex)
            {
#if DEBUG
                var exception = ex;
                while (exception.InnerException != null)
                    exception = exception.InnerException;

                return new ObjectResult(exception.Message) { StatusCode = (int)HttpStatusCode.InternalServerError };
#else
                return new ObjectResult("Database error") { StatusCode = (int)HttpStatusCode.InternalServerError };
#endif
            }
        }
        //GET: api/Products/ByID/
        [ActionName("ByID")]
        [HttpGet("{id:range(1,250)}")]
        public Product? GetByID(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
                throw new Exception($"Product with id {id} does not exist!");

            
            
            return _context.Products.SingleOrDefault(c => c.Id == id);
            
        }
        //POST: api/Products/Add
        [ActionName("Add")]
        [HttpPost]
        public async Task<ActionResult<int>> PostAdd(Product newProduct)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            var productDB = await _context.Products.FirstOrDefaultAsync(c => c.Name == newProduct.Name);
            if (productDB != null)
                return Conflict($"Product with name {newProduct.Name} already exists");

            _context.Products.Add(newProduct); 
            await _context.SaveChangesAsync(); 

            return Ok(newProduct.Name);
        }

        //DELETE: api/Products/Delete
        [ActionName("Delete")]
        [HttpDelete]
        public async Task<ActionResult> Delete(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
                return Conflict($"Product with id {id} does not exist!");

            _context.Products.Remove(product); 
            await _context.SaveChangesAsync(); 

            return Ok("Product deleted");
        }

        //PUT: api/Products/Update
        [ActionName("Update")]
        [HttpPut]
        public async Task<ActionResult> PutUpdate(Product updatedProduct)
        {
            var product = await _context.Products.FindAsync(updatedProduct.Id); 
            if (product == null)
                return Conflict($"Product {updatedProduct.Name} does not exist!");

            product.Name = updatedProduct.Name;
            product.Description = updatedProduct.Description; 
            product.Price = updatedProduct.Price;
            product.Status = updatedProduct.Status;
            product.CategoryId = updatedProduct.CategoryId;
            await _context.SaveChangesAsync(); 

            return Ok();

        }
     }
}
