using System.Collections.Generic;

namespace TestFormApp.Schema
{
    public class TreasureTable
    {
        public const string Name = "treasure";
        public const string GuidColumn = "guid";
        public const string IdColumn = "id";
        public const string LevelColumn = "level";

        public static List<string> Columns = new List<string>()
        {
            GuidColumn,
            IdColumn,
            LevelColumn
        };
    }
}