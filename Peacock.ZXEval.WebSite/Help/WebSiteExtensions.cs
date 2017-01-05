using System;

namespace Peacock.ZXEval.WebSite.Help
{
    public static class WebSizeExtensionMethods
    {
        public static string ToShortString(this DateTime? time)
        {
            return time.HasValue ? time.Value.ToString("yyyy-MM-dd HH:mm:ss") : string.Empty;
        }
    }
}
