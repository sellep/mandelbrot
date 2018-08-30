using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;

namespace MB.Core
{

    [ServiceContract]
    public interface IContract
    {

        [OperationContract]
        ComputationRequest Request();

        [OperationContract(IsOneWay = true)]
        void Finish(Guid id, int[] iframe);
    }
}
