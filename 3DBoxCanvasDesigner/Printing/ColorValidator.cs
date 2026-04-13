namespace BoxCanvasDesigner.Printing;

/// <summary>
/// 颜色模式
/// </summary>
public enum ColorMode
{
    CMYK,       // 印刷标准四色
    RGB,        // 屏幕色彩（需转换）
    Pantone,    // 专色
    Metallic    // 金属色（金/银）
}

/// <summary>
/// 颜色验证结果
/// </summary>
public record ColorCheckResult(
    bool IsValid,
    List<string> Issues
);

/// <summary>
/// 颜色与色彩模式验证器（文档5.3.4节）
/// </summary>
public class ColorValidator
{
    /// <summary>
    /// 检查颜色模式是否适合印刷
    /// </summary>
    public static ColorCheckResult ValidateColorMode(ColorMode mode, bool isForPrint = true)
    {
        var issues = new List<string>();

        if (isForPrint && mode == ColorMode.RGB)
            issues.Add("印刷文件必须为CMYK模式，当前为RGB，导出时需自动转换（可能有色差）");

        return new ColorCheckResult(issues.Count == 0, issues);
    }

    /// <summary>
    /// 检查叠印设置
    /// </summary>
    public static ColorCheckResult ValidateOverprint(bool isBlackText, bool isWhite, bool overprintEnabled)
    {
        var issues = new List<string>();

        if (isBlackText && !overprintEnabled)
            issues.Add("黑色文字应设为叠印（Overprint），避免出现露白");

        if (isWhite && overprintEnabled)
            issues.Add("白色不可设为叠印");

        return new ColorCheckResult(issues.Count == 0, issues);
    }

    /// <summary>
    /// 检查专色设置
    /// </summary>
    public static ColorCheckResult ValidateSpotColor(ColorMode mode, bool hasSeparatePlate)
    {
        var issues = new List<string>();

        if (mode == ColorMode.Pantone && !hasSeparatePlate)
            issues.Add("专色（Pantone）需单独分色输出");

        if (mode == ColorMode.Metallic && !hasSeparatePlate)
            issues.Add("金属色（金/银）需标注为专色版");

        return new ColorCheckResult(issues.Count == 0, issues);
    }

    /// <summary>
    /// 检查是否需要白墨打底
    /// </summary>
    public static ColorCheckResult CheckWhiteInkBase(bool isDarkSubstrate, bool hasWhiteInkLayer)
    {
        var issues = new List<string>();

        if (isDarkSubstrate && !hasWhiteInkLayer)
            issues.Add("深色材料（如牛皮纸）上印刷需白墨打底层");

        return new ColorCheckResult(issues.Count == 0, issues);
    }

    /// <summary>
    /// 检查色差容差
    /// </summary>
    public static ColorCheckResult CheckColorDifference(float deltaE)
    {
        var issues = new List<string>();

        if (deltaE > 3f)
            issues.Add($"色差ΔE={deltaE:F1}超标（ISO 12647标准ΔE≤2-3）");
        else if (deltaE > 2f)
            issues.Add($"色差ΔE={deltaE:F1}接近上限，建议校色");

        return new ColorCheckResult(deltaE <= 3f, issues);
    }
}
