using PicoGK;
using Leap71.ShapeKernel;
using System.Numerics;

namespace BoneConstruct
{
    public static class Example
    {
        public static void PreviewBoxRun()
        {
            BaseCylinder oCylinder = new BaseCylinder(new LocalFrame(new Vector3(0, 0, 0), Vector3.UnitY));
            Voxels oVoxels=oCylinder.voxConstruct();
            oVoxels = oVoxels.voxOffset(1f)-oVoxels;
            Sh.PreviewVoxels(oVoxels, Cp.clrBlue);
            Console.WriteLine("Inner Box Created.");
            // Console.ReadKey();
        }
        public static void Run(float resolutionRatio = 0.2f)
        {
            string strOutputFolder = "E:/desktop_files/workplace/vscode/Csharp/LEAP71/PicoGKMedical/BoneConstruct/log";

            try
            {
                // PicoGK.Library.Go(
                //     resolutionRatio,
                //     Leap71.CoolCube.HelixHeatX.Task,
                //     strOutputFolder);
                PicoGK.Library.Go(
                    0.2f,
                    PreviewBoxRun,
                    strOutputFolder+"/PreviewBoxRun"
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