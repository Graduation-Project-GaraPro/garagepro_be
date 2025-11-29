using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Enums
{
    public enum FcmDataKey
    {
        Type,
        entityId,
        Screen
    }

    public enum NotificationType
    {
        Order,
        Repair,
        Message,
        System
    }

    public enum AppScreen
    {
        QuotationDetailFragment,
        RepairProgressDetailFragment,
        RepairOrderArchivedDetailFragment,
        HomeFragment
    }
}
