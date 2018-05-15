using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using FluentService.ServiceHelperLib;

namespace FluentService.Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Press [Enter] to run test.");
            Console.ReadLine();
            Console.WriteLine();

            ServiceHelper.CreateClient<IMyService>().ForEndpoint("tcp").WithCode(proxy =>
            {
                string text = proxy.DoSomething("Miguel");
                Console.WriteLine(text);
            }).Execute();

            ClientWorker worker = new ClientWorker();

            ServiceHelper.CreateClient<IMyServiceWithCallback, IMyCallback>().ForEndpoint("callback")
                .WithCode(proxy =>
            {
                string text = proxy.DoSomethingElse("Castro");
                Console.WriteLine(text);
            }).WithCallbackInstance(worker).Execute();

            ServiceHelper.CreateClient<IMyService>().ForEndpoint("tcp").WithCode(proxy =>
            {
                string text = proxy.DoSomething("Miguel");
                Console.WriteLine(text);
            }).ExecuteWithCallback(() =>
            {

            });

            Console.WriteLine("Press [Enter] to exit.");
            Console.ReadLine();
        }
    }

    public class ClientWorker : IMyCallback
    {
        public void DoCallback(string text)
        {
            Console.WriteLine("Callback called with text {0}.", text);
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
}
