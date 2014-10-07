using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Tizsoft;
using Tizsoft.IO;

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
        const string DataFilename = "iddata.json";
        readonly TizId _tizId;

        public TizIdManager()
        {
            _tizId = new TizIdIncrement();
        }

        public void Save(string dirPath)
        {
            var data = new TizIdData(_tizId.Current());
            var jsonStr = JsonConvert.SerializeObject(data);
            var filePath = IOUtil.CombinePathAndFile(dirPath, DataFilename);
            if (!IOUtil.CheckAndCreateDirectory(filePath))
            {
                return;
            }

            File.WriteAllText(filePath, jsonStr, Encoding.UTF8);
        }

        public void Read(string dirPath)
        {
            var filePath = IOUtil.CombinePathAndFile(dirPath, DataFilename);
            if (!File.Exists(filePath))
            {
                return;
            }

            using (var file = File.OpenText(filePath))
            {
                var content = file.ReadToEnd();

                if (string.IsNullOrEmpty(content))
                {
                    return;
                }
                var data = JsonConvert.DeserializeObject<TizIdData>(content);
                _tizId.SetCurrentId(data.Id);
            }
        }
    }
}
