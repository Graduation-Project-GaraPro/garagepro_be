using System.ComponentModel.DataAnnotations;

namespace Dtos.Parts
{
    public class PartCategorySearchDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
        public int Page { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100")]
        public int PageSize { get; set; } = 10;

        public string? SearchTerm { get; set; }

        /// <summary>
        /// Filter part categories by vehicle model ID (exact match)
        /// </summary>
        public Guid? ModelId { get; set; }

        /// <summary>
        /// Filter part categories by vehicle model name (partial match)
        /// </summary>
        public string? ModelName { get; set; }

        /// <summary>
        /// Filter part categories by vehicle brand ID (exact match)
        /// </summary>
        public Guid? BrandId { get; set; }

        /// <summary>
        /// Filter part categories by vehicle brand name (partial match)
        /// </summary>
        public string? BrandName { get; set; }

        public string SortBy { get; set; } = "CategoryName";

        public string SortOrder { get; set; } = "asc"; // "asc" or "desc"
    }
}