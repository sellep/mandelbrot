using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Xml;

namespace MB.Core
{

    public static class ServiceHelper
    {

        public static Binding CreateBinding(Uri baseAddress)
        {
            //const string _PROTOCOL_PIPE = "net.pipe://";
            const string _PROTOCOL_TCP = "net.tcp://";

            string address = baseAddress.ToString().ToLower();

            if (address.StartsWith(_PROTOCOL_TCP))
            {
                NetTcpBinding binding = new NetTcpBinding();
                binding.Security.Mode = SecurityMode.None;
                binding.ReaderQuotas.MaxArrayLength = int.MaxValue;
                binding.MaxReceivedMessageSize = int.MaxValue;
                binding.TransferMode = TransferMode.Buffered;
                return binding;
            }

            throw new Exception($"Protocol {address.Split(':')[0]} not supported");
        }

        public static void AddDataContractResolver(this ServiceEndpoint serviceEndpoint)
        {
            foreach (OperationDescription operation in serviceEndpoint.Contract.Operations)
            {
                DataContractSerializerOperationBehavior serializerBehavior = operation.Behaviors.Find<DataContractSerializerOperationBehavior>();

                if (serializerBehavior != null)
                {
                    operation.Behaviors.Remove(serializerBehavior);
                }

                serializerBehavior = new DataContractSerializerOperationBehavior(operation);

                operation.Behaviors.Add(serializerBehavior);

                serializerBehavior.DataContractResolver = new SharedTypeResolver();
            }
        }

        private class SharedTypeResolver : DataContractResolver
        {

            public SharedTypeResolver()
            {

            }

            public override bool TryResolveType(
                Type dataContractType,
                Type declaredType,
                DataContractResolver knownTypeResolver,
                out XmlDictionaryString typeName,
                out XmlDictionaryString typeNamespace)
            {
                if (!knownTypeResolver.TryResolveType(dataContractType, declaredType, null, out typeName, out typeNamespace))
                {
                    XmlDictionary dict = new XmlDictionary();

                    typeName = dict.Add(dataContractType.FullName);
                    typeNamespace = dict.Add(dataContractType.Assembly.FullName);
                }

                return true;
            }

            public override Type ResolveName(string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver)
            {
                Type type = knownTypeResolver.ResolveName(typeName, typeNamespace, declaredType, null);
                if (type != null)
                    return type;

                try
                {
                    type = Type.GetType(typeName + ", " + typeNamespace);
                }
                catch
                {
                    return declaredType;
                }

                return type ?? declaredType;
            }
        }
    }
}
