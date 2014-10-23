using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;

namespace TestFormApp
{
    public class SimpleHttpRequest
    {
        public static void HttpPostRequest(string url, object userToken, NameValueCollection valueCollection, UploadValuesCompletedEventHandler uploadValuesCompletedEventHandler)
        {
            using (var wc = new WebClient())
            {
                wc.Encoding = Encoding.UTF8;
                wc.UploadValuesCompleted += uploadValuesCompletedEventHandler;
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                wc.UploadValuesAsync(new Uri(url), "POST", valueCollection, userToken);
            }
        }

        public static void DownloadData(string url, object userToken, DownloadDataCompletedEventHandler downloadDataCompletedEventHandler)
        {
            using (var wc = new WebClient())
            {
                wc.Encoding = Encoding.UTF8;
                wc.DownloadDataCompleted += downloadDataCompletedEventHandler;
                wc.DownloadDataAsync(new Uri(url), userToken);
            }
        }
    }
}
