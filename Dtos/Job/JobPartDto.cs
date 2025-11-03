using System;
using System.ComponentModel.DataAnnotations;

namespace Dtos.Job
{
    public class JobPartDto
    {
        public Guid JobPartId { get; set; }
        
        public Guid JobId { get; set; }
        
        public Guid PartId { get; set; }
        
        public int Quantity { get; set; }
        
        public decimal UnitPrice { get; set; }
        
        public decimal TotalPrice => Quantity * UnitPrice;
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
        
        // Part details
        public string PartName { get; set; }
    }
}