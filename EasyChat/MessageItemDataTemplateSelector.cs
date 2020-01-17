using EasyChat.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace EasyChat
{
    public class MessageItemDataTemplateSelector : DataTemplateSelector
    {
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
                if ((item as Message).isItself)
                {
                    return App.Current.Resources["SelfMessageDataTemplate"] as DataTemplate;
                }
                else
                {
                    return App.Current.Resources["MessageDataTemplate"] as DataTemplate;
                }
         
        }
    }
}
