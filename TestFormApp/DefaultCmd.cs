using Newtonsoft.Json.Linq;
using Tizsoft.Log;

namespace TestFormApp
{
    public class DefaultCmd : IJsonCommand
    {
        string _functionToken;

        public DefaultCmd(string functionToken)
        {
            _functionToken = functionToken;
        }

        public void Do(JObject jObject)
        {
            GLogger.Warn(string.Format("未定義的function: <color=cyan>{0}</color>", _functionToken));
        }
    }
}