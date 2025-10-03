using Dtos.RepairOrder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Services;
using BusinessObject;
using Dtos.RoBoard;

namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RepairOrderController : ControllerBase
    {
        private readonly IRepairOrderService _repairOrderService;

        public RepairOrderController(IRepairOrderService repairOrderService)
        {
            _repairOrderService = repairOrderService;
        }

        // GET: api/RepairOrder
        [HttpGet]
        [EnableQuery]
        public async Task<IActionResult> GetRepairOrders()
        {
            var repairOrders = await _repairOrderService.GetAllRepairOrdersAsync();
            return Ok(repairOrders);
        }

        // GET: api/RepairOrder/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRepairOrder(Guid id)
        {
            var repairOrder = await _repairOrderService.GetRepairOrderCardAsync(id);
            if (repairOrder == null)
            {
                return NotFound();
            }

            // Map RoBoardCardDto to RepairOrderDto
            var repairOrderDto = new RepairOrderDto
            {
                RepairOrderId = repairOrder.RepairOrderId,
                ReceiveDate = repairOrder.ReceiveDate,
                RepairOrderType = repairOrder.RepairOrderType,
                EstimatedCompletionDate = repairOrder.EstimatedCompletionDate,
                CompletionDate = repairOrder.CompletionDate,
                Cost = repairOrder.Cost,
                EstimatedAmount = repairOrder.EstimatedAmount,
                PaidAmount = repairOrder.PaidAmount,
                PaidStatus = repairOrder.PaidStatus,
                EstimatedRepairTime = repairOrder.EstimatedRepairTime,
                Note = repairOrder.Note,
                CreatedAt = repairOrder.CreatedAt,
                UpdatedAt = repairOrder.UpdatedAt,
                IsArchived = repairOrder.IsArchived,
                ArchivedAt = repairOrder.ArchivedAt,
                ArchivedByUserId = repairOrder.ArchivedBy,
                BranchId = repairOrder.Branch?.BranchId ?? Guid.Empty,
                StatusId = repairOrder.StatusId,
                VehicleId = repairOrder.Vehicle?.VehicleId ?? Guid.Empty,
                UserId = repairOrder.Customer?.UserId ?? string.Empty,
                RepairRequestId = Guid.NewGuid() // This would need to be properly mapped
            };

            return Ok(repairOrderDto);
        }

        // POST: api/RepairOrder
        [HttpPost]
        public async Task<IActionResult> CreateRepairOrder(CreateRepairOrderDto createRepairOrderDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Map DTO to entity (this would require access to the actual RepairOrder entity)
            // For now, we'll just return a placeholder
            return CreatedAtAction(nameof(GetRepairOrder), new { id = Guid.NewGuid() }, createRepairOrderDto);
        }

        // PUT: api/RepairOrder/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRepairOrder(Guid id, UpdateRepairOrderDto updateRepairOrderDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Implementation would go here
            return NoContent();
        }

        // DELETE: api/RepairOrder/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRepairOrder(Guid id)
        {
            var result = await _repairOrderService.DeleteRepairOrderAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        // GET: api/RepairOrder/kanban
        [HttpGet("kanban")]
        [EnableQuery]
        public async Task<IActionResult> GetKanbanBoard()
        {
            var kanbanBoard = await _repairOrderService.GetKanbanBoardAsync();
            return Ok(kanbanBoard);
        }

        // GET: api/RepairOrder/listview
        [HttpGet("listview")]
        [EnableQuery]
        public async Task<IActionResult> GetListView()
        {
            var listView = await _repairOrderService.GetListViewAsync();
            return Ok(listView);
        }
        
        // GET: api/RepairOrder/status/{statusId}
        [HttpGet("status/{statusId}")]
        [EnableQuery]
        public async Task<IActionResult> GetRepairOrdersByStatus(Guid statusId)
        {
            var repairOrders = await _repairOrderService.GetRepairOrdersByStatusAsync(statusId);
            return Ok(repairOrders);
        }
        
        // POST: api/RepairOrder/status/update
        [HttpPost("status/update")]
        public async Task<IActionResult> UpdateRepairOrderStatus([FromBody] UpdateRoBoardStatusDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _repairOrderService.UpdateRepairOrderStatusAsync(updateDto);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        
        // POST: api/RepairOrder/status/batch-update
        [HttpPost("status/batch-update")]
        public async Task<IActionResult> BatchUpdateRepairOrderStatus([FromBody] BatchUpdateRoBoardStatusDto batchUpdateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _repairOrderService.BatchUpdateRepairOrderStatusAsync(batchUpdateDto);
            if (!result.OverallSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        
        // POST: api/RepairOrder/archive
        [HttpPost("archive")]
        public async Task<IActionResult> ArchiveRepairOrder([FromBody] ArchiveRepairOrderDto archiveDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _repairOrderService.ArchiveRepairOrderAsync(archiveDto);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        
        // POST: api/RepairOrder/restore
        [HttpPost("restore")]
        public async Task<IActionResult> RestoreRepairOrder([FromBody] RestoreRepairOrderDto restoreDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _repairOrderService.RestoreRepairOrderAsync(restoreDto);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        
        // GET: api/RepairOrder/archived
        [HttpGet("archived")]
        [EnableQuery]
        public async Task<IActionResult> GetArchivedRepairOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var archivedOrders = await _repairOrderService.GetArchivedRepairOrdersAsync(null, "ArchivedAt", "Desc", page, pageSize);
            return Ok(archivedOrders);
        }
    }
}