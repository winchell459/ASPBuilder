using System.IO;
using UnityEngine;
using UnityEngine.UI;


namespace Clingo {
    public class SolverUI : MonoBehaviour
    {
        public Text text;
        public ClingoSolver solver;
        public InputField inputField;

        public string aspcode = "";

        public string ASPcode { get { return aspcode; } set { aspcode = value; } }

        void Start()
        {
        }

        public void solve()
        {
            string path = ClingoUtil.CreateFile(aspcode);
            print("Created " + path);
            solver.aspFilePath = path;
            solver.Solve();
        }


        // Update is called once per frame
        void Update()
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
            }

            inputField.text = aspcode;

            if (solver.SolverStatus == ClingoSolver.Status.SATISFIABLE)
            {
                text.text = solver.SolutionOutput;
            }
            else
            {
                text.text = solver.SolverStatus.ToString();
                //text.text += "\n" + solver.outputasppath;
                //text.text += "\n" + solver.outputclingopath;
                text.text += "\n" + solver.ClingoConsoleError;
                //text.text += "\n" + solver.outputarguments;
            }
        }
    }
}