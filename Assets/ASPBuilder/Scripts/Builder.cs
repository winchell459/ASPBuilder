using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Clingo;
namespace ASPBuilder
{
    public class Builder : MonoBehaviour
    {
        public ClingoSolver clingoSolver;
        public ASPRun[] runs;
        private List<ASPRun> runsQueue = new List<ASPRun>();
        //private int runsIndex;

        

       

        


        
        // Start is called before the first frame update
        void Start()
        {
            foreach (ASPRun run in runs) runsQueue.Add(run);
            StartRun();
        }

       
        // Update is called once per frame
        void Update()
        {
            if(runsQueue.Count > 0)
            {
                if (clingoSolver.SolverStatus == ClingoSolver.Status.SATISFIABLE)
                {
                    Dictionary<string, List<List<string>>> solution = new Dictionary<string, List<List<string>>>(clingoSolver.answerSet);
                    runsQueue[0].solution = solution;
                    if (runsQueue[0].SATISFIABLE(solution, clingoSolver.Duration) == ASPRun.ASPRunAction.requeue)
                    {
                        StartRun();
                    }
                    else if(runsQueue[0].SATISFIABLE(clingoSolver.answerSet, clingoSolver.Duration) == ASPRun.ASPRunAction.dequeue)
                    {
                        runsQueue.RemoveAt(0);
                        StartRun();
                    }
                }
                else if (clingoSolver.SolverStatus == ClingoSolver.Status.UNSATISFIABLE)
                {
                    if (runsQueue[0].UNSATISFIABLE() == ASPRun.ASPRunAction.dequeue)
                    {
                        runsQueue.RemoveAt(0);
                        StartRun();
                    }
                }
                else if (clingoSolver.SolverStatus == ClingoSolver.Status.ERROR)
                {

                }
                else if (clingoSolver.SolverStatus == ClingoSolver.Status.TIMEDOUT)
                {

                }
            }
            
        }

        private void StartRun()
        {
            if (runsQueue.Count > 0)
            {
                ASPRun run = runsQueue[0];
                string code = run.GetASPCode();
                string additionalParameters = run.GetASPAdditionalParameters();
                string parameters = run.GetParameters();
                ClingoSolve(code + parameters, additionalParameters);

            }
        }
       

        

        public void ClingoSolve(string code, string parameters)
        {
            string path = ClingoUtil.CreateFile(code);
            clingoSolver.Solve(path, parameters);
        }

    }
}

