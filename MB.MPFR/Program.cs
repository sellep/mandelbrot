using System;
using System.Threading;
using System.Runtime.InteropServices;
using MB.Core;

namespace Temp
{
	
    public class Program
    {
		private const string _LIB_NAME = "libmbmpfr";

		[DllImport(_LIB_NAME, EntryPoint = "compute")]
		private static extern void compute(IntPtr iframe, string r_min, string i_min, string r_step, string i_step, uint width, uint height, uint limit);
		
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
			const int width = 10;
			const int height = 10;
			const uint limit = 1000000;
			const int row = 0;
			const int col = 0;
			
			DoubleComplex min = new DoubleComplex(0, 0);
			DoubleComplex step = new DoubleComplex(0, 0);
			
			ComputationRequest request = new ComputationRequest(min, step, width, height, limit, row, col);
			
			DateTime begin = DateTime.Now;
			int[] iframe = InvokeCompute(request);
			DateTime end = DateTime.Now;
				
			Console.WriteLine($"[{iframe.Length}:{iframe[0]}] {(end - begin).TotalSeconds}s");
		}
		
		private static void RunMode(int id, Uri address, TimeSpan sleep)
		{
			while (true)
			{
				ComputationRequest request = Request(address, sleep);
                Console.Write($"{id}: [{request.Id}]");
				
				DateTime begin = DateTime.Now;
				int[] iframe = InvokeCompute(request);
				DateTime end = DateTime.Now;
				
				Console.WriteLine($" {(end - begin).TotalSeconds}s");
				
				Finish(address, request, iframe);
			}
		}
		
		private static int[] InvokeCompute(ComputationRequest request)
		{
			int[] iframe = new int[request.Width * request.Height];
			
			IntPtr ptr = Marshal.AllocHGlobal(sizeof(int) * request.Width * request.Height);
			try
			{
				compute(ptr, request.RMin, request.IMin, request.RStep, request.IStep, (uint)request.Width, (uint)request.Height, request.Limit);
				
				Marshal.Copy(ptr, iframe, 0, request.Width * request.Height);	
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
