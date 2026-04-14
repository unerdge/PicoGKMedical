using PicoGK;
using System.Numerics;

namespace BoneConstruct
{
    /// <summary>
    /// 患者骨骼模型 — 从STL加载，提供表面查询接口
    /// </summary>
    public class BoneModel
    {
        public readonly Voxels voxBone;

        public BoneModel(string strSTLPath)
        {
            // 从患者CT导出的STL加载骨骼体素
            Mesh msh = Mesh.mshFromStlFile(strSTLPath);
            voxBone = new Voxels(msh);
        }

        /// <summary>返回骨骼包围盒</summary>
        public BBox3 oBoundingBox() => voxBone.mshAsMesh().oBoundingBox();

        /// <summary>将点投影到骨骼表面，用于贴合计算</summary>
        public Vector3 vecProjectToSurface(Vector3 vecPt, Vector3 vecDir)
            => voxBone.vecRayCastToSurface(vecPt, vecDir);
    }
}
