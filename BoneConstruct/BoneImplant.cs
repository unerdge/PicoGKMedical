using PicoGK;
using Leap71.ShapeKernel;
using System.Numerics;

namespace BoneConstruct
{
    /// <summary>
    /// 个性化骨骼贴合植入物生成器
    /// 策略：在骨骼体素外侧偏移一层壳体，裁剪为目标区域
    /// </summary>
    public class BoneImplant
    {
        readonly BoneModel  m_oBone;
        readonly float      m_fThickness;   // 植入物壁厚 (mm)
        readonly float      m_fOffset;      // 骨骼表面偏移量 (mm)

        public BoneImplant(BoneModel oBone, float fThickness = 2f, float fOffset = 0.5f)
        {
            m_oBone      = oBone;
            m_fThickness = fThickness;
            m_fOffset    = fOffset;
        }

        /// <summary>
        /// 生成贴合骨骼的植入物体素
        /// </summary>
        public Voxels voxConstruct()
        {
            // 1. 骨骼外表面向外偏移 → 植入物外壁
            Voxels voxOuter = new Voxels(m_oBone.voxBone);
            voxOuter.Offset(m_fOffset + m_fThickness);

            // 2. 骨骼外表面向外偏移少量 → 植入物内壁（贴合面）
            Voxels voxInner = new Voxels(m_oBone.voxBone);
            voxInner.Offset(m_fOffset);

            // 3. 差集 → 植入物壳体
            Voxels voxImplant = new Voxels(voxOuter);
            voxImplant -= voxInner;

            return voxImplant;
        }

        /// <summary>
        /// 在植入物内部填充晶格结构（减重 + 骨整合）
        /// </summary>
        public Voxels voxConstructWithLattice()
        {
            Voxels voxShell = voxConstruct();

            // TODO: 接入 LatticeLibrary 的 ConformalCellArray
            // 参考: Leap71_Repo_Display/LEAP71_LatticeLibrary-main/Examples/Ex_LatticeLibraryConformalTask.cs
            // ILatticeType xLattice = new BodyCentreLattice();
            // voxShell &= voxGetLattice(xLattice, voxShell);

            return voxShell;
        }
    }
}
