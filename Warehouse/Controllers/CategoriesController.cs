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
        [ActionName("List")]
        [HttpGet]
        public async Task<ActionResult<List<Category>>> GetCategories()
       {
            try
            {
                return Ok(await _context.Categories.ToListAsync()); //5
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

        [ActionName("ByID")]
        [HttpGet("{id:range(1,250)}")]
        public Category? GetByID(int id)
        {
            var category = _context.Categories.Find(id);
            if (category == null)
                throw new Exception($"Category with id {id} does not exist!");

            
            
            return _context.Categories.SingleOrDefault(c => c.Id == id);
            
        }
        [ActionName("Add")]
        [HttpPost]
        public async Task<ActionResult<int>> PostAdd(Category newCategory)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            var categoryDB = await _context.Categories.FirstOrDefaultAsync(c => c.Name == newCategory.Name);
            if (categoryDB != null)
                return Conflict($"Category with name {newCategory.Name} already exists");

            _context.Categories.Add(newCategory); 
            await _context.SaveChangesAsync(); 

            return Ok(newCategory.Id);
        }
     }
}

