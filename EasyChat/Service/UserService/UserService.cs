using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyChat.Model;
using Microsoft.EntityFrameworkCore;
using Windows.Storage;

namespace EasyChat.Service
{
    public class UserService : IUserService
    {
        public string currentUserName = "";

        public UserService(string username)
        {
            this.currentUserName = username;
        }

        public DateTime FromUnixTime(string ms)
        {
            DateTime time = new DateTime(1970, 1, 1);
            var temp = Convert.ToInt64(ms);
            time = time.AddMilliseconds(Convert.ToDouble(temp));
            return time;
        }

        public double ToUnixTime(DateTime time)
        {
            DateTime unix_time = new DateTime(1970, 1, 1);
            return Convert.ToInt64((time - unix_time).TotalMilliseconds);
        }

        private async Task<StorageFolder> OpenDataFolder()
        {
            // 寻找该文件夹中的文件
            Windows.Storage.StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            if(Directory.Exists(storageFolder.Path + "\\Data"))
            {
                return await storageFolder.GetFolderAsync("Data");
            }
            await storageFolder.CreateFolderAsync("Data");
            return await storageFolder.GetFolderAsync("Data");
        }

        public async Task<User> GetUserData(string username)
        {
            if(username == "")
            {
                return null;
            }
            else
            {
                User user = new User()
                {
                    username = username,
                    userConversations = new List<UserConversation>()
                };

                StorageFolder storageFolder = await OpenDataFolder();
                try
                {
                    // 打开用户文件夹 : 如果不存在，则创建一个
                    if (!Directory.Exists(storageFolder.Path + "\\" + username))
                    {
                        await storageFolder.CreateFolderAsync(username);
                    }
                    var userFolder = await storageFolder.GetFolderAsync(username);

                    // 读取所有的子文件夹名称 : conversation
                    IReadOnlyList<IStorageItem> itemsList = await userFolder.GetItemsAsync();
                    if(itemsList.Count == 0)
                    {
                        return user;
                    }
                    else
                    {
                        foreach(var item in itemsList)
                        {
                            var conversation = new UserConversation()
                            {
                                ifGroup = true,
                                displayName = item.Name,
                                name = new List<string>(),
                                messages = new List<Message>()
                            };
                            // 打开文件夹 : 文件夹中有两类文件，一类是detail.txt(存放用户信息)，另一类是时间戳.txt
                            var conversationFolder = await userFolder.GetFolderAsync(item.Name);
                            // 读取 detail.txt
                            StorageFile userdetail = await conversationFolder.GetFileAsync("detail.txt");
                            string text = await Windows.Storage.FileIO.ReadTextAsync(userdetail);
                            for(int end = 0,begin = 0;end < text.Length; end++)
                            {
                                if(text[end] == '\n')
                                {
                                    conversation.name.Add(text.Substring(begin, end-begin));
                                    begin = end + 1;
                                    end = end + 1;
                                }
                            }
                            if(conversation.name.Count > 1)
                            {
                                conversation.ifGroup = false;
                            }

                            // 逐个读取Message，进行排序
                            IReadOnlyList<IStorageItem> messagesList = await conversationFolder.GetItemsAsync();
                            foreach(var messageItem in messagesList)
                            {
                                if(messageItem.Name == "detail.txt")
                                {
                                    continue;
                                }
                                DateTime time = FromUnixTime(messageItem.Name.Substring(0, messageItem.Name.Length - 4));
                                var message = new Message()
                                {
                                    dateTime = time,
                                };

                                StorageFile messageData = await conversationFolder.GetFileAsync(messageItem.Name);
                                string messageText = await Windows.Storage.FileIO.ReadTextAsync(messageData);
                                for(int i = messageText.Length - 1;i >= 0; i--)
                                {
                                    if(messageText[i] == '\n')
                                    {
                                        message.message = messageText.Substring(0, i);
                                        message.userName = messageText.Substring(i + 1, messageText.Length - i - 1);
                                        message.isItself = (message.userName == user.username);
                                        break;
                                    }
                                }

                                conversation.messages.Add(message);
                            }

                            // 添加信息
                            user.userConversations.Add(conversation);
                        }
                        return user;
                    }
                }
                catch (Exception e)
                {
                    return null;
                }
            }
        }

        public async Task<ObservableCollection<User>> ListUserAsync()
        {
            ObservableCollection<User> result = new ObservableCollection<User>();
            StorageFolder storageFolder = await OpenDataFolder();
            IReadOnlyList<IStorageItem> itemsList = await storageFolder.GetItemsAsync();
            if(itemsList.Count != 0)
            {
                foreach(var user in itemsList)
                {
                    result.Add(await GetUserData(user.Name));
                }
            }

            return result;
        }

        public async Task<bool> RemoveUserAsync(string username)
        {
            StorageFolder storageFolder = await OpenDataFolder();
            IReadOnlyList<IStorageItem> itemsList = await storageFolder.GetItemsAsync();
            foreach(var item in itemsList)
            {
                if(item.Name == username)
                {
                    var folder = await storageFolder.GetFolderAsync(item.Name);
                    await folder.DeleteAsync();
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> SetUserDataAsync(User user)
        {
            if (user.username == "")
            {
                return false;
            }
            else
            {
                try
                {
                    await RemoveUserAsync(user.username);
                    await AddUserAsync(user);
                    return true;
                }
                catch (FileNotFoundException e)
                {
                    return false;
                }
            }
        }

        public string GetCurrentUserName()
        {
            return currentUserName;
        }

        public bool SetCurrentUserName(string userName)
        {
            currentUserName = userName;
            return true;
        }

        public async Task<bool> InitializeTest()
        {
            User userAdding = await GetUserData(currentUserName);
            if(userAdding != null)
            {
                return true;
            }

            List<Message> messages = new List<Message>();
            messages.Add(new Message()
            {
                dateTime = DateTime.Now,
                userName = currentUserName,
                isItself = true,
                message = "How do you do."

            });
            messages.Add(new Message()
            {
                dateTime = DateTime.Now.AddSeconds(1),
                userName = "test_user",
                isItself = false,
                message = "I am fine, thank you."
            });
            messages.Add(new Message()
            {
                dateTime = DateTime.Now.AddSeconds(2),
                userName = "test_user",
                isItself = false,
                message = "And you?"
            });
            messages.Add(new Message()
            {
                dateTime = DateTime.Now.AddSeconds(3),
                userName = currentUserName,
                isItself = false,
                message = "I am fine too, thanks."
            });

            List<string> name = new List<string>();
            name.Add("test_user");
            List<UserConversation> userConversations = new List<UserConversation>();
            userConversations.Add(new UserConversation()
            {
                displayName = "test_user",
                name = name,
                messages = messages
            });

            User user = new User()
            {
                username = currentUserName,
                password = "admin",
                userConversations = userConversations
            };

            return await AddUserAsync(user);
        }

        public async Task<bool> AddUserAsync(User user)
        {
            if(user.username == "")
            {
                return false;
            }
            await RemoveUserAsync(user.username);
            StorageFolder storageFolder = await OpenDataFolder();
            // 创建用户文件夹
            await storageFolder.CreateFolderAsync(user.username);

            StorageFolder userFolder = await storageFolder.GetFolderAsync(user.username);
            foreach(var conversation in user.userConversations)
            {
                // 创建会话文件夹
                await userFolder.CreateFolderAsync(conversation.displayName);
                StorageFolder conversationFolder = await userFolder.GetFolderAsync(conversation.displayName);

                // 保存detail.txt信息
                string detail_text = "";
                for(int i = 0;i < conversation.name.Count; i++)
                {
                    detail_text += conversation.name[i];
                    detail_text += "\n";
                }
                await conversationFolder.CreateFileAsync("detail.txt");
                StorageFile detail = await conversationFolder.GetFileAsync("detail.txt");
                await Windows.Storage.FileIO.WriteTextAsync(detail, detail_text);

                //保存message信息
                foreach(var message in conversation.messages)
                {
                    string name = ToUnixTime(message.dateTime).ToString();
                    string message_text = "";
                    message_text += message.message;
                    message_text += "\n";
                    message_text += message.userName;

                    await conversationFolder.CreateFileAsync(name + ".txt");
                    StorageFile file = await conversationFolder.GetFileAsync(name + ".txt");
                    await Windows.Storage.FileIO.WriteTextAsync(file, message_text);
                }
            }
            return true;
        }

        public async Task<bool> AddMessageAsync(string username, string conversation_name, Message message)
        {
            bool result = true;
            if(username == "")
            {
                return false;
            }
            else
            {
                // 打开用户对应的会话文件夹
                var dataFolder = await OpenDataFolder();
                if (!Directory.Exists(dataFolder.Path + "\\" + username))
                {
                    await dataFolder.CreateFolderAsync(username);
                }
                var userFolder = await dataFolder.GetFolderAsync(username);
                if (!Directory.Exists(userFolder.Path + "\\" + conversation_name))
                {
                    result = false;
                    await userFolder.CreateFolderAsync(conversation_name);
                }
                var conversationFolder = await userFolder.GetFolderAsync(conversation_name);

                // 创建(时间戳).txt 文件
                // 先检查是否存在，如果存在，以最新的为准
                string fileName = Convert.ToInt64(ToUnixTime(message.dateTime)).ToString() + ".txt";
                FileInfo file = new FileInfo(conversationFolder.Path + "\\" + fileName);
                if (file.Exists)
                {
                    file.Delete();
                }
                await conversationFolder.CreateFileAsync(fileName);

                // 打开文件，写入
                string message_text = "";
                message_text += message.message;
                message_text += "\n";
                message_text += message.userName;
                StorageFile messageFile = await conversationFolder.GetFileAsync(fileName);
                await Windows.Storage.FileIO.WriteTextAsync(messageFile, message_text);
                
                return result;
            }
        }

        public string GetDisplayName(List<string> name)
        {
            string result = "";
            int endIndex = (name.Count >= 3) ? 3 : name.Count;
            for(int i = 0;i < endIndex;i++)
            {
                result += name[i];
                result += ", ";
            }
            
            result = result.Substring(0, result.Length - 2);

            if (endIndex < name.Count)
            {
                result += "...";
            }

            return result;
        }

        public async Task<bool> AddDetailAsync(string username, string folderName, List<string> member)
        {
            if (username == "")
            {
                return false;
            }
            else
            {
                // 打开用户对应的会话文件夹
                var dataFolder = await OpenDataFolder();
                if (!Directory.Exists(dataFolder.Path + "\\" + username))
                {
                    await dataFolder.CreateFolderAsync(username);
                }
                var userFolder = await dataFolder.GetFolderAsync(username);
                if (!Directory.Exists(userFolder.Path + "\\" + folderName))
                {
                    await userFolder.CreateFolderAsync(folderName);
                }
                var conversationFolder = await userFolder.GetFolderAsync(folderName);

                // 创建Detail.txt
                // 保存detail.txt信息
                string detail_text = "";
                for (int i = 0; i < member.Count; i++)
                {
                    detail_text += member[i];
                    detail_text += "\n";
                }
                FileInfo file = new FileInfo(conversationFolder.Path + "detail.txt");
                if (!file.Exists)
                {
                    await conversationFolder.CreateFileAsync("detail.txt");
                    StorageFile detail = await conversationFolder.GetFileAsync("detail.txt");
                    await Windows.Storage.FileIO.WriteTextAsync(detail, detail_text);
                }
            }
            return true;
        }
    }
}
