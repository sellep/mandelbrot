using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace MB.Core
{

    public class Project
    {
        private readonly object _Sync = new object();
        private int _FrameCount = -1;

        public event EventHandler FrameChanged;

        public static Project Initialize(int width, int height, uint limit, int threads, Complex min, Complex max)
        {
            if (File.Exists(GetProjectFile()))
                return Load();

            return New(width, height, limit, threads, min, max);
        }

        private static Project New(int width, int height, uint limit, int threads, Complex min, Complex max)
        {
            EnsureBasePath();

            Project proj = new Project()
            {
                Width = width,
                Height = height,
                Limit = limit,
                Threads = threads,
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

            return proj;
        }

        [JsonIgnore]
        public ComputationPackage Current { get; private set; } = null;

        [JsonProperty]
        public int Width { get; private set; }

        [JsonProperty]
        public int Height { get; private set; }

        [JsonProperty]
        public uint Limit { get; private set; }

        [JsonProperty]
        public int Threads { get; private set; }

        public void Finish(Guid id, int[] iframe)
        {
            ComputationRequest request = null;

            lock (_Sync)
            {
                request = Current.Finish(id, iframe);
                if (request == null)
                    return;
            }

            byte[] buffer = new byte[iframe.Length * sizeof(int)];
            Buffer.BlockCopy(iframe, 0, buffer, 0, buffer.Length);
            File.WriteAllBytes(GetPartialFramePath(_FrameCount, request), buffer);

            FrameChanged?.Invoke(this, new EventArgs());
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
                path = Path.Combine(path, "bounds.json");
                File.WriteAllText(path, json);

                Current = new ComputationPackage(min, max, Width, Height, Limit, Threads);
            }
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
