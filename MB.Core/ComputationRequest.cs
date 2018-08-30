using System;
using System.Runtime.Serialization;

namespace MB.Core
{

    [Serializable]
    public class ComputationRequest
    {

        public ComputationRequest(Complex min, Complex step, int width, int height, uint limit, int row, int col)
        {
            Id = Guid.NewGuid();

            RMin = min.RValue;
            IMin = min.IValue;
            RStep = step.RValue;
            IStep = step.IValue;

            Width = width;
            Height = height;
            Limit = limit;
            Row = row;
            Col = col;
        }

        public Guid Id { get; }

        public string RMin { get; }

        public string IMin { get; }

        public string RStep { get; }

        public string IStep { get; }

        public int Width { get; }

        public int Height { get; }

        public uint Limit { get; }

        [IgnoreDataMember]
        public int Row { get; }

        [IgnoreDataMember]
        public int Col { get; }
    }
}
