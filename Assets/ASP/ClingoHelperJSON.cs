using System;
using System.Collections.Generic;

namespace Clingo
{
    namespace ClingoHelperJSON
    {
        [Serializable]
        public class Witness
        {
            public List<string> Value;
        }

        [Serializable]
        public class Call
        {
            public List<Witness> Witnesses;
        }

        [Serializable]
        public class Models
        {
            public int Number;
            public string More;
        }

        [Serializable]
        public class Time
        {
            public double Total;
            public double Solve;
            public double Model;
            public double Unsat;
            public double CPU;
        }

        [Serializable]
        public class ClingoRoot
        {
            public string Solver;
            public List<string> Input;
            public List<Call> Call;
            public string Result;
            public Models Models;
            public int Calls;
            public Time Time;
        }
    }
}