﻿using System.Collections.Generic;
using Tizsoft.Treenet;

namespace TestFormApp
{
    public struct FacebookValidateArgs
    {
        public Connection Connection { get; set; }
        public string FbToken { get; set; }
        public Dictionary<string, object> Response { get; set; }
    }
}