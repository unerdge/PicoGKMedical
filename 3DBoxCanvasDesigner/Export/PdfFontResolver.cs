using PdfSharp.Fonts;

namespace BoxCanvasDesigner.Export;

/// <summary>
/// PDF字体解析器 - 使用PDF内置字体
/// </summary>
public class PdfFontResolver : IFontResolver
{
    public byte[]? GetFont(string faceName)
    {
        // PdfSharp 6.x requires actual font bytes — returning null causes NullReferenceException.
        // Map PDF standard font names to Windows system fonts (Arial ≈ Helvetica substitute).
        string fontsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);

        string fileName = faceName switch
        {
            "Helvetica-Bold"        => "arialbd.ttf",
            "Helvetica-Oblique"     => "ariali.ttf",
            "Helvetica-BoldOblique" => "arialbi.ttf",
            "Times-Roman"           => "times.ttf",
            "Times-Bold"            => "timesbd.ttf",
            "Times-Italic"          => "timesi.ttf",
            "Times-BoldItalic"      => "timesbi.ttf",
            "Courier"               => "cour.ttf",
            "Courier-Bold"          => "courbd.ttf",
            "Courier-Oblique"       => "couri.ttf",
            "Courier-BoldOblique"   => "courbi.ttf",
            _                       => "arial.ttf"  // Helvetica -> Arial
        };

        string fontPath = Path.Combine(fontsFolder, fileName);
        return File.Exists(fontPath) ? File.ReadAllBytes(fontPath) : null;
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
