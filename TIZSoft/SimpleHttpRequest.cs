using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;

namespace SpeedRunning
{
    public class SimpleHttpRequest
    {
        public static void HttpPostRequest(string url, NameValueCollection valueCollection,
            UploadValuesCompletedEventHandler uploadValuesCompletedEventHandler, object userToken = null)
        {
            using (var wc = new WebClient())
            {
                wc.Encoding = Encoding.UTF8;
                wc.UploadValuesCompleted += uploadValuesCompletedEventHandler;
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                wc.UploadValuesAsync(new Uri(url), "POST", valueCollection, userToken);
            }
        }

        public static void HttpPostRequest(string url, byte[] data,
            UploadDataCompletedEventHandler uploadDataCompletedEventHandler, object userToken = null)
        {
            using (var wc = new WebClient())
            {
                wc.Encoding = Encoding.UTF8;
                wc.UploadDataCompleted += uploadDataCompletedEventHandler;
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                wc.UploadDataAsync(new Uri(url), "POST", data, userToken);
            }
        }

        public static void DownloadData(string url, DownloadDataCompletedEventHandler downloadDataCompletedEventHandler,
            object userToken = null)
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
