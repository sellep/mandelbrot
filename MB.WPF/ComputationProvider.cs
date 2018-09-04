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

        private Project _Proj;

        public ComputationProvider(Uri address, Project proj)
        {
            _Proj = proj;

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


        public void Dispose()
        {
            _Host.Close();
        }

        public void Finish(Guid id, int[] iframe)
        {
            _Proj.Finish(id, iframe);
        }

        public void FinishZoom(Guid id, string[] bounds)
        {
            Complex min = new Complex(bounds[0], bounds[1]);
            Complex max = new Complex(bounds[2], bounds[3]);

            _Proj.CreateFrame(min, max);
        }

        public ComputationRequest Request()
        {
            return _Proj.Request();
        }
    }
}
