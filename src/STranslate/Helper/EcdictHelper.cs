using Dapper;
using Microsoft.Data.Sqlite;
using STranslate.Model;

namespace STranslate.Helper;

public class EcdictHelper
{
    private static string _connectionString = $"Data Source={ConstStr.ECDICTPath}";

    public static string ConnectionString
    {
        get => _connectionString;
        set => _connectionString = value.StartsWith("Data Source=") ? value : $"Data Source={value}";
    }

    /// <summary>
    ///     查询内容的结果
    /// </summary>
    /// <param name="content"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public static async Task<EcdictModel?> GetECDICTAsync(string content, CancellationToken? token = null)
    {
        using var connection = new SqliteConnection(ConnectionString);
        await connection.OpenAsync(token ?? CancellationToken.None);

        // 构造查询语句
        var query = $"SELECT * FROM stardict WHERE word = '{content.ToLower()}'";
        // 使用 Dapper 执行查询数据的 SQL 语句
        // https://stackoverflow.com/questions/25540793/cancellationtoken-with-async-dapper-methods
        return await connection.QueryFirstOrDefaultAsync<EcdictModel>(new CommandDefinition(query,
            cancellationToken: token ?? CancellationToken.None));
    }
}