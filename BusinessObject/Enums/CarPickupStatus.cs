using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Enums
{
    public enum CarPickupStatus
    {
        None = 1,        // chưa có thông tin
        PickedUp = 2,    // khách đã lấy xe
        NotPickedUp = 3  // khách chưa lấy xe
    }
}
