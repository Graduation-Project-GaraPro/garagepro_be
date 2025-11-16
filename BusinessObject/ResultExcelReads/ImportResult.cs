using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ResultExcelReads
{
    public class ImportResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        // ⭐ Thêm trường này
        public List<ImportErrorDetail> Errors { get; set; } = new();

        public static ImportResult Ok(string message = "Success")
            => new ImportResult
            {
                Success = true,
                Message = message,
                Errors = new List<ImportErrorDetail>()
            };

        public static ImportResult Fail(string message, List<ImportErrorDetail>? errors = null)
            => new ImportResult
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<ImportErrorDetail>()
            };
    }
}
