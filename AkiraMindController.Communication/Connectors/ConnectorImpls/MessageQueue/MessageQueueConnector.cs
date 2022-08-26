using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using static AkiraMindController.Communication.Connectors.CommonConnectorBase;

namespace AkiraMindController.Communication.Connectors.ConnectorImpls.MessageQueue
{
    /*
    public class MessageQueueConnector : CommonConnectorBase, ISendable
    {
        public MessageQueueConnector(string recvName, string sendName)
        {
            RecvName = recvName;
            SendName = sendName;
        }

        public string RecvName { get; }
        public string SendName { get; }

        private System.Messaging.MessageQueue recvQueue;
        private System.Messaging.MessageQueue sendQueue;
        private Thread thread;

        public void Start()
        {
            thread?.Abort();
            thread = new Thread(() =>
            {
                try
                {
                    using var recvQueue = new System.Messaging.MessageQueue(RecvName);
                    recvQueue.Formatter = new XmlMessageFormatter(new[] { typeof(Payload) });

                    while (true)
                    {
                        var msg = recvQueue.Receive();
                        var payload = (Payload)msg.Body;

                        Log.WriteLine($"[mq] recv payload.typeName : {payload.typeName}");
                        var type = Type.GetType(payload.typeName);
                        Log.WriteLine($"[mq] recv payloadStr : {type}");
                        var param = Json.Deserialize(payload.payloadJson, type);
                        Log.WriteLine($"[mq] recv payload.payloadJson : {payload.payloadJson}");

                        foreach (var handler in GetTypeHandlers(type))
                            handler.Handle(param);
                    }
                }
                catch (Exception e)
                {

                }
            })
            { IsBackground = true };
            thread.Start();

            sendQueue?.Dispose();
            sendQueue = new (SendName);
        }

        public void SendMessage<T>() where T : new() => SendMessage(new T());

        public void SendMessage(object obj)
        {
            var payload = new Payload()
            {
                payloadJson = Json.Serialize(obj),
                typeName = obj.GetType().FullName
            };

            sendQueue.Send(payload);
        }

        public void Stop()
        {
            thread.Abort();
            recvQueue?.Dispose();
        }

        public static MessageQueueConnector CreateA(string name) => new MessageQueueConnector(name + "_A", name + "_B");
        public static MessageQueueConnector CreateB(string name) => new MessageQueueConnector(name + "_B", name + "_A");
    }
    */
}
