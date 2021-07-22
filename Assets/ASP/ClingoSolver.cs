using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using Clingo.ClingoHelperJSON;
using UnityEngine;

public delegate void solverCallback(string clingoOutput);

namespace Clingo
{
    public class ClingoSolver : MonoBehaviour
    {
        public enum Status { ERROR, SATISFIABLE, UNSATISFIABLE, TIMEDOUT, RUNNING, UNINITIATED, READY, CLINGONOTFOUND, ASPFILENOTFOUND }

        //public Fil aspFile = new File();
        public ProcessPriorityClass threadPriority = ProcessPriorityClass.Normal;
        public string aspFilePath = "DataFiles/ASPFiles/queens.txt";
        public string clingoExecutablePathMacOS = "DataFiles/Clingo/clingo";
        public string clingoExecutablePathWin = "DataFiles/Clingo/clingo.exe";
        public string AdditionalArguments = "";
        public int maxDuration = 10; // in seconds
        public bool FindMultipleSolutions = false;
        public int numOfSolutionsWanted = 1; // set to 0 for all possible solution
        public int seed = 42;
        public bool useRandomSeed;
        public Dictionary<string, List<List<string>>> answerSet = new Dictionary<string, List<List<string>>>();


        // Read Only
        private int totalSolutionsFound = -1;
        private bool moreSolutions = false; // Clingo's way to tell us there might be more solutions
        private double duration; // How long to run clingo
        private bool isSolverRunning = false;
        private string solutionOutput;
        private string clingoConsoleOutput;
        private string clingoConsoleError;
        private Status status = Status.UNINITIATED;


        public int Seed { get { return seed; } }
        public bool MoreSolutions { get { return moreSolutions; } }
        public int SolutionsFound { get { return totalSolutionsFound; } }
        public double Duration { get { return duration; } }
        public bool IsSolverRunning { get { return isSolverRunning; } }
        public string SolutionOutput { get { return solutionOutput; } }
        public string ClingoConsoleOutput { get { return clingoConsoleOutput; } }
        public string ClingoConsoleError { get { return clingoConsoleError; } }
        public Status SolverStatus { get { return status; } }
        // Private
        // The path has to be set in the main thread
        public Thread clingoThread;
        private static string[] trueArray = { "true" };
        private Process clingoProcess;


        public void Solve()
        {
            SetUpProcess();

            if (status == Status.READY)
            {
                status = Status.RUNNING;
                clingoThread.Start();
            }
        }


        public void Solve(string aspfilepath, string clingoArguments)
        {
            this.AdditionalArguments = clingoArguments;
            this.aspFilePath = aspfilepath;
            Solve();
        }


        public void Solve(string aspfilepath)
        {
            this.aspFilePath = aspfilepath;
            this.AdditionalArguments = "";
            Solve();
        }


        private void MyThread()
        {
            SolveHelper();
            if (status == Status.SATISFIABLE)
            {
                solutionOutput = AnswerSetToString();
            }
        }


        public string AnswerSetToString()
        {
            StringBuilder sb = new StringBuilder();

            List<string> keys = new List<string>(answerSet.Keys);
            foreach (string key in keys)
            {
                sb.Append(key + ": ");
                foreach (List<string> l in answerSet[key])
                {
                    sb.Append("[");
                    foreach (string s in l)
                    {
                        sb.Append(s);
                        sb.Append(" ");
                    }
                    sb.Append("]");
                }
                sb.Append("\n");
            }
            return sb.ToString();
        }


        private bool startedOutPutReading = false;

        private void SolveHelper()
        {
            clingoProcess.Start();

            clingoProcess.PriorityClass = threadPriority;


            if (!startedOutPutReading)
            {
                startedOutPutReading = true;
            }
            clingoProcess.BeginOutputReadLine();



            print("Wating");
            if (clingoProcess.WaitForExit(maxDuration * 1000))
            {
                print("finished in time");
            }
            else
            {
                clingoProcess.Kill();
                status = Status.TIMEDOUT;
                UnityEngine.Debug.LogWarning("Clingo Timedout.");
            }


            if (status == Status.TIMEDOUT) { return; }



            //            clingoConsoleOutput = clingoProcess.StandardOutput.ReadToEnd();
            clingoConsoleError = clingoProcess.StandardError.ReadToEnd();

            clingoProcess.OutputDataReceived -= OutputDataReceived;
            clingoProcess.CancelOutputRead();
            clingoProcess.Close();



            ClingoRoot clingoOutput = JsonUtility.FromJson<ClingoRoot>(clingoConsoleOutput);

            if (clingoOutput == null)
            {
                status = Status.ERROR;
            }
            else
            {
                if (clingoOutput.Result == "SATISFIABLE")
                {
                    status = Status.SATISFIABLE;
                }
                else if (clingoOutput.Result == "UNSATISFIABLE")
                {
                    status = Status.UNSATISFIABLE;
                }
                else if (clingoOutput.Result == "UNKNOWN")
                {
                    status = Status.ERROR;
                }

                if (status == Status.SATISFIABLE)
                {
                    var values = clingoOutput.Call[0].Witnesses[0].Value;


                    foreach (string value in values)
                    {
                        int start = value.IndexOf('(');
                        int end = value.IndexOf(')');

                        if (start < 0 || end < 0)
                        {
                            string key = value;
                            if (!answerSet.ContainsKey(key))
                            {
                                answerSet.Add(key, new List<List<string>>());
                            }

                            answerSet[key].Add(new List<string>(trueArray));
                        }
                        else
                        {
                            string key = value.Substring(0, start);
                            string keyValue = value.Substring(start + 1, end - start - 1);

                            if (!answerSet.ContainsKey(key))
                            {
                                answerSet.Add(key, new List<List<string>>());
                            }

                            string[] body = keyValue.Split(',');
                            answerSet[key].Add(new List<string>(body));

                        }
                    }
                }

                totalSolutionsFound = clingoOutput.Models.Number;
                moreSolutions = !clingoOutput.Models.More.Equals("no");
                duration = clingoOutput.Time.Total;
            }
            isSolverRunning = false;
            UnityEngine.Debug.Log("Solver is Done.");
        }

        //public void StopSolver()
        //{
        //    if (clingoThread != null && clingoThread.IsAlive)
        //    {
        //        if (clingoProcess != null)
        //        {
        //            clingoProcess.Kill();
        //        }
        //    }
        //}


        void OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            clingoConsoleOutput += e.Data + "\n";
        }


    private bool SetUpProcess()
        {
            if (status == Status.RUNNING)
            {
                UnityEngine.Debug.LogWarning("The Solver is already running.");
                return false;
            }

            status = Status.UNINITIATED;

            if (useRandomSeed)
            {
                seed = Random.Range(0, 1 << 30);
            }

            if (aspFilePath == string.Empty) {
                UnityEngine.Debug.LogError("No ASP File.");
                status = Status.ASPFILENOTFOUND;
                return false;
            }

            //string path = Path.Combine(Application.dataPath, clingoExecutablePathMacOS);
            string clingopath = "";

            if (Application.isEditor)
            {
                if (Application.platform == RuntimePlatform.WindowsEditor) {
                    clingopath = Path.Combine(System.Environment.CurrentDirectory, "Assets", clingoExecutablePathWin);
                }
                else if (Application.platform == RuntimePlatform.OSXEditor)
                {
                    clingopath = Path.Combine(System.Environment.CurrentDirectory, "Assets", clingoExecutablePathMacOS);
                }
            }
            else
            {
                if (Application.platform == RuntimePlatform.WindowsPlayer)
                {
                    clingopath = Path.Combine(System.Environment.CurrentDirectory, clingoExecutablePathWin);
                }
                else if (Application.platform == RuntimePlatform.OSXPlayer)
                {
                    clingopath = Path.Combine(System.Environment.CurrentDirectory, clingoExecutablePathMacOS);
                }
            }

            //outputclingopath = clingopath;

            if (!File.Exists(clingopath)) {
                UnityEngine.Debug.LogError("Clingo is missing.");
                status = Status.CLINGONOTFOUND;
                print(clingopath);
                return false;
            }

            //aspFilePath = AssetDatabase.GetAssetPath(aspFile);

            if (clingoThread == null) { clingoThread = new Thread(MyThread); }
            else if (clingoThread.IsAlive) {
                UnityEngine.Debug.LogWarning("Thread State while Alive: " + clingoThread.ThreadState.ToString());
                clingoThread = new Thread(MyThread);
            }
            else
            {
                clingoThread = new Thread(MyThread);
            }




            if (clingoProcess == null) { clingoProcess = new Process(); }
            clingoProcess.StartInfo.FileName = clingopath;
            UpdateClingoASPArguments();
            clingoProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            clingoProcess.StartInfo.UseShellExecute = false;
            clingoProcess.StartInfo.CreateNoWindow = true;
            clingoProcess.StartInfo.RedirectStandardOutput = true;
            clingoProcess.StartInfo.RedirectStandardError = true;

            //clingoProcess.OutputDataReceived += ((sender, e) =>
            //{
            //    clingoConsoleOutput += e.Data + "\n";
            //});

            clingoProcess.OutputDataReceived += OutputDataReceived;



            if (maxDuration < 1) { maxDuration = 10; } // 10 sec
            if (numOfSolutionsWanted < 0) { numOfSolutionsWanted = 1; }

            solutionOutput = "";
            clingoConsoleOutput = "";
            clingoConsoleError = "";
            duration = 0;
            totalSolutionsFound = -1;
            answerSet.Clear();


            status = Status.READY;
            return true;
        }


        private void UpdateClingoASPArguments()
        {
            string arguments = " --outf=2 ";

            string filepath = "";

            if (Application.isEditor)
            {
                filepath = Path.Combine(System.Environment.CurrentDirectory, "Assets", aspFilePath);
            }
            else
            {
                filepath = Path.Combine(System.Environment.CurrentDirectory, aspFilePath);
            }

            //outputasppath = filepath;
            //string clingopath = Path.Combine(System.Environment.CurrentDirectory, "DataFiles/Clingo/clingo");
            //string path = Path.Combine(Application.dataPath, aspFilePath);

            //arguments += aspFilePath + " ";
            arguments += "\"" + filepath + "\" ";

            if (FindMultipleSolutions)
            {
                arguments += numOfSolutionsWanted.ToString() + " "; // 0 to show all answers
            }
            arguments += "--sign-def=rnd --seed=" + seed;
            arguments += " " + AdditionalArguments;

            //outputarguments = arguments;
            clingoProcess.StartInfo.Arguments = arguments;
        }
    }
}