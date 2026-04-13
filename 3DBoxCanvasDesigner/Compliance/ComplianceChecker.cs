using BoxCanvasDesigner.Physics;

namespace BoxCanvasDesigner.Compliance;

/// <summary>
/// 合规认证类型
/// </summary>
public enum CertificationType
{
    FSC,                  // FSC森林认证
    FoodContactSafety,    // 食品接触材料安全
    GBT_Standard,         // 中国GB/T标准
    EU_Packaging,         // 欧盟包装法规
    EPR,                  // 生产者延伸责任
    PlasticRestriction    // 限塑令
}

/// <summary>
/// 合规检查条目
/// </summary>
public record ComplianceItem(
    CertificationType Type,
    bool IsCompliant,
    string Description,
    string? Suggestion
);

/// <summary>
/// 合规认证检查器（文档11.1节）
/// 在设计阶段自动检查合规性
/// </summary>
public class ComplianceChecker
{
    /// <summary>
    /// 全面合规检查
    /// </summary>
    public static List<ComplianceItem> CheckAll(
        BoxParameters parameters,
        MaterialInfo material,
        bool isFoodProduct,
        bool isExportToEU,
        bool isPlasticPackaging)
    {
        var results = new List<ComplianceItem>();

        results.Add(CheckFSC(material));
        results.Add(CheckFoodSafety(material, isFoodProduct));
        results.Add(CheckGBTStandard(parameters, material));
        if (isExportToEU)
            results.Add(CheckEUPackaging(material));
        results.Add(CheckEPR(material, parameters));
        if (isPlasticPackaging)
            results.Add(CheckPlasticRestriction());

        return results;
    }

    /// <summary>
    /// FSC认证检查
    /// </summary>
    public static ComplianceItem CheckFSC(MaterialInfo material)
    {
        // 纸质材料应有FSC认证
        bool isPaper = material.Type is MaterialType.WhiteCardboard
            or MaterialType.GrayCardboard
            or MaterialType.KraftPaper
            or MaterialType.CorrugatedE
            or MaterialType.CorrugatedB
            or MaterialType.CorrugatedBC;

        return new ComplianceItem(
            CertificationType.FSC,
            !isPaper, // 默认假设未认证，实际需查材料库
            isPaper ? "纸质材料建议使用FSC认证原料" : "非纸质材料不适用FSC",
            isPaper ? "选择FSC认证供应商可提升环保评分" : null
        );
    }

    /// <summary>
    /// 食品接触材料安全检查
    /// </summary>
    public static ComplianceItem CheckFoodSafety(MaterialInfo material, bool isFoodProduct)
    {
        if (!isFoodProduct)
            return new ComplianceItem(CertificationType.FoodContactSafety, true, "非食品类产品，不适用", null);

        // 灰底白板纸含再生纤维，不适合直接接触食品
        bool isSafe = material.Type != MaterialType.GrayCardboard;

        return new ComplianceItem(
            CertificationType.FoodContactSafety,
            isSafe,
            isSafe ? "材料符合食品接触安全要求" : "灰底白板纸含再生纤维，不适合直接接触食品",
            isSafe ? null : "建议使用食品级白卡纸(SBS)或牛皮纸"
        );
    }

    /// <summary>
    /// GB/T标准检查
    /// </summary>
    public static ComplianceItem CheckGBTStandard(BoxParameters parameters, MaterialInfo material)
    {
        bool isCorrugated = material.Type is MaterialType.CorrugatedE
            or MaterialType.CorrugatedB
            or MaterialType.CorrugatedBC;

        string standard = isCorrugated ? "GB/T 6543（瓦楞纸箱）" : "GB/T 12025";

        // 简化检查：尺寸在合理范围内
        bool isCompliant = parameters.LengthMM >= 50 && parameters.LengthMM <= 1500 &&
                          parameters.WidthMM >= 50 && parameters.WidthMM <= 1200 &&
                          parameters.HeightMM >= 30 && parameters.HeightMM <= 1000;

        return new ComplianceItem(
            CertificationType.GBT_Standard,
            isCompliant,
            isCompliant ? $"尺寸符合{standard}" : $"尺寸可能超出{standard}标准范围",
            isCompliant ? null : "建议校验具体标准条款"
        );
    }

    /// <summary>
    /// 欧盟包装法规检查
    /// </summary>
    public static ComplianceItem CheckEUPackaging(MaterialInfo material)
    {
        // 需要可回收性标识和材料分类标识
        return new ComplianceItem(
            CertificationType.EU_Packaging,
            true,
            "需在包装上标注可回收标识和材料编码",
            "添加循环箭头标识 + 数字编码"
        );
    }

    /// <summary>
    /// EPR费用预估
    /// </summary>
    public static ComplianceItem CheckEPR(MaterialInfo material, BoxParameters parameters)
    {
        // 简化估算：按材料重量计算
        float L = parameters.LengthMM / 1000f;
        float W = parameters.WidthMM / 1000f;
        float H = parameters.HeightMM / 1000f;
        float areaSqM = 2 * (L * W + L * H + W * H) * 1.5f;
        float weightKg = areaSqM * material.WeightGSM / 1000f;

        // 中国EPR约0.3-0.8元/kg
        float eprCost = weightKg * 0.5f;

        return new ComplianceItem(
            CertificationType.EPR,
            true,
            $"EPR费用预估：约¥{eprCost:F3}/个（包装重量约{weightKg * 1000:F0}g）",
            null
        );
    }

    /// <summary>
    /// 限塑令检查
    /// </summary>
    public static ComplianceItem CheckPlasticRestriction()
    {
        return new ComplianceItem(
            CertificationType.PlasticRestriction,
            false,
            "一次性塑料包装受限塑令约束",
            "建议使用纸质替代方案以符合法规"
        );
    }
}
