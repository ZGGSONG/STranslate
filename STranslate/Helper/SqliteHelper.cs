using SQLite;
using STranslate.Model;
using STranslate.ViewModel;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace STranslate.Helper
{
    public class SqliteHelper : IDisposable
    {
        public SqliteHelper()
        {
            _sqlDB = new SQLiteConnection(_dbName);
            _sqlDB.CreateTable<SqliteModel>();
        }

        public void Insert(DateTime time, string source, string target, LanguageEnum sourceLang, LanguageEnum targetLang, string api)
        {
            try
            {
                var model = new SqliteModel
                {
                    Time = time,
                    SourceText = source,
                    TargetText = target,
                    SourceLang = sourceLang.ToString(),
                    TargetLang = targetLang.ToString(),
                    Api = api,
                };

                //查询最大数量
                var count = _sqlDB.Table<SqliteModel>().Count();
                if (count >= SettingsVM.Instance.MaxHistoryCount)
                {
                    _sqlDB.Execute("delete from histories where id in (select id from histories limit (?))"
                        , count + 1 - SettingsVM.Instance.MaxHistoryCount);
                }
                //手动切换目标语言强制插入替换
                _sqlDB.Table<SqliteModel>()
                    .Where(x => x.SourceText.Equals(model.SourceText))
                    .ToList().ForEach(x =>
                    {
                        _sqlDB.Delete(x);
                    });
                _sqlDB.Insert(model);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"本地记录插入错误\n{ex.Message}");
            }
        }

        /// <summary>
        /// 查询记录
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public string Query(string str)
        {
            var query = _sqlDB.Table<SqliteModel>().Where(x => x.SourceText.Equals(str));
            if (query.Count() <= 0)
            {
                return string.Empty;
            }
            else//如果超过一个删除多余的
            {
                var tmp = query.ToList();
                for (int i = 1; i < tmp.Count; i++)
                {
                    _sqlDB.Delete(tmp[i]);
                }
                return query.First().TargetText;
            }
        }

        public void Dispose()
        {
            _sqlDB.Close();
        }

        private readonly SQLiteConnection _sqlDB;

        /// <summary>
        /// 数据文件
        /// </summary>
        private static string _dbName => $"{_ApplicationData}\\{_AppName.ToLower()}.db";

        /// <summary>
        /// C:\Users\user\AppData\Local\STranslate
        /// </summary>
        private static string _ApplicationData => $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\{_AppName}";
        private static readonly string _AppName = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location);
    }
}
