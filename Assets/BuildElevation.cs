using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASPBuilder;

public class BuildElevation : BuildTerrainASPRun
{
    public double SolveTime;

    public int height = 10, cpus = 1;
    
    public Transform[] builds;
    public ASPRun SolutionSource;

    //public override void Run()
    //{
    //    string code = GetASPCode();
    //    string parameters = GetASPParameters();
    //    FindObjectOfType<Builder>().ClingoSolve(code, parameters);
    //}
    public override string GetASPCode()
    {
        string input = Utility.CopyPredicates(SolutionSource.solution, "block", "block2D");
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

            1{block(XX,YY,ZZ,Type): height(YY)}1 :- block2D(XX,_,ZZ,Type), XX > 0.
            1{block(XX,YY,ZZ,Type): height(YY)}1 :- block2D(XX,ZZ,Type), XX > 0.

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

    
}
