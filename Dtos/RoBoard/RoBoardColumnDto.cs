using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Dtos.RoBoard
{
    public class RoBoardColumnDto
    {
        public Guid OrderStatusId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string StatusName { get; set; }
        
        public int OrderIndex { get; set; }
        
        public List<RoBoardCardDto> Cards { get; set; } = new List<RoBoardCardDto>();
        
        public List<RoBoardLabelDto> AvailableLabels { get; set; } = new List<RoBoardLabelDto>();
        
        public int CardCount => Cards?.Count ?? 0;
    }
    
    public class RoBoardLabelDto
    {
        public Guid LabelId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string LabelName { get; set; }
        
        public string Description { get; set; }
        
        public RoBoardColorDto Color { get; set; }
    }
    
    public class RoBoardColorDto
    {
        public Guid ColorId { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string ColorName { get; set; }
        
        [Required]
        [MaxLength(7)]
        public string HexCode { get; set; }
    }
}