using System;
using System.Collections.Generic;
using System.Text;

namespace MB.Core
{

    public class DoubleComplex : Complex
    {

        public DoubleComplex(double r, double i)
            : base(r.ToString(), i.ToString())
        {
            R = r;
            I = i;
        }

        public double R { get; }

        public double I { get; }

        public double AbsSquared()
        {
            return R * R +I * I;
        }

        public static DoubleComplex operator *(DoubleComplex a, DoubleComplex b)
        {
            return new DoubleComplex(a.R * b.R - a.I * b.I, a.R * b.I + a.I * b.R);
        }

        public static DoubleComplex operator +(DoubleComplex a, DoubleComplex b)
        {
            return new DoubleComplex(a.R + b.R, a.I + b.I);
        }

        public static DoubleComplex operator -(DoubleComplex a, DoubleComplex b)
        {
            return new DoubleComplex(a.R - b.R, a.I - b.I);
        }
    }
}
