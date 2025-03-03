using PunchPal.Core.Models;
using PunchPal.Tools;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Data.SQLite;
using System.IO;
using System.Text.RegularExpressions;

namespace PunchPal.Core.Services
{
    public class OldDataService
    {
        private static bool _imported = false;
        public static void ImportFromOldDatabase(PunchDbContext context)
        {
            if (_imported)
            {
                return;
            }
            try
            {
                var dirs = Directory.EnumerateDirectories(PathTools.RomingPath);
                var appNameLower = PathTools.AppName.ToLower();
                foreach (var directory in dirs)
                {
                    var name = Path.GetFileName(directory).ToLower();
                    if (name == appNameLower || !name.Contains("punch") || !File.Exists(Path.Combine(directory, "data.sqlite")))
                    {
                        continue;
                    }
                    var oldPath = Path.Combine(directory, "data.sqlite");
                    ParseOldDatabase(oldPath, context);
                }
            }
            catch (Exception)
            {
                //ignore
            }
            _imported = true;
        }

        private static void ParseOldDatabase(string oldPath, PunchDbContext context)
        {
            List<(long time, string remark, string userId)> results = new List<(long, string, string)>();
            string connectionString = $"Data Source={oldPath};";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open(); // 打开数据库连接

                string recordTable = FindTableName(connection);
                if (string.IsNullOrWhiteSpace(recordTable))
                {
                    return;
                }
                var (timeCol, remarkCol, userIdCol) = FindColumnName(connection, recordTable);
                if (string.IsNullOrWhiteSpace(timeCol) || string.IsNullOrWhiteSpace(remarkCol) || string.IsNullOrWhiteSpace(userIdCol))
                {
                    return;
                }
                string dataQuery = $"SELECT {timeCol}, {remarkCol}, {userIdCol} FROM {recordTable};";

                using (SQLiteCommand command = new SQLiteCommand(dataQuery, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            long timeValue = reader.GetInt64(0);
                            string recordValue = reader.GetString(1);
                            string userValue = Regex.Replace(reader.GetString(2), "[a-z]", "");
                            results.Add((timeValue, recordValue, userValue));
                        }
                    }
                }
            }
            try
            {
                foreach (var (time, remark, userId) in results)
                {
                    var record = new PunchRecord
                    {
                        PunchTime = time,
                        Remark = remark,
                        UserId = userId,
                        PunchType = PunchRecord.PunchTypeImport
                    };
                    context.PunchRecords.AddOrUpdate(record);
                }
                context.SaveChanges();
            }
            catch (Exception)
            {
                // ignore
            }
        }

        private static string FindTableName(SQLiteConnection connection)
        {
            string query = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%';"; // 排除系统表
            string recordTable = null;
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var tableName = reader.GetString(0);
                        var tableNameLower = reader.GetString(0).ToLower();
                        if (tableNameLower.Contains("punch") || tableNameLower.Contains("record"))
                        {
                            recordTable = tableName;
                            break;
                        }
                    }
                }
            }
            return recordTable;
        }

        private static (string timeCol, string remarkCol, string userId) FindColumnName(SQLiteConnection connection, string tableName)
        {
            // **1. 获取表结构**
            string pragmaQuery = $"PRAGMA table_info({tableName});";
            string timeColumn = null, remarkColumn = null, userColumn = null;

            using (SQLiteCommand command = new SQLiteCommand(pragmaQuery, connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string columnName = reader.GetString(1);
                        string columnNameLower = columnName.ToLower();
                        string columnType = reader.GetString(2).ToUpper();

                        if (columnType == "INTEGER" && (columnNameLower.Contains("time") || columnNameLower.Contains("date")))
                        {
                            timeColumn = columnName;
                        }
                        else if ((columnType == "VARCHAR" || columnType == "NVARCHAR" || columnType == "TEXT") && (columnNameLower.Contains("remark") || columnNameLower.Contains("desc") || columnNameLower.Contains("address")))
                        {
                            remarkColumn = columnName;
                        }
                        else if ((columnType == "VARCHAR" || columnType == "NVARCHAR" || columnType == "TEXT") && (columnNameLower.Contains("user") || columnNameLower.Contains("id") || columnNameLower.Contains("number")))
                        {
                            userColumn = columnName;
                        }
                    }
                }
            }
            return (timeColumn, remarkColumn, userColumn);
        }
    }
}
