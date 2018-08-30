using System;
using System.Collections.Generic;
using System.Text;

namespace MB.Core
{

    public abstract class Complex
    {

        public Complex(string rValue, string iValue)
        {
            RValue = rValue;
            IValue = iValue;
        }

        public string RValue { get; }

        public string IValue { get; }
    }
}
