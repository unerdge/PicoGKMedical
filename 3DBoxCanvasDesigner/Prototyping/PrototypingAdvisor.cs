using BoxCanvasDesigner.Physics;

namespace BoxCanvasDesigner.Prototyping;

/// <summary>
/// 打样路径类型
/// </summary>
public enum PrototypePath
{
    DiecutPaper,       // 刀版纸张打样（传统纸质）
    Print3DPlastic,    // 3D打印塑料件
    Print3DQuick,      // 3D打印快速原型（缩比）
    HybridRigid        // 混合路径：3D打印盒体+纸张裱糊
}

/// <summary>
/// 打样切割方式
/// </summary>
public enum CuttingMethod
{
    CNCCutter,         // 数控切割机（高精度）
    LaserCutter,       // 激光切割（中等成本）
    ManualCut          // 手工裁切（最低成本）
}

/// <summary>
/// 打样建议
/// </summary>
public record PrototypeRecommendation(
    PrototypePath Path,
    string PathDescription,
    string RecommendedMaterial,
    string EstimatedTime,
    string EstimatedCost,
    CuttingMethod? CuttingMethod,
    List<string> Steps,
    List<string> Notes
);

/// <summary>
/// 双路径打样顾问（文档7节）
/// 根据盒型和材料推荐最合适的打样路径
/// </summary>
public class PrototypingAdvisor
{
    /// <summary>
    /// 推荐打样路径
    /// </summary>
    public static PrototypeRecommendation Recommend(
        BoxParameters parameters,
        MaterialInfo material,
        bool needQuickValidation = false)
    {
        // 决策树
        if (needQuickValidation)
            return QuickPrototype(parameters);

        if (parameters.Type == BoxType.RigidBox)
            return HybridRigidPrototype(parameters, material);

        bool isPaper = material.Type is MaterialType.WhiteCardboard
            or MaterialType.GrayCardboard
            or MaterialType.KraftPaper
            or MaterialType.CorrugatedE
            or MaterialType.CorrugatedB
            or MaterialType.CorrugatedBC;

        return isPaper
            ? PaperDiecutPrototype(parameters, material)
            : Plastic3DPrototype(parameters);
    }

    /// <summary>
    /// 刀版纸张打样
    /// </summary>
    private static PrototypeRecommendation PaperDiecutPrototype(
        BoxParameters parameters, MaterialInfo material)
    {
        bool isCorrugated = material.Type is MaterialType.CorrugatedE
            or MaterialType.CorrugatedB
            or MaterialType.CorrugatedBC;

        string time = isCorrugated ? "1-3小时" : "1-2小时";
        string cost = isCorrugated ? "¥10-30/个" : "¥5-20/个";
        var cutting = isCorrugated ? Prototyping.CuttingMethod.CNCCutter : Prototyping.CuttingMethod.LaserCutter;

        return new PrototypeRecommendation(
            PrototypePath.DiecutPaper,
            "刀版纸张打样 — 使用实际材料裁切折叠",
            material.Name,
            time,
            cost,
            cutting,
            new()
            {
                "导出刀版文件（DXF/PDF）",
                $"使用{cutting}裁切",
                "手工折叠成型",
                "验证尺寸和结构"
            },
            new()
            {
                "刀版文件需区分：切割线（实线）、折叠线（虚线）、压痕线（点线）",
                "标注出血区、安全区、粘合区"
            }
        );
    }

    /// <summary>
    /// 3D打印塑料件
    /// </summary>
    private static PrototypeRecommendation Plastic3DPrototype(BoxParameters parameters)
    {
        return new PrototypeRecommendation(
            PrototypePath.Print3DPlastic,
            "3D打印塑料件 — PETG/PLA材料",
            "PETG（接近PET透明盒质感）",
            "2-4小时",
            "¥20-50/个",
            null,
            new()
            {
                "系统自动缩放壁厚至可打印范围",
                "检查打印平台尺寸",
                "导出STL/3MF文件",
                "切片软件打开 → 打印",
                "后处理（打磨/喷漆）"
            },
            new()
            {
                "壁厚自动从纸厚缩放为1.5mm（FDM默认）",
                "插舌增加0.4mm间隙补偿打印机公差"
            }
        );
    }

    /// <summary>
    /// 快速原型（缩比）
    /// </summary>
    private static PrototypeRecommendation QuickPrototype(BoxParameters parameters)
    {
        return new PrototypeRecommendation(
            PrototypePath.Print3DQuick,
            "3D打印快速原型 — PLA缩比模型",
            "PLA（低成本快速迭代）",
            "0.5-1小时",
            "¥3-10/个",
            null,
            new()
            {
                "自动缩比至打印平台范围",
                "PLA材料快速打印",
                "验证外观和大致尺寸"
            },
            new() { "仅用于尺寸验证，不验证材质和折叠" }
        );
    }

    /// <summary>
    /// 混合路径：天地盖精装盒
    /// </summary>
    private static PrototypeRecommendation HybridRigidPrototype(
        BoxParameters parameters, MaterialInfo material)
    {
        return new PrototypeRecommendation(
            PrototypePath.HybridRigid,
            "混合路径 — 3D打印盒体结构 + 纸张裱糊",
            "PLA盒体 + 特种纸裱糊",
            "3-6小时",
            "¥30-80/个",
            null,
            new()
            {
                "3D打印盒体结构（盒盖+盒底）",
                "手工裁切特种纸",
                "裱糊纸张到盒体外表面",
                "组装验证"
            },
            new()
            {
                "天地盖需分别打印盖和底",
                "盖的内径需略大于底的外径"
            }
        );
    }
}
