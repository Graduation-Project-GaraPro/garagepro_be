using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Dtos.Job
{
    public class AssignTechnicianDto
    {
        [Required]
        public Guid TechnicianId { get; set; }
        
        public List<Guid> JobIds { get; set; } = new List<Guid>();
    }
    
    public class ReassignTechnicianDto
    {
        [Required]
        public Guid NewTechnicianId { get; set; }
    }
}