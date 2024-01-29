using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Data.Sqlite;
using STranslate.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace STranslate.Helper
{
    public class SqlHelper
    {
        #region Asynchronous method

        /// <summary>
        /// 创建数据库
        /// </summary>
        /// <returns></returns>
        public static async Task InitializeDBAsync()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            // 创建表的 SQL 语句
            string createTableSql =
                @"
                CREATE TABLE IF NOT EXISTS History (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Time TEXT NOT NULL,
                    SourceLang TEXT,
                    TargetLang TEXT,
                    SourceText TEXT,
                    Data TEXT,
                    Favorite INTEGER,
                    Remark TEXT
                );
            ";
            await connection.ExecuteAsync(createTableSql);
        }

        /// <summary>
        /// 删除记录
        /// </summary>
        /// <param name="history"></param>
        /// <returns></returns>
        public static async Task<bool> DeleteDataAsync(HistoryModel history)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            return await connection.DeleteAsync(history);
        }

        /// <summary>
        /// 删除所有记录
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> DeleteAllDataAsync()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            return await connection.DeleteAllAsync<HistoryModel>();
        }

        /// <summary>
        /// 插入数据-异步
        /// </summary>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <param name="forceWrite"></param>
        /// <returns></returns>
        public static async Task InsertDataAsync(HistoryModel history, long count, bool forceWrite = false)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var curCount = await connection.QueryFirstOrDefaultAsync<long>("SELECT COUNT(*) FROM History");
            if (curCount > count)
            {
                var sql = @"DELETE FROM History WHERE Id IN (SELECT Id FROM History ORDER BY Id ASC LIMIT @Limit)";

                await connection.ExecuteAsync(sql, new { Limit = curCount - count + 1 });
            }

            if (forceWrite)
            {
                // 使用 Dapper 的 FirstOrDefault 方法进行查询
                var existingHistory = await connection.QueryFirstOrDefaultAsync<HistoryModel>(
                    "SELECT * FROM History WHERE SourceText = @SourceText AND SourceLang = @SourceLang AND TargetLang = @TargetLang",
                    new
                    {
                        history.SourceText,
                        history.SourceLang,
                        history.TargetLang
                    }
                );

                if (existingHistory != null)
                {
                    // 使用 Dapper.Contrib 的 Update 方法更新数据
                    existingHistory.Time = history.Time;
                    existingHistory.Data = history.Data;
                    await connection.UpdateAsync(existingHistory);
                    return;
                }
            }

            // 使用 Dapper.Contrib 的 Insert 方法插入数据
            await connection.InsertAsync(history);
        }

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <param name="content"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static async Task<HistoryModel?> GetDataAsync(string content, string source, string target)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            // 使用 Dapper 执行查询数据的 SQL 语句
            return await connection.QueryFirstOrDefaultAsync<HistoryModel>(
                "SELECT * FROM History WHERE SourceText = @SourceText AND SourceLang = @SourceLang AND TargetLang = @TargetLang",
                new
                {
                    SourceText = content,
                    SourceLang = source,
                    TargetLang = target
                }
            );
        }

        /// <summary>
        /// 查询所有数据
        /// </summary>
        /// <returns></returns>
        public static async Task<IEnumerable<HistoryModel>> GetDataAsync()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            // 使用 Dapper 执行查询数据的 SQL 语句
            return await connection.GetAllAsync<HistoryModel>();
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="pageNum"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<HistoryModel>?> GetDataAsync(int pageNum, int pageSize)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            // 计算起始行号
            int startRow = (pageNum - 1) * pageSize + 1;

            // 使用 Dapper 进行分页查询
            string query = @"SELECT * FROM (SELECT ROW_NUMBER() OVER (ORDER BY Time DESC) AS RowNum, * FROM History) AS p WHERE RowNum BETWEEN @StartRow AND @EndRow";

            return await connection.QueryAsync<HistoryModel>(query, new { StartRow = startRow, EndRow = startRow + pageSize - 1 });
        }

        public static async Task<IEnumerable<HistoryModel>> GetDataCursorPagedAsync(int pageSize, DateTime cursor)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            // 使用 Dapper 进行分页查询
            string query = @"SELECT * FROM History WHERE Time < @Cursor ORDER BY Time DESC LIMIT @PageSize OFFSET 0";

            return await connection.QueryAsync<HistoryModel>(query, new { PageSize = pageSize, Cursor = cursor });
        }

        #endregion Asynchronous method

        #region Synchronous method

        public static void InitializeDB()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            // 创建表的 SQL 语句
            string createTableSql =
                @"
                CREATE TABLE IF NOT EXISTS History (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Time TEXT NOT NULL,
                    SourceLang TEXT,
                    TargetLang TEXT,
                    SourceText TEXT,
                    Data TEXT,
                    Favorite INTEGER,
                    Remark TEXT
                );
            ";
            connection.Execute(createTableSql);
        }

        /// <summary>
        /// 删除记录
        /// </summary>
        /// <param name="history"></param>
        /// <returns></returns>
        public static bool DeleteData(HistoryModel history)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            return connection.Delete(history);
        }

        /// <summary>
        /// 删除所有记录
        /// </summary>
        /// <returns></returns>
        public static bool DeleteAllData()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            return connection.DeleteAll<HistoryModel>();
        }

        /// <summary>
        /// 插入数据
        /// </summary>
        /// <param name="history"></param>
        /// <param name="count"></param>
        /// <param name="forceWrite"></param>
        public static void InsertData(HistoryModel history, long count, bool forceWrite = false)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var curCount = connection.QueryFirstOrDefault<long>("SELECT COUNT(*) FROM History");
            if (curCount > count)
            {
                var sql = @"DELETE FROM History WHERE Id IN (SELECT Id FROM History ORDER BY Id ASC LIMIT @Limit)";

                connection.Execute(sql, new { Limit = curCount - count + 1 });
            }

            if (forceWrite)
            {
                // 使用 Dapper 的 FirstOrDefault 方法进行查询
                var existingHistory = connection.QueryFirstOrDefault<HistoryModel>(
                    "SELECT * FROM History WHERE SourceText = @SourceText AND SourceLang = @SourceLang AND TargetLang = @TargetLang",
                    new
                    {
                        history.SourceText,
                        history.SourceLang,
                        history.TargetLang
                    }
                );

                if (existingHistory != null)
                {
                    // 使用 Dapper.Contrib 的 Update 方法更新数据
                    existingHistory.Time = history.Time;
                    existingHistory.Data = history.Data;
                    connection.Update(existingHistory);
                    return;
                }
            }

            // 使用 Dapper.Contrib 的 Insert 方法插入数据
            connection.Insert(history);
        }

        /// <summary>
        /// 查询所有数据
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<HistoryModel> GetData()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            // 使用 Dapper 执行查询数据的 SQL 语句
            return connection.GetAll<HistoryModel>();
        }

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <param name="content"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static HistoryModel? GetData(string content, string source, string target)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            // 使用 Dapper 执行查询数据的 SQL 语句
            return connection.QueryFirstOrDefault<HistoryModel>(
                "SELECT * FROM History WHERE SourceText = @SourceText AND SourceLang = @SourceLang AND TargetLang = @TargetLang",
                new
                {
                    SourceText = content,
                    SourceLang = source,
                    TargetLang = target
                }
            );
        }

        #endregion Synchronous method

        #region Properties

        /// <summary>
        /// 连接字符串
        /// </summary>
        private static string ConnectionString => $"Data Source={DbName}";

        /// <summary>
        /// 数据文件
        /// </summary>
        private static string DbName => $"{ApplicationPath}\\{AppName.ToLower()}.db";

#if true

        /// <summary>
        /// C:\Users\user\AppData\Local\STranslate
        /// </summary>
        private static string ApplicationPath => $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\{AppName}";
#else
        /// <summary>
        /// 当前目录
        /// </summary>
        private static string ApplicationPath => $"{AppDomain.CurrentDomain.BaseDirectory}";
#endif
        private static readonly string AppName = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly()!.Location);

        #endregion Properties
    }
}
