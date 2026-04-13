using BoxCanvasDesigner.Physics;

namespace BoxCanvasDesigner.Recommendation;

/// <summary>
/// 智能包装推荐引擎 — 约束求解器
/// 核心逻辑：枚举所有(盒型×材料×工艺)组合 → 硬约束剪枝 → 成本排序 → 三档输出
/// </summary>
public class PackagingSolver
{
    /// <summary>
    /// 内部候选方案
    /// </summary>
    private record Candidate(
        BoxType BoxType,
        MaterialInfo Material,
        CostEstimator.PrintingMethod Printing,
        int ColorCount,
        CostEstimator.PostProcessing PostProcess,
        CostBreakdown Cost,
        float ExperienceScore
    );

    /// <summary>
    /// 求解最优包装方案
    /// </summary>
    public RecommendationResult Solve(ProductProfile profile)
    {
        var (innerL, innerW, innerH) = profile.GetBoxInnerDimensions();

        // Step 1: 展开解空间
        var allBoxTypes = Enum.GetValues<BoxType>();
        var allMaterials = MaterialLibrary.GetAllMaterials();
        var allPrintings = new[] {
            CostEstimator.PrintingMethod.None,
            CostEstimator.PrintingMethod.Flexo,
            CostEstimator.PrintingMethod.Offset,
            CostEstimator.PrintingMethod.Digital
        };
        var allPostProcess = new[] {
            CostEstimator.PostProcessing.None,
            CostEstimator.PostProcessing.Lamination,
            CostEstimator.PostProcessing.UV,
            CostEstimator.PostProcessing.HotStamping
        };

        int totalCombinations = 0;
        var feasible = new List<Candidate>();

        // Step 2: 硬约束剪枝
        foreach (var boxType in allBoxTypes)
        foreach (var material in allMaterials)
        foreach (var printing in allPrintings)
        foreach (var postProcess in allPostProcess)
        {
            totalCombinations++;

            // 约束1：材料承重
            if (profile.WeightKg > material.MaxLoadKg)
                continue;

            // 约束2：食品安全
            if (profile.RequiresFoodSafe && material.Type == MaterialType.GrayCardboard)
                continue;

            // 约束3：印刷可行性 — 无印刷则不加后加工
            if (printing == CostEstimator.PrintingMethod.None &&
                postProcess != CostEstimator.PostProcessing.None)
                continue;

            // 约束4：数量与印刷匹配
            if (printing == CostEstimator.PrintingMethod.Offset && profile.Quantity < 500)
                continue;

            // Step 3: 成本计算
            var boxParams = new BoxParameters(boxType, innerL, innerW, innerH, material.ThicknessMM);
            int colorCount = printing == CostEstimator.PrintingMethod.None ? 0 :
                            printing == CostEstimator.PrintingMethod.Flexo ? 2 : 4;

            var cost = CostEstimator.CalculateCost(
                boxParams, material, profile.Quantity,
                printing, colorCount, postProcess);

            // 约束5：预算检查（软约束）
            if (profile.BudgetPerUnit.HasValue && cost.UnitCost > profile.BudgetPerUnit.Value * 1.5f)
                continue;

            // 计算体验分
            float experienceScore = CalculateExperienceScore(boxType, material, postProcess, profile.Usage);

            feasible.Add(new Candidate(boxType, material, printing, colorCount, postProcess, cost, experienceScore));
        }

        // Step 4: 排序并输出三档方案
        var sorted = feasible.OrderBy(c => c.Cost.UnitCost).ToList();

        PackagingSolution? budget = null;
        PackagingSolution? recommended = null;
        PackagingSolution? premium = null;

        if (sorted.Count > 0)
        {
            // 方案A：成本最低
            var a = sorted[0];
            budget = ToSolution(a, SolutionTier.BudgetFriendly, profile);
        }

        if (sorted.Count > 1)
        {
            // 方案B：性价比最优（成本-体验帕累托最优）
            var b = sorted.OrderByDescending(c =>
                0.4f * (1f / c.Cost.UnitCost) + 0.6f * c.ExperienceScore
            ).First();
            recommended = ToSolution(b, SolutionTier.Recommended, profile);
        }

        if (sorted.Count > 2)
        {
            // 方案C：体验最佳
            var c = sorted.OrderByDescending(c => c.ExperienceScore).First();
            premium = ToSolution(c, SolutionTier.Premium, profile);
        }

        return new RecommendationResult(
            profile, budget, recommended, premium,
            totalCombinations, feasible.Count);
    }

    private PackagingSolution ToSolution(Candidate c, SolutionTier tier, ProductProfile profile)
    {
        var reasons = GenerateReasons(c, profile);
        float sustainability = EstimateSustainability(c.Material, c.PostProcess);

        return new PackagingSolution(
            tier, c.BoxType, c.Material, c.Printing, c.ColorCount,
            c.PostProcess, c.Cost, sustainability, reasons);
    }

    private float CalculateExperienceScore(BoxType box, MaterialInfo material,
        CostEstimator.PostProcessing postProcess, UsageScenario usage)
    {
        float score = 0.5f;

        // 盒型体验分
        score += box switch
        {
            BoxType.RigidBox => 0.3f,
            BoxType.TuckEnd => 0.15f,
            BoxType.PillowBox => 0.2f,
            _ => 0.1f
        };

        // 后加工体验分
        score += postProcess switch
        {
            CostEstimator.PostProcessing.HotStamping => 0.2f,
            CostEstimator.PostProcessing.UV => 0.15f,
            CostEstimator.PostProcessing.Lamination => 0.1f,
            _ => 0f
        };

        return Math.Min(1f, score);
    }

    private List<string> GenerateReasons(Candidate c, ProductProfile profile)
    {
        var reasons = new List<string>();

        // 盒型选择理由
        reasons.Add(c.BoxType switch
        {
            BoxType.Mailer => "电商场景飞机盒自锁设计免胶带，降低打包成本",
            BoxType.CorrugatedRSC => "标准瓦楞箱通用性强，成本最优",
            BoxType.TuckEnd => "插舌式盒适合零售展示，成本适中",
            BoxType.RigidBox => "天地盖精装盒高端质感，品牌感最佳",
            BoxType.PillowBox => "枕头盒造型独特，适合小件礼品",
            BoxType.AutoLockBottom => "自动锁底免胶水，组装效率高",
            _ => $"选择{c.BoxType}盒型"
        });

        // 材料选择理由
        reasons.Add($"产品{profile.WeightKg}kg，{c.Material.Name}最大承重{c.Material.MaxLoadKg}kg，满足需求");

        // 印刷选择理由
        if (c.Printing == CostEstimator.PrintingMethod.Digital && profile.Quantity < 500)
            reasons.Add($"{profile.Quantity}个的量级，数码印刷无起版费，总成本更低");
        else if (c.Printing == CostEstimator.PrintingMethod.Flexo)
            reasons.Add("柔印适合单色/双色，成本最低");

        return reasons;
    }

    private float EstimateSustainability(MaterialInfo material, CostEstimator.PostProcessing postProcess)
    {
        float score = material.Type switch
        {
            MaterialType.KraftPaper => 85f,
            MaterialType.CorrugatedE or MaterialType.CorrugatedB or MaterialType.CorrugatedBC => 75f,
            MaterialType.WhiteCardboard => 65f,
            MaterialType.GrayCardboard => 60f,
            _ => 50f
        };

        // 覆膜降低可回收性
        if (postProcess == CostEstimator.PostProcessing.Lamination)
            score -= 20f;

        return Math.Max(0f, Math.Min(100f, score));
    }
}
