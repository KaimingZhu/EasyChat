using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyChat.Service
{
    public interface IWebService
    {
        Task<bool> BuiildConnectionAsync();
        Task<bool> ReceiveAsync();
        Task<bool> SendAsync(string request);
        bool End_Connection();
        string Get_RawData();
        bool GetConnectStatus();

    }
}
