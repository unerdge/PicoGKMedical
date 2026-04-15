using PicoGK;
using FluidSimulation;

const string LOG_FOLDER = @"E:\desktop_files\workplace\vscode\Csharp\LEAP71\PicoGKMedical\FluidSimulation\log";

// ① PicoGK 建模 + 导出 STL
// PicoGK.Library.Go(0.3f, AeroTubeDesign.Run, LOG_FOLDER);

// ② 建模完成后，驱动 ANSYS Fluent 仿真（使用导出的流道 STL）
string stlPath = Path.Combine(LOG_FOLDER, "AeroTube_Tube.stl");

if (File.Exists(stlPath))
{
    Console.WriteLine("\n[主程序] 几何建模完成，启动 ANSYS Fluent 仿真...");
    SimResult result = AnsysDriver.Run(stlPath, LOG_FOLDER);

    Console.WriteLine("\n========== 仿真结果 ==========");
    Console.WriteLine($"  切向速度：{result.TangentialVelocity:F2} m/s  (目标: 30-47 m/s)");
    Console.WriteLine($"  轴向速度：{result.AxialVelocity:F3} m/s  (目标: ≤ 0.2 m/s)");
    Console.WriteLine($"  是否达标：{(result.TangentialVelocity >= 30f && result.AxialVelocity <= 0.2f ? "✓ 达标" : "✗ 未达标")}");
    Console.WriteLine("================================\n");
}
else
{
    Console.WriteLine($"[主程序] 未找到 STL 文件：{stlPath}，跳过仿真。");
}
