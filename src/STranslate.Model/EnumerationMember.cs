namespace STranslate.Model;

public class EnumerationMember
{
    public string Root { get; set; } = "";
    public string Description { get; set; } = "";
    public object? Value { get; set; }
    public bool IsEnabled { get; set; } = true;
}
