using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ASPBuilder
{
    public class BuildParameterNeighbors : ASPParameters
    {
        public ASPRun Neighbor;
        public override string GetParameters()
        {
            string parameter = "";
            if (Neighbor.Status == ASPRun.ASPRunStatus.finished)
            {
                int max_width = 0, max_depth = 0;

                foreach (List<string> block in FindObjectOfType<Clingo.ClingoSolver>().answerSet["block"])
                {
                    if (int.Parse(block[0]) > max_width) max_width = int.Parse(block[0]);
                    if (int.Parse(block[2]) > max_depth) max_depth = int.Parse(block[2]);
                }

                foreach (List<string> block in FindObjectOfType<Clingo.ClingoSolver>().answerSet["block"])
                {
                    if (int.Parse(block[0]) == max_width)
                    {
                        parameter += $"\n block({0},{block[1]},{block[2]}, {block[3]}).";
                    }
                    if (int.Parse(block[2]) == max_depth)
                    {

                    }
                }
            }
            Debug.Log(parameter);
            return parameter;
        }
    }
}

