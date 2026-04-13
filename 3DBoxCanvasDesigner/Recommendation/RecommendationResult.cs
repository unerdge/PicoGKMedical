using BoxCanvasDesigner.Physics;

namespace BoxCanvasDesigner.Recommendation;

/// <summary>
/// 方案等级
/// </summary>
public enum SolutionTier
{
    BudgetFriendly,   // 方案A：最省钱
    Recommended,      // 方案B：性价比最优（推荐）
    Premium           // 方案C：高端体验
}

/// <summary>
/// 单个推荐方案
/// </summary>
public record PackagingSolution(
    SolutionTier Tier,
    BoxType BoxType,
    MaterialInfo Material,
    CostEstimator.PrintingMethod Printing,
    int ColorCount,
    CostEstimator.PostProcessing PostProcess,
    CostBreakdown Cost,
    float SustainabilityScore,
    List<string> Reasons
)
{
    public string GetSummary()
    {
        string tierLabel = Tier switch
        {
            SolutionTier.BudgetFriendly => "方案A（最省钱）",
            SolutionTier.Recommended => "方案B（推荐 - 性价比最优）",
            SolutionTier.Premium => "方案C（高端体验）",
            _ => "方案"
        };

        string star = Tier == SolutionTier.Recommended ? " ★" : "";

        return $@"{tierLabel}{star}
  盒型：{BoxType}
  材料：{Material.Name}
  印刷：{Printing} ({ColorCount}色)
  后加工：{PostProcess}
  单价：¥{Cost.UnitCost:F2}/个
  环保评分：{SustainabilityScore:F0}/100";
    }
}

/// <summary>
/// 推荐结果（包含三档方案）
/// </summary>
public record RecommendationResult(
    ProductProfile Input,
    PackagingSolution? BudgetSolution,
    PackagingSolution? RecommendedSolution,
    PackagingSolution? PremiumSolution,
    int TotalCombinationsEvaluated,
    int FeasibleCombinations
)
{
    public string GetFullReport()
    {
        var lines = new List<string>
        {
            "═══════════════════════════════════════",
            "      智能包装推荐报告",
            "═══════════════════════════════════════",
            $"产品：{Input.ProductName}",
            $"尺寸：{Input.LengthCM}×{Input.WidthCM}×{Input.HeightCM} cm",
            $"重量：{Input.WeightKg} kg",
            $"数量：{Input.Quantity} 个",
            $"评估组合：{TotalCombinationsEvaluated} → 可行：{FeasibleCombinations}",
            "───────────────────────────────────────"
        };

        if (BudgetSolution != null)
        {
            lines.Add(BudgetSolution.GetSummary());
            lines.Add("───────────────────────────────────────");
        }
        if (RecommendedSolution != null)
        {
            lines.Add(RecommendedSolution.GetSummary());
            lines.Add("───────────────────────────────────────");
        }
        if (PremiumSolution != null)
        {
            lines.Add(PremiumSolution.GetSummary());
        }

        lines.Add("═══════════════════════════════════════");
        return string.Join("\n", lines);
    }
}
