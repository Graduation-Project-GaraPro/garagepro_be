using System.ComponentModel.DataAnnotations;

namespace Dtos.Quotations
{
    public class CreateRevisionJobsDto
    {
        [Required]
        [MaxLength(500)]
        public string RevisionReason { get; set; }
    }
}