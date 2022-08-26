using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AkiraMindController.Communication.Connectors
{
    public interface IResponser
    {
        void Response<T>(T obj);
    }
}
