using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyChat.Model
{
    public class Message : INotifyPropertyChanged
    {
        private string _username;
        public string userName
        {
            get { return _username; }
            set
            {
                _username = value;
                OnPropertyChanged("username");
            }
        }
        private string _message;
        public string message
        {
            get { return _message; }
            set
            {
                _message = value;
                OnPropertyChanged("message");
            }
        }
        private DateTime _dateTime;
        public DateTime dateTime
        {
            get { return _dateTime; }
            set
            {
                _dateTime = value;
                OnPropertyChanged("dateTime");
            }
        }

        private bool _isItself;
        public bool isItself
        {
            get { return _isItself; }
            set
            {
                _isItself = value;
                OnPropertyChanged("isItself");
            }
        }

        public Message(string username, string message, bool isItself, DateTime time)
        {
            this._isItself = isItself;
            this._username = username;
            this._message = message;
            this._dateTime = time;
        }

        public Message()
        {

        }

        protected void OnPropertyChanged(string propertyName="")
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
