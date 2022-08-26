using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AkiraMindController.Communication.Connectors
{
    public interface ISendable
    {
        void SendMessage(object obj);
        void SendMessage<T>() where T : new();

        X SendMessageWithResponse<T, X>() where T : new() where X : new();
    }
}
