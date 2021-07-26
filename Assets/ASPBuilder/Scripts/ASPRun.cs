using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ASPBuilder
{
    public abstract class ASPRun : MonoBehaviour
    {
        [SerializeField] private ASPParameters[] Parameters;
        public string GetParameters()
        {
            string parameters = "";
            foreach(ASPParameters parameter in Parameters)
            {
                parameters += parameter.GetParameters();
            }
            return parameters;
        }

        //public abstract void Run();
        public abstract string GetASPCode();
        public abstract string GetASPAdditionalParameters();
        public abstract ASPRunAction SATISFIABLE(Dictionary<string, List<List<string>>> solution, double runtime);
        public abstract ASPRunAction UNSATISFIABLE();
        public abstract ASPRunAction TIMEOUT();
        public abstract ASPRunAction ERROR();
        public Dictionary<string, List<List<string>>> solution {
            get {
                return solutions.Count > 0 ? solutions[solutions.Count - 1] : null;
            }
            set
            {
                //Dictionary<string, List<List<string>>> copy = new Dictionary<string, List<List<string>>>(value);
                //solutions.Add(copy);
                solutions.Add(value);
            }
        }
        private List<Dictionary<string, List<List<string>>>> solutions = new List<Dictionary<string, List<List<string>>>>();

        public Dictionary<string, List<List<string>>> GetSolution(int index)
        {
            return solutions[index];
        }

        //public ASPRunAction SATISFIABLE

        public enum ASPRunStatus
        {
            ready,
            running,
            finished
        }
        public ASPRunStatus Status;
        public enum ASPRunAction
        {
            dequeue,
            requeue,
            randomrequeue,
            hold
        }
    }
}

