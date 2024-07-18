using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Dapper.Contrib.Extensions;
using TableAttribute = Dapper.Contrib.Extensions.TableAttribute;

namespace STranslate.Model;

[Table("stardict")]
public class EcdictModel
{
    [Key] [Column("id")] public int Id { get; set; }

    /// <summary>
    ///     单词名称
    /// </summary>
    [Column("word")]
    public string Word { get; set; } = "";

    /// <summary>
    ///     单词名称
    /// </summary>
    [Column("sw")]
    public string Sw { get; set; } = "";

    /// <summary>
    ///     音标，以英语英标为主
    /// </summary>
    [Column("phonetic")]
    public string Phonetic { get; set; } = "";

    /// <summary>
    ///     单词释义（英文），每行一个释义
    /// </summary>
    [Column("definition")]
    public string Definition { get; set; } = "";

    /// <summary>
    ///     单词释义（中文），每行一个释义
    /// </summary>
    [Column("translation")]
    public string Translation { get; set; } = "";

    /// <summary>
    ///     词语位置，用 "/" 分割不同位置
    /// </summary>
    [Column("pos")]
    public string Pos { get; set; } = "";

    /// <summary>
    ///     柯林斯星级
    /// </summary>
    [Column("collins")]
    public int Collins { get; set; }

    /// <summary>
    ///     是否是牛津三千核心词汇
    /// </summary>
    [Column("oxford")]
    public int Oxford { get; set; }

    /// <summary>
    ///     字符串标签：zk/中考，gk/高考，cet4/四级 等等标签，空格分割
    /// </summary>
    [Column("tag")]
    public string Tag { get; set; } = "";

    /// <summary>
    ///     英国国家语料库词频顺序
    /// </summary>
    [Column("bnc")]
    public int Bnc { get; set; }

    /// <summary>
    ///     当代语料库词频顺序
    /// </summary>
    [Column("frq")]
    public int Frq { get; set; }

    /// <summary>
    ///     时态复数等变换，使用 "/" 分割不同项目，见后面表格
    /// </summary>
    /// p 过去式（did）
    /// d 过去分词（done）
    /// i 现在分词（doing）
    /// 3 第三人称单数（does）
    /// r 形容词比较级（-er）
    /// t 形容词最高级（-est）
    /// s 名词复数形式
    /// 0 Lemma，如 perceived 的 Lemma 是 perceive
    /// 1 Lemma 的变换形式，比如 s 代表 apples 是其 lemma 的复数形式
    /// <Example>
    ///     d:perceived/p:perceived/3:perceives/i:perceiving
    ///     perceive 的过去式（p） 为 perceived，过去分词（d）为 perceived, 现在分词（'i'）是 perceiving，第三人称单数（3）为 perceives
    /// </Example>
    [Column("exchange")]
    public string Exchange { get; set; } = "";

    /// <summary>
    ///     json 扩展信息，字典形式保存例句（待添加）
    /// </summary>
    [Column("detail")]
    public string Detail { get; set; } = "";

    /// <summary>
    ///     读音音频 url （待添加）
    /// </summary>
    [Column("audio")]
    public string Audio { get; set; } = "";

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{Word}\n");
        if (!string.IsNullOrEmpty(Phonetic))
            sb.AppendLine($"[{Phonetic}]\n");
        if (!string.IsNullOrEmpty(Translation))
            sb.AppendLine($"{Translation}\n");
        if (!string.IsNullOrEmpty(Exchange))
            sb.AppendLine(ExchangeConverter(Exchange));

        return sb.ToString().TrimEnd();
    }


    private string ExchangeConverter(string content)
    {
        var sb = new StringBuilder();
        var exgArray = content.Split('/');
        foreach (var item in exgArray)
        {
            var kvParts = item.Split(':');
            if (kvParts?.Length != 2)
                continue;

            var key = kvParts[0];
            var value = kvParts[1];

            var nKey = key switch
            {
                "p" => "过去式",
                "d" => "过去分词",
                "i" => "现在分词",
                "3" => "第三人称单数",
                "r" => "形容词比较级",
                "t" => "形容词最高级",
                "s" => "复数形式",
                "0" => "Lemma",
                "1" => "Lemma 的变换形式",
                _ => ""
            };
            sb.AppendLine($"{nKey} {value}");
        }

        return sb.ToString();
    }

    private string TagConverter(string content)
    {
        //zk gk cet6 ky toefl ielts gre
        var sb = new StringBuilder();
        var exgArray = content.Split(' ');
        foreach (var item in exgArray)
        {
            var value = item switch
            {
                "zk" => "中",
                "gk" => "高",
                "cet4" => "四",
                "cet6" => "六",
                "ky" => "研",
                "toefl" => "托福",
                "ielts" => "雅思",
                "gre" => "GRE",
                _ => ""
            };
            sb.Append($"{value}/");
        }

        return sb.ToString().TrimEnd('/');
    }
}