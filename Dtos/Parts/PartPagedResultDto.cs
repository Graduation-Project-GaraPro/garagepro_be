using System.Collections.Generic;

namespace Dtos.Parts
{
    public class PartPagedResultDto
    {
        public List<PartDto> Items { get; set; } = new List<PartDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }
}
