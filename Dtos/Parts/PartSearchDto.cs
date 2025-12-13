namespace Dtos.Parts
{
    public class PartSearchDto
    {
        public string SearchTerm { get; set; } // Search by name
        public Guid? PartCategoryId { get; set; }
        public Guid? BranchId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string SortBy { get; set; } = "Name"; // Name, Price, Stock, CreatedAt
        public string SortOrder { get; set; } = "Asc"; // Asc, Desc
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
