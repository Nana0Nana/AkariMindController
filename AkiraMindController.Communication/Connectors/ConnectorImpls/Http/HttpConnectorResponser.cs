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

        public HttpConnectorResponser(Stream outputStream)
        {
            this.outputStream = outputStream;
        }

        public void Response<T>(T obj)
        {
            using var writer = new StreamWriter(outputStream);
            writer.WriteLine(Utils.SerializeToPayloadString(obj));
        }
    }
}
