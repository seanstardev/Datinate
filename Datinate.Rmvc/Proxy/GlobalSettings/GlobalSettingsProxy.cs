using Datinate.Rmvc.Delegate.GlobalSettings;
using Datinate.Shared;
using RMVC;
using System.Diagnostics;
using System.Text.Json;

namespace Datinate.Rmvc.Proxy
{
    public class GlobalSettingsProxy : RModel
    {
        public string ProjectRoot { get; }

        public DatRootDTO[] DatRootPaths { get; private set; } = Array.Empty<DatRootDTO>();
        public string MameSlHashPath { get; set; } = string.Empty;

        readonly RadioSessionDelegate radioSession;
        readonly string cfgFullpath;

        private static readonly JsonSerializerOptions JsonOpts = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        public GlobalSettingsProxy()
        {
            radioSession = new RadioSessionDelegate("Datinate");
            ProjectRoot = radioSession.ProjectPath;
            cfgFullpath = Path.Combine(radioSession.ProjectPath, "Datinate.cfg");
        }

        public void Startup()
        {
            _ = LoadCfg();
        }

        public bool SaveDatRootPathsCfg(DatRootDTO[] datRoots, string mameHashPath)
        {
            DatRootPaths = datRoots;
            MameSlHashPath = mameHashPath;
            return Save();
        }

        public bool Save()
        {
            try
            {
                var cfg = new DatinateCfg
                {
                    DatRoots = DatRootPaths,
                    GlobalDats = new GlobalDats
                    {
                        MameSlHashPath = MameSlHashPath
                    }
                };

                var json = JsonSerializer.Serialize(cfg, JsonOpts);

                if (File.Exists(cfgFullpath))
                    File.Delete(cfgFullpath);

                File.WriteAllText(cfgFullpath, json);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: Failed to save configuration - {ex.Message}");
            }
            return false;
        }

        public DatRootDTO[] LoadCfg()
        {
            try
            {
                if (!File.Exists(cfgFullpath))
                {
                    DatRootPaths = Array.Empty<DatRootDTO>();
                    MameSlHashPath = string.Empty;
                    return DatRootPaths;
                }

                var json = File.ReadAllText(cfgFullpath);

                var cfg = JsonSerializer.Deserialize<DatinateCfg>(json) ?? new DatinateCfg();

                DatRootPaths = cfg.DatRoots ?? Array.Empty<DatRootDTO>();

                var gd = cfg.GlobalDats ?? new GlobalDats();

                MameSlHashPath = gd.MameSlHashPath ?? string.Empty;

                return DatRootPaths;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: Failed to load configuration - {ex.Message}");
                return Array.Empty<DatRootDTO>();
            }
        }

        private class GlobalDats
        {
            public string? MameSlHashPath { get; set; }
        }

        private class DatinateCfg
        {
            public DatRootDTO[]? DatRoots { get; set; }
            public GlobalDats? GlobalDats { get; set; }
        }
    }
}
