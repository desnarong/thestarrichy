namespace TheStarRichyProject.Helper
{
    public class LanguageHelper
    {
        public static string ActiveIcon(string lang)
        {
            string ret = "";
            switch (lang)
            {
                case "en":
                    ret = "flag-icon-us";
                    break;
                case "th":
                    ret = "flag-icon-th";
                    break;
                case "lo":
                    ret = "flag-icon-la";
                    break;
                case "km":
                    ret = "flag-icon-kh";
                    break;
                case "my":
                    ret = "flag-icon-mm";
                    break;
                default:
                    ret = "";
                    break;
            }
            return ret;
        }
        public static string ActiveImage(string lang)
        {
            string ret = "";
            switch (lang)
            {
                case "en":
                    ret = "en";
                    break;
                case "th":
                    ret = "th";
                    break;
                case "lo":
                    ret = "la";
                    break;
                case "km":
                    ret = "kh";
                    break;
                case "my":
                    ret = "mm";
                    break;
                default:
                    ret = "";
                    break;
            }
            return ret;
        }
    }
}
