namespace STranslate.Model;

/// <summary>
/// 外部调用Messenger
/// </summary>
/// <param name="ECAction"></param>
/// <param name="Content"></param>
/// <param name="internalScreenshot"></param>
public record ExternalCallMessenger(ExternalCallAction ECAction, string Content, bool internalScreenshot);
