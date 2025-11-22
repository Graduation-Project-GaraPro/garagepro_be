using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ResultExcelReads
{
    public class ImportErrorDetail
    {
        public string SheetName { get; set; }
        public int? RowNumber { get; set; }
        public string? ColumnName { get; set; }
        public string Message { get; set; }
        public string? ErrorCode { get; set; }

        public ImportErrorDetail(
            string sheetName,
            string message,
            int? rowNumber = null,
            string? columnName = null,
            string? errorCode = null)
        {
            SheetName = sheetName;
            Message = message;
            RowNumber = rowNumber;
            ColumnName = columnName;
            ErrorCode = errorCode;
        }

        public override string ToString()
        {
            var rowPart = RowNumber.HasValue ? $"Row {RowNumber}" : null;
            var colPart = !string.IsNullOrWhiteSpace(ColumnName) ? $"Column {ColumnName}" : null;
            var location = string.Join(", ", new[] { rowPart, colPart }.Where(x => !string.IsNullOrEmpty(x)));

            return string.IsNullOrEmpty(location)
                ? $"[{SheetName}] {Message}"
                : $"[{SheetName}] {location}: {Message}";
        }
    }
}
