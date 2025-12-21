namespace Dtos.Parts
{
    public class PartSearchDto
    {
        public string SearchTerm { get; set; } // Search by name
        public Guid? PartCategoryId { get; set; }
        public Guid? BranchId { get; set; }
        
        /// <summary>
        /// Filter parts by vehicle model ID
        /// </summary>
        public Guid? ModelId { get; set; }
        
        /// <summary>
        /// Filter parts by vehicle model name (partial match)
        /// </summary>
        public string ModelName { get; set; }
        
        /// <summary>
        /// Filter parts by vehicle brand ID
        /// </summary>
        public Guid? BrandId { get; set; }
        
        /// <summary>
        /// Filter parts by vehicle brand name (partial match)
        /// </summary>
        public string BrandName { get; set; }
        
        /// <summary>
        /// Filter parts by category name (partial match)
        /// </summary>
        public string CategoryName { get; set; }
        
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string SortBy { get; set; } = "Name"; // Name, Price, Stock, CreatedAt, ModelName, BrandName, CategoryName
        public string SortOrder { get; set; } = "Asc"; // Asc, Desc
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
