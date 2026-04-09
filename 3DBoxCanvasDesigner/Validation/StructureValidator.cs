namespace BoxCanvasDesigner.Validation;

/// <summary>
/// 验证结果
/// </summary>
public record ValidationResult(
    bool IsValid,
    List<string> Errors,
    List<string> Warnings
)
{
    public static ValidationResult Success() => new(true, new(), new());
}

/// <summary>
/// 盒型结构验证器
/// </summary>
public class StructureValidator
{
    private const float MIN_TAB_WIDTH_MM = 15f;
    private const float MIN_FLAP_OVERLAP_RATIO = 0.5f;
    private const float MIN_FOLD_RADIUS_RATIO = 1.5f;
    private const float MIN_DIMENSION_MM = 10f;
    private const float MAX_DIMENSION_MM = 2000f;
    private const float MIN_WALL_THICKNESS_MM = 0.5f;
    private const float MAX_WALL_THICKNESS_MM = 10f;

    /// <summary>
    /// 验证盒型参数
    /// </summary>
    public ValidationResult Validate(BoxParameters parameters)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        // 1. 尺寸合理性检查
        ValidateDimensions(parameters, errors, warnings);

        // 2. 壁厚检查
        ValidateWallThickness(parameters, errors, warnings);

        // 3. 翻盖重叠检查
        ValidateFlapOverlap(parameters, errors, warnings);

        // 4. 插舌宽度检查
        ValidateTabWidth(parameters, errors, warnings);

        // 5. 折叠半径检查
        ValidateFoldRadius(parameters, errors, warnings);

        // 6. 盒型特定检查
        ValidateBoxTypeSpecific(parameters, errors, warnings);

        bool isValid = errors.Count == 0;
        return new ValidationResult(isValid, errors, warnings);
    }

    /// <summary>
    /// 验证尺寸合理性
    /// </summary>
    private void ValidateDimensions(BoxParameters parameters, List<string> errors, List<string> warnings)
    {
        if (parameters.LengthMM < MIN_DIMENSION_MM)
            errors.Add($"长度过小: {parameters.LengthMM}mm < {MIN_DIMENSION_MM}mm");
        if (parameters.WidthMM < MIN_DIMENSION_MM)
            errors.Add($"宽度过小: {parameters.WidthMM}mm < {MIN_DIMENSION_MM}mm");
        if (parameters.HeightMM < MIN_DIMENSION_MM)
            errors.Add($"高度过小: {parameters.HeightMM}mm < {MIN_DIMENSION_MM}mm");

        if (parameters.LengthMM > MAX_DIMENSION_MM)
            warnings.Add($"长度过大: {parameters.LengthMM}mm > {MAX_DIMENSION_MM}mm（可能需要特殊加工）");
        if (parameters.WidthMM > MAX_DIMENSION_MM)
            warnings.Add($"宽度过大: {parameters.WidthMM}mm > {MAX_DIMENSION_MM}mm（可能需要特殊加工）");
        if (parameters.HeightMM > MAX_DIMENSION_MM)
            warnings.Add($"高度过大: {parameters.HeightMM}mm > {MAX_DIMENSION_MM}mm（可能需要特殊加工）");

        // 长宽比检查
        float aspectRatio = Math.Max(parameters.LengthMM, parameters.WidthMM) /
                           Math.Min(parameters.LengthMM, parameters.WidthMM);
        if (aspectRatio > 5)
            warnings.Add($"长宽比过大: {aspectRatio:F1}:1（可能影响结构稳定性）");
    }

    /// <summary>
    /// 验证壁厚
    /// </summary>
    private void ValidateWallThickness(BoxParameters parameters, List<string> errors, List<string> warnings)
    {
        if (parameters.WallThicknessMM < MIN_WALL_THICKNESS_MM)
            errors.Add($"壁厚过小: {parameters.WallThicknessMM}mm < {MIN_WALL_THICKNESS_MM}mm");
        if (parameters.WallThicknessMM > MAX_WALL_THICKNESS_MM)
            warnings.Add($"壁厚过大: {parameters.WallThicknessMM}mm > {MAX_WALL_THICKNESS_MM}mm");

        // 壁厚与尺寸比例检查
        float minDimension = Math.Min(Math.Min(parameters.LengthMM, parameters.WidthMM), parameters.HeightMM);
        if (parameters.WallThicknessMM > minDimension * 0.1f)
            warnings.Add($"壁厚相对尺寸过大（壁厚/最小尺寸 > 10%）");
    }

    /// <summary>
    /// 验证翻盖重叠
    /// </summary>
    private void ValidateFlapOverlap(BoxParameters parameters, List<string> errors, List<string> warnings)
    {
        float flapDepth = parameters.GetFlapDepth();
        float minOverlap = parameters.HeightMM * MIN_FLAP_OVERLAP_RATIO;

        if (flapDepth < minOverlap)
            warnings.Add($"翻盖深度不足: {flapDepth:F1}mm < {minOverlap:F1}mm（建议≥高度的50%）");

        if (flapDepth > parameters.HeightMM * 1.2f)
            warnings.Add($"翻盖深度过大: {flapDepth:F1}mm > {parameters.HeightMM * 1.2f:F1}mm（可能造成浪费）");
    }

    /// <summary>
    /// 验证插舌宽度
    /// </summary>
    private void ValidateTabWidth(BoxParameters parameters, List<string> errors, List<string> warnings)
    {
        float tabWidth = parameters.GetTabWidth();

        if (tabWidth < MIN_TAB_WIDTH_MM)
            errors.Add($"插舌宽度过小: {tabWidth:F1}mm < {MIN_TAB_WIDTH_MM}mm（无法有效锁扣）");

        if (tabWidth > parameters.WidthMM * 0.6f)
            warnings.Add($"插舌宽度过大: {tabWidth:F1}mm > {parameters.WidthMM * 0.6f:F1}mm（可能影响折叠）");
    }

    /// <summary>
    /// 验证折叠半径
    /// </summary>
    private void ValidateFoldRadius(BoxParameters parameters, List<string> errors, List<string> warnings)
    {
        float minFoldRadius = parameters.WallThicknessMM * MIN_FOLD_RADIUS_RATIO;

        // 对于厚材料，检查折叠半径是否足够
        if (parameters.WallThicknessMM > 3.0f)
        {
            warnings.Add($"材料较厚（{parameters.WallThicknessMM}mm），建议折叠半径≥{minFoldRadius:F1}mm");
        }
    }

    /// <summary>
    /// 盒型特定验证
    /// </summary>
    private void ValidateBoxTypeSpecific(BoxParameters parameters, List<string> errors, List<string> warnings)
    {
        switch (parameters.Type)
        {
            case BoxType.TuckEnd:
                // 插舌式盒需要足够的插舌宽度
                if (parameters.GetTabWidth() < 20f)
                    warnings.Add("插舌式盒建议插舌宽度≥20mm以确保锁扣牢固");
                break;

            case BoxType.AutoLockBottom:
                // 自动锁底盒需要合适的高度
                if (parameters.HeightMM < 50f)
                    warnings.Add("自动锁底盒建议高度≥50mm以确保锁扣有效");
                break;

            case BoxType.PillowBox:
                // 枕头盒适合小尺寸
                if (parameters.LengthMM > 200f || parameters.WidthMM > 200f)
                    warnings.Add("枕头盒通常用于小尺寸包装（建议≤200mm）");
                break;

            case BoxType.RigidBox:
                // 天地盖精装盒需要较厚材料
                if (parameters.WallThicknessMM < 2.0f)
                    warnings.Add("天地盖精装盒建议壁厚≥2mm以确保结构强度");
                break;

            case BoxType.CorrugatedRSC:
                // 瓦楞箱通常用于大尺寸
                if (parameters.LengthMM < 100f || parameters.WidthMM < 100f)
                    warnings.Add("瓦楞箱通常用于较大尺寸包装（建议≥100mm）");
                break;
        }
    }

    /// <summary>
    /// 快速验证（仅检查致命错误）
    /// </summary>
    public bool QuickValidate(BoxParameters parameters)
    {
        return parameters.LengthMM >= MIN_DIMENSION_MM &&
               parameters.WidthMM >= MIN_DIMENSION_MM &&
               parameters.HeightMM >= MIN_DIMENSION_MM &&
               parameters.WallThicknessMM >= MIN_WALL_THICKNESS_MM &&
               parameters.GetTabWidth() >= MIN_TAB_WIDTH_MM;
    }
}
