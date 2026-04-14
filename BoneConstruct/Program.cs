using PicoGK;
using Leap71.ShapeKernel;
using System.Numerics;

namespace BoneConstruct
{

    public static class Program
    {
        public static void Main(string[] args)
        {
            PicoGK.Library.Go(0.5f, Run);
        }

        static void Run()
        {
            // 1. 加载患者骨骼STL（由CT重建导出）
            string strBoneSTL = "Input/bone.stl";
            BoneModel oBone = new BoneModel(strBoneSTL);

            // 2. 生成贴合植入物
            BoneImplant oImplant = new BoneImplant(oBone, fThickness: 2f, fOffset: 0.5f);
            PicoGK.Voxels voxImplant = oImplant.voxConstruct();

            // 3. 预览
            Leap71.ShapeKernel.Sh.PreviewVoxels(oBone.voxBone,  new PicoGK.ColorFloat("AAAAFF99"), 0f, 0.8f);
            Leap71.ShapeKernel.Sh.PreviewVoxels(voxImplant,     new PicoGK.ColorFloat("FF8C00CC"));

            // 4. 导出STL用于3D打印
            string strOut = Leap71.ShapeKernel.Sh.strGetExportPath(
                Leap71.ShapeKernel.Sh.EExport.STL, "BoneImplant");
            Leap71.ShapeKernel.Sh.ExportVoxelsToSTLFile(voxImplant, strOut);

            PicoGK.Library.Log($"导出完成: {strOut}");
        }
    }
}