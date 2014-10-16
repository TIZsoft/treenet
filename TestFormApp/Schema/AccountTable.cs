using System.Collections.Generic;

namespace TestFormApp.Schema
{
    public class AccountTable
    {
        public const string Name = "account";
        public const string GuidColumn = "guid";
        public const string FbIdColumn = "fb_id";
        public const string CreateTimeColumn = "createtime";

        public static List<string> Columns = new List<string>()
        {
            GuidColumn,
            FbIdColumn,
            CreateTimeColumn
        };
    }
}