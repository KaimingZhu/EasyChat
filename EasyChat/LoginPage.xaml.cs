using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using EasyChat.Service;
using EasyChat.ViewModel;
using Microsoft.EntityFrameworkCore;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace EasyChat
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LoginPage : Page
    {

        public LoginPageViewModel viewModel { get; set; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (!Application.Current.Resources.ContainsKey("HasBeenNavigatedTo"))
            {
                viewModel = new LoginPageViewModel(new UserService(""), new WebService());
                ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(400, 500));
                Application.Current.Resources.Add("HasBeenNavigatedTo", true);
            }
            else
            {
                var userService = (UserService)e.Parameter;
                viewModel = new LoginPageViewModel(userService,new WebService());
                ApplicationView.GetForCurrentView().TryResizeView(new Size(400, 500));
            }
        }

        public LoginPage()
        {
            this.InitializeComponent();
        }

        private async void Login_Button_ClickAsync(object sender, RoutedEventArgs e)
        {
            // TODO : 访问服务器，请求信息
            bool result = await viewModel.LoginAsync(UserId.Text, Password.Password);
            //bool result = true;
            if (result)
            {
                // 如果正确，则继续跳转
                this.viewModel.GetUserService().SetCurrentUserName(UserId.Text);
                Frame.Navigate(typeof(MainPage), this.viewModel.GetUserService());
            }
        }

        private void Register_Button_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(RegisterPage));
        }
    }
}
