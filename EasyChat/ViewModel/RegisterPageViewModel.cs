using EasyChat.Model;
using EasyChat.Service;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace EasyChat.ViewModel
{
    public class RegisterPageViewModel
    {
        private IUserService _userService;
        private IWebService _webService;

        public RegisterPageViewModel(IUserService userService, IWebService webService)
        {
            _userService = userService;
            _webService = webService;
        }

        public IUserService GetUserService()
        {
            return _userService;
        }

        public IWebService GetWebService()
        {
            return _webService;
        }

        public async Task<bool> RegisterAsync(string name, string password)
        {
            bool connect_state = await _webService.BuiildConnectionAsync();
            if (connect_state == false)
            {
                return connect_state;
            }

            try
            {
                // 创建Json信息
                Send_LoginRegisterJson request = new Send_LoginRegisterJson("signin", name, password);
                // 将该信息解析为Json
                string json = JsonConvert.SerializeObject(request);
                // 发送
                await _webService.SendAsync(json);
                // 等待接收
                DateTime begin_time = DateTime.Now;
                DateTime now = DateTime.Now;
                string received_json = "";
                while ((now - begin_time).TotalSeconds < 30)
                {
                    await _webService.ReceiveAsync();
                    received_json = _webService.Get_RawData();
                    if (received_json == "")
                    {
                        now = DateTime.Now;
                        continue;
                    }
                    break;
                }
                if (received_json == "")
                {
                    _webService.End_Connection();
                    return false;
                }

                Receive_LoginRegisterJson receive_data = JsonConvert.DeserializeObject<Receive_LoginRegisterJson>(received_json);
                if (receive_data.state)
                {
                    _webService.End_Connection();
                    return true;
                }
                else
                {
                    _webService.End_Connection();
                    ContentDialog connectFailedDialog = new ContentDialog
                    {
                        Title = "RegisterFailed",
                        Content = receive_data.reason,
                        CloseButtonText = "Back"
                    };
                    await connectFailedDialog.ShowAsync();
                    return false;
                }

            }
            catch (Exception ex)
            {
                _webService.End_Connection();
                return false;
            }
        }
    }
}
