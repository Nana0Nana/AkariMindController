using System;

namespace AkiraMindController.Communication.Connectors
{
    public abstract class RegisterHandler
    {
        internal protected abstract void Handle(object obj, IResponser responser);
        internal protected abstract bool Check(object callback);
    }

    public class RegisterHandler<T> : RegisterHandler
    {
        private readonly IConnector.OnReceviceMessageFunc<T> callback;

        public RegisterHandler(IConnector.OnReceviceMessageFunc<T> callback)
        {
            this.callback = callback;
        }

        internal protected override bool Check(object callback)
        {
            return ((MulticastDelegate)callback).Equals(this.callback);
        }

        internal protected override void Handle(object obj, IResponser responser)
        {
            callback((T)obj, responser);
        }
    }
}
