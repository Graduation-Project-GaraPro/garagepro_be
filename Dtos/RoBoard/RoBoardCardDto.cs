using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BusinessObject.Enums;

namespace Dtos.RoBoard
{
    public class RoBoardCardDto
    {
        public Guid RepairOrderId { get; set; }
        
        public string RepairOrderType { get; set; }
        
        public DateTime ReceiveDate { get; set; }
        
        public DateTime? EstimatedCompletionDate { get; set; }
        
        public DateTime? CompletionDate { get; set; }
        
        public decimal Cost { get; set; }
        
        public decimal EstimatedAmount { get; set; }
        
        public decimal PaidAmount { get; set; }
        
        [Required]
        public PaidStatus PaidStatus { get; set; }
        
        public long? EstimatedRepairTime { get; set; }
        
        public string Note { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
        
        public bool IsArchived { get; set; }
        
        public DateTime? ArchivedAt { get; set; }
        
        public string ArchiveReason { get; set; }
        
        public string ArchivedBy { get; set; }
        
        public bool IsCancelled { get; set; }
        
        public DateTime? CancelledAt { get; set; }
        
        public string CancelReason { get; set; }
        
        public int StatusId { get; set; }
        
        public string StatusName { get; set; }
        
        public RoBoardVehicleDto Vehicle { get; set; }
        
        public RoBoardCustomerDto Customer { get; set; }
        
        public RoBoardBranchDto Branch { get; set; }
        
   
        public List<RoBoardLabelDto> AssignedLabels { get; set; } = new List<RoBoardLabelDto>();
        
        public List<string> TechnicianNames { get; set; } = new List<string>();
        
        public int OrderIndex { get; set; }
        
        public bool IsOverdue => EstimatedCompletionDate.HasValue && 
                                EstimatedCompletionDate.Value < DateTime.UtcNow && 
                                CompletionDate == null;
        
        public decimal CompletionPercentage => EstimatedAmount > 0 ? (PaidAmount / EstimatedAmount) * 100 : 0;
        
        public int DaysInCurrentStatus { get; set; }
        
        // Progress calculation
        public int TotalJobs { get; set; }
        public int CompletedJobs { get; set; }
        public decimal ProgressPercentage { get; set; }
        
        public bool IsVehiclePickedUp { get; set; }
        
        public DateTime? VehiclePickupDate { get; set; }
        
        public bool IsFullyPaid { get; set; }
        
        public DateTime? FullPaymentDate { get; set; }
        
        // Computed completion properties
        public bool CanBeArchived => StatusName == "Completed" && IsFullyPaid && IsVehiclePickedUp;
        
        public string CompletionSubStatus => GetCompletionSubStatus();
        
        private string GetCompletionSubStatus()
        {
            if (StatusName != "Completed") return "";
            
            if (IsFullyPaid && IsVehiclePickedUp) return "Ready to Archive";
            if (!IsFullyPaid && !IsVehiclePickedUp) return "Pending Payment & Pickup";
            if (!IsFullyPaid) return "Pending Payment";
            if (!IsVehiclePickedUp) return "Pending Vehicle Pickup";
            return "";
        }
    }
    
    public class RoBoardVehicleDto
    {
        public Guid VehicleId { get; set; }
        
        public string LicensePlate { get; set; }
        
        public string VIN { get; set; }
        
        public string BrandName { get; set; }
        
        public string ModelName { get; set; }
        
        public string ColorName { get; set; }
    }
    
    public class RoBoardCustomerDto
    {
        public string UserId { get; set; }
        
        public string FirstName { get; set; }
        
        public string LastName { get; set; }
        
        public DateTime? Birthday { get; set; }
        
        // Computed property for FullName based on FirstName and LastName
        public string FullName => $"{FirstName} {LastName}".Trim();
        
        public string Email { get; set; }
        
        public string PhoneNumber { get; set; }
    }
    
    public class CreateCustomerDto
    {
        [Required(ErrorMessage = "First name is required")]
        [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public string FirstName { get; set; }
        
        [Required(ErrorMessage = "Last name is required")]
        [MaxLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public string LastName { get; set; }
        
        public DateTime? Birthday { get; set; }
        
        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^[0-9+\-\(\)\s]{10,20}$", ErrorMessage = "Phone number must be 10-20 digits and can include +, -, (), spaces")]
        [MaxLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string PhoneNumber { get; set; }
        
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; }
        
        // Computed property for FullName
        public string FullName => $"{FirstName} {LastName}";
    }

    public class QuickCreateCustomerDto
    {
        [Required(ErrorMessage = "First name is required")]
        [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public string FirstName { get; set; }
        
        [Required(ErrorMessage = "Last name is required")]
        [MaxLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public string LastName { get; set; }
        
        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^0[0-9]{9,19}$", ErrorMessage = "Phone number must start with 0 and contain 10-20 digits")]
        [MaxLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string PhoneNumber { get; set; }
        
        // Optional fields
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime? Birthday { get; set; }
    }

    public class UpdateCustomerDto
    {
        [Required(ErrorMessage = "First name is required")]
        [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public string FirstName { get; set; }
        
        [Required(ErrorMessage = "Last name is required")]
        [MaxLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public string LastName { get; set; }
        
        public DateTime? Birthday { get; set; }
        
        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^[0-9+\-\(\)\s]{10,20}$", ErrorMessage = "Phone number must be 10-20 digits and can include +, -, (), spaces")]
        [MaxLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string PhoneNumber { get; set; }
        
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; }
        
        // Computed property for FullName
        public string FullName => $"{FirstName} {LastName}";
    }

    public class RoBoardBranchDto
    {
        public Guid BranchId { get; set; }
        
        public string BranchName { get; set; }
        
        public string Address { get; set; }
        
        public string PhoneNumber { get; set; }
    }
}