using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyChat.Model;
using EasyChat.Service;
using GalaSoft.MvvmLight.Messaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EasyChat.ViewModel
{
    public class MainPageViewModel
    {

        private IUserService _userService;
        private IWebService _webService;

        public MainPageViewModel(IUserService userService)
        {
            _userService = userService;
            _webService = new WebService();
        }

        public async Task<ObservableCollection<UserConversation>> GetUserConversationAsync()
        {
            var name = _userService.GetCurrentUserName();
            if (name == "")
            {
                return new ObservableCollection<UserConversation>();
            }
            var data = await _userService.GetUserData(name);
            var userConversations = new ObservableCollection<UserConversation>();
            if(data.userConversations != null)
            {
                foreach (UserConversation uc in data.userConversations)
                {
                    userConversations.Add(uc);
                }
            }
            return userConversations;
        }

        public IUserService GetUserService()
        {
            return _userService;
        }

        public async Task SendMessageAsync(Send_SendMessageJson sendJson)
        {
            // 转变为Json
            string json = JsonConvert.SerializeObject(sendJson);
            // 发送信息
            // 建立连接
            if (!_webService.GetConnectStatus())
            {
                await _webService.BuiildConnectionAsync();
            }
            // 发送信息
            await _webService.SendAsync(json);
        }

        public async Task SendMessageAsync(Send_ReadMessageJson readJson)
        {
            // 转变为Json
            string json = JsonConvert.SerializeObject(readJson);
            // 发送信息
            // 建立连接
            if (!_webService.GetConnectStatus())
            {
                await _webService.BuiildConnectionAsync();
            }
            // 发送信息
            await _webService.SendAsync(json);
        }

        public async Task AddMessageAsync(UserConversation chosenConversation, string message_owner, string message_send, DateTime sendTime)
        {
            string folderName = chosenConversation.displayName;
            Message message = new Message()
            {
                message = message_send,
                dateTime = sendTime,
                userName = message_owner
            };
            await _userService.AddMessageAsync(_userService.GetCurrentUserName(), folderName, message);
        }

        public async Task<Receive_ReadJson> ReadMessageAsync()
        {
            // 读取信息，直到有响应/超过一段时间后再结束
            if (!_webService.GetConnectStatus())
            {
                await _webService.BuiildConnectionAsync();
            }
            string data_received = _webService.Get_RawData();
            DateTime beginTime = DateTime.Now;
            while(data_received == "" && ((DateTime.Now - beginTime).TotalSeconds < 10))
            {
                await _webService.ReceiveAsync();
                data_received = _webService.Get_RawData();
            }

            // 解析数据,返回
            Receive_ReadJson receive_ReadJson = new Receive_ReadJson() {
                jsonMessages = new List<JsonMessage>()
            };
            JObject jObject = JObject.Parse(data_received);
            receive_ReadJson.state = (bool)jObject["state"];
            JArray jarray = JArray.Parse(jObject["messages"].ToString());
            for(int i = 0; i < jarray.Count; i++)
            {
                JObject messageObject = JObject.Parse(jarray[i].ToString());
                JsonMessage message = new JsonMessage() {
                    room = new List<string>()
                };
                message.from = (string)messageObject["from0"];
                message.message = (string)messageObject["message"];
                message.time = (string)messageObject["time"];
                JArray nameArray = JArray.Parse(messageObject["room"].ToString());
                for (int j = 0; j < nameArray.Count; j++)
                {
                    message.room.Add(nameArray[j].ToString());
                }
                receive_ReadJson.jsonMessages.Add(message);
            }
            return receive_ReadJson;
        }

        public async Task SaveMessgaeAsync(Receive_ReadJson receivedJson)
        {
            // 将返回的信息提出，逐个保存
            if (receivedJson.jsonMessages != null)
            {
                foreach (var item in receivedJson.jsonMessages)
                {
                    Message message = new Message()
                    {
                        dateTime = _userService.FromUnixTime(item.time),
                        message = item.message,
                        userName = item.from
                    };

                    List<string> member = new List<string>();
                    foreach (var friend in item.room)
                    {
                        if (friend != _userService.GetCurrentUserName())
                        {
                            member.Add(friend);
                        }
                    }
                    string folderName = _userService.GetDisplayName(member);

                    // 保存信息，如果不存在该会话，添加Detail.txt
                    bool result = await _userService.AddMessageAsync(_userService.GetCurrentUserName(), folderName, message);
                    if (!result)
                    {
                        await _userService.AddDetailAsync(_userService.GetCurrentUserName(), folderName, member);
                    }
                }
            }
        }

        public async Task AddConversationAsync(string name)
        {
            // 添加单人会话
            List<string> nameList = new List<string>();
            nameList.Add(name);
            await _userService.AddDetailAsync(_userService.GetCurrentUserName(), name, nameList);
        }

        public async Task AddConversationAsync(List<string> name)
        {
            await _userService.AddDetailAsync(_userService.GetCurrentUserName(), _userService.GetDisplayName(name), name);
        }
    }
}
