using BusinessObject.RequestEmergency;
using System.ComponentModel.DataAnnotations;

public class EmergencyMedia
{
    [Key]
    public Guid MediaId { get; set; }

    [Required]
    public Guid EmergencyRequestId { get; set; }

    [Required]
    public string FileUrl { get; set; } = string.Empty; // link ảnh

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public RequestEmergency RequestEmergency { get; set; }
}
