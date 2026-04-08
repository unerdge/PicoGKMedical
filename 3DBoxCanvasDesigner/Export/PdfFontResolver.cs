using PdfSharp.Fonts;

namespace BoxCanvasDesigner.Export;

/// <summary>
/// PDF字体解析器 - 使用PDF内置字体
/// </summary>
public class PdfFontResolver : IFontResolver
{
    public byte[]? GetFont(string faceName)
    {
        // 对于PDF内置字体，返回null表示使用PDF的14种标准字体
        // 这些字体不需要嵌入到PDF中
        return null;
    }

    public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        // 将字体族名称映射到PDF标准字体
        string faceName = familyName.ToLower() switch
        {
            "helvetica" when isBold && isItalic => "Helvetica-BoldOblique",
            "helvetica" when isBold => "Helvetica-Bold",
            "helvetica" when isItalic => "Helvetica-Oblique",
            "helvetica" => "Helvetica",
            "times" or "times-roman" when isBold && isItalic => "Times-BoldItalic",
            "times" or "times-roman" when isBold => "Times-Bold",
            "times" or "times-roman" when isItalic => "Times-Italic",
            "times" or "times-roman" => "Times-Roman",
            "courier" when isBold && isItalic => "Courier-BoldOblique",
            "courier" when isBold => "Courier-Bold",
            "courier" when isItalic => "Courier-Oblique",
            "courier" => "Courier",
            _ => "Helvetica" // 默认字体
        };

        // 返回字体信息
        // mustSimulateBold和mustSimulateItalic设为false，因为我们使用的是真实的PDF字体变体
        return new FontResolverInfo(faceName, mustSimulateBold: false, mustSimulateItalic: false);
    }
}
