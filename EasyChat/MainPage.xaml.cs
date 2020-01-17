using EasyChat.Model;
using EasyChat.Service;
using EasyChat.ViewModel;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace EasyChat
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private List<string> AddGroup_NameList { get; set; }

        public ObservableCollection<UserConversation> ConversationCollections { get; set; }

        public UserConversation ChosenConversation { get; set; }

        public ObservableCollection<Message> ChosenMessages { get; set; }

        private bool ifFinished;

        private DispatcherTimer timer;

        public MainPageViewModel viewModel { get; set; }

        private string PortNumber = "6666";

        public string title = "Zyuuuu";

        public MainPage()
        {
            ConversationCollections = new ObservableCollection<UserConversation>();
            ChosenMessages = new ObservableCollection<Message>();
            ChosenConversation = new UserConversation();
            timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 3) };
            AddGroup_NameList = new List<string>();

            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            // 初始化
            var userService = (UserService)e.Parameter;
            viewModel = new MainPageViewModel(userService);
            await RefreshCollectionAsync();
            ifFinished = false;
            ConversationTitle.Text = "Welcome, " + userService.currentUserName;
            ApplicationView.GetForCurrentView().TryResizeView(new Size(800, 500));            
        }

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {

        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {

        }

        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {

        }

        private async void FriendListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var temp = (UserConversation)e.ClickedItem;
            ChosenConversation = new UserConversation()
            {
                displayName = temp.displayName,
                Id = temp.Id,
                ifGroup = temp.ifGroup,
                name = new List<string>(),
                messages = new List<Message>()
            };

            ChosenMessages.Clear();
            for(int i = 0;i < temp.name.Count; i++)
            {
                ChosenConversation.name.Add(temp.name[i]);
            }
            for (int i = 0; i < temp.messages.Count; i++)
            {
                ChosenConversation.messages.Add(temp.messages[i]);
                ChosenMessages.Add(new Message(temp.messages[i].userName, (string)temp.messages[i].message, temp.messages[i].isItself, temp.messages[i].dateTime));
            }
            await RefreshMessageListAsync();

            ConversationTitle.Text = ChosenConversation.displayName;
        }

        private void Logout_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Logout
            var temp = viewModel.GetUserService();
            temp.SetCurrentUserName("");

            //等待线程关闭
            ifFinished = true;
            timer.Stop();

            ApplicationView.GetForCurrentView().TryResizeView(new Size(400, 500));
            Frame.Navigate(typeof(LoginPage),temp);
        }

        private async void Send_Button_ClickAsync(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if(MessageBox.Text != "")
            {
                // 构造Json
                DateTime sendTime = DateTime.Now;
                string message_send = MessageBox.Text;
                Send_SendMessageJson dataSend = new Send_SendMessageJson()
                {
                    action = "send",
                    from = viewModel.GetUserService().GetCurrentUserName(),
                    to = new List<string>(),
                    message = message_send
                };
                for(int i = 0;i < ChosenConversation.name.Count; i++)
                {
                    dataSend.to.Add(ChosenConversation.name[i]);
                }

                // 发送
                await viewModel.SendMessageAsync(dataSend);

                //保存数据
                await viewModel.AddMessageAsync(ChosenConversation, viewModel.GetUserService().GetCurrentUserName(), message_send, sendTime);

                //更新信息列表
                await RefreshMessageListAsync();
            }
        }

        private async Task RefreshCollectionAsync()
        {
            ConversationCollections.Clear();
            var temp = await viewModel.GetUserConversationAsync();
            for (int i = 0; i < temp.Count; i++)
            {
                ConversationCollections.Add(new UserConversation(temp[i].Id, temp[i].ifGroup, temp[i].displayName, temp[i].name, temp[i].messages));
            }
        }

        private async Task RefreshMessageListAsync()
        {
            var temp = await viewModel.GetUserConversationAsync();
            for(int i = 0;i < temp.Count; i++)
            {
                if(temp[i].displayName == ChosenConversation.displayName)
                {
                    ChosenMessages.Clear();
                    foreach(var item in temp[i].messages)
                    {
                        ChosenMessages.Add(item);
                    }
                    break;
                }
            }
        }

        public async Task<Task> RefreshThread()
        {
            return Task.Run( async () =>
            {
                    // 发送读取请求
                    Send_ReadMessageJson readJson = new Send_ReadMessageJson("read",viewModel.GetUserService().GetCurrentUserName());
                    await viewModel.SendMessageAsync(readJson);
                    // 获得读取信息
                    Receive_ReadJson receivedJson = await viewModel.ReadMessageAsync();

                    if (receivedJson.jsonMessages == null)
                    {
                        // 如果没有更新，则保持原状
                    }
                    else
                    {
                        // 如果有更新
                        this.Invoke(async () =>
                        {
                            // 保存数据
                            await viewModel.SaveMessgaeAsync(receivedJson);
                            // 刷新
                            await RefreshCollectionAsync();
                            await RefreshMessageListAsync();
                        });
                    }
            });
        }

        public async void Invoke(Action action, Windows.UI.Core.CoreDispatcherPriority Priority = Windows.UI.Core.CoreDispatcherPriority.Normal)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Priority, () => { action(); });
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            runTimePicker();
        }

        private void runTimePicker()
        {
            timer.Tick += new EventHandler<object>(async (sende, ei) =>
            {
                   await Dispatcher.TryRunAsync
                   (CoreDispatcherPriority.Normal, new DispatchedHandler( async () =>
                   {
                       await RefreshThread();
                   }));
            });

            timer.Start();
        }

        private async void AddFriend_Confirm_Click(object sender, RoutedEventArgs e)
        {
            // 添加好友
            string name = AddFriend_userId.Text;
            if(name != "")
            {
                // 保存Conversation信息
                await viewModel.AddConversationAsync(name);

                // 更新页面
                await RefreshCollectionAsync();
            }
        }

        private void AddGroup_Confirm_Click(object sender, RoutedEventArgs e)
        {
            if(AddGroup_userId.Text != "")
            {
                AddGroup_NameList.Add(AddGroup_userId.Text);
                AddGroup_userId.Text = "";
            }
        }

        private async void AddGroup_Commit_Click(object sender, RoutedEventArgs e)
        {
            if (AddGroup_NameList.Count != 0)
            {
                // 保存Conversation信息
                await viewModel.AddConversationAsync(AddGroup_NameList);

                // 清空数组
                AddGroup_NameList.Clear();

                // 更新页面
                await RefreshCollectionAsync();
            }
        }
    }
}
