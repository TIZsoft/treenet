using System.Collections.Generic;

namespace Tizsoft.Database.Helper_Interface
{
    public interface IMySqlStoredProcedureHelper
    {
        string Function { get; }
        IList<KeyValuePair<string, object>> Parameters();
    }
}