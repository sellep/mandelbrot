using System;
using System.Runtime.Serialization;

namespace MB.Core
{

    [Serializable]
    public class ComputationRequest
    {

        public ComputationRequest(Complex min, Complex max, int width, int height, int partialWidth, int partialHeight, uint limit, int row, int col)
        {
            Id = Guid.NewGuid();

            RMin = min.RValue;
            IMin = min.IValue;
            RMax = max.RValue;
            IMax = max.IValue;

            Width = width;
            Height = height;
            PartialWidth = partialWidth;
            PartialHeight = partialHeight;
            Limit = limit;
            Row = row;
            Col = col;
        }

        public Guid Id { get; }

        public string RMin { get; }

        public string IMin { get; }

        public string RMax { get; }

        public string IMax { get; }

        public int Width { get; }

        public int Height { get; }

        public int PartialWidth { get; }

        public int PartialHeight { get; }

        public uint Limit { get; }

        public int Row { get; }

        public int Col { get; }
    }
}
