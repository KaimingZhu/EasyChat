using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyChat.Model;

namespace EasyChat.Service
{
    public interface IUserService
    {
        DateTime FromUnixTime(string ms);

        double ToUnixTime(DateTime time);

        Task<bool> AddUserAsync(User newUser);

        Task<User> GetUserData(string username);

        Task<bool> SetUserDataAsync(User user);

        Task<bool> RemoveUserAsync(string username);

        Task<bool> AddMessageAsync(string username, string conversation_name, Message message);
        Task<ObservableCollection<User>> ListUserAsync();
        string GetCurrentUserName();

        bool SetCurrentUserName(string userName);

        Task<bool> InitializeTest();

        string GetDisplayName(List<string> name);
        Task<bool> AddDetailAsync(string userName, string folderName, List<string> member);
    }
}
