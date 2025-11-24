namespace TheStarRichyProject
{
    public class CommonConfig
    {
        protected static CommonConfig _instance;

        protected static IConfiguration _config;

        public IConfiguration Config => _config;
        public static CommonConfig Instance => _instance;


        public static string AddVersion(string path)
        {
            if (path.StartsWith("vendor"))
            {
                return AddVersionVendor(path);
            }
            else if (path.StartsWith("fonts") || path.StartsWith("images"))
            {
                return AddVersionLib(path);
            }
            else if (path.StartsWith("app") || path.StartsWith("css"))
            {
                return AddVersionApp(path);
            }
            else
            {
                return path + "?v=" + DateTime.Now.ToString("ddMMyyyyhhmm");
            }
        }

        public static string AddVersionLib(string path)
        {

            return path + "?v=" + _config["Version:Lib"];// DateTime.Now.ToString("ddMMyyyyhhmm");
        }
        public static string AddVersionVendor(string path)
        {
            return path + "?v=" + _config["Version:Vendor"];//DateTime.Now.ToString("ddMMyyyyhhmm");
        }
        public static string AddVersionApp(string path)
        {
            return path + "?v=" + _config["Version:App"];//DateTime.Now.ToString("ddMMyyyyhhmm");
        }




        protected CommonConfig()
        {
        }

        public static void Initialize() //IConfiguration config
        {
            var configuration = new ConfigurationBuilder()
            .AddJsonFile($"appsettings.json")
            .Build();

            _config = configuration;
            _instance = new CommonConfig();
        }

    }
}
