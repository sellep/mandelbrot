using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Linq;
using System.Runtime.InteropServices;
using System.Drawing;

namespace MB.Core
{

    public class Project
    {
        private const uint _NUMBER_LENGTH = 128;

        private readonly object _Sync = new object();
        private int _FrameCount = -1;

        private ComputationPackage _Package = null;
        private ComputationRequest _ZoomRequest = null;

        public event EventHandler FrameChanged;
        public event EventHandler<int[,]> FrameFinished;

        public static Project New(string name, bool auto, int width, int height, int threads, Complex min, Complex max)
        {
            string path = GetProjectBasePath(name);

            if (Directory.Exists(path))
                throw new Exception("Project already exists");

            Directory.CreateDirectory(path);
            Directory.CreateDirectory(Path.Combine(path, "_frames"));

            FrameHelper.MakeGrid(width, height, threads, out int cols, out int rows);

            Project proj = new Project()
            {
                Name = name,
                Auto = auto,
                Width = width,
                Height = height,
                Threads = threads,
                PartialWidth = width / cols,
                PartialHeight = height / rows,
                Cols = cols,
                Rows = rows
            };

            string json = JsonConvert.SerializeObject(proj);
            File.WriteAllText(GetProjectFile(name), json);

            proj.Palette = InitializePalette();

            proj.CreateFrame(min, max);

            return proj;
        }

        public static Project Load(string name)
        {
            string json = File.ReadAllText(GetProjectFile(name));
            Project proj = JsonConvert.DeserializeObject<Project>(json);

            proj.Name = name;

            proj.Palette = InitializePalette();

            int latestFrame = Directory
                .GetDirectories(GetProjectBasePath(name))
                .Select(f =>
                {
                    if (int.TryParse(Path.GetFileName(f), out int val))
                        return val;
                    return -1;
                })
                .Max();

            string framePath = Path.Combine(GetProjectBasePath(name), latestFrame.ToString());

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
                        ComputationRequest request = new ComputationRequest(bounds.Item1, bounds.Item2, proj.Width, proj.Height, proj.PartialWidth, proj.PartialHeight, (uint)proj.Palette.Length - 1, col, row, ComputationType.Render);
                        proj._Package.AddRequest(request);
                    }
                }
            }

            return proj;
        }

        private static Color[] InitializePalette()
        {
            string paletteBinPath = GetPaletteBinPath();

            BinaryFormatter formatter = new BinaryFormatter();

            if (File.Exists(paletteBinPath))
            {
                using (Stream s = File.OpenRead(paletteBinPath))
                {
                    return (Color[])formatter.Deserialize(s);
                }
            }

            Color[] palette = ColorInterpolator.CreatePalette(20, Color.Black, Color.Black, 50000).ToArray();

            using (Stream s = File.OpenWrite(paletteBinPath))
            {
                formatter.Serialize(s, palette);
            }

            return palette;
        }

        [JsonIgnore]
        public string Name { get; private set; }

        [JsonProperty]
        public bool Auto { get; private set; }

        [JsonProperty]
        public int Width { get; private set; }

        [JsonProperty]
        public int Height { get; private set; }

        [JsonProperty]
        public int PartialWidth { get; private set; }

        [JsonProperty]
        public int PartialHeight { get; private set; }

        [JsonProperty]
        public int Threads { get; private set; }

        [JsonProperty]
        public int Cols { get; private set; }

        [JsonProperty]
        public int Rows { get; private set; }

        [JsonIgnore]
        public Color[] Palette { get; private set; }

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
                File.WriteAllBytes(GetPartialFramePath(Name, _FrameCount, request), buffer);
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

                string path = Path.Combine(GetProjectBasePath(Name), _FrameCount.ToString());

                Directory.CreateDirectory(path);

                Tuple<Complex, Complex> complexDim = Tuple.Create(min, max);
                string json = JsonConvert.SerializeObject(complexDim);
                path = Path.Combine(path, "_bounds.json");
                File.WriteAllText(path, json);

               _Package = new ComputationPackage(min, max, Width, Height, PartialWidth, PartialHeight, (uint)Palette.Length - 1, Cols, Rows, Threads);
            }
        }

        public string GetCurrentBitmapPath(string name)
        {
            return Path.Combine(GetProjectBasePath(name), "_frames", $"{_FrameCount}.png");
        }

        private static string GetPartialFramePath(string name, int frameCount, ComputationRequest request)
        {
            return Path.Combine(GetProjectBasePath(name), frameCount.ToString(), $"{request.Col}.{request.Row}.dat");
        }

        private static string GetProjectFile(string name)
        {
            return Path.Combine(GetProjectBasePath(name), "project.json");
        }

        private static string GetProjectBasePath(string name)
        {
            return Path.Combine(GetBasePath(), name);
        }

        private static string GetPaletteBinPath()
        {
            return Path.Combine(GetBasePath(), "palette.bin");
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
