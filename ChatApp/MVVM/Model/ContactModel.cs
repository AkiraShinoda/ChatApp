using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.MVVM.Model
{
    class ContactModel
    {
        public string? Username { get; set; }
        public string? IconColor { get; set; }

        public ObservableCollection<MessageModel>? Messages { get; set; }
        public string? LastMessage
        {
            get
            {
                // Messages が空でないかチェックし、空であれば空文字列を返す
                return Messages != null && Messages.Any() ? Messages.Last().Message : "";
            }
        }
    }
}
