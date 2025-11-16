using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.ExcelImportSerivces;

namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MasterImportController : ControllerBase
    {
        private readonly IMasterDataImportService _importService;

        public MasterImportController(IMasterDataImportService importService)
        {
            _importService = importService;
        }

        [HttpPost("excel")]
        public async Task<IActionResult> ImportFromExcel(IFormFile file)
        {
            var result = await _importService.ImportFromExcelAsync(file);

            if (!result.Success)
                return BadRequest(result);   // trả về ImportResult với Errors

            return Ok(result);
        }
    }
}
