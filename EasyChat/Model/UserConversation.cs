using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyChat.Model
{
    public class UserConversation : INotifyPropertyChanged
    {
        private string _id;
        public string Id
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChanged("id");
            }
        }

        private bool _ifGroup;
        public bool ifGroup
        {
            get { return _ifGroup; }
            set
            {
                _ifGroup= value;
                OnPropertyChanged("ifGroup");
            }
        }

        private string _displayName;
        public string displayName
        {
            get { return _displayName; }
            set
            {
                _displayName = value;
                OnPropertyChanged("displayName");
            }
        }

        private List<string> _name;
        public List<string> name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("name");
            }
        }

        private List<Message> _messages;
        public List<Message> messages
        {
            get { return _messages; }
            set
            {
                _messages = value;
                OnPropertyChanged("messages");
            }
        }

        public UserConversation(string id, bool ifGroup, string displayName, List<string> name, List<Message> messages)
        {
            this._id = id;
            this._ifGroup = ifGroup;
            this._displayName = displayName;
            this._messages = messages;
            this._name = name;
        }

        public UserConversation()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName = "")
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }

}

