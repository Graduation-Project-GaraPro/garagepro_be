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
    [FromQuery] string? search,
    [FromQuery] StockFilter stockFilter = StockFilter.All)
        {
            if (modelId == Guid.Empty) return BadRequest("modelId is required.");
            if (branchId == Guid.Empty) return BadRequest("branchId is required.");

            var query = _context.PartCategories
                .AsNoTracking()
                .Where(pc => pc.ModelId == modelId);

            // ✅ LỌC PartCategory theo chi nhánh dựa trên PartInventory
            // - All: có record inventory ở branch
            // - InStock: stock > 0 ở branch
            // - OutOfStock: không có stock > 0 ở branch (kể cả không có record -> Stock = 0)
            query = query.Where(pc =>
                stockFilter == StockFilter.All
                    ? pc.Parts.Any(p => p.PartInventories.Any(pi => pi.BranchId == branchId))
                    : stockFilter == StockFilter.InStock
                        ? pc.Parts.Any(p => p.PartInventories.Any(pi => pi.BranchId == branchId && pi.Stock > 0))
                        : pc.Parts.Any(p => !p.PartInventories.Any(pi => pi.BranchId == branchId && pi.Stock > 0))
            );

            // 🔍 SEARCH
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim();
                var like = $"%{keyword}%";

                query = query.Where(pc =>
                    EF.Functions.Like(pc.CategoryName, like) ||
                    (pc.Description != null && EF.Functions.Like(pc.Description, like)) ||
                    pc.Parts.Any(p => EF.Functions.Like(p.Name, like))
                );
            }

            var result = await query
                .OrderBy(pc => pc.CategoryName)
                .Select(pc => new PartCategoryListDto
                {
                    PartCategoryId = pc.LaborCategoryId,   // ✅ đúng PK
                    CategoryName = pc.CategoryName,
                    Description = pc.Description,
                    ModelName = pc.VehicleModel.ModelName,
                    BrandName = pc.VehicleModel.Brand.BrandName,

                    // ✅ TotalParts theo stockFilter
                    TotalParts =
                        stockFilter == StockFilter.All
                            ? pc.Parts.Count(p => p.PartInventories.Any(pi => pi.BranchId == branchId))
                            : stockFilter == StockFilter.InStock
                                ? pc.Parts.Count(p => p.PartInventories.Any(pi => pi.BranchId == branchId && pi.Stock > 0))
                                : pc.Parts.Count(p => !p.PartInventories.Any(pi => pi.BranchId == branchId && pi.Stock > 0))
                })
                .ToListAsync();

            return Ok(result);
        }




        [HttpGet("part-categories/{id}")]
        public async Task<IActionResult> GetPartCategoryDetail(
            [FromRoute] Guid id,
            [FromQuery] Guid branchId,
            [FromQuery] Guid? modelId,
            [FromQuery] StockFilter stockFilter = StockFilter.All)
        {
            if (id == Guid.Empty) return BadRequest("id is required.");
            if (branchId == Guid.Empty) return BadRequest("branchId is required.");

            var result = await _context.PartCategories
                .AsNoTracking()
                .Where(pc =>
                    pc.LaborCategoryId == id &&
                    (!modelId.HasValue || pc.ModelId == modelId.Value)
                )
                .Select(pc => new PartCategoryDetailDto
                {
                    PartCategoryId = pc.LaborCategoryId,
                    CategoryName = pc.CategoryName,
                    Description = pc.Description,
                    ModelName = pc.VehicleModel.ModelName,
                    BrandName = pc.VehicleModel.Brand.BrandName,

                    Parts = pc.Parts
                        .Where(p =>
                            stockFilter == StockFilter.All
                                ? p.PartInventories.Any(pi => pi.BranchId == branchId)
                                : stockFilter == StockFilter.InStock
                                    ? p.PartInventories.Any(pi => pi.BranchId == branchId && pi.Stock > 0)
                                    : !p.PartInventories.Any(pi => pi.BranchId == branchId && pi.Stock > 0)
                        )
                        .OrderBy(p => p.Name)
                        .Select(p => new PartInventoryDto
                        {
                            PartId = p.PartId,
                            PartName = p.Name,
                            Price = p.Price,
                            WarrantyMonths = p.WarrantyMonths,

                            Stock = p.PartInventories
                                .Where(pi => pi.BranchId == branchId)
                                .Select(pi => pi.Stock)
                                .FirstOrDefault()   // không có record => 0
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (result == null) return NotFound();
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