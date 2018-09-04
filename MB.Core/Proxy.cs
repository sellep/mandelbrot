using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;

namespace MB.Core
{

    public class Proxy : ClientBase<IContract>, IContract, IDisposable
    {

        public Proxy(Uri address)
            : base(ServiceHelper.CreateBinding(address), new EndpointAddress(new Uri(address, typeof(IContract).Name)))
        {

        }

        public void Finish(Guid id, int[] iframe)
        {
            Channel.Finish(id, iframe);
        }

        public void FinishZoom(Guid id, string[] bounds)
        {
            Channel.FinishZoom(id, bounds);
        }

        public ComputationRequest Request()
        {
            return Channel.Request();
        }
    }
}
