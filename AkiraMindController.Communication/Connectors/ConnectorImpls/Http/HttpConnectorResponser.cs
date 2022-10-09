using AkiraMindController.Communication.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AkiraMindController.Communication.Connectors.ConnectorImpls.Http
{
    public class HttpConnectorResponser : IResponser
    {
        private Stream outputStream;
        public bool HasResponsed { get; private set; } = false;

        public HttpConnectorResponser(Stream outputStream)
        {
            this.outputStream = outputStream;
        }

        public void Response<T>(T obj)
        {
            if (HasResponsed)
                return;
            HasResponsed = true;
            using var writer = new StreamWriter(outputStream);
            writer.WriteLine(MessageContentPacker.SerializeToPayloadString(obj));
        }
    }
}
