using BusinessObject;
using BusinessObject.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Tests
{
    [TestClass]
    public class RepairOrderServiceTests
    {
        private Mock<IRepairOrderRepository> _mockRepairOrderRepository;
        private Mock<IOrderStatusRepository> _mockOrderStatusRepository;
        private Mock<ILabelRepository> _mockLabelRepository;
        private Mock<Microsoft.AspNetCore.SignalR.IHubContext<Hubs.RepairOrderHub>> _mockHubContext;
        private Mock<Repositories.ServiceRepositories.IServiceRepository> _mockServiceRepository;
        private RepairOrderService _repairOrderService;

        [TestInitialize]
        public void Setup()
        {
            _mockRepairOrderRepository = new Mock<IRepairOrderRepository>();
            _mockOrderStatusRepository = new Mock<IOrderStatusRepository>();
            _mockLabelRepository = new Mock<ILabelRepository>();
            _mockHubContext = new Mock<Microsoft.AspNetCore.SignalR.IHubContext<Hubs.RepairOrderHub>>();
            _mockServiceRepository = new Mock<Repositories.ServiceRepositories.IServiceRepository>();

            _repairOrderService = new RepairOrderService(
                _mockRepairOrderRepository.Object,
                _mockOrderStatusRepository.Object,
                _mockLabelRepository.Object,
                _mockHubContext.Object,
                _mockServiceRepository.Object
            );
        }

        [TestMethod]
        public async Task UpdateCostFromInspectionAsync_WithCompletedInspectionWithoutQuotation_ShouldUpdateCost()
        {
            // Arrange
            var repairOrderId = Guid.NewGuid();
            var serviceId1 = Guid.NewGuid();
            var serviceId2 = Guid.NewGuid();

            var repairOrder = new RepairOrder
            {
                RepairOrderId = repairOrderId,
                Cost = 0,
                Inspections = new List<Inspection>
                {
                    new Inspection
                    {
                        InspectionId = Guid.NewGuid(),
                        RepairOrderId = repairOrderId,
                        Status = InspectionStatus.Completed,
                        ServiceInspections = new List<ServiceInspection>
                        {
                            new ServiceInspection
                            {
                                ServiceInspectionId = Guid.NewGuid(),
                                ServiceId = serviceId1,
                                Service = new Service
                                {
                                    ServiceId = serviceId1,
                                    Price = 1000000 // 1,000,000
                                }
                            },
                            new ServiceInspection
                            {
                                ServiceInspectionId = Guid.NewGuid(),
                                ServiceId = serviceId2,
                                Service = new Service
                                {
                                    ServiceId = serviceId2,
                                    Price = 500000 // 500,000
                                }
                            }
                        },
                        Quotations = new List<Quotation>() // No quotations
                    }
                }
            };

            _mockRepairOrderRepository
                .Setup(repo => repo.GetRepairOrderWithFullDetailsAsync(repairOrderId))
                .ReturnsAsync(repairOrder);

            _mockRepairOrderRepository
                .Setup(repo => repo.UpdateAsync(It.IsAny<RepairOrder>()))
                .ReturnsAsync((RepairOrder ro) => ro);

            // Act
            var result = await _repairOrderService.UpdateCostFromInspectionAsync(repairOrderId);

            // Assert
            Assert.AreEqual(1500000, result.Cost); // 1,000,000 + 500,000
            _mockRepairOrderRepository.Verify(repo => repo.UpdateAsync(It.IsAny<RepairOrder>()), Times.Once);
        }

        [TestMethod]
        public async Task UpdateCostFromInspectionAsync_WithCompletedInspectionWithApprovedQuotation_ShouldNotUpdateCost()
        {
            // Arrange
            var repairOrderId = Guid.NewGuid();
            var serviceId1 = Guid.NewGuid();

            var repairOrder = new RepairOrder
            {
                RepairOrderId = repairOrderId,
                Cost = 0,
                Inspections = new List<Inspection>
                {
                    new Inspection
                    {
                        InspectionId = Guid.NewGuid(),
                        RepairOrderId = repairOrderId,
                        Status = InspectionStatus.Completed,
                        ServiceInspections = new List<ServiceInspection>
                        {
                            new ServiceInspection
                            {
                                ServiceInspectionId = Guid.NewGuid(),
                                ServiceId = serviceId1,
                                Service = new Service
                                {
                                    ServiceId = serviceId1,
                                    Price = 1000000 // 1,000,000
                                }
                            }
                        },
                        Quotations = new List<Quotation>
                        {
                            new Quotation
                            {
                                QuotationId = Guid.NewGuid(),
                                Status = QuotationStatus.Approved,
                                TotalAmount = 1000000
                            }
                        }
                    }
                }
            };

            _mockRepairOrderRepository
                .Setup(repo => repo.GetRepairOrderWithFullDetailsAsync(repairOrderId))
                .ReturnsAsync(repairOrder);

            _mockRepairOrderRepository
                .Setup(repo => repo.UpdateAsync(It.IsAny<RepairOrder>()))
                .ReturnsAsync((RepairOrder ro) => ro);

            // Act
            var result = await _repairOrderService.UpdateCostFromInspectionAsync(repairOrderId);

            // Assert
            Assert.AreEqual(0, result.Cost); // Cost should remain 0 since there's an approved quotation
            _mockRepairOrderRepository.Verify(repo => repo.UpdateAsync(It.IsAny<RepairOrder>()), Times.Never);
        }

        [TestMethod]
        public async Task UpdateCostFromInspectionAsync_WithNoCompletedInspections_ShouldNotUpdateCost()
        {
            // Arrange
            var repairOrderId = Guid.NewGuid();

            var repairOrder = new RepairOrder
            {
                RepairOrderId = repairOrderId,
                Cost = 500000,
                Inspections = new List<Inspection>
                {
                    new Inspection
                    {
                        InspectionId = Guid.NewGuid(),
                        RepairOrderId = repairOrderId,
                        Status = InspectionStatus.InProgress, // Not completed
                        ServiceInspections = new List<ServiceInspection>(),
                        Quotations = new List<Quotation>()
                    }
                }
            };

            _mockRepairOrderRepository
                .Setup(repo => repo.GetRepairOrderWithFullDetailsAsync(repairOrderId))
                .ReturnsAsync(repairOrder);

            _mockRepairOrderRepository
                .Setup(repo => repo.UpdateAsync(It.IsAny<RepairOrder>()))
                .ReturnsAsync((RepairOrder ro) => ro);

            // Act
            var result = await _repairOrderService.UpdateCostFromInspectionAsync(repairOrderId);

            // Assert
            Assert.AreEqual(500000, result.Cost); // Cost should remain unchanged
            _mockRepairOrderRepository.Verify(repo => repo.UpdateAsync(It.IsAny<RepairOrder>()), Times.Never);
        }

        [TestMethod]
        public async Task UpdateCostFromInspectionAsync_WithMixedInspections_ShouldUpdateCostOnlyForInspectionsWithoutApprovedQuotations()
        {
            // Arrange
            var repairOrderId = Guid.NewGuid();
            var serviceId1 = Guid.NewGuid();
            var serviceId2 = Guid.NewGuid();
            var serviceId3 = Guid.NewGuid();

            var repairOrder = new RepairOrder
            {
                RepairOrderId = repairOrderId,
                Cost = 0,
                Inspections = new List<Inspection>
                {
                    // Completed inspection without quotation - should be included in cost calculation
                    new Inspection
                    {
                        InspectionId = Guid.NewGuid(),
                        RepairOrderId = repairOrderId,
                        Status = InspectionStatus.Completed,
                        ServiceInspections = new List<ServiceInspection>
                        {
                            new ServiceInspection
                            {
                                ServiceInspectionId = Guid.NewGuid(),
                                ServiceId = serviceId1,
                                Service = new Service
                                {
                                    ServiceId = serviceId1,
                                    Price = 1000000 // 1,000,000
                                }
                            }
                        },
                        Quotations = new List<Quotation>() // No quotations
                    },
                    // Completed inspection with approved quotation - should NOT be included in cost calculation
                    new Inspection
                    {
                        InspectionId = Guid.NewGuid(),
                        RepairOrderId = repairOrderId,
                        Status = InspectionStatus.Completed,
                        ServiceInspections = new List<ServiceInspection>
                        {
                            new ServiceInspection
                            {
                                ServiceInspectionId = Guid.NewGuid(),
                                ServiceId = serviceId2,
                                Service = new Service
                                {
                                    ServiceId = serviceId2,
                                    Price = 2000000 // 2,000,000
                                }
                            }
                        },
                        Quotations = new List<Quotation>
                        {
                            new Quotation
                            {
                                QuotationId = Guid.NewGuid(),
                                Status = QuotationStatus.Approved,
                                TotalAmount = 2000000
                            }
                        }
                    },
                    // In-progress inspection - should NOT be included in cost calculation
                    new Inspection
                    {
                        InspectionId = Guid.NewGuid(),
                        RepairOrderId = repairOrderId,
                        Status = InspectionStatus.InProgress,
                        ServiceInspections = new List<ServiceInspection>
                        {
                            new ServiceInspection
                            {
                                ServiceInspectionId = Guid.NewGuid(),
                                ServiceId = serviceId3,
                                Service = new Service
                                {
                                    ServiceId = serviceId3,
                                    Price = 3000000 // 3,000,000
                                }
                            }
                        },
                        Quotations = new List<Quotation>() // No quotations
                    }
                }
            };

            _mockRepairOrderRepository
                .Setup(repo => repo.GetRepairOrderWithFullDetailsAsync(repairOrderId))
                .ReturnsAsync(repairOrder);

            _mockRepairOrderRepository
                .Setup(repo => repo.UpdateAsync(It.IsAny<RepairOrder>()))
                .ReturnsAsync((RepairOrder ro) => ro);

            // Act
            var result = await _repairOrderService.UpdateCostFromInspectionAsync(repairOrderId);

            // Assert
            Assert.AreEqual(1000000, result.Cost); // Only the first inspection (1,000,000) should be included
            _mockRepairOrderRepository.Verify(repo => repo.UpdateAsync(It.IsAny<RepairOrder>()), Times.Once);
        }
    }
}