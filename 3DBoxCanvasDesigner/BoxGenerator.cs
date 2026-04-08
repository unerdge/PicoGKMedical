using PicoGK;
using System.Numerics;

namespace BoxCanvasDesigner;

/// <summary>
/// 盒型几何生成器
/// </summary>
public class BoxGenerator
{
    private readonly BoxParameters _params;

    public BoxGenerator(BoxParameters parameters)
    {
        _params = parameters;
    }

    /// <summary>
    /// 生成盒子的体素几何
    /// </summary>
    public Voxels GenerateVoxels()
    {
        float L = _params.LengthMM;
        float W = _params.WidthMM;
        float H = _params.HeightMM;
        float wall = _params.WallThicknessMM;

        Lattice lat = new Lattice();

        // 底面四条边
        Vector3 p0 = new Vector3(-L/2, -W/2, 0);
        Vector3 p1 = new Vector3(L/2, -W/2, 0);
        Vector3 p2 = new Vector3(L/2, W/2, 0);
        Vector3 p3 = new Vector3(-L/2, W/2, 0);

        lat.AddBeam(p0, p1, wall, wall, false);
        lat.AddBeam(p1, p2, wall, wall, false);
        lat.AddBeam(p2, p3, wall, wall, false);
        lat.AddBeam(p3, p0, wall, wall, false);

        // 顶面四条边
        Vector3 p4 = new Vector3(-L/2, -W/2, H);
        Vector3 p5 = new Vector3(L/2, -W/2, H);
        Vector3 p6 = new Vector3(L/2, W/2, H);
        Vector3 p7 = new Vector3(-L/2, W/2, H);

        lat.AddBeam(p4, p5, wall, wall, false);
        lat.AddBeam(p5, p6, wall, wall, false);
        lat.AddBeam(p6, p7, wall, wall, false);
        lat.AddBeam(p7, p4, wall, wall, false);

        // 四条竖边
        lat.AddBeam(p0, p4, wall, wall, false);
        lat.AddBeam(p1, p5, wall, wall, false);
        lat.AddBeam(p2, p6, wall, wall, false);
        lat.AddBeam(p3, p7, wall, wall, false);

        return new Voxels(lat);
    }

    /// <summary>
    /// 预览盒子（添加到PicoGK查看器）
    /// </summary>
    public void Preview()
    {
        try
        {
            // 设置材质颜色
            Library.oViewer().SetGroupMaterial(0, "FF6B35", 0.0f, 1.0f); // 橙色

            Voxels voxBox = GenerateVoxels();
            Library.oViewer().Add(voxBox);

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
