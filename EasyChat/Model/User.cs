using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyChat.Model
{
    public class User
    {
        public string username { get; set; }
        public string password { get; set; }

        public List<UserConversation> userConversations { get; set; }
    }
}
