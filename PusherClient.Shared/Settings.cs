namespace PusherClient
{    
    public sealed partial class Settings
    {        
        private static Settings defaultInstance = new Settings();
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        public string ProtocolVersion {
            get
            {
                return "5";
            }
        }
        
        public string ClientName
        {
            get
            {
                return "pusher-dotnet-client";
            }
        }
        
        public string VersionNumber
        {
            get
            {
                return "0.0.1";
            }
        }
    }
}
