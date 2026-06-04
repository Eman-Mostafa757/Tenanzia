using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tenanzia.API.DTOs.Products;
using Tenanzia.API.Interfaces;
using Tenanzia.API.Models;

namespace Tenanzia.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductsController : ControllerBase
    {

        private readonly TenanziaContext _context;
        private readonly ITenantService _tenantService;

        public ProductsController(TenanziaContext context, ITenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var tenantId = _tenantService.GetTenantId();
            var products = _context.Products
                .Where(p => p.TenantId == tenantId && p.IsActive)
                .OrderBy(p => p.Name)
                .Select(p => new ProductResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Unit = p.Unit,
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt
                }).ToList();

            return Ok(products);
        }

        [HttpPost]
        public IActionResult Create(CreateProductDto dto)
        {
            var tenantId = _tenantService.GetTenantId();
            var product = new Product
            {
                TenantId = tenantId,
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Unit = dto.Unit,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Products.Add(product);
            _context.SaveChanges();
            return Ok(product);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, CreateProductDto dto)
        {
            var tenantId = _tenantService.GetTenantId();
            var product = _context.Products
                .FirstOrDefault(p => p.Id == id && p.TenantId == tenantId);

            if (product == null) return NotFound();

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.Unit = dto.Unit;
            _context.SaveChanges();
            return Ok("Updated successfully");
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var tenantId = _tenantService.GetTenantId();
            var product = _context.Products
                .FirstOrDefault(p => p.Id == id && p.TenantId == tenantId);

            if (product == null) return NotFound();

            // Soft delete
            product.IsActive = false;
            _context.SaveChanges();
            return Ok("Deleted successfully");
        }

    }
}
