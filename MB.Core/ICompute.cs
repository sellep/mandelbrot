using System;
using System.Collections.Generic;
using System.Text;

namespace MB.Core
{

    public interface ICompute
    {

        TimeSpan Compute(ComputationRequest request, out int[,] iters);
    }
}
