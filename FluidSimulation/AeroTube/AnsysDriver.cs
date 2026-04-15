using System.Diagnostics;

namespace FluidSimulation
{
    /// <summary>
    /// 驱动 ANSYS Fluent 学生版进行 CFD 仿真，通过 Journal 脚本自动化。
    /// 学生版限制：512k 单元，4 核，无商业用途。
    /// </summary>
    public static class AnsysDriver
    {
        // ── 路径配置（按实际安装版本修改）────────────────────────────────
        // 学生版通常安装在同一路径，版本号可能是 v241/v242/v251
        static readonly string FLUENT_EXE = FindFluentExe();

        // ── 边界条件（对应课题要求）──────────────────────────────────────
        const float INLET_VELOCITY  = 0.2f;     // 轴向进口速度 m/s
        const float INLET_TEMP      = 300f;     // 温度 K（常温空气）
        const int   ITER_COUNT      = 300;      // 迭代步数（学生版建议 200-500）

        public static SimResult Run(string stlPath, string logFolder)
        {
            string journalPath = Path.Combine(logFolder, "aero_tube.jou");
            string resultPath  = Path.Combine(logFolder, "aero_result.csv");

            WriteJournal(journalPath, stlPath, resultPath);
            RunFluent(journalPath, logFolder);
            return ParseResult(resultPath);
        }

        // ── Journal 脚本生成 ─────────────────────────────────────────────
        static void WriteJournal(string journalPath, string stlPath, string resultPath)
        {
            // Fluent Scheme journal：导入 STL → 网格 → 边界条件 → 求解 → 导出结果
            // 注意：STL 路径用正斜杠，Fluent 在 Windows 上也接受
            string stl  = stlPath.Replace('\\', '/');
            string csv  = resultPath.Replace('\\', '/');

            File.WriteAllText(journalPath, $"""
                ; ── 导入几何 ──────────────────────────────────────────────
                /file/import/stl "{stl}" yes

                ; ── 网格检查与修复 ─────────────────────────────────────────
                /mesh/check
                /mesh/repair-improve/repair

                ; ── 求解器设置（压力基，稳态，k-epsilon 湍流模型）──────────
                /define/models/solver/pressure-based yes
                /define/models/viscous/ke-standard yes

                ; ── 流体材料（空气，默认）──────────────────────────────────
                /define/materials/change-create air air yes ideal-gas no no no no no no

                ; ── 边界条件 ────────────────────────────────────────────────
                ; 进口：速度入口，轴向 {INLET_VELOCITY} m/s
                /define/boundary-conditions/velocity-inlet
                    inlet () yes yes no {INLET_VELOCITY} no 0 no 0 no 1 no {INLET_TEMP}

                ; 出口：压力出口，表压 0 Pa
                /define/boundary-conditions/pressure-outlet
                    outlet () no 0 no {INLET_TEMP} no yes no no yes 5 1

                ; 壁面：无滑移（默认）
                /define/boundary-conditions/wall wall () no 0 no no no 0.5 no 1

                ; ── 求解控制 ─────────────────────────────────────────────────
                /solve/set/under-relaxation/pressure 0.3
                /solve/set/under-relaxation/mom 0.5
                /solve/set/under-relaxation/k 0.5
                /solve/set/under-relaxation/epsilon 0.5

                ; ── 初始化并迭代 ──────────────────────────────────────────────
                /solve/initialize/initialize-flow
                /solve/iterate {ITER_COUNT}

                ; ── 导出结果到 CSV ────────────────────────────────────────────
                ; 出口面：面积加权平均速度分量
                /report/surface-integrals/area-weighted-avg
                    outlet () velocity-magnitude ()
                /report/surface-integrals/area-weighted-avg
                    outlet () x-velocity ()
                /report/surface-integrals/area-weighted-avg
                    outlet () tangential-velocity ()

                ; 导出出口截面数据
                /file/export/ascii "{csv}"
                    outlet ()
                    yes
                    velocity-magnitude x-velocity y-velocity z-velocity pressure
                    ()
                    q

                /exit yes
                """);
        }

        // ── 启动 Fluent 进程 ─────────────────────────────────────────────
        static void RunFluent(string journalPath, string logFolder)
        {
            if (!File.Exists(FLUENT_EXE))
                throw new FileNotFoundException($"未找到 Fluent 可执行文件：{FLUENT_EXE}\n请修改 AnsysDriver.cs 中的路径。");

            string fluentLog = Path.Combine(logFolder, "fluent_output.log");

            Console.WriteLine($"[ANSYS] 启动 Fluent 仿真...");
            Console.WriteLine($"[ANSYS] Journal: {journalPath}");

            // 持久打开日志文件，lock 保证 stdout/stderr 两个线程互斥写入
            using var writer = new StreamWriter(fluentLog, append: false) { AutoFlush = true };
            var logLock = new object();

            void AppendLog(string text)
            {
                lock (logLock) writer.WriteLine(text);
            }

            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName               = FLUENT_EXE,
                    // 3ddp = 3D 双精度，-g = 无 GUI，-i = journal 文件，-t2 = 2核（学生版最多4核）
                    Arguments              = $"3ddp -g -i \"{journalPath}\" -t2",
                    UseShellExecute        = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError  = true,
                    WorkingDirectory       = logFolder
                }
            };

            proc.OutputDataReceived += (_, e) => { if (e.Data != null) AppendLog(e.Data); };
            proc.ErrorDataReceived  += (_, e) => { if (e.Data != null) AppendLog("[ERR] " + e.Data); };

            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();
            proc.WaitForExit();

            Console.WriteLine($"[ANSYS] 仿真完成，日志：{fluentLog}");
        }

        // ── 解析 CSV 结果 ────────────────────────────────────────────────
        static SimResult ParseResult(string csvPath)
        {
            if (!File.Exists(csvPath))
            {
                Console.WriteLine("[ANSYS] 警告：未找到结果文件，返回默认值。请检查 fluent_output.log");
                return new SimResult(0f, 0f, 0f);
            }

            float vMag = 0f, vAx = 0f, vTan = 0f;
            int   count = 0;

            foreach (string line in File.ReadLines(csvPath).Skip(2)) // 跳过标题行
            {
                var cols = line.Split(',');
                if (cols.Length < 5) continue;
                if (!float.TryParse(cols[0], out float vm)) continue;

                vMag += vm;
                vAx  += float.Parse(cols[1]);
                // 切向速度 = sqrt(vy² + vz²)（轴向为 x 方向时）
                float vy = float.Parse(cols[2]);
                float vz = float.Parse(cols[3]);
                vTan += MathF.Sqrt(vy * vy + vz * vz);
                count++;
            }

            if (count == 0) return new SimResult(0f, 0f, 0f);

            var result = new SimResult(vTan / count, vAx / count, vMag / count);
            Console.WriteLine($"[ANSYS] 结果 → 切向速度: {result.TangentialVelocity:F2} m/s | 轴向速度: {result.AxialVelocity:F3} m/s");
            return result;
        }

        // ── 自动查找 Fluent 安装路径 ─────────────────────────────────────
        static string FindFluentExe()
        {
            return @"D:\ANSYSACADEMICSTUDENT\ANSYS Inc\ANSYS Student\v261\fluent\ntbin\win64\fluent.exe";
            // 按版本号从新到旧搜索
            string[] versions = { "v261", "v251", "v242", "v241", "v232", "v231" };
            string   basePath = @"C:\Program Files\ANSYS Inc";

            foreach (string ver in versions)
            {
                string path = Path.Combine(basePath, ver, "fluent", "ntbin", "win64", "fluent.exe");
                if (File.Exists(path)) return path;
            }

            // 学生版可能在不同目录
            string studentPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "ANSYS Inc", "ANSYS Student", "v242", "fluent", "ntbin", "win64", "fluent.exe");
            if (File.Exists(studentPath)) return studentPath;

            return Path.Combine(basePath, "v242", "fluent", "ntbin", "win64", "fluent.exe");
        }
    }

    public record SimResult(float TangentialVelocity, float AxialVelocity, float MeanVelocity);
}
