using PicoGK;

namespace BoneConstruct
{
    public static class Example
    {
        public static void Run()
        {
            string strOutputFolder = "E:/desktop_files/workplace/vscode/C#(picoGK)/LEAP71/PicoGK_Examples-mainj/log";

            try
            {
                PicoGK.Library.Go(
                    0.2f,
                    Leap71.CoolCube.HelixHeatX.Task);
                // PicoGK.Library.Go(
                //     0.1f,
                //     Practice.Task3,
                //     strOutputFolder
                //     );
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to run Task.");
                Console.WriteLine(e.ToString());
            }
        }
    }
}