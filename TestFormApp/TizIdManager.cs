using System.IO;
using System.Text;
using Newtonsoft.Json;
using Tizsoft;

namespace TestFormApp
{
    public class TizIdData
    {
        public uint Id;

        public TizIdData(uint id)
        {
            Id = id;
        }
    }

    public class TizIdManager
    {
        const string Filename = "iddata.json";
        TizIdData _data;
        TizId _tizId;

        static string CombineFilePath(string a, string b)
        {
            return string.Format("{0}/{1}", a, b);
        }

        public TizIdManager()
        {
            _tizId = new TizIdIncrement();
        }

        public void Save(string dirPath)
        {
            _data = new TizIdData(_tizId.Current());

            var jsonStr = JsonConvert.SerializeObject(_data);
            File.WriteAllText(CombineFilePath(dirPath, Filename), jsonStr, Encoding.UTF8);
        }

        public void Read(string dirPath)
        {
            if (!File.Exists(CombineFilePath(dirPath, Filename)))
            {
                _data = null;
                return;
            }

            using (var file = File.OpenText(CombineFilePath(dirPath, Filename)))
            {
                var content = file.ReadToEnd();

                if (string.IsNullOrEmpty(content))
                {
                    return;
                }
                _data = JsonConvert.DeserializeObject<TizIdData>(content);
            }
        }
    }
}
