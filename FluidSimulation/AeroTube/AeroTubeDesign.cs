using PicoGK;
using Leap71.ShapeKernel;
using System.Numerics;

namespace FluidSimulation
{
    /// <summary>
    /// 空气动力旋转底座 - 两级喉管旋流设计
    ///
    /// 设计参数（依据课题要求）：
    ///   直管内径：200mm（半径100mm）
    ///   轴向速度：0.2 m/s（极低）
    ///   第一级喉管出口切向速度：30-47 m/s
    ///   旋流数 S = v_tan / v_axial ≈ 150（必须电机主动驱动）
    ///
    /// 电机参数估算：
    ///   旋流叶片在r=45mm处 → ω = 30/0.045 ≈ 667 rad/s ≈ 6400 RPM
    ///   推荐：57mm无刷直流电机，6000-8000RPM，额定功率50W
    /// </summary>
    public static class AeroTubeDesign
    {
        // ── 主管参数（mm）──────────────────────────────────────────────
        const float TUBE_OUTER_R    = 100f;     // 外径半径
        const float TUBE_INNER_R    = 97f;      // 内径半径（3mm壁厚）
        const float TUBE_LENGTH     = 600f;     // 总长度

        // ── 两级喉管参数 ────────────────────────────────────────────────
        // 第一级：角动量守恒使外圈切向速度达到 30-47 m/s
        // 收缩比：r_throat/r_tube = 64/97 ≈ 0.66
        const float THROAT_MIN_R    = 64f;      // 喉管最小内半径
        const float THROAT_LEN      = 100f;     // 每级喉管总长度
        const float THROAT1_Z       = 100f;     // 第一级喉管起点（距进口）
        const float THROAT2_Z       = 320f;     // 第二级喉管起点

        // ── 旋流叶片参数（安装在各级喉管最小截面处）─────────────────────
        // 叶片角度接近90°（切向），产生极高旋流数
        const int   VANE_COUNT      = 8;        // 叶片数
        const float HUB_R           = 8f;       // 电机轴/轮毂半径
        const float VANE_TIP_R      = 58f;      // 叶片尖端半径（< THROAT_MIN_R，留余量）
        const float VANE_THICKNESS  = 2.5f;     // 叶片厚度
        const float VANE_CHORD      = 25f;      // 弦长
        static readonly float BLADE_ANGLE = 78f * MathF.PI / 180f; // 与轴向夹角

        public static void Run()
        {
            Console.WriteLine("[AeroTube] 开始建模...");

            // ① 外管实体
            var oOrigin = new LocalFrame();
            var oOuter  = new BaseCylinder(oOrigin, TUBE_LENGTH, TUBE_OUTER_R);
            oOuter.SetPolarSteps(360);
            oOuter.SetLengthSteps(20);
            Voxels voxOuter = oOuter.voxConstruct();
            Console.WriteLine("[AeroTube] 外管实体完成");

            // ② 内部流道（含两级喉管 sin² 平滑轮廓）
            var oChannel = new BaseCylinder(oOrigin, TUBE_LENGTH, TUBE_INNER_R);
            oChannel.SetRadius(new SurfaceModulation(fChannelRadius));
            oChannel.SetPolarSteps(360);
            // SetRadius 内部已设置 SetLengthSteps(500)
            Voxels voxChannel = oChannel.voxConstruct();
            Console.WriteLine("[AeroTube] 流道轮廓完成");

            // ③ 管壁 = 外管 − 流道（含喉管收缩壁）
            Voxels voxTube = voxOuter - voxChannel;

            // ④ 第一级旋流器（在喉管最窄处）
            Voxels voxStage1 = BuildSwirlerStage(THROAT1_Z + THROAT_LEN * 0.5f);
            Console.WriteLine("[AeroTube] 第一级旋流叶片完成");

            // ⑤ 第二级旋流器（在喉管最窄处）
            Voxels voxStage2 = BuildSwirlerStage(THROAT2_Z + THROAT_LEN * 0.5f);
            Console.WriteLine("[AeroTube] 第二级旋流叶片完成");

            // ⑥ 合并为单一实体（用于导出）
            Voxels voxResult = voxTube + voxStage1 + voxStage2;

            // ⑦ 三维预览
            // 管壁：半透明蓝色（可透视内部叶片）
            Sh.PreviewVoxels(voxTube,   new ColorFloat("2B6CB0"), 0.25f, 0.1f, 0.9f);
            // 第一级叶片：橙色（不透明）
            Sh.PreviewVoxels(voxStage1, new ColorFloat("E05C2A"), 1.0f,  0.5f, 0.4f);
            // 第二级叶片：金色（不透明）
            Sh.PreviewVoxels(voxStage2, new ColorFloat("D4A017"), 1.0f,  0.5f, 0.4f);

            // ⑧ 导出 STL（完整组件 + 各部件分开）
            Sh.ExportVoxelsToSTLFile(voxResult, Sh.strGetExportPath(Sh.EExport.STL, "AeroTube_Full"));
            Sh.ExportVoxelsToSTLFile(voxTube,   Sh.strGetExportPath(Sh.EExport.STL, "AeroTube_Tube"));
            Sh.ExportVoxelsToSTLFile(voxStage1, Sh.strGetExportPath(Sh.EExport.STL, "AeroTube_Stage1_Vanes"));
            Sh.ExportVoxelsToSTLFile(voxStage2, Sh.strGetExportPath(Sh.EExport.STL, "AeroTube_Stage2_Vanes"));

            Console.WriteLine("[AeroTube] 建模完成！");
            Console.WriteLine($"  喉管收缩比：{THROAT_MIN_R / TUBE_INNER_R:F2}");
            Console.WriteLine($"  叶片角度：{BLADE_ANGLE * 180f / MathF.PI:F0}° (与轴向)");
            Console.WriteLine($"  叶片数量：{VANE_COUNT} 片 × 2 级");
        }

        /// <summary>
        /// 流道内径随轴向位置的变化函数（两级喉管的 sin² 平滑轮廓）
        /// fLR = 0..1，对应 z = 0..TUBE_LENGTH
        /// </summary>
        static float fChannelRadius(float fPhi, float fLR)
        {
            float fZ = fLR * TUBE_LENGTH;

            // 第一级喉管段
            float fT1 = (fZ - THROAT1_Z) / THROAT_LEN;
            if (fT1 >= 0f && fT1 <= 1f)
            {
                // sin²(t·π)：t=0时r=TUBE_INNER_R，t=0.5时r=THROAT_MIN_R，t=1时r=TUBE_INNER_R
                float fFrac = MathF.Pow(MathF.Sin(fT1 * MathF.PI), 2f);
                return TUBE_INNER_R - (TUBE_INNER_R - THROAT_MIN_R) * fFrac;
            }

            // 第二级喉管段
            float fT2 = (fZ - THROAT2_Z) / THROAT_LEN;
            if (fT2 >= 0f && fT2 <= 1f)
            {
                float fFrac = MathF.Pow(MathF.Sin(fT2 * MathF.PI), 2f);
                return TUBE_INNER_R - (TUBE_INNER_R - THROAT_MIN_R) * fFrac;
            }

            return TUBE_INNER_R;
        }

        /// <summary>
        /// 构建单级旋流叶片组：VANE_COUNT 片近切向叶片 + 中心轮毂圆柱
        /// </summary>
        static Voxels BuildSwirlerStage(float fCenterZ)
        {
            Voxels voxStage = new Voxels();
            float  fMidR    = (HUB_R + VANE_TIP_R) * 0.5f;  // 叶片中径
            float  fSpan    = VANE_TIP_R - HUB_R;            // 叶片展长

            for (int i = 0; i < VANE_COUNT; i++)
            {
                float fPhi = 2f * MathF.PI / VANE_COUNT * i;

                // 叶片中心坐标
                Vector3 vecPos = new Vector3(
                    fMidR * MathF.Cos(fPhi),
                    fMidR * MathF.Sin(fPhi),
                    fCenterZ);

                // 径向：叶片展向
                Vector3 vecRadial = new Vector3(MathF.Cos(fPhi), MathF.Sin(fPhi), 0f);

                // 切向（右手坐标系）
                Vector3 vecTan = new Vector3(-MathF.Sin(fPhi), MathF.Cos(fPhi), 0f);

                // 弦向 = 切向分量（主） + 轴向分量（次）
                // 78°叶片几乎指向切向，将轴向气流偏转为旋转
                Vector3 vecChord = Vector3.Normalize(
                    MathF.Sin(BLADE_ANGLE) * vecTan +
                    MathF.Cos(BLADE_ANGLE) * Vector3.UnitZ);

                // LocalFrame：Z=弦向（BaseBox 的 "Length" 方向），X=展向（"Width" 方向）
                var oFrame = new LocalFrame(vecPos, vecChord, vecRadial);

                // 叶片薄板：弦长 × 展长 × 厚度
                var oVane = new BaseBox(oFrame, VANE_CHORD, fSpan, VANE_THICKNESS);
                voxStage += oVane.voxConstruct();
            }

            // 电机轮毂（实心圆柱，表示电机安装位置）
            var oHubFrame = new LocalFrame(new Vector3(0f, 0f, fCenterZ - VANE_CHORD * 0.5f));
            var oHub      = new BaseCylinder(oHubFrame, VANE_CHORD, HUB_R);
            oHub.SetLengthSteps(5);
            voxStage += oHub.voxConstruct();

            return voxStage;
        }
    }
}
