using Newtonsoft.Json.Linq;

namespace TestFormApp
{
    public interface IJsonCommand
    {
        void Do(JObject jObject);
    }
}