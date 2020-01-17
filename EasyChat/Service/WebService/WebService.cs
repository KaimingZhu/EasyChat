using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.UI.Xaml.Controls;

namespace EasyChat.Service
{
    public class WebService : IWebService
    {
        private StreamSocket socket;
        private List<string> ip_address;
        private List<string> port;
        private string raw_data;
        private bool ifConnected;

        public WebService()
        {
            socket = new StreamSocket();
            socket.Control.KeepAlive = true;
            ip_address = new List<string>();
            ip_address.Add("192.168.1.4");
            port = new List<string>();
            port.Add("6666");
            raw_data = "";
            ifConnected = false;
        }

        /// <summary>
        /// 建立连接
        /// </summary>
        /// <returns>成功/失败</returns>
        public async Task<bool> BuiildConnectionAsync()
        {
            bool result = false;
            List<string> error = new List<string>();
            for(int i = 0;i < ip_address.Count; i++)
            {
                try
                {
                    await this.socket.ConnectAsync(new HostName(ip_address[i]), port[i]);
                    result = true;
                    break;
                }
                catch(Exception ex)
                {
                    SocketErrorStatus errorStatus = Windows.Networking.Sockets.SocketError.GetStatus(ex.GetBaseException().HResult);
                    error.Add(errorStatus.ToString() != "Unknown" ? errorStatus.ToString() : ex.Message);
                    continue;
                }
            }
            if(result == false)
            {
                ContentDialog connectFailedDialog = new ContentDialog
                {
                    Title = "ConnectionFailed",
                    Content = "",
                    CloseButtonText = "Ok"
                };

                for(int i = 0;i < error.Count; i++)
                {
                    connectFailedDialog.Content += ("Time " + i.ToString() + ": " + error[i] + ".\n");
                }

                connectFailedDialog.Content += "\n Please check your connection status and try again later.";
                await connectFailedDialog.ShowAsync();
            }

            ifConnected = result;
            return result;
        }

        /// <summary>
        /// 接收信息
        /// </summary>
        /// <param name="data"></param>
        /// <returns>成功/失败</returns>
        public async Task<bool> ReceiveAsync()
        {
            if (!ifConnected)
            {
                return false;
            }

            Stream inputStream = socket.InputStream.AsStreamForRead();
            StreamReader streamReader = new StreamReader(inputStream);
            this.raw_data = await streamReader.ReadLineAsync();

            return true;
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <returns>成功/失败</returns>
        public async Task<bool> SendAsync(string request)
        {
            if (!ifConnected)
            {
                return false;
            }

            Stream outputStream = socket.OutputStream.AsStreamForWrite();
            var streamWriter = new StreamWriter(outputStream);
            streamWriter.AutoFlush = true;
            await streamWriter.WriteLineAsync(request);
            
            return true;
        }

        /// <summary>
        /// 终止连接
        /// </summary>
        /// <returns></returns>
        public bool End_Connection()
        {
            socket.Dispose();
            ifConnected = false;
            socket = new StreamSocket();
            return true;
        }

        /// <summary>
        /// 获得未序列化的Json数据
        /// </summary>
        /// <returns></returns>
        public string Get_RawData()
        {
            var temp = raw_data;
            raw_data = "";
            return temp;
        }

        public bool GetConnectStatus()
        {
            return ifConnected;
        }
    }
}
