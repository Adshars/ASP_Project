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
using AutoMapper;
using WarehouseAPI.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;


namespace WarehouseAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]/[Action]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
       private readonly WarehouseDbContext _context;
       private readonly IMapper _mapper;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(WarehouseDbContext context, IMapper mapper, ILogger<ProductsController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/Products/List
        [ActionName("List")]
        [HttpGet]
        public async Task<ActionResult<List<ProductDTO>>> GetProducts()
        {
            try
            {
                var products = await _context.Products.ToListAsync();
                var productDtos = _mapper.Map<List<ProductDTO>>(products);
                _logger.LogInformation("Pobrano listę produktów ({Count}) przez użytkownika {User}", productDtos.Count, User.Identity?.Name);

                return Ok(productDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas pobierania listy produktów");

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
        public ActionResult<ProductDTO> GetByID(int id)
        //public Product? GetByID(int id)
        {
            try
            {
                var product = _context.Products.Find(id);
                if (product == null)
                {

                    _logger.LogWarning("Produkt o id {Id} nie istnieje (żądane przez {User})", id, User.Identity?.Name);
                    throw new Exception($"Product with id {id} does not exist!");
                }
                var productDTo = _mapper.Map<ProductDTO>(product);
                _logger.LogInformation("Pobrano produkt {Id} przez użytkownika {User}", id, User.Identity?.Name);

                return Ok(productDTo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas pobierania produktu {Id}", id);

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

        // POST: api/Products/Add
        [ActionName("Add")]
        [HttpPost]
        public async Task<ActionResult<int>> PostAdd(ProductCreateDTO newProductDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var productDB = await _context.Products.FirstOrDefaultAsync(c => c.Name == newProductDto.Name);
                if (productDB != null)
                {
                    _logger.LogWarning("Próba dodania produktu o nazwie {Name}, który już istnieje (użytkownik: {User})", newProductDto.Name, User.Identity?.Name);

                    return Conflict($"Product with name {newProductDto.Name} already exists");
                }
                var product = _mapper.Map<Product>(newProductDto);
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Dodano nowy produkt {Name} (id: {Id}) przez użytkownika {User}", product.Name, product.Id, User.Identity?.Name);


                return Ok(product.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas dodawania produktu {Name}", newProductDto.Name);

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

        //DELETE: api/Products/Delete
        [ActionName("Delete")]
        [HttpDelete]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var product = _context.Products.Find(id);
                if (product == null)
                {
                    _logger.LogWarning("Próba usunięcia nieistniejącego produktu o id {Id} (użytkownik: {User})", id, User.Identity?.Name);

                    return Conflict($"Product with id {id} does not exist!");
                }
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Usunięto produkt {Id} przez użytkownika {User}", id, User.Identity?.Name);

                return Ok("Product deleted");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas usuwania produktu {Id}", id);

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

        //PUT: api/Products/Update
        [ActionName("Update")]
        [HttpPut]
        public async Task<ActionResult> PutUpdate(ProductDTO updatedProductDto)
        {
            try
            {
                var product = await _context.Products.FindAsync(updatedProductDto.Id);
                if (product == null)
                {
                    _logger.LogWarning("Próba aktualizacji nieistniejącego produktu {Id} (użytkownik: {User})", updatedProductDto.Id, User.Identity?.Name);
             
                return Conflict($"Product {updatedProductDto.Name} does not exist!");
                }
                _mapper.Map(updatedProductDto, product);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Zaktualizowano produkt {Id} przez użytkownika {User}", updatedProductDto.Id, User.Identity?.Name);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas aktualizacji produktu {Id}", updatedProductDto.Id);

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
