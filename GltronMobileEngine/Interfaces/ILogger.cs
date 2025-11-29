namespace GltronMobileEngine.Interfaces
{
    public interface ILogger
    {
        void Info(string tag, string message);
        void Warn(string tag, string message);
        void Error(string tag, string message);
        void Debug(string tag, string message);
    }
    
    public static class Logger
    {
        private static ILogger? _instance;
        
        public static void SetLogger(ILogger logger)
        {
            _instance = logger;
        }
        
        public static void Info(string tag, string message)
        {
            _instance?.Info(tag, message);
        }
        
        public static void Warn(string tag, string message)
        {
            _instance?.Warn(tag, message);
        }
        
        public static void Error(string tag, string message)
        {
            _instance?.Error(tag, message);
        }
        
        public static void Debug(string tag, string message)
        {
            _instance?.Debug(tag, message);
        }
    }
}
