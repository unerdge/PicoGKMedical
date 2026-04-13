using System.Numerics;
using BoxCanvasDesigner.Dieline;

namespace BoxCanvasDesigner.Printing;

/// <summary>
/// 标准纸张尺寸
/// </summary>
public record SheetSize(string Name, float WidthMM, float HeightMM);

/// <summary>
/// 排版结果
/// </summary>
public record NestingResult(
    SheetSize Sheet,
    int CountPerSheet,
    int Rows,
    int Columns,
    float MaterialUtilization,
    bool GrainDirectionOptimal,
    List<string> Notes
);

/// <summary>
/// 排版/拼版优化器（文档7.1节）
/// 材料利用率优化算法
/// </summary>
public class NestingOptimizer
{
    /// <summary>
    /// 标准纸张尺寸库
    /// </summary>
    public static readonly SheetSize[] StandardSheets = new[]
    {
        new SheetSize("正度", 787f, 1092f),
        new SheetSize("大度", 889f, 1194f),
        new SheetSize("瓦楞卷宽1000", 1000f, 2000f),
        new SheetSize("瓦楞卷宽1100", 1100f, 2000f),
        new SheetSize("瓦楞卷宽1200", 1200f, 2000f),
        new SheetSize("瓦楞卷宽1400", 1400f, 2000f)
    };

    private const float GRIPPER_MARGIN_MM = 12f;   // 咬口位
    private const float MIN_GAP_MM = 3f;            // 相邻刀版间距

    /// <summary>
    /// 优化排版方案
    /// </summary>
    public static NestingResult OptimizeNesting(DielineData dieline, SheetSize? preferredSheet = null)
    {
        float dieW = dieline.Bounds.Width + MIN_GAP_MM;
        float dieH = dieline.Bounds.Height + MIN_GAP_MM;

        SheetSize bestSheet = preferredSheet ?? StandardSheets[0];
        NestingResult? bestResult = null;

        var sheetsToTry = preferredSheet != null ? new[] { preferredSheet } : StandardSheets;

        foreach (var sheet in sheetsToTry)
        {
            // 考虑咬口位
            float usableW = sheet.WidthMM - GRIPPER_MARGIN_MM;
            float usableH = sheet.HeightMM;

            // 尝试两种方向
            var normal = TryLayout(usableW, usableH, dieW, dieH, sheet);
            var rotated = TryLayout(usableW, usableH, dieH, dieW, sheet);

            var better = normal.MaterialUtilization >= rotated.MaterialUtilization ? normal : rotated;

            if (bestResult == null || better.MaterialUtilization > bestResult.MaterialUtilization)
            {
                bestResult = better;
                bestSheet = sheet;
            }
        }

        return bestResult ?? new NestingResult(bestSheet, 0, 0, 0, 0f, false, new() { "无法排版" });
    }

    private static NestingResult TryLayout(float sheetW, float sheetH,
        float dieW, float dieH, SheetSize sheet)
    {
        int cols = (int)(sheetW / dieW);
        int rows = (int)(sheetH / dieH);
        int count = cols * rows;

        float usedArea = count * dieW * dieH;
        float totalArea = sheet.WidthMM * sheet.HeightMM;
        float utilization = usedArea / totalArea;

        var notes = new List<string>();

        if (utilization < 0.5f)
            notes.Add("材料利用率偏低，建议调整刀版尺寸或更换纸张规格");

        // 纸纹方向检查（简化：高度方向应沿纸纹）
        bool grainOptimal = dieH < dieW;

        return new NestingResult(sheet, count, rows, cols, utilization, grainOptimal, notes);
    }

    /// <summary>
    /// 获取推荐纸张尺寸
    /// </summary>
    public static SheetSize RecommendSheetSize(DielineData dieline)
    {
        var result = OptimizeNesting(dieline);
        return result.Sheet;
    }
}
