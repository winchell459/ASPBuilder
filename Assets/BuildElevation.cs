using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASPBuilder;

public class BuildElevation : ASPRun
{
    public double SolveTime;

    public int height = 10, cpus = 1;
    public GameObject GrassPrefab, WaterPrefab, SandPrefab;
    public Transform[] builds;

    //public override void Run()
    //{
    //    string code = GetASPCode();
    //    string parameters = GetASPParameters();
    //    FindObjectOfType<Builder>().ClingoSolve(code, parameters);
    //}
    public override string GetASPCode()
    {
        string input = Utility.CopyPredicates(FindObjectOfType<Clingo.ClingoSolver>().answerSet, "block", "block2D");
        return elevation + input;
    }
    public override string GetASPAdditionalParameters()
    {
        return $" -c max_height={height} --parallel-mode {cpus}";
    }
    public override ASPRunAction SATISFIABLE(Dictionary<string, List<List<string>>> solution, double runtime)
    {
        SolveTime = runtime;
        PlaceTerrain(solution, builds[0], "block");
        Status = ASPRunStatus.finished;
        return ASPRunAction.dequeue;
    }
    public override ASPRunAction UNSATISFIABLE()
    {
        Status = ASPRunStatus.finished;
        return ASPRunAction.dequeue;
    }
    public override ASPRunAction TIMEOUT()
    {
        throw new System.NotImplementedException();
    }
    public override ASPRunAction ERROR()
    {
        throw new System.NotImplementedException();
    }


    

    string elevation = @"
                
            
            #const max_height = 10.

            height(1..max_height).

            1{block(XX,YY,ZZ,Type): height(YY)}1 :- block2D(XX,_,ZZ,Type).
            1{block(XX,YY,ZZ,Type): height(YY)}1 :- block2D(XX,ZZ,Type).

            :- block(XX,Y1,ZZ,_), block(XX-1,Y2,ZZ,_), Y1 < Y2-1.
            :- block(XX,Y1,ZZ,_), block(XX-1,Y2,ZZ,_), Y1 > Y2+1.

            :- block(XX,Y1,ZZ,_), block(XX+1,Y2,ZZ,_), Y1 < Y2-1.
            :- block(XX,Y1,ZZ,_), block(XX+1,Y2,ZZ,_), Y1 > Y2+1.

            :- block(XX,Y1,ZZ,_), block(XX,Y2,ZZ-1,_), Y1 < Y2-1.
            :- block(XX,Y1,ZZ,_), block(XX,Y2,ZZ-1,_), Y1 > Y2+1.

            :- block(XX,Y1,ZZ,_), block(XX,Y2,ZZ+1,_), Y1 < Y2-1.
            :- block(XX,Y1,ZZ,_), block(XX,Y2,ZZ+1,_), Y1 > Y2+1.

            :- {block(_,YY,_,_): YY == 1} < 1.
            :- {block(_,YY,_,_): YY == max_height} < 1.


            %neghboring waters must have same height
            :- block(XX,Y1,ZZ,water), block(XX-1,Y2,ZZ,water), block(XX+1,Y3,ZZ,water), Y3 == Y2, not Y1 == Y2.
            %:- block(XX,Y1,ZZ,water), block(XX+1,Y2,ZZ,water), not Y1 == Y2.
            :- block(XX,Y1,ZZ,water), block(XX,Y2,ZZ-1,water), block(XX,Y3,ZZ+1,water), Y3 == Y2, not Y1 == Y2.
            %:- block(XX,Y1,ZZ,water), block(XX,Y2,ZZ+1,water), not Y1 == Y2.

            %:- block(XX,Y1,ZZ,water), block(XX-1,Y2,ZZ+1,water), not Y1 == Y2.
            %:- block(XX,Y1,ZZ,water), block(XX+1,Y2,ZZ+1,water), not Y1 == Y2.
            %:- block(XX,Y1,ZZ,water), block(XX+1,Y2,ZZ-1,water), not Y1 == Y2.
            %:- block(XX,Y1,ZZ,water), block(XX-1,Y2,ZZ+1,water), not Y1 == Y2.


            %water must not be higher then neighbor
            :- block(XX,Y1,ZZ,water), block(XX-1,Y2,ZZ,Type), Y1 > Y2-1, Type != water.
            :- block(XX,Y1,ZZ,water), block(XX+1,Y2,ZZ,Type), Y1 > Y2-1, Type != water.
            :- block(XX,Y1,ZZ,water), block(XX,Y2,ZZ-1,Type), Y1 > Y2-1, Type != water.
            :- block(XX,Y1,ZZ,water), block(XX,Y2,ZZ+1,Type), Y1 > Y2-1, Type != water.

            :- block(XX,Y1,ZZ,water), block(XX-1,Y2,ZZ+1,Type), Y1 > Y2-1, Type != water.
            :- block(XX,Y1,ZZ,water), block(XX+1,Y2,ZZ+1,Type), Y1 > Y2-1, Type != water.
            :- block(XX,Y1,ZZ,water), block(XX+1,Y2,ZZ-1,Type), Y1 > Y2-1, Type != water.
            :- block(XX,Y1,ZZ,water), block(XX-1,Y2,ZZ+1,Type), Y1 > Y2-1, Type != water.


            
            ";

    void PlaceTerrain(Dictionary<string, List<List<string>>> solution, Transform buildLocation, string key)
    {
        foreach (List<string> block in solution[key])
        {
            int x = 0;
            float y = 0;
            int z = 0;
            string type = "grass";
            GameObject prefab = GrassPrefab;
            if (block.Count == 3)
            {
                x = int.Parse(block[0]);
                z = int.Parse(block[1]);
                type = block[2];
            }
            else if (block.Count == 4)
            {
                x = int.Parse(block[0]);
                z = int.Parse(block[2]);
                y = int.Parse(block[1]);
                type = block[3];
            }


            if (type == "water") prefab = WaterPrefab;
            else if (type == "sand") prefab = SandPrefab;
            GameObject blockObj = Instantiate(prefab, buildLocation);
            blockObj.transform.localPosition = new Vector3(x, y / 10, z);
        }
    }
}
