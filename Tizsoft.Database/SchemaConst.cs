using System.Collections.Generic;

namespace Tizsoft.Database
{
    public class SchemaConst
    {
        //Table constants
        public static string AccountTable = "account";

        //Field constants
        public static string GuidField = "guid";
        public static string FbIdField = "fb_id";
        public static string NameField = "name";
        public static string LevelField = "level";

        //Helper table fields list constants
        public static List<string> AccountFields = new List<string>()
        {
            GuidField,
            FbIdField,
            NameField,
            LevelField
        };
    }
}