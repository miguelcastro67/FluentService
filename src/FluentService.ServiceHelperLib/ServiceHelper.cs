using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

namespace FluentService.ServiceHelperLib
{
    public class ServiceHelper
    {
        static string _EndpointName = string.Empty;

        public static ServiceClient<T, T> CreateClient<T>()
        {
            return new ServiceClient<T, T>();
        }

        public static ServiceClient<T, U> CreateClient<T, U>()
        {
            return new ServiceClient<T, U>();
        }
    }

    public class ServiceClient<T, U>
    {
        ChannelFactory<T> _ChannelFactory = null;
        T _Proxy = default(T);
        Action<T> _ClientCode = null;
        InstanceContext _InstanceContext = null;
        Action<U> _CallbackCode = null;
        string _EndpointName = string.Empty;
        EndpointAddress _EndpointAddress = null;
        Binding _Binding = null;
        string _Address = string.Empty;
        ServiceEndpoint _ServiceEndpoint = null;

        public ServiceClient()
        {
            _ChannelFactory = CreateFactory<T>();
            _Proxy = _ChannelFactory.CreateChannel();
        }

        public ServiceClient<T, U> ForEndpoint(string endpointName)
        {
            _EndpointName = endpointName;
            return this;
        }

        public ServiceClient<T, U> ForEndpoint(EndpointAddress address, Binding binding)
        {
            _EndpointAddress = address;
            _Binding = binding;
            return this;
        }

        public ServiceClient<T, U> ForEndpoint(string address, Binding binding)
        {
            _Address = address;
            _Binding = binding;
            return this;
        }

        public ServiceClient<T, U> ForEndpoint(ServiceEndpoint endpoint)
        {
            _ServiceEndpoint = endpoint;
            return this;
        }

        public ServiceClient<T, U> WithCode(Action<T> clientCode)
        {
            _ClientCode = clientCode;
            return this;
        }

        public ServiceClient<T, U> WithCallbackInstance(object callbackInstance)
        {
            _InstanceContext = new InstanceContext(callbackInstance);
            return this;
        }

        public T GetProxy()
        {
            return _Proxy;
        }

        public void Execute()
        {
            _ClientCode.Invoke(_Proxy);
            _ChannelFactory.Close();
        }

        public void ExecuteWithCallback(Action completed)
        {
            _ClientCode.BeginInvoke(_Proxy, ServiceClientCallback, null);
        }

        void ServiceClientCallback(IAsyncResult ar)
        {
            _ClientCode.EndInvoke(ar);
            _ChannelFactory.Close();
        }

        ChannelFactory<T> CreateFactory<T>()
        {
            ChannelFactory<T> channelFactory = null;

            if (_ServiceEndpoint != null)
            {
                if (_InstanceContext == null)
                    channelFactory = new ChannelFactory<T>(_ServiceEndpoint);
                else
                    channelFactory = new DuplexChannelFactory<T>(_InstanceContext, _ServiceEndpoint);
            }
            else
            {
                if (_Address != string.Empty)
                    _EndpointAddress = new EndpointAddress(_Address);

                if (_EndpointAddress != null)
                {
                    if (_Binding == null)
                    {
                        string protocol = _EndpointAddress.Uri.Scheme.ToLower();
                        if (protocol == "net.tcp")
                            _Binding = new NetTcpBinding();
                        else if (protocol == "http")
                            _Binding = new BasicHttpBinding();
                        else if (protocol == "net.pipe")
                            _Binding = new NetNamedPipeBinding();
                        else if (protocol == "net.msmq")
                            _Binding = new NetMsmqBinding();
                        else
                            throw new InvalidOperationException("Unknown address scheme.");
                    }

                    if (_InstanceContext == null)
                        channelFactory = new ChannelFactory<T>(_Binding, _EndpointAddress);
                    else
                        channelFactory = new DuplexChannelFactory<T>(_InstanceContext, _Binding, _EndpointAddress);
                }
                else
                {
                    string endpointName = (_EndpointName == string.Empty ? "*" : _EndpointName);
                    if (_InstanceContext == null)
                        channelFactory = new ChannelFactory<T>(endpointName);
                    else
                        channelFactory = new DuplexChannelFactory<T>(_InstanceContext, endpointName);
                }
            }

            if (channelFactory == null)
                throw new ApplicationException("Unable to create channel factory for service client.");

            return channelFactory;
        }
    }
}
