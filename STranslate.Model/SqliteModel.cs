using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STranslate.Model
{
    [Table("Histories")]
    public class SqliteModel
    {
        [PrimaryKey, AutoIncrement]
        [Column("id")]
        public int Id { get; set; }

        [Column("time")]
        public DateTime Time { get; set; }

        [Column("source_lang")]
        public string SourceLang { get; set; }

        [Column("target_lang")]
        public string TargetLang { get; set; }

        [Column("source_text")]
        public string SourceText { get; set; }

        [Column("target_text")]
        public string TargetText { get; set; }

        [Column("api")]
        public string Api { get; set; }

        [Column("remark")]
        public string Remark { get; set; }
    }
}
