using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repositories.PartRepositories;
using Repositories.QuotationRepositories;
using Repositories.ServiceRepositories;
using Repositories;
using Repositories.CampaignRepositories;
using AutoMapper;
using BusinessObject.Enums;
using BusinessObject;
using Dtos.Quotations;
using Microsoft.AspNetCore.SignalR;
using Services.CampaignServices;
using Services.Hubs;
using BusinessObject.Campaigns;

namespace Services.QuotationServices
{
    public class CustomerResponseQuotationService : ICustomerResponseQuotationService
    {
        private readonly IQuotationRepository _quotationRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly IPromotionalCampaignRepository _promotionalCampaignRepo;
        private readonly IHubContext<QuotationHub> _quotationHubContext;
        private readonly IMapper _mapper;

        public CustomerResponseQuotationService(
            IQuotationRepository quotationRepository,
            IServiceRepository serviceRepository,
            IPromotionalCampaignRepository promotionalCampaignRepo,
            IHubContext<QuotationHub> quotationHubContext,
            IMapper mapper)
        {
            _quotationRepository = quotationRepository;
            _serviceRepository = serviceRepository;
            _promotionalCampaignRepo = promotionalCampaignRepo;
            _quotationHubContext = quotationHubContext;
            _mapper = mapper;
        }

        public async Task<QuotationDto> ProcessCustomerResponseAsync(CustomerQuotationResponseDto responseDto, string userId)
        {
            // Lấy quotation cùng toàn bộ dữ liệu liên quan
            var quotation = await _quotationRepository.GetByIdAsync(responseDto.QuotationId);
            if (quotation == null)
                throw new ArgumentException($"Quotation with ID {responseDto.QuotationId} not found.");
            if(quotation.UserId != userId)
            {
                throw new ArgumentException($"Quotation with ID {responseDto.QuotationId} not your own");

            }
            // Validate customer response
            await ValidateCustomerResponseAsync(quotation, responseDto);

            // Cập nhật trạng thái và thời gian phản hồi của khách hàng
            quotation.Status = Enum.Parse<QuotationStatus>(responseDto.Status);
            quotation.CustomerResponseAt = DateTime.UtcNow;
            quotation.Note = responseDto.CustomerNote;

            // Xử lý lựa chọn dịch vụ và phụ tùng
            await ProcessServiceAndPartSelectionAsync(quotation, responseDto);

            // Tính toán lại tổng tiền dựa trên lựa chọn của khách hàng
            await RecalculateQuotationTotalAsync(quotation);

            // Lưu lại thay đổi
            var updatedQuotation = await _quotationRepository.UpdateAsync(quotation);

            // Gửi real-time notification
            await SendQuotationUpdateNotificationAsync(updatedQuotation);

            return _mapper.Map<QuotationDto>(updatedQuotation);
        }

        private async Task ValidateCustomerResponseAsync(Quotation quotation, CustomerQuotationResponseDto responseDto)
        {
            // Kiểm tra các dịch vụ bắt buộc phải được chọn
            var requiredServices = quotation.QuotationServices.Where(qs => qs.IsRequired).ToList();
            var selectedServiceIds = responseDto.SelectedServices.Select(s => s.QuotationServiceId).ToHashSet();

            foreach (var requiredService in requiredServices)
            {
                if (!selectedServiceIds.Contains(requiredService.QuotationServiceId))
                {
                    throw new InvalidOperationException($"Required service '{requiredService.Service?.ServiceName}' must be selected.");
                }
            }

            // Kiểm tra các dịch vụ được gửi lên có thuộc về quotation này không
            var quotationServiceIds = quotation.QuotationServices.Select(qs => qs.QuotationServiceId).ToHashSet();

            foreach (var selectedService in responseDto.SelectedServices)
            {
                if (!quotationServiceIds.Contains(selectedService.QuotationServiceId))
                {
                    throw new ArgumentException($"Service with ID {selectedService.QuotationServiceId} does not belong to this quotation.");
                }

                // Kiểm tra promotion (nếu có) có áp dụng được cho service này không
                if (selectedService.AppliedPromotionId.HasValue)
                {
                    var quotationService = quotation.QuotationServices
                        .First(qs => qs.QuotationServiceId == selectedService.QuotationServiceId);

                    var isPromotionApplicable = await _promotionalCampaignRepo.IsPromotionApplicableForServiceAsync(
                        selectedService.AppliedPromotionId.Value,
                        quotationService.ServiceId,
                        quotationService.Price);

                    if (!isPromotionApplicable)
                    {
                        throw new InvalidOperationException($"Promotion is not applicable for service '{quotationService.Service?.ServiceName}'.");
                    }
                }
            }
        }

        private async Task ProcessServiceAndPartSelectionAsync(Quotation quotation, CustomerQuotationResponseDto responseDto)
        {
            var selectedServiceIds = responseDto.SelectedServices.Select(s => s.QuotationServiceId).ToHashSet();
            var selectedServiceDict = responseDto.SelectedServices.ToDictionary(s => s.QuotationServiceId);

            foreach (var quotationService in quotation.QuotationServices)
            {
                // Cập nhật trạng thái chọn dịch vụ
                quotationService.IsSelected = selectedServiceIds.Contains(quotationService.QuotationServiceId);

                if (quotationService.IsSelected)
                {
                    var serviceDto = selectedServiceDict[quotationService.QuotationServiceId];
                    var selectedPartIds = serviceDto.SelectedPartIds?.ToHashSet() ?? new HashSet<Guid>();

                    // Cập nhật promotion cho dịch vụ
                    if (serviceDto.AppliedPromotionId.HasValue)
                    {
                        quotationService.AppliedPromotionId = serviceDto.AppliedPromotionId.Value;

                        // Tính toán discount value
                        var promotional = await _promotionalCampaignRepo.GetByIdAsync(serviceDto.AppliedPromotionId.Value);
                        if (promotional != null)
                        {

                            var discountValue = _promotionalCampaignRepo.CalculateActualDiscountValue(
                           promotional,
                           quotationService.Price);

                            quotationService.DiscountValue = discountValue;
                        }
                       
                    }
                    else
                    {
                        quotationService.AppliedPromotionId = null;
                        quotationService.DiscountValue = 0;
                    }

                    // Cập nhật lựa chọn phụ tùng cho dịch vụ này
                    foreach (var part in quotationService.QuotationServiceParts)
                    {
                        part.IsSelected = selectedPartIds.Contains(part.QuotationServicePartId);
                    }
                }
                else
                {
                    // Nếu dịch vụ không được chọn, reset promotion và bỏ chọn tất cả phụ tùng
                    quotationService.AppliedPromotionId = null;
                    quotationService.DiscountValue = 0;

                    foreach (var part in quotationService.QuotationServiceParts)
                    {
                        part.IsSelected = false;
                    }
                }
            }

            // Kiểm tra và điều chỉnh lựa chọn phụ tùng nếu cần
            await ValidateAndCorrectPartSelectionAsync(quotation);
        }

        private async Task RecalculateQuotationTotalAsync(Quotation quotation)
        {
            decimal totalAmount = 0;

            foreach (var quotationService in quotation.QuotationServices.Where(qs => qs.IsSelected))
            {
                // Tính giá dịch vụ sau discount
                var servicePrice = quotationService.Price - quotationService.DiscountValue;
                totalAmount += servicePrice;
                decimal partTotals = 0;
                // Tính giá phụ tùng được chọn
                foreach (var part in quotationService.QuotationServiceParts.Where(p => p.IsSelected))
                {
                    totalAmount += part.Price * part.Quantity;
                    partTotals += part.Price * part.Quantity; 
                }

                var finalPrice = quotationService.Price + partTotals - quotationService.DiscountValue;
                quotationService.FinalPrice = finalPrice;
            }

            quotation.TotalAmount = totalAmount;
            quotation.RepairOrder.Cost += totalAmount;
            quotation.UpdatedAt = DateTime.UtcNow;
        }
        private async Task SendQuotationUpdateNotificationAsync(Quotation quotation)
        {
            await _quotationHubContext
                .Clients
                .Group($"Quotation_{quotation.QuotationId}")
                .SendAsync("QuotationUpdated", new
                {
                    quotation.QuotationId,
                    quotation.UserId,
                    quotation.RepairOrderId,
                    quotation.TotalAmount,
                    quotation.Status,
                    quotation.Note,
                    UpdatedAt = quotation.UpdatedAt ?? DateTime.UtcNow,
                    CustomerRespondedAt = quotation.CustomerResponseAt
                });
        }



        /// Validates and corrects part selection based on whether services are advanced or not.
        private async Task ValidateAndCorrectPartSelectionAsync(Quotation quotation)
        {
            foreach (var quotationService in quotation.QuotationServices)
            {
                // Load the full service information to check if it's advanced
                var service = await _serviceRepository.GetByIdAsync(quotationService.ServiceId);

                if (service != null)
                {
                    // Get all selected parts for this service
                    var selectedParts = quotationService.QuotationServiceParts
                        .Where(qsp => qsp.IsSelected)
                        .ToList();

                    // If it's not an advanced service, ensure only one part is selected
                    if (!service.IsAdvanced && selectedParts.Count > 1)
                    {
                        // Keep only the first selected part and deselect the rest
                        for (int i = 1; i < selectedParts.Count; i++)
                        {
                            selectedParts[i].IsSelected = false;
                        }
                    }

                }
            }
        }



    }
}
