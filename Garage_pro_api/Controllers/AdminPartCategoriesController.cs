using DataAccessLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [Authorize("PART_VIEW_ADMIN")]
    [ApiController]
    public class AdminPartCategoriesController : ControllerBase
    {
        private readonly MyAppDbContext _context;

        public AdminPartCategoriesController (MyAppDbContext context)
        {
            _context = context;
        }

        [HttpGet("brands")]
        public async Task<IActionResult> GetBrands([FromQuery] string? search)
        {
            var query = _context.VehicleBrands.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(b =>
                    b.BrandName.Contains(search));
            }

            var result = await query
                .OrderBy(b => b.BrandName)
                .Select(b => new BrandDropdownDto
                {
                    BrandId = b.BrandID,
                    BrandName = b.BrandName
                })
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("models")]
        public async Task<IActionResult> GetModelsByBrand(
            [FromQuery] Guid brandId,
            [FromQuery] string? search)
        {
            var query = _context.VehicleModels
                .Where(m => m.BrandID == brandId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(m =>
                    m.ModelName.Contains(search));
            }

            var result = await query
                .OrderBy(m => m.ModelName)
                .Select(m => new ModelDropdownDto
                {
                    ModelId = m.ModelID,
                    ModelName = m.ModelName,
                    ManufacturingYear = m.ManufacturingYear
                })
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("branches")]
        public async Task<IActionResult> GetBranches([FromQuery] string? search)
        {
            var query = _context.Branches.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(b =>
                    b.BranchName.Contains(search));
            }

            var result = await query
                .OrderBy(b => b.BranchName)
                .Select(b => new BranchDropdownDto
                {
                    BranchId = b.BranchId,
                    BranchName = b.BranchName
                })
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("part-categories")]
        public async Task<IActionResult> GetPartCategories(
            [FromQuery] Guid modelId,
            [FromQuery] Guid branchId,
            [FromQuery] string? search)
        {
            var query = _context.PartCategories
                .Where(pc => pc.ModelId == modelId)
                .AsQueryable();

            // 🔍 SEARCH
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(pc =>
                    pc.CategoryName.Contains(search) ||
                    (pc.Description != null && pc.Description.Contains(search)) ||

                    // search theo tên Part (rất hay dùng)
                    pc.Parts.Any(p => p.Name.Contains(search))
                );
            }

            var result = await query
                .Select(pc => new PartCategoryListDto
                {
                    PartCategoryId = pc.LaborCategoryId,
                    CategoryName = pc.CategoryName,
                    Description = pc.Description,

                    ModelName = pc.VehicleModel.ModelName,
                    BrandName = pc.VehicleModel.Brand.BrandName,

                    TotalParts = pc.Parts.Count(p =>
                        p.PartInventories.Any(pi => pi.BranchId == branchId))
                })
                .ToListAsync();

            return Ok(result);
        }


        [HttpGet("part-categories/{id}")]
        public async Task<IActionResult> GetPartCategoryDetail(
             Guid id,
             Guid branchId,
             StockFilter stockFilter = StockFilter.All)
        {
            var data = await _context.PartCategories
                .Where(pc => pc.LaborCategoryId == id)
                .Select(pc => new
                {
                    pc.LaborCategoryId,
                    pc.CategoryName,
                    pc.Description,
                    ModelName = pc.VehicleModel.ModelName,
                    BrandName = pc.VehicleModel.Brand.BrandName,

                    Parts = pc.Parts.Select(p => new
                    {
                        p.PartId,
                        p.Name,
                        p.Price,
                        p.WarrantyMonths,
                        Stock = p.PartInventories
                            .Where(pi => pi.BranchId == branchId)
                            .Select(pi => pi.Stock)
                            .FirstOrDefault()
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (data == null)
                return NotFound();

            
            var parts = data.Parts.Where(p =>
                stockFilter == StockFilter.All ||
                (stockFilter == StockFilter.InStock && p.Stock > 0) ||
                (stockFilter == StockFilter.OutOfStock && p.Stock == 0)
            );

            var result = new PartCategoryDetailDto
            {
                PartCategoryId = data.LaborCategoryId,
                CategoryName = data.CategoryName,
                Description = data.Description,
                ModelName = data.ModelName,
                BrandName = data.BrandName,

                Parts = parts.Select(p => new PartInventoryDto
                {
                    PartId = p.PartId,
                    PartName = p.Name,
                    Price = p.Price,
                    WarrantyMonths = p.WarrantyMonths,
                    Stock = p.Stock
                }).ToList()
            };

            return Ok(result);
        }



    }
}

public enum StockFilter
{
    All = 0,
    InStock = 1,
    OutOfStock = 2
}
public class PartCategoryListDto
{
    public Guid PartCategoryId { get; set; }
    public string CategoryName { get; set; }
    public string? Description { get; set; }

    public string ModelName { get; set; }
    public string BrandName { get; set; }

    public int TotalParts { get; set; }
}
public class PartCategoryDetailDto
{
    public Guid PartCategoryId { get; set; }
    public string CategoryName { get; set; }
    public string? Description { get; set; }

    public string ModelName { get; set; }
    public string BrandName { get; set; }

    public List<PartInventoryDto> Parts { get; set; }
}
public class PartInventoryDto
{
    public Guid PartId { get; set; }
    public string PartName { get; set; }
    public decimal Price { get; set; }
    public int? WarrantyMonths { get; set; }

    public int Stock { get; set; }
}

public class BrandDropdownDto
{
    public Guid BrandId { get; set; }
    public string BrandName { get; set; }
}
public class ModelDropdownDto
{
    public Guid ModelId { get; set; }
    public string ModelName { get; set; }
    public int ManufacturingYear { get; set; }
}
public class BranchDropdownDto
{
    public Guid BranchId { get; set; }
    public string BranchName { get; set; }
}