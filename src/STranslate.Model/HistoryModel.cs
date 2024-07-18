using Dapper.Contrib.Extensions;

namespace STranslate.Model;

[Table("History")]
public class HistoryModel
{
    [Key] public int Id { get; set; }

    /// <summary>
    ///     记录时间
    /// </summary>
    public DateTime Time { get; set; }

    /// <summary>
    ///     源语言
    /// </summary>
    public string SourceLang { get; set; } = "";

    /// <summary>
    ///     目标语言
    /// </summary>
    public string TargetLang { get; set; } = "";

    /// <summary>
    ///     需翻译内容
    /// </summary>
    public string SourceText { get; set; } = "";

    /// <summary>
    ///     收藏
    /// </summary>
    public bool Favorite { get; set; }

    /// <summary>
    ///     备注
    /// </summary>
    public string Remark { get; set; } = "";

    /// <summary>
    ///     服务
    /// </summary>
    public string Data { get; set; } = "";

    public override bool Equals(object? obj)
    {
        if (obj is HistoryModel other)
            return Id == other.Id
                   && Time == other.Time
                   && SourceLang == other.SourceLang
                   && TargetLang == other.TargetLang
                   && SourceText == other.SourceText
                   && Favorite == other.Favorite
                   && Remark == other.Remark
                   && Data == other.Data;
        return false;
    }

    public override int GetHashCode()
    {
        unchecked // Overflow is fine, just wrap
        {
            var hash = 17;
            // Suitable nullity checks etc, of course :)
            hash = hash * 23 + Id.GetHashCode();
            hash = hash * 23 + Time.GetHashCode();
            hash = hash * 23 + (SourceLang != null ? SourceLang.GetHashCode() : 0);
            hash = hash * 23 + (TargetLang != null ? TargetLang.GetHashCode() : 0);
            hash = hash * 23 + (SourceText != null ? SourceText.GetHashCode() : 0);
            hash = hash * 23 + Favorite.GetHashCode();
            hash = hash * 23 + (Remark != null ? Remark.GetHashCode() : 0);
            hash = hash * 23 + (Data != null ? Data.GetHashCode() : 0);
            return hash;
        }
    }
}