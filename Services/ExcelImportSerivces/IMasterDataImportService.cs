using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.ResultExcelReads;
using Microsoft.AspNetCore.Http;

namespace Services.ExcelImportSerivces
{
    public interface IMasterDataImportService
    {
        Task<ImportResult> ImportFromExcelAsync(IFormFile file);
    }
}
