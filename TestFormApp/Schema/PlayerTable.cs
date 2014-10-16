using System.Collections.Generic;

namespace TestFormApp.Schema
{
    public class PlayerTable
    {
        public const string Name = "player";
        public const string GuidColumn = "guid";
        public const string DiamondColumn = "diamond";
        public const string MoneyColumn = "money";
        public const string ApColumn = "ap";
        public const string ExpColumn = "exp";
        public const string LevelColumn = "level";
        public const string ScoreColumn = "score";
        public const string Attr1Column = "attr1";
        public const string Attr2Column = "attr2";
        public const string Attr3Column = "attr3";
        public const string Item1Column = "item1";
        public const string Item2Column = "item2";
        public const string Item3Column = "item3";
        public const string Item4Column = "item4";
        public const string Item5Column = "item5";
        public const string Item6Column = "item6";
        public const string LatestTimeColumn = "latesttime";

        public static List<string> AttrColumns = new List<string>()
        {
            Attr1Column,
            Attr2Column,
            Attr3Column
        };

        public static List<string> ItemColumns = new List<string>()
        {
            Item1Column,
            Item2Column,
            Item3Column,
            Item4Column,
            Item5Column,
            Item6Column
        };

        public static List<string> WriteBackColumns = new List<string>()
        {
            DiamondColumn,
            MoneyColumn,
            ApColumn,
            ExpColumn,
            LevelColumn,
            ScoreColumn,
            Attr1Column,
            Attr2Column,
            Attr3Column,
            Item1Column,
            Item2Column,
            Item3Column,
            Item4Column,
            Item5Column,
            Item6Column
        };

        public static List<string> Columns = new List<string>()
        {
            GuidColumn,
            DiamondColumn,
            MoneyColumn,
            ApColumn,
            ExpColumn,
            LevelColumn,
            ScoreColumn,
            Attr1Column,
            Attr2Column,
            Attr3Column,
            Item1Column,
            Item2Column,
            Item3Column,
            Item4Column,
            Item5Column,
            Item6Column,
            LatestTimeColumn
        };
    }
}