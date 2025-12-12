using System.Collections.Generic;

namespace Dtos.Parts
{
    public class PartCategoryPagedResultDto
    {
        public List<PartCategoryDto> Items { get; set; } = new List<PartCategoryDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }
}