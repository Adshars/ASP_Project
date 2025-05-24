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


namespace WarehouseAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]/[Action]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly WarehouseDbContext _context;
       private readonly IMapper _mapper;

        public ProductsController(WarehouseDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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
                return Ok(productDtos);
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
        public ActionResult<ProductDTO> GetByID(int id)
        //public Product? GetByID(int id)
        {
            try
            {
                var product = _context.Products.Find(id);
                if (product == null)
                    throw new Exception($"Product with id {id} does not exist!");

                var productDTo = _mapper.Map<ProductDTO>(product);
                return Ok(productDTo);
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
                    return Conflict($"Product with name {newProductDto.Name} already exists");

                var product = _mapper.Map<Product>(newProductDto);
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return Ok(product.Id);
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

        //DELETE: api/Products/Delete
        [ActionName("Delete")]
        [HttpDelete]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var product = _context.Products.Find(id);
                if (product == null)
                    return Conflict($"Product with id {id} does not exist!");

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                return Ok("Product deleted");
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

        //PUT: api/Products/Update
        [ActionName("Update")]
        [HttpPut]
        public async Task<ActionResult> PutUpdate(ProductDTO updatedProductDto)
        {
            try
            {
                var product = await _context.Products.FindAsync(updatedProductDto.Id);
                if (product == null)
                    return Conflict($"Product {updatedProductDto.Name} does not exist!");

                _mapper.Map(updatedProductDto, product);
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
    }
}
