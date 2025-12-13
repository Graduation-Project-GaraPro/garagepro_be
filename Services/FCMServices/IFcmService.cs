using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.FcmDataModels;

namespace Services.FCMServices
{
    public interface IFcmService
    {
        Task SendFcmMessageAsync(string deviceToken, FcmDataPayload payload);
        Task SendFcmMessageWithDataAsync(string deviceToken, FcmDataPayload payload);

    }
}
