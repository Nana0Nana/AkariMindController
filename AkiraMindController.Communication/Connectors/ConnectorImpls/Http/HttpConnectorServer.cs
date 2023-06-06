using AkiraMindController.Communication.Bases;
using AkiraMindController.Communication.Connectors.CommonMessages;
using AkiraMindController.Communication.Utils;
using System;
using System.Linq;
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

            try
            {
                var param = MessageContentPacker.DeserializeFromPayloadString(r.Request.QueryString["payload"]);
                r.Response.StatusCode = 200;
                using var stream = r.Response.OutputStream;
                var responser = new HttpConnectorResponser(stream);

                foreach (var handler in GetTypeHandlers(param.GetType()))
                    handler.Handle(param, responser);
            }
            catch (Exception e)
            {
                Log.WriteLine($"[server] type handler throw exception : {e.Message} , stack : {MessageContentPacker.SerializeToPayloadString(e.StackTrace)}");
            }
            finally
            {
                r.Response.Close();
            }
        }

        public void Start()
        {
            var c = new AutoFaderTarget()
            {
                bellRanges = new ValueRange[] { new(1, 4), new(5, 6) },
                damageRanges = new ValueRange[] { new(2, 5), new(6, 7) },
                targetRanges = new ValueRange[] { new(3, 6), new(7, 8) },
                moveableRange = new ValueRange(100, 600),
                finalTargetFrame = 2857,
                targetPlaceRange = new ValueRange(5000, 6000)
            }.Serialize();
            Log.WriteLine($"TEST : {c}");

            var r = new AutoFaderTarget();
            r.Deerialize(c);
            Log.WriteLine($"TEST2 : {string.Join(",", r.damageRanges.Select(x => x.ToString()).ToArray())}");

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
                        Log.SetEnableLog(r.Request.Url.LocalPath != "/ping");
                        ProcessRequest(r);
                        Log.SetEnableLog(true);
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
