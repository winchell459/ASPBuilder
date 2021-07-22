using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Clingo;

[CustomEditor(typeof(SolverUI))]
public class SolverUIEditor : Editor
{

    string arguments;
    ClingoSolver.Status clingoStatus;
    string solution;
    string aspcode;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SolverUI solverUI = (SolverUI)target;
        ClingoSolver solver = solverUI.solver;
        clingoStatus = solver.SolverStatus;

        aspcode = solverUI.ASPcode;

        //EditorGUILayout.PrefixLabel("ASP Code");
        //solverUI.ASPcode = EditorGUILayout.TextArea(solverUI.ASPcode);

        EditorGUILayout.PrefixLabel("ASP Code");
        aspcode = EditorGUILayout.TextArea(aspcode);
        solverUI.ASPcode = aspcode;

        EditorGUILayout.PrefixLabel("Additional Arguments");
        arguments = EditorGUILayout.TextArea(arguments);



        if (GUILayout.Button("Run"))
        {
            if (clingoStatus != ClingoSolver.Status.RUNNING)
            {
                string path = ClingoUtil.CreateFile(solverUI.ASPcode);
                solver.Solve(path, arguments);
            }
        }


        EditorGUILayout.LabelField(clingoStatus.ToString());

        if (clingoStatus != ClingoSolver.Status.RUNNING)
        {
            solution = solver.AnswerSetToString();
            EditorGUILayout.TextArea(solution);
        }
    }
}
