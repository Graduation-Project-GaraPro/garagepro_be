using BusinessObject;

namespace Services
{
    public interface ILabelService
    {
        Task<IEnumerable<Label>> GetAllLabelsAsync();
        Task<IEnumerable<Label>> GetLabelsByOrderStatusIdAsync(int orderStatusId); // Changed from Guid to int
        Task<IEnumerable<Label>> GetDefaultLabelsByOrderStatusIdAsync(int orderStatusId);
        Task<Label?> GetLabelByIdAsync(Guid id);
        Task<Label> CreateLabelAsync(Label label);
        Task<Label> UpdateLabelAsync(Label label);
        Task<bool> DeleteLabelAsync(Guid id);
        Task<bool> LabelExistsAsync(Guid id);
        // Removed GetAvailableColorsAsync as we're using fixed color data
    }
}