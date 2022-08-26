using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static AkiraMindController.Communication.Connectors.CommonConnectorBase;

namespace AkiraMindController.Communication
{
    internal class Utils
    {
        public static object DeserializeFromPayloadString(string payloadStr)
        {
            var jsonStr = Encoding.UTF8.GetString(Convert.FromBase64String(payloadStr));
            Log.WriteLine($"[utils] jsonStr : {jsonStr}");
            var payload = Json.Deserialize<Payload>(jsonStr);
            Log.WriteLine($"[utils] payload.typeName : {payload.typeName}");
            var type = Type.GetType(payload.typeName);
            Log.WriteLine($"[utils] payloadStr : {type}");
            var param = Json.Deserialize(payload.payloadJson, type);
            Log.WriteLine($"[utils] payload.payloadJson : {payload.payloadJson}");

            return param;
        }

        public static string SerializeToPayloadString(object obj)
        {
            var payload = new Payload()
            {
                payloadJson = Json.Serialize(obj),
                typeName = obj.GetType().FullName
            };

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(Json.Serialize(payload)));
        }
    }
}
