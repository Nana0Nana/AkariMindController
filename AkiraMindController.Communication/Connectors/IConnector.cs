using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AkiraMindController.Communication.Connectors
{
    public interface IConnector
    {
        delegate void OnReceviceMessageFunc<T>(T message, IResponser responser);

        void RegisterMessageHandler<T>(OnReceviceMessageFunc<T> handler);
        void UnregisterMessageHandler<T>(OnReceviceMessageFunc<T> handler);
        void UnregisterSpecifyMessageAllHandler<T>();
        void UnregisterAllMessageHandler();
    }
}
