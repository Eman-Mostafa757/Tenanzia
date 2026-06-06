using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tenanzia.API.DTOs.Products;
using Tenanzia.API.Interfaces;
using Tenanzia.API.Models;
using Tenanzia.API.Services;

namespace Tenanzia.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductsController : ControllerBase
    {

        private readonly TenanziaContext _context;
        private readonly ITenantService _tenantService;
        private readonly NotificationService _notificationService;

        public ProductsController(TenanziaContext context, ITenantService tenantService, NotificationService notificationService)
        {
            _context = context;
            _tenantService = tenantService;
            _notificationService = notificationService;
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
                    CreatedAt = p.CreatedAt,
                    StockQuantity = p.StockQuantity,
                    LowStockThreshold = p.LowStockThreshold,
                    TrackStock= p.TrackStock,
                    IsLowStock= p.TrackStock && p.StockQuantity <= p.LowStockThreshold
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
                StockQuantity= dto.StockQuantity,
                LowStockThreshold = dto.LowStockThreshold,
                TrackStock = dto.TrackStock,
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
            product.StockQuantity = dto.StockQuantity;
            product.LowStockThreshold = dto.LowStockThreshold;
            product.TrackStock = dto.TrackStock;
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
        [HttpGet("low-stock")]
        public IActionResult GetLowStock() {
            var tenantId = _tenantService.GetTenantId();
            var lowStockProducts = _context.Products
                .Where(p => p.TenantId == tenantId && p.IsActive && p.TrackStock && p.StockQuantity <= p.LowStockThreshold)
                .Select(p => new ProductResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Unit = p.Unit,
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt,
                    StockQuantity = p.StockQuantity,
                    LowStockThreshold = p.LowStockThreshold,
                    TrackStock = p.TrackStock,
                    IsLowStock = true
                }).ToList();
            
            return Ok(lowStockProducts);

        }
        [HttpPatch("{id}/stock")]
        public IActionResult UpdateStock(int id, [FromBody] int quantity)
        {
            var tenantId = _tenantService.GetTenantId();
            var product = _context.Products
                .FirstOrDefault(p => p.Id == id && p.TenantId == tenantId);
            if (product == null) return NotFound();
            product.StockQuantity = quantity;
            _context.SaveChanges();

            // Check for low stock and send notification if needed
            if (product.TrackStock && product.StockQuantity <= product.LowStockThreshold)
            {
                var owners = _context.UserRoles
                    .Include(ur => ur.Role)
                    .Where(ur => ur.Role.Name == "Owner" || ur.Role.Name == "Manager")
                    .Join(_context.UserTenants.Where(ut => ut.TenantId == tenantId),
                        ur => ur.UserId, ut => ut.UserId, (ur, ut) => ur.UserId)
                    .ToList();

                foreach (var ownerId in owners)
                {
                    _notificationService.Create(
                        tenantId,
                        ownerId??0,
                        "⚠️ Low Stock Alert",
                        $"{product.Name} is running low — only {product.StockQuantity} left",
                        "stock"
                    );
                }
            }

            return Ok("Stock updated successfully");
        }
        [HttpGet("{id}")]
        public IActionResult GetById(int id) {
            var tenantId = _tenantService.GetTenantId();
            var product = _context.Products
                .Where(p => p.Id == id && p.TenantId == tenantId && p.IsActive)
                .Select(p => new ProductResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Unit = p.Unit,
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt,
                    StockQuantity = p.StockQuantity,
                    LowStockThreshold = p.LowStockThreshold,
                    TrackStock = p.TrackStock,
                    IsLowStock = p.TrackStock && p.StockQuantity <= p.LowStockThreshold
                }).FirstOrDefault();
            if (product == null) return NotFound();
            return Ok(product);
        }


      


    }
}
