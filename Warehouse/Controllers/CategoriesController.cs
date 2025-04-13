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
    [Route("api/[controller]/[Action]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly WarehouseDbContext _context;

        public CategoriesController(WarehouseDbContext context)
        {
            _context = context;
        }

        // GET: api/Categories/List
        [ActionName("List")]
        [HttpGet]
        public async Task<ActionResult<List<Category>>> GetListAsync()
        {
            try
            {
                return Ok(await _context.Categories.ToListAsync());
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

        //GET: api/Categories/ByID/
        [ActionName("ByID")]
        [HttpGet("{id:range(1,250)}")]
        public ActionResult<Category?> GetByID(int id)
        {
            try
            {
                var category = _context.Categories.Find(id);
                if (category == null)
                    throw new Exception($"Category with id {id} does not exist!");
                return Ok(category);
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

        //POST: api/Categories/Add
        [ActionName("Add")]
        [HttpPost]
        public async Task<ActionResult<int>> PostAdd(Category newCategory)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var categoryDB = await _context.Categories.FirstOrDefaultAsync(c => c.Name == newCategory.Name);
                if (categoryDB != null)
                    return Conflict($"Category with name {newCategory.Name} already exists");

                _context.Categories.Add(newCategory);
                await _context.SaveChangesAsync();

                return Ok(newCategory.Id);
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

        //DELETE: api/Categories/Delete
        [ActionName("Delete")]
        [HttpDelete]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var category = _context.Categories.Find(id);
                if (category == null)
                    return Conflict($"Category with id {id} does not exist!");

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                return Ok();
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

        //PUT: api/Categories/Update
        [ActionName("Update")]
        [HttpPut]
        public async Task<ActionResult> PutUpdate(Category updatedCategory)
        {
            try
            {
                var category = await _context.Categories.FindAsync(updatedCategory.Id);
                if (category == null)
                    return Conflict($"Category {updatedCategory.Name} does not exist!");

                category.Name = updatedCategory.Name;
                category.Description = updatedCategory.Description;
                await _context.SaveChangesAsync();

                return Ok();
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
        //api/Categories/ListWitProducts
        [ActionName("ListWithProducts")]
        [HttpGet]
        public async Task<ActionResult<List<object>>> GetListWithProducts()
        {
            try
            {
                var categories = await (from c in _context.Categories
                                        orderby c.Id ascending
                                        select new
                                        {
                                            c.Id,
                                            c.Name,
                                            c.Description,
                                            Products = from h in c.Products
                                                       orderby h.Id ascending
                                                       select new
                                                       {
                                                           h.Id,
                                                           h.Name,
                                                           h.Description,
                                                       }
                                        }).ToListAsync();

                return Ok(categories);
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
    }
}
