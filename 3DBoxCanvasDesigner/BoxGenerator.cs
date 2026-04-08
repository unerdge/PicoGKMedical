using PicoGK;
using System.Numerics;
using Leap71.ShapeKernel;

namespace BoxCanvasDesigner;

/// <summary>
/// 盒型几何生成器（使用ShapeKernel简化实现）
/// </summary>
public class BoxGenerator
{
    private readonly BoxParameters _params;

    public BoxGenerator(BoxParameters parameters)
    {
        _params = parameters;
    }

    /// <summary>
    /// 生成盒子的体素几何（使用voxOffset创建空心壁面）
    /// </summary>
    public Voxels GenerateVoxels()
    {
        float L = _params.LengthMM;
        float W = _params.WidthMM;
        float H = _params.HeightMM;
        float wall = _params.WallThicknessMM;

        // 创建外部盒子（包含壁厚的完整尺寸）
        LocalFrame oFrame = new LocalFrame(new Vector3(0, 0, H / 2), Vector3.UnitZ);
        BaseBox oBox = new BaseBox(oFrame, L + 2 * wall, W + 2 * wall, H);
        Voxels voxOuter = oBox.voxConstruct();

        // 使用voxOffset创建空心结构：外壳 - 内部 = 壁面
        // voxOffset(-wall)会向内收缩wall的距离，得到内部空腔
        Voxels voxInner = voxOuter.voxOffset(-wall);
        Voxels voxWalls = voxOuter - voxInner;

        return voxWalls;
    }

    /// <summary>
    /// 预览盒子（使用ShapeKernel的预览方法）
    /// </summary>
    public void Preview()
    {
        try
        {
            Voxels voxBox = GenerateVoxels();
            Sh.PreviewVoxels(voxBox, Cp.clrWarning); // 橙色

            string typeStr = _params.Type switch
            {
                BoxType.TuckEnd => "插舌式盒",
                BoxType.Mailer => "邮寄盒",
                BoxType.CorrugatedRSC => "瓦楞标准开槽箱",
                _ => "未知类型"
            };

            Library.Log($"盒型: {typeStr}");
            Library.Log($"内部尺寸: {_params.LengthMM} × {_params.WidthMM} × {_params.HeightMM} mm");
            Library.Log($"壁厚: {_params.WallThicknessMM} mm");
            Library.Log("3D盒型生成成功");
        }
        catch (Exception ex)
        {
            Library.Log($"生成错误: {ex.Message}");
        }
    }
}
