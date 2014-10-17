using System;
using System.Collections.Generic;
using TestFormApp.Schema;
using TestFormApp.User;
using Tizsoft;
using Tizsoft.Database;
using Tizsoft.Log;

namespace TestFormApp
{
    public class DatabaseQuery
    {
        DatabaseConnector _dbConnector;

        string GetWriteBackDuplicateOption(UserData.IdLevelType type)
        {
            string result = string.Empty;

            switch (type)
            {
                case UserData.IdLevelType.Character:
                    result = CharacterTable.LevelColumn;
                    break;

                case UserData.IdLevelType.Skateboard:
                    result = SkateBoardTable.LevelColumn;
                    break;

                case UserData.IdLevelType.Treasure:
                    result = TreasureTable.LevelColumn;
                    break;
            }

            return string.Format(@"`{0}`=VALUES(`{0}`)", result);
        }

        public DatabaseQuery(EventArgs args)
        {
            _dbConnector = new DatabaseConnector();
            _dbConnector.Connect(args);
        }

        public UserData GetUserData(string guid)
        {
            return GetUserDataByToken(guid, AccountType.Guid);
        }

        public UserData GetUserData(Guid guid)
        {
            UserData userData;
            return HasUserData(GuidUtil.ToBase64(guid), AccountType.Guid, out userData) ? userData : new UserData();
        }

        public bool HasUserData(string account, AccountType type, out UserData userData)
        {
            userData = new UserData();

            if (_dbConnector.Count(AccountTable.Name, new KeyValuePair<string, string>(AccountColumn(type), account)) == 0)
                return false;

            userData = GetUserDataByToken(account, type);
            return true;
        }

        public UserData CreateNewUser(string guid)
        {
            return CreateNewUser(guid, AccountType.Guid);
        }

        public UserData CreateNewUser(Guid guid)
        {
            return CreateNewUser(GuidUtil.ToBase64(guid), AccountType.Guid);
        }

        public UserData CreateNewUser(string account, AccountType type)
        {
            var guid = type != AccountType.Guid ? GuidUtil.ToBase64(GuidUtil.New()) : account;
            var fieldColumns = new List<string>() { AccountColumn(type) };
            var valueColumns = new List<object>() { account };

            if (type != AccountType.Guid)
            {
                fieldColumns.Add(AccountTable.GuidColumn);
                valueColumns.Add(guid);
            }

            _dbConnector.Create(AccountTable.Name, fieldColumns, valueColumns);
            _dbConnector.Create(PlayerTable.Name, new List<string>() {PlayerTable.GuidColumn}, new List<object>() {guid});
            _dbConnector.Create(CharacterTable.Name, CharacterTable.Columns, new List<object>() {guid, "1", "1"});
            _dbConnector.Create(SkateBoardTable.Name, SkateBoardTable.Columns, new List<object>() {guid, "1", "1"});
            return GetUserDataByToken(account, type);
        }

        string AccountColumn(AccountType type)
        {
            switch (type)
            {
                case AccountType.Facebook:
                    return AccountTable.FbIdColumn;

                default:
                    return AccountTable.GuidColumn;
            }
        }

        public UserData GetUserDataByToken(string account, AccountType type)
        {
            var condition = type == AccountType.Guid
                ? string.Format(@"`{0}`='{1}'", AccountTable.GuidColumn, account)
                : string.Empty;
            var guid = type == AccountType.Guid ? account : string.Empty;
            List<Dictionary<string, object>> sqlResult;
            var result = new UserData();

            try
            {
                if (type != AccountType.Guid)
                {
                    sqlResult = _dbConnector.Request(AccountTable.Name, null,
                        string.Format(@"`{0}`='{1}'", AccountColumn(type), account));

                    if (sqlResult.Count > 0)
                    {
                        guid = (string)sqlResult[0][AccountTable.GuidColumn];
                    }
                    condition = string.Format(@"`{0}`='{1}'", AccountTable.GuidColumn, guid);
                }
                    
                sqlResult = _dbConnector.Request(PlayerTable.Name, null, condition);
                result.SetPlayerData(sqlResult[0]);

                sqlResult = _dbConnector.Request(CharacterTable.Name, null, condition);
                result.SetIdLevelData(sqlResult, UserData.IdLevelType.Character);

                sqlResult = _dbConnector.Request(SkateBoardTable.Name, null, condition);
                result.SetIdLevelData(sqlResult, UserData.IdLevelType.Skateboard);

                sqlResult = _dbConnector.Request(TreasureTable.Name, null, condition);
                result.SetIdLevelData(sqlResult, UserData.IdLevelType.Treasure);
            }
            catch (Exception exception)
            {
                GLogger.Fatal(exception);
                throw;
            }

            return result;
        }

        public void WriteUserData(UserData userData)
        {
            try
            {
                //write player data back
                _dbConnector.Update(PlayerTable.Name, PlayerTable.WriteBackColumns,
                    userData.GetPlayerDataWriteBackList(),
                    string.Format(@"`{0}`='{1}'", PlayerTable.GuidColumn, userData.Guid));

                //write available characters back
                _dbConnector.MultiCreateOnDuplicate(CharacterTable.Name, CharacterTable.Columns,
                    userData.GetIdLevelDataWriteBackList(UserData.IdLevelType.Character),
                    GetWriteBackDuplicateOption(UserData.IdLevelType.Character));

                //write available skateboards back
                _dbConnector.MultiCreateOnDuplicate(SkateBoardTable.Name, SkateBoardTable.Columns,
                    userData.GetIdLevelDataWriteBackList(UserData.IdLevelType.Skateboard),
                    GetWriteBackDuplicateOption(UserData.IdLevelType.Skateboard));

                //write available treasures back
                _dbConnector.MultiCreateOnDuplicate(TreasureTable.Name, TreasureTable.Columns,
                    userData.GetIdLevelDataWriteBackList(UserData.IdLevelType.Treasure),
                    GetWriteBackDuplicateOption(UserData.IdLevelType.Treasure));
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }
    }
}