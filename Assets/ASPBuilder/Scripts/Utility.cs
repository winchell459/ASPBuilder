using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ASPBuilder
{
    public static class Utility 
    {
        //public static string SetupSolverParameters(int[] constants)
        //{
        //    string parameters = "";
        //    foreach(int value)
        //}

        public static T[] ConvertArray<T>(List<T> array)
        {
            T[] newArray = new T[array.Count];
            for(int i = 0; i < array.Count; i += 1)
            {
                newArray[i] = array[i];
            }
            return newArray;
        }

        public static List<T> ConvertArray<T>(T[] array)
        {
            List<T> newArray = new List<T>();
            for (int i = 0; i < array.Length; i += 1)
            {
                newArray.Add( array[i]);
            }
            return newArray;
        }
        public static string CopyPredicates(Dictionary<string, List<List<string>>> solution, string predicate)
        {
            return CopyPredicates(solution, predicate, predicate);
        }

            public static string CopyPredicates(Dictionary<string, List<List<string>>> solution, string predicate, string newPredicate)
        {
            string aspCode = "";
            foreach(List<string> values in solution[predicate])
            {
                string parameters = "";
                for (int i = 0; i < values.Count; i += 1)
                {
                    if (i > 0) parameters += ",";
                    parameters += $"{values[i]}";
                }
                aspCode += $"{newPredicate}({parameters}).\n";
                
                
            }

            return aspCode;
        }
    }
}

