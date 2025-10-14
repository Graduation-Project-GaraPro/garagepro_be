using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Dtos.RoBoard
{
    public class UpdateRoBoardStatusDto
    {
        [Required]
        public Guid RepairOrderId { get; set; }
        
        [Required]
        public Guid NewStatusId { get; set; }
        
        public Guid? PreviousStatusId { get; set; }
        
        // Optional: Specific position within the new column
        public int? NewOrderIndex { get; set; }
        
        // Optional: Add or remove specific labels during the move
        public List<Guid> LabelsToAdd { get; set; } = new List<Guid>();
        
        public List<Guid> LabelsToRemove { get; set; } = new List<Guid>();
        
        // Optional: Add notes about the status change
        [MaxLength(500)]
        public string ChangeNote { get; set; }
        
        // For validation: Frontend can send this to ensure no conflicts
        public DateTime? LastModifiedAt { get; set; }
    }
    
    public class BatchUpdateRoBoardStatusDto
    {
        [Required]
        public List<UpdateRoBoardStatusDto> Updates { get; set; } = new List<UpdateRoBoardStatusDto>();
        
        [MaxLength(500)]
        public string BatchNote { get; set; }
    }
    
    public class RoBoardStatusUpdateResultDto
    {
        public bool Success { get; set; }
        
        public string Message { get; set; }
        
        public Guid RepairOrderId { get; set; }
        
        public Guid? OldStatusId { get; set; }
        
        public Guid? NewStatusId { get; set; }
        
        public DateTime UpdatedAt { get; set; }
        
        // Updated card data to reflect changes
        public RoBoardCardDto UpdatedCard { get; set; }
        
        // Any validation errors or warnings
        public List<string> Warnings { get; set; } = new List<string>();
        
        public List<string> Errors { get; set; } = new List<string>();
    }
    
    public class BatchRoBoardStatusUpdateResultDto
    {
        public bool OverallSuccess { get; set; }
        
        public int SuccessfulUpdates { get; set; }
        
        public int FailedUpdates { get; set; }
        
        public List<RoBoardStatusUpdateResultDto> Results { get; set; } = new List<RoBoardStatusUpdateResultDto>();
        
        public string BatchMessage { get; set; }
        
        public DateTime ProcessedAt { get; set; }
    }
    
    public class RoBoardMoveValidationDto
    {
        public Guid RepairOrderId { get; set; }
        
        public Guid FromStatusId { get; set; }
        
        public Guid ToStatusId { get; set; }
        
        public bool IsValid { get; set; }
        
        public string ValidationMessage { get; set; }
        
        public List<string> Requirements { get; set; } = new List<string>();
        
        public List<RoBoardLabelDto> AvailableLabelsInNewStatus { get; set; } = new List<RoBoardLabelDto>();
        
        public List<RoBoardLabelDto> LabelsToBeRemoved { get; set; } = new List<RoBoardLabelDto>();
    }
    
    public class ArchiveRepairOrderDto
    {
        [Required]
        public Guid RepairOrderId { get; set; }
        
        [Required]
        [MaxLength(500)]
        public string ArchiveReason { get; set; }
        
        public string? ArchivedByUserId { get; set; }
    }
    
    public class RestoreRepairOrderDto
    {
        [Required]
        public Guid RepairOrderId { get; set; }
        
        [MaxLength(500)]
        public string RestoreReason { get; set; }
        
        public string RestoredByUserId { get; set; }
    }
    
    public class ArchiveOperationResultDto
    {
        public bool Success { get; set; }
        
        public string Message { get; set; }
        
        public Guid RepairOrderId { get; set; }
        
        public bool IsArchived { get; set; }
        
        public DateTime? ArchivedAt { get; set; }
        
        public DateTime? RestoredAt { get; set; }
        
        public List<string> Errors { get; set; } = new List<string>();
        
        public List<string> Warnings { get; set; } = new List<string>();
    }
}