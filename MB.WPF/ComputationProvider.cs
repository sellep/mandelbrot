using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using MB.Core;

namespace MB.WPF
{

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ComputationProvider : IContract, IDisposable
    {
        private ServiceHost _Host;

        private ComputationPackage _Package = null;

        public ComputationProvider(Uri address)
        {
            _Host = new ServiceHost(this);
            CreateEndpoint(_Host, typeof(IContract), address);
            _Host.Open();
        }

        public static void CreateEndpoint(ServiceHost serviceHost, Type contract, Uri baseAddress)
        {
            ServiceEndpoint endpoint = serviceHost.AddServiceEndpoint(
                contract,
                ServiceHelper.CreateBinding(baseAddress),
                new Uri(baseAddress, contract.Name));

            endpoint.AddDataContractResolver();
        }

        public void Apply(ComputationPackage package)
        {
            _Package = package;
        }

        public void Dispose()
        {
            _Host.Close();
        }

        public void Finish(Guid id, int[] iframe)
        {
            _Package.Finish(id, iframe);
        }

        public ComputationRequest Request()
        {
            if (_Package == null)
                return null;

            return _Package.Next();
        }
    }
}
