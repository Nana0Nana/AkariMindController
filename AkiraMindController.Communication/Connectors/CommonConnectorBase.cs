using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AkiraMindController.Communication.Connectors
{
    public abstract class CommonConnectorBase : IConnector
    {
        [Serializable]
        internal protected class Payload
        {
            public string typeName;
            public string payloadJson;
        }

        private Dictionary<Type, List<RegisterHandler>> registeredHandlers = new();

        public void RegisterMessageHandler<T>(IConnector.OnReceviceMessageFunc<T> handler)
        {
            var type = typeof(T);
            if (!registeredHandlers.TryGetValue(type, out var list))
            {
                list = new();
                registeredHandlers[type] = list;
            }

            if (list.Any(x => x.Check(handler)))
            {
                //it already has been added.
                return;
            }

            list.Add(new RegisterHandler<T>(handler));
            Log.WriteLine($"Registered message handler : {typeof(T).FullName} , id : {handler?.GetHashCode()}");
        }

        public void UnregisterMessageHandler<T>(IConnector.OnReceviceMessageFunc<T> handler)
        {
            var type = typeof(T);
            if (!registeredHandlers.TryGetValue(type, out var list))
                return;

            if (list.FirstOrDefault(x => x.Check(handler)) is RegisterHandler refHndler)
            {
                list.Remove(refHndler);
                Log.WriteLine($"Unregistered message handler : {typeof(T).FullName} , id : {handler?.GetHashCode()}");
            }
        }

        public void UnregisterSpecifyMessageAllHandler<T>()
        {
            var type = typeof(T);
            if (!registeredHandlers.TryGetValue(type, out var list))
                return;

            list.Clear();
            Log.WriteLine($"Unregistered specify message handlers : {typeof(T).FullName}");
        }

        public void UnregisterAllMessageHandler()
        {
            registeredHandlers.Clear();
            Log.WriteLine($"Unregistered all message handlers");
        }

        public IEnumerable<RegisterHandler> GetTypeHandlers<T>() => GetTypeHandlers(typeof(T));
        public IEnumerable<RegisterHandler> GetTypeHandlers(Type type) => registeredHandlers.TryGetValue(type, out var list) ? list : Enumerable.Empty<RegisterHandler>();
    }
}
