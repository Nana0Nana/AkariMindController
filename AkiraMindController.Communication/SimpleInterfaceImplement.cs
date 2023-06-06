using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AkiraMindController.Communication
{
    public static class SimpleInterfaceImplement
    {
        public static Func<string, Type, object> Deserialize { get; set; }
        public static Func<object, string> Serialize { get; set; }
        public static Action<string> Log { get; set; }
    }

    internal static class Log
    {
        private static bool disableLog = false;

        public static void SetEnableLog(bool enableLog) => disableLog = !enableLog;
        public static void WriteLine(string msg)
        {
            if (!disableLog)
                SimpleInterfaceImplement.Log(msg);
        }
    }

    internal static class Json
    {
        public static T Deserialize<T>(string json) => string.IsNullOrEmpty(json) ? default : (T)SimpleInterfaceImplement.Deserialize(json, typeof(T));
        public static object Deserialize(string json, Type type) => SimpleInterfaceImplement.Deserialize(json, type);
        public static string Serialize<T>(T obj) => SimpleInterfaceImplement.Serialize(obj);
    }
}
