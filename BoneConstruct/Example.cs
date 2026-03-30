using PicoGK;
using Leap71.ShapeKernel;
using System.Numerics;

namespace BoneConstruct
{
    public static class Example
    {
        public static void PreviewBoxRun()
        {
            BaseBox oBox1           = new BaseBox(new LocalFrame(), 10, 10, 10);
            BaseBox oBox2           = new BaseBox(new LocalFrame(), -10, 10, 10);
            Sh.PreviewBoxWireframe(         oBox1,      Cp.clrRed);
            Sh.PreviewBoxWireframe(         oBox2,      Cp.clrBlack);
            Console.WriteLine("Inner Box Created.");
            // Console.ReadKey();
        }
        public static void Run(float resolutionRatio = 0.2f)
        {
#pragma warning disable CS0219 // 变量已被赋值，但从未使用过它的值
            string strOutputFolder = "E:/desktop_files/workplace/vscode/Csharp/LEAP71/PicoGKMedical/BoneConstruct/log";
#pragma warning restore CS0219 // 变量已被赋值，但从未使用过它的值

            try
            {
                // PicoGK.Library.Go(
                //     resolutionRatio,
                //     Leap71.CoolCube.HelixHeatX.Task);
                PicoGK.Library.Go(
                    resolutionRatio,
                    PreviewBoxRun
                    );
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to run Task.");
                Console.WriteLine(e.ToString());
            }
        }
    }
}