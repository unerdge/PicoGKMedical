using PicoGK;
using Leap71.ShapeKernel;
using System.Numerics;
using System.Runtime.InteropServices;

namespace BoneConstruct
{

    public static class Program
    {
        static string BoneSTL="E:/desktop_files/workplace/vscode/Csharp/LEAP71/PicoGKMedical/BoneConstruct/BoneSTL/left_arm_skel.stl";
        static string ImplantSTL="E:/desktop_files/workplace/vscode/Csharp/LEAP71/PicoGKMedical/BoneConstruct/Output/BoneImplant.stl";
        static string LogPath = "E:/desktop_files/workplace/vscode/Csharp/LEAP71/PicoGKMedical/BoneConstruct/log/";
        public static void Main(string[] args)
        {
            // ScaleSTL(1.0f,BoneSTL,BoneSTL);
            PicoGK.Library.Go(0.3f, () => Run(ImplantSTL));
        }
        static void ScaleSTL(float scale, string strIn, string strOut)
        {
            Mesh msh = Mesh.mshFromStlFile(strIn);
            Mesh scaledMsh=msh.mshCreateTransformed(new Vector3(scale,scale,scale), Vector3.Zero);
            scaledMsh.SaveToStlFile(strOut);
        }
        static void Run(string strOut)
        {
            // 1. 加载患者骨骼STL（由CT重建导出）
            string strBoneSTL = BoneSTL;
            BoneModel oBone = new BoneModel(strBoneSTL);

            // 2. 选择填充模式：
            //   FillMode.None             — 纯壳体，无晶格
            //   FillMode.RandomizedTPMS   — 随机化 Schwarz Primitive（仿骨小梁，推荐）
            //   FillMode.SchwarzPrimitive — 规则 Schwarz Primitive
            //   FillMode.SchwarzDiamond   — Schwarz Diamond
            //   FillMode.Gyroid           — Gyroid
            FillMode eFill = FillMode.RandomizedTPMS;

            BoneImplant oImplant = new BoneImplant(
                oBone,
                eFillMode:      eFill,
                fThickness:     0.6f,
                fOffset:        -0.6f,
                fUnitSize:      4f,     // 晶格单元尺寸 (mm)，越小越密
                fWallThickness: 0.9f);  // 晶格壁厚 (mm)

            Voxels voxImplant = oImplant.voxConstruct();

            // 3. 预览（骨骼半透明蓝 + 植入物橙色）
            Sh.PreviewVoxels(voxImplant,    new ColorFloat("#FF8C00"),0.8f);
            Library.oViewer().RequestScreenShot(LogPath + $"Screenshot_00.TGA");
            Uf.Wait(0.5f);
            Sh.PreviewVoxels(oBone.voxBone, new ColorFloat("#9F9F9F"),0.5f);
            Library.oViewer().RequestScreenShot(LogPath + $"Screenshot_01.TGA");
            Uf.Wait(0.5f);

            // 4. 导出STL用于3D打印
            Sh.ExportVoxelsToSTLFile(voxImplant, strOut);

            Library.Log($"导出完成: {strOut}");
        }
    }
}