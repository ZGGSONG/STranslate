using Dapper.Contrib.Extensions;
using System;
using System.Windows.Documents;

namespace STranslate.Model
{
    [Table("History")]
    public class HistoryModel
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 记录时间
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// 源语言
        /// </summary>
        public string SourceLang { get; set; } = "";

        /// <summary>
        /// 目标语言
        /// </summary>
        public string TargetLang { get; set; } = "";

        /// <summary>
        /// 需翻译内容
        /// </summary>
        public string SourceText { get; set; } = "";

        /// <summary>
        /// 收藏
        /// </summary>
        public bool Favorite { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; } = "";

        /// <summary>
        /// 服务
        /// </summary>
        public string Data { get; set; } = "";
    }
}
