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

        public void SetCharacterData(List<Dictionary<string, object>> data)
        {
            if (data == null)
                return;

            Characters.Clear();

            foreach (var character in data)
            {
                if (character != null)
                {
                    Characters.Add(new IdLevelData()
                    {
                        Id = (byte)character[CharacterTable.IdColumn],
                        Level = (byte)character[CharacterTable.LevelColumn]
                    });    
                }
            }
        }

        public void SetSkateBoardData(List<Dictionary<string, object>> data)
        {
            if (data == null)
                return;

            SkateBoards.Clear();

            foreach (var skateboard in data)
            {
                if (skateboard != null)
                {
                    SkateBoards.Add(new IdLevelData()
                    {
                        Id = (byte)skateboard[SkateBoardTable.IdColumn],
                        Level = (byte)skateboard[SkateBoardTable.LevelColumn]
                    });
                }
            }
        }

        public void SetTreasureData(List<Dictionary<string, object>> data)
        {
            if (data == null)
                return;

            Treasures.Clear();

            foreach (var treasure in data)
            {
                if (treasure != null)
                {
                    Treasures.Add(new IdLevelData()
                    {
                        Id = (byte)treasure[TreasureTable.IdColumn],
                        Level = (byte)treasure[TreasureTable.LevelColumn]
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
            List<IdLevelData> source = null;
            switch (type)
            {
                case IdLevelType.Character:
                    source = Characters;
                    break;

                case IdLevelType.Skateboard:
                    source = SkateBoards;
                    break;

                case IdLevelType.Item:
                    source = Items;
                    break;

                case IdLevelType.Treasure:
                    source = Treasures;
                    break;

                default:
                    source = Attributes;
                    break;
            }

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
    }
}