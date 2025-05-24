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
using WarehouseAPI.DTO;
using AutoMapper;
using Warehouse.DTO;
using Microsoft.AspNetCore.Authorization;



namespace WarehouseAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]/[Action]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly WarehouseDbContext _context;
        private readonly IMapper _mapper;

        public CategoriesController(WarehouseDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Categories/List
        [ActionName("List")]
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<List<CategoryDTO>>> GetListAsync()
        {
            try
            {
                var categories = await _context.Categories.ToListAsync();
                var dto = _mapper.Map<List<CategoryDTO>>(categories);
                return Ok(dto);
                
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

        // GET: api/Categories/ByID/5
        [HttpGet("{id:range(1,250)}")]
        [ActionName("ByID")]
        public async Task<ActionResult<CategoryDTO>> GetByID(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                    return NotFound($"Category with id {id} does not exist!");

                var dto = _mapper.Map<CategoryDTO>(category);
                return Ok(dto);
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

        // POST: api/Categories/Add
        [HttpPost]
        [ActionName("Add")]
        public async Task<ActionResult<int>> PostAdd(CategoryDTO newCategoryDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var categoryExists = await _context.Categories.FirstOrDefaultAsync(c => c.Name == newCategoryDto.Name);
                if (categoryExists != null)
                    return Conflict($"Category with name {newCategoryDto.Name} already exists");

                var category = _mapper.Map<Category>(newCategoryDto);
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                return Ok(category.Id);
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
        public async Task<ActionResult> PutUpdate(CategoryDTO updatedCategoryDto)
        {
            try
            {
                var category = await _context.Categories.FindAsync(updatedCategoryDto.Id);
                if (category == null)
                    return Conflict($"Category {updatedCategoryDto.Name} does not exist!");

                _mapper.Map(updatedCategoryDto, category);
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
        // GET: api/Categories/ListWithProducts
        [HttpGet]
        [ActionName("ListWithProducts")]
        public async Task<ActionResult<List<CategoryDetailsDTO>>> GetListWithProducts()
        {
            try
            {
                var categories = await _context.Categories
                    .Include(c => c.Products)
                    .OrderBy(c => c.Id)
                    .ToListAsync();

                var dto = _mapper.Map<List<CategoryDetailsDTO>>(categories);
                return Ok(dto);
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