using EasyChat.Service;
using EasyChat.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
    public sealed partial class RegisterPage : Page
    {
        public RegisterPageViewModel viewModel { get; set; }

        public RegisterPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            viewModel = new RegisterPageViewModel(new UserService(""), new WebService());
            ApplicationView.GetForCurrentView().TryResizeView(new Size(400, 500));
        }

        private async void Register_Button_Click(object sender, RoutedEventArgs e)
        {
            if(Password.Password != Password_Confirm.Password)
            {
                ContentDialog registerFailedDialog = new ContentDialog
                {
                    Title = "Two password is not persistence",
                    Content = "Please input again",
                    CloseButtonText = "Back"
                };

                await registerFailedDialog.ShowAsync();
            }
            else
            {
                bool result = await viewModel.RegisterAsync(UserId.Text, Password.Password);
                if(result == true)
                {
                    ContentDialog registerFailedDialog = new ContentDialog
                    {
                        Title = "Register Success!",
                        Content = "We will go back to the Login Page",
                        CloseButtonText = "OK"
                    };

                    await registerFailedDialog.ShowAsync();
                }

                Frame.Navigate(typeof(LoginPage),viewModel.GetUserService());
            }
            Password.Password = "";
            UserId.Text = "";
            Password_Confirm.Password = "";
        }

        private void Back_Button_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(LoginPage));
        }
    }
}
