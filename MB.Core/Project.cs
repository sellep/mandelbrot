using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Linq;
using System.Runtime.InteropServices;

namespace MB.Core
{

    public class Project
    {
        private const uint _NUMBER_LENGTH = 10;

        private readonly object _Sync = new object();
        private int _FrameCount = -1;

        private ComputationPackage _Package = null;
        private ComputationRequest _ZoomRequest = null;

        public event EventHandler FrameChanged;
        public event EventHandler<int[,]> FrameFinished;

        public static Project Initialize(int width, int height, uint limit, int threads, Complex min, Complex max)
        {
            if (File.Exists(GetProjectFile()))
                return Load();

            return New(width, height, limit, threads, min, max);
        }

        private static Project New(int width, int height, uint limit, int threads, Complex min, Complex max)
        {
            EnsureBasePath();

            FrameHelper.MakeGrid(width, height, threads, out int rows, out int cols);

            Project proj = new Project()
            {
                Width = width,
                Height = height,
                Limit = limit,
                Threads = threads,
                PartialWidth = width / cols,
                PartialHeight = height / rows,
                Cols = cols,
                Rows = rows
            };

            string json = JsonConvert.SerializeObject(proj);
            File.WriteAllText(GetProjectFile(), json);

            proj.CreateFrame(min, max);

            return proj;
        }

        private static Project Load()
        {
            string json = File.ReadAllText(GetProjectFile());
            Project proj = JsonConvert.DeserializeObject<Project>(json);

            int latestFrame = Directory
                .GetDirectories(GetBasePath())
                .Select(f =>
                {
                    if (int.TryParse(Path.GetFileName(f), out int val))
                        return val;
                    return -1;
                })
                .Max();

            string framePath = Path.Combine(GetBasePath(), latestFrame.ToString());

            Tuple<Complex, Complex> bounds = JsonConvert.DeserializeObject<Tuple<Complex, Complex>>(File.ReadAllText(Path.Combine(framePath, "_bounds.json")));

            proj._FrameCount = latestFrame;
            proj._Package = new ComputationPackage(bounds.Item1, bounds.Item2, proj.Width, proj.Height);

            IEnumerable<string> dats = Directory.GetFiles(framePath, "*.dat").Select(f => Path.GetFileName(f));

            for (int row = 0; row < proj.Rows; row++)
            {
                for (int col = 0; col < proj.Cols; col++)
                {
                    if (dats.Contains($"{col}.{row}.dat"))
                    {
                        byte[] raw = File.ReadAllBytes(Path.Combine(framePath, $"{col}.{row}.dat"));
                        int[] partialFrame = new int[proj.PartialWidth * proj.PartialWidth];
                        Buffer.BlockCopy(raw, 0, partialFrame, 0, raw.Length);

                        proj._Package.MapPartialFrame(partialFrame, proj.PartialWidth, proj.PartialHeight, col, row);
                    }
                    else
                    {
                        ComputationRequest request = new ComputationRequest(bounds.Item1, bounds.Item2, proj.Width, proj.Height, proj.PartialWidth, proj.PartialHeight, proj.Limit, col, row, ComputationType.Render);
                        proj._Package.AddRequest(request);
                    }
                }
            }

            return proj;
        }

        [JsonProperty]
        public int Width { get; private set; }

        [JsonProperty]
        public int Height { get; private set; }

        [JsonProperty]
        public int PartialWidth { get; private set; }

        [JsonProperty]
        public int PartialHeight { get; private set; }

        [JsonProperty]
        public uint Limit { get; private set; }

        [JsonProperty]
        public int Threads { get; private set; }

        [JsonProperty]
        public int Cols { get; private set; }

        [JsonProperty]
        public int Rows { get; private set; }

        [JsonIgnore]
        public int[,] CurrentFrame => _Package.Frame;

        public ComputationRequest Request()
        {
            lock (_Sync)
            {
                if (_ZoomRequest != null)
                {
                    ComputationRequest tmp = _ZoomRequest;
                    _ZoomRequest = null;
                    return tmp;
                }

                if (_Package != null)
                    return _Package.Next();

                return null;
            }
        }

        public void Finish(Guid id, int[] iframe)
        {
            ComputationRequest request = null;

            lock (_Sync)
            {
                if (_Package == null)
                    return;

                request = _Package.Finish(id, iframe);
                if (request == null)
                    return;

                byte[] buffer = new byte[iframe.Length * sizeof(int)];
                Buffer.BlockCopy(iframe, 0, buffer, 0, buffer.Length);
                File.WriteAllBytes(GetPartialFramePath(_FrameCount, request), buffer);
            }

            if (_Package.IsDone)
            {
                FrameFinished?.Invoke(this, _Package.Frame);
            }
            else
            {
                FrameChanged?.Invoke(this, new EventArgs());
            }
        }

        public void SetZoomRequest(int zoomWidth, int zoomHeight, int offsetX, int offsetY)
        {
            lock (_Sync)
            {
                _ZoomRequest = new ComputationRequest(_Package.Min, _Package.Max, Width, Height, zoomWidth, zoomHeight, _NUMBER_LENGTH, offsetX, offsetY, ComputationType.Zoom);

                _Package = null;
            }
        }

        public void CreateFrame(Complex min, Complex max)
        {
            lock (_Sync)
            {
                _FrameCount++;

                string path = Path.Combine(GetBasePath(), _FrameCount.ToString());

                Directory.CreateDirectory(path);

                Tuple<Complex, Complex> complexDim = Tuple.Create(min, max);
                string json = JsonConvert.SerializeObject(complexDim);
                path = Path.Combine(path, "_bounds.json");
                File.WriteAllText(path, json);

               _Package = new ComputationPackage(min, max, Width, Height, PartialWidth, PartialHeight, Limit, Cols, Rows, Threads);
            }
        }

        public string GetCurrentBitmapPath()
        {
            return Path.Combine(GetBasePath(), _FrameCount.ToString(), "_frame.png");
        }

        private static string GetPartialFramePath(int frameCount, ComputationRequest request)
        {
            return Path.Combine(GetBasePath(), frameCount.ToString(), $"{request.Col}.{request.Row}.dat");
        }

        private static string GetProjectFile()
        {
            return Path.Combine(GetBasePath(), "project.json");
        }

        private static void EnsureBasePath()
        {
            string path = GetBasePath();

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private static string GetBasePath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mandelbrot");
        }
    }
}
