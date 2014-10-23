using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using Tizsoft.Log;

namespace TestFormApp.JsonCommand
{
    public class IapValidateCmd : IJsonCommand
    {
        #region IJsonCommand Members

        const string AppStoreTestUrl = "https://sandbox.itunes.apple.com/verifyReceipt";
        const string AppStoreUrl = "https://buy.itunes.apple.com/verifyReceipt";
        IapValidateArgs _args;

        string GetValidateUrl()
        {
            return _args.IsSandBox ? AppStoreTestUrl : AppStoreUrl;
        }

        public IapValidateCmd(IapValidateArgs args)
        {
            _args = args;
        }

        void OnPostResponse(object sender, UploadValuesCompletedEventArgs args)
        {
            if (args.Error != null)
            {
                var responseStream = ((WebException)args.Error).Response.GetResponseStream();
                using (var reader = new StreamReader(responseStream))
                {
                    GLogger.Error(reader.ReadToEnd());
                }
            }
            else
            {
                //var response = JObject.Parse(Encoding.UTF8.GetString(args.Result));
                GLogger.Debug(Encoding.UTF8.GetString(args.Result));
            }
        }

        public void Do(JObject jObject)
        {
            var receiptData = new NameValueCollection();
            receiptData["receipt-data"] = (string)jObject.SelectToken("param.receiptdata");
            SimpleHttpRequest.HttpPostRequest(GetValidateUrl(), null, receiptData, OnPostResponse);
        }

        #endregion
    }
}