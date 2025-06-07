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
using Microsoft.Extensions.Logging;



namespace WarehouseAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]/[Action]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly WarehouseDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(WarehouseDbContext context, IMapper mapper, ILogger<CategoriesController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
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
                _logger.LogInformation("Pobrano listê kategorii ({Count}) przez u¿ytkownika {User}", dto.Count, User.Identity?.Name);
                return Ok(dto);
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "B³¹d podczas pobierania listy kategorii");

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
                {
                    _logger.LogWarning("Kategoria o id {Id} nie istnieje (¿¹dane przez {User})", id, User.Identity?.Name);
                    return NotFound($"Category with id {id} does not exist!");
                }
                var dto = _mapper.Map<CategoryDTO>(category);
                _logger.LogInformation("Pobrano kategoriê {Id} przez u¿ytkownika {User}", id, User.Identity?.Name);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "B³¹d podczas pobierania kategorii {Id}", id);

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
                {
                    _logger.LogWarning("Próba dodania kategorii o nazwie {Name}, która ju¿ istnieje (u¿ytkownik: {User})", newCategoryDto.Name, User.Identity?.Name);
                    return Conflict($"Category with name {newCategoryDto.Name} already exists");
                }
                var category = _mapper.Map<Category>(newCategoryDto);
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Dodano now¹ kategoriê {Name} (id: {Id}) przez u¿ytkownika {User}", category.Name, category.Id, User.Identity?.Name);

                return Ok(category.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "B³¹d podczas dodawania kategorii {Name}", newCategoryDto.Name);

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
                {
                    _logger.LogWarning("Próba usuniêcia nieistniej¹cej kategorii o id {Id} (u¿ytkownik: {User})", id, User.Identity?.Name);

                    return Conflict($"Category with id {id} does not exist!");
                }
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Usuniêto kategoriê {Id} przez u¿ytkownika {User}", id, User.Identity?.Name);

                return Ok();
            }
            catch (Exception ex)
            {
               _logger.LogError(ex, "B³¹d podczas usuwania kategorii {Id}", id);

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
                {
                    _logger.LogWarning("Próba aktualizacji nieistniej¹cej kategorii {Id} (u¿ytkownik: {User})", updatedCategoryDto.Id, User.Identity?.Name);

                    return Conflict($"Category {updatedCategoryDto.Name} does not exist!");
                }
                _mapper.Map(updatedCategoryDto, category);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Zaktualizowano kategoriê {Id} przez u¿ytkownika {User}", updatedCategoryDto.Id, User.Identity?.Name);


                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "B³¹d podczas aktualizacji kategorii {Id}", updatedCategoryDto.Id);

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
                _logger.LogInformation("Pobrano listê kategorii z produktami ({Count}) przez u¿ytkownika {User}", dto.Count, User.Identity?.Name);

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "B³¹d podczas pobierania kategorii z produktami");

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