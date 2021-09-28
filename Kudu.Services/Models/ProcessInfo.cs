namespace Kudu.Services.Models
{
    public class ProcessInfo
    {
        public string User { get; set; }
        public string PID { get; set; }
        public string CPU { get; set; }
        public string Memory { get; set; }
        public string VSZ { get; set; }
        public string RSS { get; set; }
        public string TTY { get; set; }
        public string STAT { get; set; }
        public string Start { get; set; }
        public string Time { get; set; }
        public string Command { get; set; }
    }
}
