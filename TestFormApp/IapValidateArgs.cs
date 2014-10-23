using System;
using Tizsoft.Treenet.Interface;

namespace TestFormApp
{
    public class IapValidateArgs : EventArgs
    {
        public bool IsSandBox { get; set; }
        //public string Receipt { get; set; }
        public IConnection Connection { get; set; }
    }
}