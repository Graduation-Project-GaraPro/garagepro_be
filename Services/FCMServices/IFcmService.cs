using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.FCMServices
{
    public interface IFcmService
    {
        Task SendNotificationAsync(string deviceToken, string title, string body);
    }
}
