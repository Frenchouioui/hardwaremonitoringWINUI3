namespace HardwareMonitorWinUI3.Models
{
    public enum BackdropStyle
    {
        Acrylic = 0,
        Mica = 1,
        MicaAlt = 2
    }

    public class AppSettings
    {
        public int WindowX { get; set; } = -1;
        public int WindowY { get; set; } = -1;
        public int WindowWidth { get; set; } = 1200;
        public int WindowHeight { get; set; } = 800;
        public bool IsMaximized { get; set; }

        public int BackdropStyle { get; set; } = (int)Models.BackdropStyle.MicaAlt;

        public int RefreshInterval { get; set; } = 250;

        public bool ShowCPU { get; set; } = true;
        public bool ShowGPU { get; set; } = true;
        public bool ShowMotherboard { get; set; } = true;
        public bool ShowStorage { get; set; } = true;
        public bool ShowMemory { get; set; } = true;
        public bool ShowNetwork { get; set; } = true;
        public bool ShowController { get; set; } = true;
    }
}
