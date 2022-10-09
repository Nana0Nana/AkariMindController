using AkiraMindController.Communication.Bases;
using AkiraMindController.Communication.Utils;
using System;
using System.Net;
using System.Text;
using System.Threading;

namespace AkiraMindController.Communication.Connectors.ConnectorImpls.Http
{
    public class HttpConnectorServer : CommonConnectorBase
    {
        HttpListener server;
        private Thread thread;

        public bool IsConnectingAvailable => server.IsListening;

        public HttpConnectorServer(int port = 28570)
        {
            server = new HttpListener();
            server.Prefixes.Add($"http://*:{port}/");
        }

        private void ProcessRequest(HttpListenerContext r)
        {
            Log.WriteLine($"[server] path : {r.Request.Url.LocalPath}");
            var param = MessageContentPacker.DeserializeFromPayloadString(r.Request.QueryString["payload"]);
            var responser = new HttpConnectorResponser(r.Response.OutputStream);

            foreach (var handler in GetTypeHandlers(param.GetType()))
                handler.Handle(param, responser);

            r.Response.StatusCode = 200;
            r.Response.OutputStream.Close();
            r.Response.Close();
        }

        public void Start()
        {
            Log.WriteLine($"TEST : {Json.Serialize(new AutoFaderTarget()
            {
                bellRanges = new ValueRange[] { new(1, 4), new(5, 6) },
                damageRanges = new ValueRange[] { new(2, 5), new(6, 7) },
                targetRanges = new ValueRange[] { new(3, 6), new(7, 8) },
                moveableRange = new ValueRange(100, 600),
                finalTargetFrame = 2857,
                finalTargetPlace = 1234
            })}");

            server.Start();
            thread?.Abort();
            thread = new Thread(() =>
            {
                while (server.IsListening)
                {
                    try
                    {
                        Log.WriteLine($"[server] ");
                        Log.WriteLine($"[server] waiting new request...");
                        var r = server.GetContext();
                        ProcessRequest(r);
                        Log.WriteLine($"[server] request processed");
                    }
                    catch (Exception e)
                    {
                        Log.WriteLine($"[server] request process throw exception : {e.Message} StackTrace : {Convert.ToBase64String(Encoding.UTF8.GetBytes(e.StackTrace))}");
                    }
                }
            })
            { IsBackground = true };
            thread.Start();
        }

        public void Stop()
        {
            server.Stop();
        }
    }
}
