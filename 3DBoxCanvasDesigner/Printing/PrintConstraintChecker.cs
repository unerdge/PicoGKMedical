namespace BoxCanvasDesigner.Printing;

/// <summary>
/// 印刷工艺类型（详细版）
/// </summary>
public enum PrintTechnology
{
    Offset,    // 胶印
    Flexo,     // 柔印
    Digital    // 数码印刷
}

/// <summary>
/// 印刷约束检查结果
/// </summary>
public record PrintCheckResult(
    bool IsValid,
    List<string> Errors,
    List<string> Warnings
);

/// <summary>
/// 印刷工艺约束规则引擎（文档5.3节）
/// 内置印刷工艺的物理限制，实时校验并提示
/// </summary>
public class PrintConstraintChecker
{
    // 最小正文字号 (pt)
    private static readonly Dictionary<PrintTechnology, float> MinFontSize = new()
    {
        [PrintTechnology.Offset] = 6f,
        [PrintTechnology.Flexo] = 8f,
        [PrintTechnology.Digital] = 5f
    };

    // 最小正向线宽 (mm)
    private static readonly Dictionary<PrintTechnology, float> MinLineWidth = new()
    {
        [PrintTechnology.Offset] = 0.15f,
        [PrintTechnology.Flexo] = 0.25f,
        [PrintTechnology.Digital] = 0.1f
    };

    // 最小反白线宽 (mm)
    private static readonly Dictionary<PrintTechnology, float> MinReverseLineWidth = new()
    {
        [PrintTechnology.Offset] = 0.25f,
        [PrintTechnology.Flexo] = 0.4f,
        [PrintTechnology.Digital] = 0.2f
    };

    // 最小反白文字 (pt)
    private static readonly Dictionary<PrintTechnology, float> MinReverseFontSize = new()
    {
        [PrintTechnology.Offset] = 7f,
        [PrintTechnology.Flexo] = 9f,
        [PrintTechnology.Digital] = 6f
    };

    /// <summary>
    /// 检查字号是否满足印刷要求
    /// </summary>
    public static PrintCheckResult CheckFontSize(PrintTechnology tech, float fontSizePt, bool isReverse = false)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        float minSize = isReverse ? MinReverseFontSize[tech] : MinFontSize[tech];

        if (fontSizePt < minSize)
            errors.Add($"{tech}工艺{(isReverse ? "反白" : "")}最小字号为{minSize}pt，当前{fontSizePt}pt");
        else if (fontSizePt < minSize * 1.2f)
            warnings.Add($"字号{fontSizePt}pt接近{tech}工艺最小值{minSize}pt，建议增大");

        return new PrintCheckResult(errors.Count == 0, errors, warnings);
    }

    /// <summary>
    /// 检查线宽是否满足印刷要求
    /// </summary>
    public static PrintCheckResult CheckLineWidth(PrintTechnology tech, float lineWidthMM, bool isReverse = false)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        float minWidth = isReverse ? MinReverseLineWidth[tech] : MinLineWidth[tech];

        if (lineWidthMM < minWidth)
            errors.Add($"{tech}工艺{(isReverse ? "反白" : "")}最小线宽为{minWidth}mm，当前{lineWidthMM}mm");

        return new PrintCheckResult(errors.Count == 0, errors, warnings);
    }

    /// <summary>
    /// 检查陷印设置（Trapping）
    /// </summary>
    public static PrintCheckResult CheckTrapping(float trappingMM)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        if (trappingMM < 0.1f)
            errors.Add($"陷印量{trappingMM}mm不足，最小需要0.1mm");
        else if (trappingMM > 0.4f)
            warnings.Add($"陷印量{trappingMM}mm偏大，建议0.1-0.4mm");

        return new PrintCheckResult(errors.Count == 0, errors, warnings);
    }

    /// <summary>
    /// 检查出血区和安全区
    /// </summary>
    public static PrintCheckResult CheckBleedAndSafety(
        float bleedMM, float safetyMarginMM,
        float elementDistanceFromEdgeMM)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        if (bleedMM < 3f)
            errors.Add($"出血量{bleedMM}mm不足，标准为3mm");

        if (safetyMarginMM < 3f)
            warnings.Add($"安全区{safetyMarginMM}mm偏小，建议≥3mm");

        if (elementDistanceFromEdgeMM < safetyMarginMM)
            warnings.Add("关键元素（文字/Logo）位于安全区外，可能被裁切");

        return new PrintCheckResult(errors.Count == 0, errors, warnings);
    }

    /// <summary>
    /// 检查套印精度
    /// </summary>
    public static PrintCheckResult CheckRegistration(float registrationErrorMM)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        if (registrationErrorMM > 0.3f)
            errors.Add($"套印误差{registrationErrorMM}mm超标，允许范围±0.1-0.2mm");
        else if (registrationErrorMM > 0.2f)
            warnings.Add($"套印误差{registrationErrorMM}mm偏大，建议≤0.2mm");

        return new PrintCheckResult(errors.Count == 0, errors, warnings);
    }
}
