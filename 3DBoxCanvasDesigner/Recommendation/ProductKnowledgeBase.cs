using BoxCanvasDesigner.Physics;

namespace BoxCanvasDesigner.Recommendation;

/// <summary>
/// 产品类别
/// </summary>
public enum ProductCategory
{
    Electronics,     // 手机/电子产品
    Cosmetics,       // 化妆品
    FoodDry,         // 食品（干货）
    FoodCold,        // 食品（冷链）
    WineGlass,       // 酒类/玻璃瓶
    Clothing,        // 服装/鞋类
    Hardware,        // 五金/工具
    Medical,         // 医疗器械
    Toys,            // 玩具
    General          // 通用
}

/// <summary>
/// 产品类别知识条目
/// </summary>
public record CategoryKnowledge(
    ProductCategory Category,
    BoxType[] RecommendedBoxTypes,
    MaterialType[] RecommendedMaterials,
    string CushionRequirement,
    string[] SpecialRequirements
);

/// <summary>
/// 产品类别知识库 — 各行业包装经验数据
/// 作为推荐引擎的知识底座
/// </summary>
public static class ProductKnowledgeBase
{
    private static readonly Dictionary<ProductCategory, CategoryKnowledge> _knowledge = new()
    {
        [ProductCategory.Electronics] = new(
            ProductCategory.Electronics,
            new[] { BoxType.RigidBox, BoxType.TuckEnd },
            new[] { MaterialType.GrayCardboard },
            "EPE/EVA内衬",
            new[] { "防静电", "磁吸扣" }
        ),
        [ProductCategory.Cosmetics] = new(
            ProductCategory.Cosmetics,
            new[] { BoxType.TuckEnd, BoxType.RigidBox },
            new[] { MaterialType.WhiteCardboard },
            "无或轻量",
            new[] { "烫金", "UV", "高颜值" }
        ),
        [ProductCategory.FoodDry] = new(
            ProductCategory.FoodDry,
            new[] { BoxType.TuckEnd, BoxType.AutoLockBottom },
            new[] { MaterialType.WhiteCardboard, MaterialType.KraftPaper },
            "无",
            new[] { "食品级油墨", "开窗" }
        ),
        [ProductCategory.FoodCold] = new(
            ProductCategory.FoodCold,
            new[] { BoxType.CorrugatedRSC },
            new[] { MaterialType.CorrugatedBC },
            "EPS泡沫",
            new[] { "防潮", "保温" }
        ),
        [ProductCategory.WineGlass] = new(
            ProductCategory.WineGlass,
            new[] { BoxType.RigidBox },
            new[] { MaterialType.GrayCardboard },
            "EVA定位托",
            new[] { "隔断结构", "防碰撞" }
        ),
        [ProductCategory.Clothing] = new(
            ProductCategory.Clothing,
            new[] { BoxType.Mailer },
            new[] { MaterialType.CorrugatedE, MaterialType.KraftPaper },
            "拷贝纸包裹",
            new[] { "品牌感", "拆箱体验" }
        ),
        [ProductCategory.Hardware] = new(
            ProductCategory.Hardware,
            new[] { BoxType.CorrugatedRSC },
            new[] { MaterialType.CorrugatedB, MaterialType.CorrugatedBC },
            "珍珠棉隔板",
            new[] { "承重优先" }
        ),
        [ProductCategory.Medical] = new(
            ProductCategory.Medical,
            new[] { BoxType.RigidBox },
            new[] { MaterialType.GrayCardboard },
            "定制吸塑内衬",
            new[] { "无菌", "追溯码" }
        ),
        [ProductCategory.Toys] = new(
            ProductCategory.Toys,
            new[] { BoxType.TuckEnd },
            new[] { MaterialType.WhiteCardboard },
            "扎带/吸塑固定",
            new[] { "可视化展示", "开窗" }
        ),
        [ProductCategory.General] = new(
            ProductCategory.General,
            new[] { BoxType.TuckEnd, BoxType.Mailer, BoxType.CorrugatedRSC },
            new[] { MaterialType.WhiteCardboard, MaterialType.CorrugatedE },
            "按需",
            Array.Empty<string>()
        )
    };

    /// <summary>
    /// 获取产品类别知识
    /// </summary>
    public static CategoryKnowledge GetKnowledge(ProductCategory category)
    {
        return _knowledge.TryGetValue(category, out var k) ? k : _knowledge[ProductCategory.General];
    }

    /// <summary>
    /// 根据使用场景推荐盒型优先级排序
    /// 按成本从低到高排序
    /// </summary>
    public static BoxType[] GetBoxTypePriority(UsageScenario scenario)
    {
        return scenario switch
        {
            UsageScenario.EcommerceShipping => new[] { BoxType.Mailer, BoxType.CorrugatedRSC, BoxType.TuckEnd },
            UsageScenario.RetailShelf => new[] { BoxType.TuckEnd, BoxType.AutoLockBottom, BoxType.RigidBox },
            UsageScenario.GiftPremium => new[] { BoxType.RigidBox, BoxType.PillowBox, BoxType.TuckEnd },
            UsageScenario.FoodBeverage => new[] { BoxType.TuckEnd, BoxType.AutoLockBottom, BoxType.CorrugatedRSC },
            UsageScenario.Industrial => new[] { BoxType.CorrugatedRSC, BoxType.Mailer },
            _ => new[] { BoxType.TuckEnd, BoxType.Mailer, BoxType.CorrugatedRSC }
        };
    }
}
