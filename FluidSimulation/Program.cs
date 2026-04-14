using PicoGK;
using FluidSimulation;

PicoGK.Library.Go(
    1.2f,        // 体素分辨率 0.3mm（精度与速度的平衡）
    AeroTubeDesign.Run,
    "E:/desktop_files/workplace/vscode/Csharp/LEAP71/PicoGKMedical/FluidSimulation/log"
);
