using System;
using System.Threading;
using System.Runtime.InteropServices;
using MB.Core;
using System.Text;

namespace Temp
{

    public class Program
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct dim
        {
            public uint x;
            public uint y;
        }

        [DllImport("libmbmpfr", EntryPoint = "zoom")]
        private static extern void zoom(StringBuilder str, uint str_length, string r_min, string i_min, string r_max, string i_max, dim size, dim zoom_size, dim offset);

        [DllImport("libmbmpfr", EntryPoint = "compute")]
        private static extern void compute(IntPtr iframe, string r_min, string i_min, string r_max, string i_max, dim size, dim partial_size, dim idx, uint limit);

        public static void Main(string[] args)
        {
            TimeSpan sleep = TimeSpan.FromSeconds(10);

            ParseArguments(args, out bool test, out int id, out Uri address);

            if (test)
            {
                RunTest();
            }
            else
            {
                RunMode(id, address, sleep);
            }
        }

        private static void ParseArguments(string[] args, out bool test, out int id, out Uri address)
        {
            const string _ARG_TEST = "test";
            const string _ARG_ID = "id=";
            const string _ARG_ADDRESS = "address=";

            test = false;
            id = default(int);
            address = null;

            foreach (string arg in args)
            {
                if (arg.StartsWith(_ARG_TEST))
                {
                    test = true;
                }
                else if (arg.StartsWith(_ARG_ID))
                {
                    if (!int.TryParse(arg.Substring(_ARG_ID.Length), out id))
                    {
                        Console.Error.WriteLine("not a valid id (int)");
                        Environment.Exit(1);
                    }
                }
                else if (arg.StartsWith(_ARG_ADDRESS))
                {
                    address = new Uri(arg.Substring(_ARG_ADDRESS.Length));
                }
                else
                {
                    Console.Error.WriteLine($"unexpected argument: {arg}");
                    Environment.Exit(1);
                }
            }
        }

        private static void RunTest()
        {
            const uint limit = 1000000;
            const int row = 0;
            const int col = 0;

            const int height = 16;

            int width = Console.WindowWidth > 32
                ? 32
                : Console.WindowWidth;

            int partialWidth = width;
            int partialHeight = height;

            DoubleComplex min = new DoubleComplex(-2, -1.5);
            DoubleComplex max = new DoubleComplex(1, 1.5);

            ComputationRequest request = new ComputationRequest(min, max, width, height, partialWidth, partialHeight, limit, row, col);

            DateTime begin = DateTime.Now;
            int[] iframe = InvokeCompute(request);
            DateTime end = DateTime.Now;

            WriteFrame(iframe, width, height, limit);

            Console.WriteLine($"(frame size: {width}:{height}) {(end - begin).TotalSeconds}s");
        }

        private static void WriteFrame(int[] iframe, int width, int height, uint limit)
        {
            int[,] frame = FrameHelper.ToMulti(iframe, width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int val = frame[x, y];

                    Console.Write(val > limit / 2
                        ? '+'
                        : ' ');
                }

                Console.WriteLine();
            }
        }

        private static void RunMode(int id, Uri address, TimeSpan sleep)
        {
            while (true)
            {
                ComputationRequest request = Request(address, sleep);
                Console.Write($"{id}: [{request.Id}({request.Type})]");


                if (request.Type == ComputationType.Render)
                {
                    DateTime begin = DateTime.Now;
                    int[] iframe = InvokeCompute(request);
                    DateTime end = DateTime.Now;

                    Finish(address, request, iframe);

                    Console.WriteLine($" {(end - begin).TotalSeconds}s");
                }
                else
                {
                    DateTime begin = DateTime.Now;
                    string[] bounds = InvokeZoom(request);
                    DateTime end = DateTime.Now;

                    FinishZoom(address, request, bounds);

                    Console.WriteLine($" {(end - begin).TotalSeconds}s");
                }
            }
        }

        private static string[] InvokeZoom(ComputationRequest request)
        {
            dim size = new dim
            {
                x = (uint)request.Width,
                y = (uint)request.Height
            };

            dim zoom_size = new dim
            {
                x = (uint)request.PartialWidth,
                y = (uint)request.PartialHeight
            };

            dim offset = new dim
            {
                x = (uint)request.Col,
                y = (uint)request.Row
            };

            StringBuilder sb = new StringBuilder(4 * (int)request.Limit);
            zoom(sb, request.Limit, request.RMin, request.IMin, request.RMax, request.IMax, size, zoom_size, offset);

            string str = sb.ToString();

            return new[]
            {
                str.Substring(0 * (int)request.Limit, (int)request.Limit),
                str.Substring(1 * (int)request.Limit, (int)request.Limit),
                str.Substring(2 * (int)request.Limit, (int)request.Limit),
                str.Substring(3 * (int)request.Limit, (int)request.Limit)
            };
        }

        private static int[] InvokeCompute(ComputationRequest request)
        {
            int[] iframe = new int[request.PartialWidth * request.PartialHeight];

            dim size = new dim
            {
                x = (uint)request.Width,
                y = (uint)request.Height
            };

            dim partial = new dim
            {
                x = (uint)request.PartialWidth,
                y = (uint)request.PartialHeight
            };

            dim offset = new dim
            {
                x = (uint)request.Col * partial.x,
                y = (uint)request.Row * partial.y
            };

            IntPtr ptr = Marshal.AllocHGlobal(sizeof(int) * request.PartialWidth * request.PartialHeight);
            try
            {
                compute(ptr, request.RMin, request.IMin, request.RMax, request.IMax, size, partial, offset, request.Limit);

                Marshal.Copy(ptr, iframe, 0, request.PartialWidth * request.PartialHeight);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }

            return iframe;
        }

        private static void Finish(Uri address, ComputationRequest request, int[] iframe)
        {
            using (Proxy proxy = new Proxy(address))
            {
                proxy.Finish(request.Id, iframe);
            }
        }

        private static void FinishZoom(Uri address, ComputationRequest request, string[] bounds)
        {
            using (Proxy proxy = new Proxy(address))
            {
                proxy.FinishZoom(request.Id, bounds);
            }
        }

        private static ComputationRequest Request(Uri address, TimeSpan sleep)
        {
            while (true)
            {
                try
                {
                    using (Proxy proxy = new Proxy(address))
                    {
                        ComputationRequest request = proxy.Request();
                        if (request != null)
                            return request;

                        Thread.Sleep((int)sleep.TotalMilliseconds);
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Service failure: {ex.Message}");
                    Thread.Sleep((int)sleep.TotalMilliseconds);
                }
            }
        }
    }
}
