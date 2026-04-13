namespace BoxCanvasDesigner.Export;

/// <summary>
/// 3D打印技术
/// </summary>
public enum PrintTech3D
{
    FDM,   // 最常见桌面级
    SLA,   // 光固化树脂
    SLS,   // 粉末/尼龙
    MJF    // 多射流熔融
}

/// <summary>
/// 3D打印材料
/// </summary>
public record PrintMaterial3D(
    string Name,
    string SimulatesPackaging,
    string Properties,
    string UseCase
);

/// <summary>
/// 打印平台尺寸
/// </summary>
public record PrintBedSize(string PrinterName, float X_MM, float Y_MM, float Z_MM);

/// <summary>
/// 壁厚规格
/// </summary>
public record WallThicknessSpec(float MinMM, float RecommendedMM, float ToleranceMM);

/// <summary>
/// 3D打印导出配置
/// </summary>
public record Print3DConfig(
    PrintTech3D Technology,
    float WallThicknessOverrideMM,
    float TabClearanceOffsetMM,
    float ScaleRatio
);

/// <summary>
/// 3D打印导出器（文档3.3-3.4节）
/// 壁厚缩放、打印预设、平台检查
/// </summary>
public class Print3DExporter
{
    /// <summary>
    /// 各打印技术壁厚规格
    /// </summary>
    public static readonly Dictionary<PrintTech3D, WallThicknessSpec> WallThicknessSpecs = new()
    {
        [PrintTech3D.FDM] = new(0.8f, 1.5f, 0.5f),
        [PrintTech3D.SLA] = new(0.5f, 1.2f, 0.1f),
        [PrintTech3D.SLS] = new(0.7f, 1.2f, 0.2f),
        [PrintTech3D.MJF] = new(0.5f, 1.0f, 0.2f)
    };

    /// <summary>
    /// 常见打印平台尺寸
    /// </summary>
    public static readonly PrintBedSize[] CommonPrinters = new[]
    {
        new PrintBedSize("Ender 3 (入门级)", 220, 220, 250),
        new PrintBedSize("Prusa MK4 (中端)", 250, 210, 210),
        new PrintBedSize("Bambu X1C (大幅面)", 300, 300, 400),
        new PrintBedSize("Raise3D (工业级)", 330, 240, 300)
    };

    /// <summary>
    /// 3D打印材料对照表
    /// </summary>
    public static readonly PrintMaterial3D[] Materials = new[]
    {
        new PrintMaterial3D("PLA", "通用原型", "低成本、易打印、硬脆", "快速尺寸验证"),
        new PrintMaterial3D("PETG", "PET透明塑料盒", "透明、韧性好、食品安全", "透明包装正式打样"),
        new PrintMaterial3D("TPU", "吸塑托盘、柔性内衬", "柔性、弹性好", "内衬/缓冲结构验证"),
        new PrintMaterial3D("ABS", "硬质塑料容器", "耐冲击、可后处理", "功能性原型"),
        new PrintMaterial3D("尼龙(PA)", "工程塑料件", "高强度、耐磨", "铰链/卡扣结构验证"),
        new PrintMaterial3D("树脂(SLA)", "高精度小件", "表面光滑、精度高", "精细结构、透明件")
    };

    /// <summary>
    /// 将纸厚缩放为可打印壁厚
    /// </summary>
    public static float ScaleWallThickness(float paperThicknessMM, PrintTech3D tech)
    {
        var spec = WallThicknessSpecs[tech];
        // 纸厚(0.3-6mm) → 打印壁厚(spec.Recommended)
        return Math.Max(spec.MinMM, Math.Max(paperThicknessMM, spec.RecommendedMM));
    }

    /// <summary>
    /// 检查盒子是否超出打印平台
    /// </summary>
    public static (bool fits, PrintBedSize? printer, string? suggestion) CheckPrintBedFit(
        BoxParameters parameters, PrintBedSize? selectedPrinter = null)
    {
        float L = parameters.LengthMM;
        float W = parameters.WidthMM;
        float H = parameters.HeightMM;

        var printersToCheck = selectedPrinter != null
            ? new[] { selectedPrinter }
            : CommonPrinters;

        foreach (var printer in printersToCheck)
        {
            if (L <= printer.X_MM && W <= printer.Y_MM && H <= printer.Z_MM)
                return (true, printer, null);
        }

        // 建议缩放或分割
        float maxDim = Math.Max(L, Math.Max(W, H));
        float largestBed = CommonPrinters.Max(p => Math.Max(p.X_MM, Math.Max(p.Y_MM, p.Z_MM)));
        float suggestedScale = largestBed / maxDim * 0.9f;

        return (false, null,
            $"盒子尺寸({L}x{W}x{H}mm)超出打印平台，建议缩比{suggestedScale:F1}:1或分割打印");
    }

    /// <summary>
    /// 获取打印配置
    /// </summary>
    public static Print3DConfig GetDefaultConfig(PrintTech3D tech)
    {
        var spec = WallThicknessSpecs[tech];
        return new Print3DConfig(tech, spec.RecommendedMM, 0.4f, 1.0f);
    }
}
