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
            var param = Utils.DeserializeFromPayloadString(r.Request.QueryString["payload"]);
            var responser = new HttpConnectorResponser(r.Response.OutputStream);

            foreach (var handler in GetTypeHandlers(param.GetType()))
                handler.Handle(param, responser);

            r.Response.StatusCode = 200;
            r.Response.Close();
        }

        public void Start()
        {
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
                        Log.WriteLine($"[server] request process throw exception : {e.Message}");
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
