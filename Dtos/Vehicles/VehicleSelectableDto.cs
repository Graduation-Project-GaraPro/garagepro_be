using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Vehicles
{
    public enum VehicleBookingState
    {
        Available = 0,   // chọn được
        InGarage = 1,    // có RepairOrder IsArchived = false (xe đang nằm xưởng, KHÔNG chọn)
        PickedUp = 2     // có RepairOrder IsArchived = true (đã lấy xe, chọn được)
    }

    public class VehicleSelectableDto
    {
        public Guid VehicleId { get; set; }

        public string LicensePlate { get; set; } = "";
        public string? VIN { get; set; }

        public int Year { get; set; }
        public long? Odometer { get; set; }

        public DateTime LastServiceDate { get; set; }
        public DateTime? NextServiceDate { get; set; }

        public string? WarrantyStatus { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Guid BrandId { get; set; }
        public Guid ModelId { get; set; }
        public Guid ColorId { get; set; }

        // Display names
        public string BrandName { get; set; } = "";
        public string ModelName { get; set; } = "";
        public string ColorName { get; set; } = "";

        // flags
        public bool HasActiveRepairRequest { get; set; }
        public bool HasOpenRepairOrder { get; set; }
        public bool HasArchivedRepairOrder { get; set; }

        public bool IsSelectable { get; set; }
        public VehicleBookingState State { get; set; }
    }


}
