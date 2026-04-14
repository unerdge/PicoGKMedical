using PicoGK;
using Leap71.ShapeKernel;
using Leap71.LatticeLibrary;
using System.Numerics;

namespace BoneConstruct
{
    /// <summary>
    /// 植入物内部晶格填充方式
    /// </summary>
    public enum FillMode
    {
        /// <summary>纯壳体，不填充晶格</summary>
        None,

        /// <summary>随机化 Schwarz Primitive TPMS（仿骨小梁结构）</summary>
        RandomizedTPMS,

        /// <summary>规则 Schwarz Primitive TPMS</summary>
        SchwarzPrimitive,

        /// <summary>Schwarz Diamond TPMS</summary>
        SchwarzDiamond,

        /// <summary>Gyroid TPMS</summary>
        Gyroid,
    }

    /// <summary>
    /// 个性化骨骼贴合植入物生成器
    /// 策略：在骨骼体素外侧偏移一层壳体，内部填充可选晶格结构
    /// </summary>
    public class BoneImplant
    {
        readonly BoneModel  m_oBone;
        readonly float      m_fThickness;       // 植入物壁厚 (mm)
        readonly float      m_fOffset;          // 骨骼表面偏移量 (mm)
        readonly FillMode   m_eFillMode;

        // TPMS 参数
        readonly float      m_fUnitSize;        // 晶格单元尺寸 (mm)
        readonly float      m_fWallThickness;   // 晶格壁厚 (mm)

        public BoneImplant(
            BoneModel   oBone,
            FillMode    eFillMode       = FillMode.None,
            float       fThickness      = 2f,
            float       fOffset         = 0.5f,
            float       fUnitSize       = 10f,
            float       fWallThickness  = 0.5f)
        {
            m_oBone             = oBone;
            m_eFillMode         = eFillMode;
            m_fThickness        = fThickness;
            m_fOffset           = fOffset;
            m_fUnitSize         = fUnitSize;
            m_fWallThickness    = fWallThickness;
        }

        /// <summary>
        /// 生成植入物体素（壳体 + 可选晶格填充）
        /// </summary>
        public Voxels voxConstruct()
        {
            Voxels voxShell = voxBuildShell();

            if (m_eFillMode == FillMode.None)
                return voxShell;
            
            Voxels voxTot = new Voxels(m_oBone.voxBone);
            Voxels voxFill = voxBuildFill(voxTot);
            return voxFill+voxShell;
        }

        // ── 内部方法 ─────────────────────────────────────────────────────────

        /// <summary>生成贴合骨骼的中空壳体</summary>
        Voxels voxBuildShell()
        {
            Voxels voxOuter = new Voxels(m_oBone.voxBone);
            voxOuter.Offset(m_fOffset + m_fThickness);

            Voxels voxInner = new Voxels(m_oBone.voxBone);
            voxInner.Offset(m_fOffset);

            Voxels voxShell = new Voxels(voxOuter);
            voxShell -= voxInner;
            return voxShell;
        }

        /// <summary>在壳体包围盒内生成 TPMS 晶格，裁剪到壳体范围内</summary>
        Voxels voxBuildFill(Voxels voxBounding)
        {
            BBox3 oBBox = voxBounding.oCalculateBoundingBox();

            IImplicit xPattern = m_eFillMode switch
            {
                FillMode.RandomizedTPMS => vxBuildRandomizedTPMS(oBBox),
                FillMode.SchwarzPrimitive => new ImplicitSchwarzPrimitive(m_fUnitSize, m_fWallThickness),
                FillMode.SchwarzDiamond   => new ImplicitSchwarzDiamond(m_fUnitSize, m_fWallThickness),
                FillMode.Gyroid           => new ImplicitSplitWallGyroid(m_fUnitSize, m_fWallThickness, true),
                _ => throw new NotImplementedException($"FillMode {m_eFillMode} not implemented")
            };

            // 用隐式函数与体素求交 → 晶格裁剪到壳体内
            Voxels voxLattice = voxBounding.voxIntersectImplicit(xPattern);
            return voxLattice;
        }

        /// <summary>构建随机化 Schwarz Primitive（仿骨小梁随机性）</summary>
        IImplicit vxBuildRandomizedTPMS(BBox3 oBBox)
        {
            float fDeformAmp        = m_fUnitSize * 0.4f;   // 形变幅度 = 单元尺寸的 40%
            float fUnderlyingGrid   = m_fUnitSize * 2f;

            BBox3 oGrown = oBBox;
            oGrown.Grow(fDeformAmp + 0.2f);

            RandomDeformationField oField = new RandomDeformationField(
                oGrown,
                fUnderlyingGrid,
                -fDeformAmp,
                fDeformAmp);

            return new ImplicitRandomizedSchwarzPrimitive(m_fUnitSize, m_fWallThickness, oField);
        }
    }
}
