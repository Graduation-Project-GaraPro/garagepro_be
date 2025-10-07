using BusinessObject.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Services.Technician;

namespace Garage_pro_api.Controllers
{
    [Route("odata/[controller]")]
    [ApiController]
    [Authorize(Roles = "Technician")]
    public class SpecificationController : ODataController
    {
        private readonly ISpecificationService _specificationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public SpecificationController(
            ISpecificationService specificationService,
            UserManager<ApplicationUser> userManager)
        {
            _specificationService = specificationService;
            _userManager = userManager;
        }

        /// <summary>
        /// 🔹 Lấy tất cả thông số kỹ thuật của các xe
        /// </summary>
        [HttpGet("all")]
        [EnableQuery]
        public async Task<IActionResult> GetAllSpecifications()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new { Message = "Bạn cần đăng nhập để xem thông số kỹ thuật." });

            var specs = await _specificationService.GetAllSpecificationsAsync();
            if (specs == null || !specs.Any())
                return Ok(new { Message = "Chưa có dữ liệu thông số kỹ thuật trong hệ thống." });

            var result = specs.Select(v => new
            {
                v.LookupID,
                v.Automaker,
                v.NameCar,
                Categories = v.SpecificationsDatas
                    .Where(sd => sd.Specification != null && sd.Specification.SpecificationCategory != null)
                    .GroupBy(sd => new
                    {
                        CategoryName = sd.Specification.SpecificationCategory.Title,
                        CategoryOrder = sd.Specification.SpecificationCategory.DisplayOrder
                    })
                    .OrderBy(g => g.Key.CategoryOrder)
                    .Select(g => new
                    {
                        Category = g.Key.CategoryName,
                        DisplayOrder = g.Key.CategoryOrder,
                        Fields = g.OrderBy(sd => sd.Specification.DisplayOrder)
                                  .Select(sd => new
                                  {
                                      Label = sd.Specification.Label,
                                      Value = sd.Value,
                                      DisplayOrder = sd.Specification.DisplayOrder
                                  })
                                  .ToList()
                    })
                    .ToList()
            });

            return Ok(result.AsQueryable());
        }

        /// <summary>
        /// 🔹 Tìm kiếm theo Automaker hoặc NameCar
        /// </summary>
        [HttpGet("search")]
        [EnableQuery]
        public async Task<IActionResult> SearchSpecifications([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return BadRequest(new { Message = "Vui lòng nhập từ khóa tìm kiếm (Automaker hoặc NameCar)." });

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new { Message = "Bạn cần đăng nhập để tìm kiếm thông số kỹ thuật." });

            var specs = await _specificationService.SearchSpecificationsAsync(keyword);
            if (specs == null || !specs.Any())
                return Ok(new { Message = $"Không tìm thấy xe nào có chữ '{keyword}'." });

            var result = specs.Select(v => new
            {
                v.LookupID,
                v.Automaker,
                v.NameCar,
                Categories = v.SpecificationsDatas
                    .Where(sd => sd.Specification != null && sd.Specification.SpecificationCategory != null)
                    .GroupBy(sd => new
                    {
                        CategoryName = sd.Specification.SpecificationCategory.Title,
                        CategoryOrder = sd.Specification.SpecificationCategory.DisplayOrder
                    })
                    .OrderBy(g => g.Key.CategoryOrder)
                    .Select(g => new
                    {
                        Category = g.Key.CategoryName,
                        DisplayOrder = g.Key.CategoryOrder,
                        Fields = g.OrderBy(sd => sd.Specification.DisplayOrder)
                                  .Select(sd => new
                                  {
                                      Label = sd.Specification.Label,
                                      Value = sd.Value,
                                      DisplayOrder = sd.Specification.DisplayOrder
                                  })
                                  .ToList()
                    })
                    .ToList()
            });

            return Ok(result.AsQueryable());
        }
    }
}
