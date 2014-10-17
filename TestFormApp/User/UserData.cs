using System.Collections.Generic;
using TestFormApp.Config;
using TestFormApp.Schema;

namespace TestFormApp.User
{
    public class UserData
    {
        public enum IdLevelType
        {
            Character,
            Skateboard,
            Item,
            Treasure,
            Attribute
        }

        public string Guid { get; set; }
        public string FbId { get; set; }
        public uint Diamond { get; set; }
        public uint Money { get; set; }
        public byte Ap { get; set; }
        public uint Exp { get; set; }
        public byte Level { get; set; }
        public uint Score { get; set; }
        public List<IdLevelData> Characters { get; private set; }
        public List<IdLevelData> SkateBoards { get; private set; }
        public List<IdLevelData> Items { get; private set; }
        public List<IdLevelData> Treasures { get; private set; }
        public List<IdLevelData> Attributes { get; private set; }

        string GetIdColumn(IdLevelType type)
        {
            switch (type)
            {
                case IdLevelType.Character:
                    return CharacterTable.IdColumn;

                case IdLevelType.Skateboard:
                    return SkateBoardTable.IdColumn;

                default:
                    return TreasureTable.IdColumn;
            }
        }

        string GetLevelColumn(IdLevelType type)
        {
            switch (type)
            {
                case IdLevelType.Character:
                    return CharacterTable.IdColumn;

                case IdLevelType.Skateboard:
                    return SkateBoardTable.IdColumn;

                default:
                    return TreasureTable.IdColumn;
            }
        }

        public UserData()
        {
            Characters = new List<IdLevelData>(GameConfig.MaxCharactersAvailable);
            SkateBoards = new List<IdLevelData>(GameConfig.MaxSkateBoardsAvailable);
            Items = new List<IdLevelData>(GameConfig.MaxItemsAvailable);
            Treasures = new List<IdLevelData>(GameConfig.MaxTreasuresAvailable);
            Attributes = new List<IdLevelData>(GameConfig.MaxAttributeCount);
        }

        public void SetPlayerData(Dictionary<string, object> data)
        {
            if (data == null)
                return;

            Guid = (string) data[PlayerTable.GuidColumn];
            Diamond = (uint) data[PlayerTable.DiamondColumn];
            Money = (uint) data[PlayerTable.MoneyColumn];
            Ap = (byte) data[PlayerTable.ApColumn];
            Exp = (uint) data[PlayerTable.ExpColumn];
            Level = (byte) data[PlayerTable.LevelColumn];
            Score = (uint) data[PlayerTable.ScoreColumn];

            Items.Clear();

            foreach (var itemColumn in PlayerTable.ItemColumns)
            {
                Items.Add(new IdLevelData()
                {
                    Id = (byte) PlayerTable.ItemColumns.IndexOf(itemColumn),
                    Level = (byte) data[itemColumn]
                });
            }

            Attributes.Clear();

            foreach (var attrColumn in PlayerTable.AttrColumns)
            {
                Attributes.Add(new IdLevelData()
                {
                    Id = (byte)PlayerTable.ItemColumns.IndexOf(attrColumn),
                    Level = (byte)data[attrColumn]
                });
            }
        }

        public void SetIdLevelData(List<Dictionary<string, object>> data, IdLevelType type)
        {
            var source = GetIdLevelDataByType(type);

            source.Clear();

            foreach (var idLevelData in data)
            {
                if (idLevelData != null)
                {
                    source.Add(new IdLevelData()
                    {
                        Id = (byte)idLevelData[GetIdColumn(type)],
                        Level = (byte)idLevelData[GetLevelColumn(type)]
                    });
                }
            }
        }

        public List<object> GetPlayerDataWriteBackList()
        {
            var result = new List<object>()
            {
                Diamond,
                Money,
                Ap,
                Exp,
                Level,
                Score
            };

            foreach (var attribute in Attributes)
            {
                result.Add(attribute.Level);
            }

            foreach (var item in Items)
            {
                result.Add(item.Level);
            }

            return result;
        }

        public List<List<object>> GetIdLevelDataWriteBackList(IdLevelType type)
        {
            var result = new List<List<object>>();
            var source = GetIdLevelDataByType(type);

            foreach (var data in source)
            {
                var valueList = new List<object>();
                valueList.Add(Guid);
                valueList.Add(data.Id);
                valueList.Add(data.Level);
                result.Add(valueList);
            }

            return result;
        }

        List<IdLevelData> GetIdLevelDataByType(IdLevelType type)
        {
            switch (type)
            {
                case IdLevelType.Character:
                    return Characters;

                case IdLevelType.Skateboard:
                    return SkateBoards;

                case IdLevelType.Item:
                    return Items;

                case IdLevelType.Treasure:
                    return Treasures;

                default:
                    return Attributes;
            }
        }
    }
}