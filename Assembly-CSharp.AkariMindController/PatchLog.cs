using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AkariMindControllers
{
    public static class PatchLog
    {
        public const string FilePath = "patchLog_0.log";

        static PatchLog()
        {
            try
            {
                File.Delete(FilePath);
                WriteLine("Log init time : " + DateTime.Now.ToShortTimeString());
            }
            catch { }
        }

        public static void WriteLine(string msg)
        {
            File.AppendAllText(FilePath, msg + Environment.NewLine);
            Debug.Log(msg);
        }
    }
}
