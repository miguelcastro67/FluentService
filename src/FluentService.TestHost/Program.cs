using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace FluentService.TestHost
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceHost host = new ServiceHost(typeof(MyService));
            host.Open();

            Console.WriteLine("Services Running. Press [Enter] to exit.");
            Console.ReadLine();
        }
    }

    [ServiceContract]
    public interface IMyService
    {
        [OperationContract]
        string DoSomething(string text);
    }

    [ServiceContract(CallbackContract = typeof(IMyCallback))]
    public interface IMyServiceWithCallback
    {
        [OperationContract]
        string DoSomethingElse(string text);
    }

    [ServiceContract]
    public interface IMyCallback
    {
        [OperationContract]
        void DoCallback(string text);
    }

    public class MyService : IMyService, IMyServiceWithCallback
    {
        public string DoSomething(string text)
        {
            return string.Format("You sent me the following text: {0}", text);
        }

        public string DoSomethingElse(string text)
        {
            var callbackChannel = OperationContext.Current.GetCallbackChannel<IMyCallback>();
            if (callbackChannel != null)
                callbackChannel.DoCallback(text);

            return string.Format("You sent me the following text: {0} and I sent it back during a callback", text);
        }
    }
}
