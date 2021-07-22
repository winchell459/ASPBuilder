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
    }
}

