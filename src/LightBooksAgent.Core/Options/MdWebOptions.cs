namespace LightBooksAgent.Core.Options;

public sealed class MdWebOptions
{
    public const string SectionName = "MdWeb";

    public string ProjectPath { get; set; } = string.Empty;

    public string DefaultTheme { get; set; } = "themes/default";

    public string WeChatTheme { get; set; } = "themes/wechat";

    public string OutputBasePath { get; set; } = "./exports";
}
