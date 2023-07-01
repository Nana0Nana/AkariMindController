using AkiraMindController.Communication.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AkiraMindController.Communication.AkariCommand
{
    [Serializable]
    public class DumpNoteManagerAutoPlayData
    {
        [Serializable]
        public class ReturnValue
        {
            public string dumpFilePath;
        }
    }
}
