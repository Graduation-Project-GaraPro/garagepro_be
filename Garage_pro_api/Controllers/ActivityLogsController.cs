﻿using System.Text;
using BusinessObject.SystemLogs;
using Dtos.Logs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repositories.LogRepositories;
using Services.LogServices;

namespace Garage_pro_api.Controllers
{
    
    [Route("api/[controller]")]
    [Authorize("LOG_VIEW")]
    [ApiController]
    public class ActivityLogsController : ControllerBase
    {
        private readonly ILogService _logService;

        public ActivityLogsController(ILogService logService)
        {
            _logService = logService;
        }

        [HttpGet("activities")]
        public async Task<IActionResult> GetActivityLogs()
        {
            try
            {
                var logs = await _logService.GetUserActivityLogsAsync();
                return Ok(logs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving activity logs.", error = ex.Message });
            }
        }

        [HttpPost("search")]
        [HttpPost("get-all")]
        [HttpGet("search")] // Cung cấp cả GET và POST cho linh hoạt
        public async Task<ActionResult<LogSearchResult>> GetAllLogs([FromBody] LogSearchRequest request)
        {
            try
            {
                // Validate request
                if (request == null)
                {
                    return BadRequest(new { message = "Request cannot be null" });
                }

                if (request.PageNumber < 1)
                    request.PageNumber = 1;

                if (request.PageSize < 1 || request.PageSize > 1000)
                    request.PageSize = 50;

                if (request.Days.HasValue && (request.Days < 1 || request.Days > 365))
                    request.Days = 30;

                // Validate date range
                if (request.StartDate.HasValue && request.EndDate.HasValue)
                {
                    if (request.StartDate.Value > request.EndDate.Value)
                    {
                        return BadRequest(new { message = "Start date cannot be after end date" });
                    }
                }

                var result = await _logService.GetAllLogsAsync(request);

               
                return Ok(result);
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync(ex, "Error searching logs in controller");
                return StatusCode(500, new
                {
                    message = "An error occurred while searching logs",
                    detail = ex.Message
                });
            }
        }

        [HttpGet("system")]
        public async Task<IActionResult> GetSystemLogs()
        {
            try
            {
                var logs = await _logService.GetAllSystemLogsAsync();
                return Ok(logs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving system logs.", error = ex.Message });
            }
        }

        //[HttpGet("date-range")]
        //public async Task<IActionResult> GetLogsByDateRange([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        //{
        //    try
        //    {
        //        var logs = await _logService.GetLogsByDateRangeAsync(fromDate, toDate);
        //        return Ok(logs);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = "Error retrieving logs by date range.", error = ex.Message });
        //    }
        //}

        [HttpGet("source/{source}")]
        public async Task<IActionResult> GetLogsBySource(LogSource source)
        {
            try
            {
                var logs = await _logService.GetLogsBySourceAsync(source);
                return Ok(logs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving logs by source.", error = ex.Message });
            }
        }

        [HttpGet("quick-stats")]
        public async Task<ActionResult<LogStatistics>> GetQuickStats( int days = 7)
        {
            try
            {
                if (days < 1 || days > 365)
                    days = 7;
                var stats  =await _logService.GetLogStatistics(days);

                return Ok(stats);
            }
            catch(Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving logs by source.", error = ex.Message });

            }

        }

        [HttpPost("export")]
        public async Task<IActionResult> ExportLogs([FromBody] LogSearchRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { message = "Request cannot be null" });
                }

                // Tăng page size để lấy tất cả log thoả mãn filter
                request.PageSize = 100000;
                request.PageNumber = 1;

                var result = await _logService.GetAllLogsAsync(request);

                // Tạo file CSV
                var csvBuilder = new StringBuilder();
                csvBuilder.AppendLine("Timestamp,Level,Source,UserName,UserId,IP Address,Message,Details");

                foreach (var log in result.Logs)
                {
                    csvBuilder.AppendLine($"\"{log.Timestamp:yyyy-MM-dd HH:mm:ss}\",\"{log.Level}\",\"{log.Source}\",\"{log.UserName}\",\"{log.UserId}\",\"{log.IpAddress}\",\"{EscapeCsvField(log.Message)}\",\"{EscapeCsvField(log.Details ?? "")}\"");
                }

                var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
                var fileName = $"logs_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                return File(bytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync(ex, "Error exporting logs");
                return StatusCode(500, new { message = "An error occurred while exporting logs" });
            }
        }

        //[HttpGet("user/{userId}")]
        //public async Task<IActionResult> GetLogsByUserId(string userId)
        //{
        //    try
        //    {
        //        var logs = await _logService.GetLogsByUserIdAsync(userId);
        //        return Ok(logs);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = "Error retrieving logs by user ID.", error = ex.Message });
        //    }
        //}

        [HttpGet("files/{source}")]
        public async Task<IActionResult> GetLogsFromFile(LogSource source, [FromQuery] int days = 7)
        {
            try
            {
                var logs = await _logService.GetLogsFromFileAsync(source, days);
                return Ok(logs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving logs from file.", error = ex.Message });
            }
        }
        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field)) return "";
            return field.Replace("\"", "\"\"");
        }
        //[HttpGet("files")]
        //public async Task<IActionResult> GetAllLogsFromFiles([FromQuery] int days = 7)
        //{
        //    try
        //    {
        //        var logs = await _logService.GetAllLogsFromFilesAsync(days);
        //        return Ok(logs);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = "Error retrieving all logs from files.", error = ex.Message });
        //    }
        //}
    }
}
