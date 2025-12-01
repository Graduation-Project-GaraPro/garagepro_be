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
using Repositories.UnitOfWork;
using Dtos.PromotionalAplieds;

namespace Services.QuotationServices
{
    public class CustomerResponseQuotationService : ICustomerResponseQuotationService
    {
        private readonly IQuotationRepository _quotationRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly IPromotionalCampaignRepository _promotionalCampaignRepo;
        private readonly IHubContext<QuotationHub> _quotationHubContext;
        private readonly IUnitOfWork _iUnitOfWork;
        private readonly IHubContext<PromotionalHub> _promotionalHub;
        private readonly IMapper _mapper;

        public CustomerResponseQuotationService(
            IQuotationRepository quotationRepository,
            IServiceRepository serviceRepository,
            IPromotionalCampaignRepository promotionalCampaignRepo,
            IHubContext<QuotationHub> quotationHubContext,
            IHubContext<PromotionalHub> promotionalHub,
            IUnitOfWork iUnitOfWork,
            IMapper mapper)
        {
            _quotationRepository = quotationRepository;
            _serviceRepository = serviceRepository;
            _promotionalCampaignRepo = promotionalCampaignRepo;
            _quotationHubContext = quotationHubContext;
            _promotionalHub = promotionalHub;
            _iUnitOfWork = iUnitOfWork;
            _mapper = mapper;
        }

        public async Task<QuotationDto> ProcessCustomerResponseAsync(
            CustomerQuotationResponseDto responseDto,
            string userId)
        {
            // Bắt đầu transaction
            await _iUnitOfWork.BeginTransactionAsync();

            try
            {
                // 1. Lấy quotation
                var quotation = await _quotationRepository.GetByIdAsync(responseDto.QuotationId);
                if (quotation == null)
                    throw new ArgumentException($"Quotation with ID {responseDto.QuotationId} not found.");

                if (quotation.UserId != userId)
                    throw new ArgumentException($"Quotation with ID {responseDto.QuotationId} not your own");

                var status = Enum.Parse<QuotationStatus>(responseDto.Status);

                if (status == QuotationStatus.Approved)
                {
                    await ValidateCustomerResponseAsync(quotation, responseDto);

                    // Xử lý service + part + promotion
                    await ProcessServiceAndPartSelectionAsync(quotation, responseDto);

                    // Tính lại tổng
                    await RecalculateQuotationTotalAsync(quotation);

                    quotation.Status = status;
                    quotation.CustomerResponseAt = DateTime.UtcNow;
                    quotation.CustomerNote = responseDto.CustomerNote;
                }
                else if (status == QuotationStatus.Rejected)
                {
                    // When rejected, customer pays inspection fee for all services
                    quotation.TotalAmount = quotation.InspectionFee;
                    
                    // Update RO cost with inspection fee
                    if (quotation.RepairOrder != null)
                    {
                        quotation.RepairOrder.Cost = quotation.InspectionFee;
                    }
                    
                    quotation.Status = status;
                    quotation.CustomerResponseAt = DateTime.UtcNow;
                    quotation.CustomerNote = responseDto.CustomerNote;
                }

                
               await _quotationRepository.UpdateAsync(quotation); 

                
                await _iUnitOfWork.SaveChangesAsync();

                
                await _iUnitOfWork.CommitAsync();

                
                await SendQuotationUpdateNotificationAsync(quotation);

                await NotifyPromotionsAppliedAsync(quotation);

                // Send SignalR notification to managers when customer responds to quotation
                await NotifyManagersOfCustomerResponseAsync(quotation, status);
                return _mapper.Map<QuotationDto>(quotation);
            }
            catch
            {                
                await _iUnitOfWork.RollbackAsync();
                throw;
            }
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

            // Only calculate for selected services that are NOT Good
            foreach (var quotationService in quotation.QuotationServices.Where(qs => qs.IsSelected && !qs.IsGood))
            {
                // Tính giá dịch vụ sau discount
                //var servicePrice = quotationService.Price - quotationService.DiscountValue;
                //totalAmount += servicePrice;
                decimal partTotals = 0;
                // Tính giá phụ tùng được chọn
                foreach (var part in quotationService.QuotationServiceParts.Where(p => p.IsSelected))
                {
                   
                    partTotals += part.Price * part.Quantity; 
                }

                // Tính toán discount value
                if(quotationService.AppliedPromotionId.HasValue)
                {
                    var promotional = await _promotionalCampaignRepo.GetByIdAsync(quotationService.AppliedPromotionId.Value);
                    if (promotional != null)
                    {
                        if (promotional.UsageLimit <= 0)
                            throw new Exception("Promotion has reached usage limit.");

                        // Tính discount
                        var discountValue = _promotionalCampaignRepo.CalculateActualDiscountValue(
                            promotional,
                            quotationService.Price + partTotals);

                        // Giảm limit
                        promotional.UsageLimit--;
                        promotional.UsedCount++;

                        // Lưu lại vào DB
                        _promotionalCampaignRepo.Update(promotional);


                        quotationService.DiscountValue = discountValue;
                    }
                }    
                
                var finalPrice = quotationService.Price + partTotals - quotationService.DiscountValue;
                totalAmount += finalPrice;
                quotationService.FinalPrice = finalPrice;
            }

            quotation.TotalAmount = totalAmount;
            
            // Update RO cost with final total (replace, not add)
            if (quotation.RepairOrder != null)
            {
                quotation.RepairOrder.Cost = totalAmount;
            }
            
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

        private async Task NotifyPromotionsAppliedAsync(Quotation quotation)
        {
            // Only on approved quotations
            if (quotation.Status != QuotationStatus.Approved)
                return;

            // Ensure QuotationServices and AppliedPromotion are loaded in GetByIdAsync
            if (quotation.QuotationServices == null || !quotation.QuotationServices.Any())
                return;

            var servicesWithPromo = quotation.QuotationServices
                .Where(qs => qs.AppliedPromotionId.HasValue)
                .Select(qs => new PromotionAppliedServiceDto
                {
                    QuotationServiceId = qs.QuotationServiceId,
                    AppliedPromotionId = qs.AppliedPromotionId,
                    PromotionName = qs.AppliedPromotion?.Name
                })
                .ToList();

            if (!servicesWithPromo.Any())
                return;

            var payload = new PromotionAppliedNotificationDto
            {
                QuotationId = quotation.QuotationId,
                UserId = quotation.UserId,
                Services = servicesWithPromo
            };

            // 1) Send to global promotions dashboard group
            await _promotionalHub.Clients
                .Group("promotions-dashboard")
                .SendAsync("PromotionAppliedToQuotation", payload);

            // 2) Also send to each promotion-specific group
            var promotionIds = servicesWithPromo
                .Where(s => s.AppliedPromotionId.HasValue)
                .Select(s => s.AppliedPromotionId!.Value)
                .Distinct()
                .ToList();

            foreach (var promotionId in promotionIds)
            {
                var groupName = $"promotion-{promotionId}";
                await _promotionalHub.Clients
                    .Group(groupName)
                    .SendAsync("PromotionAppliedToQuotation", payload);
            }
        }

        private async Task NotifyManagersOfCustomerResponseAsync(Quotation quotation, QuotationStatus status)
        {
            var customerName = quotation.User != null 
                ? $"{quotation.User.FirstName} {quotation.User.LastName}".Trim() 
                : "Unknown Customer";

            var selectedServicesCount = quotation.QuotationServices?.Count(qs => qs.IsSelected) ?? 0;
            var totalServicesCount = quotation.QuotationServices?.Count ?? 0;

            await _quotationHubContext.Clients
                .Group("Managers")
                .SendAsync("CustomerRespondedToQuotation", new
                {
                    QuotationId = quotation.QuotationId,
                    RepairOrderId = quotation.RepairOrderId,
                    InspectionId = quotation.InspectionId,
                    CustomerId = quotation.UserId,
                    CustomerName = customerName,
                    Status = status.ToString(),
                    TotalAmount = quotation.TotalAmount,
                    SelectedServicesCount = selectedServicesCount,
                    TotalServicesCount = totalServicesCount,
                    CustomerNote = quotation.CustomerNote,
                    RespondedAt = quotation.CustomerResponseAt ?? DateTime.UtcNow,
                    Message = status == QuotationStatus.Approved 
                        ? $"Customer {customerName} approved quotation (${quotation.TotalAmount:F2})"
                        : $"Customer {customerName} rejected quotation"
                });

            Console.WriteLine($"[CustomerResponseQuotationService] Sent CustomerRespondedToQuotation to Managers group for Quotation {quotation.QuotationId}");
        }

    }
}
