using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASPBuilder;

public class BuildVegetation : BuildTerrainASPRun
{
    public double SolveTime;
    public int width = 10, depth = 10, height = 10, cpus = 1, waterPercent = 30, grassPercent = 30;
    
    public Transform[] builds;

    //public override void Run()
    //{
    //    string code = GetASPCode();
    //    string parameters = GetASPParameters();
    //    FindObjectOfType<Builder>().ClingoSolve(code, parameters);
    //}
    public override string GetASPCode()
    {
        return vegetation;
    }
    public override string GetASPAdditionalParameters()
    {
        return $" -c max_width={width} -c max_depth={depth} -c step_count={height} -c water_percent={waterPercent} -c grass_percent={grassPercent} --parallel-mode {cpus}";
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


    

    string vegetation = @"
            #const max_width = 10.
            #const max_depth = 10.
            
            #const water_percent = 30.
            #const grass_percent = 30.

            width(1..max_width).
            depth(1..max_depth).
            

            block_types(grass;water;sand).

            1{block(XX,ZZ,TYPE): block_types(TYPE)}1 :- width(XX), depth(ZZ).


            %neghboring waters must not be grass
            :- block(XX,ZZ,water), block(XX-1,ZZ,grass).
            :- block(XX,ZZ,water), block(XX+1,ZZ,grass).
            :- block(XX,ZZ,water), block(XX,ZZ-1,grass).
            :- block(XX,ZZ,water), block(XX,ZZ+1,grass).

            :- block(XX,ZZ,water), block(XX-1,ZZ-1,grass).
            :- block(XX,ZZ,water), block(XX-1,ZZ+1,grass).

            :- block(XX,ZZ,water), block(XX+1,ZZ-1,grass).
            :- block(XX,ZZ,water), block(XX+1,ZZ+1,grass).


            %sand cannot be surrounded by grass
            :- block(XX,ZZ,sand), {block(XX-1,ZZ,grass); block(XX+1,ZZ,grass);block(XX,ZZ-1,grass);block(XX,ZZ+1,grass)}==4.

            %grass cannot be surrounded by sand
            :- block(XX,ZZ,grass), {block(XX-1,ZZ,sand); block(XX+1,ZZ,sand)}==2.
            :- block(XX,ZZ,grass), {block(XX,ZZ-1,sand);block(XX,ZZ+1,sand)}==2.
            
            %sand must have a water or sand neighbor
            sand_depth(1..3).
            :- block(XX,ZZ,sand), {
                                        block(XX-Depth,ZZ, water): sand_depth(Depth);
                                        block(XX+Depth,ZZ,water): sand_depth(Depth);
                                        block(XX,ZZ-Depth,water): sand_depth(Depth);
                                        block(XX,ZZ+Depth,water): sand_depth(Depth)} < 1.


            %water must have water neighbor
            :- block(XX,ZZ,water), {
                                        block(XX-1,ZZ, water);
                                        block(XX+1,ZZ,water);
                                        block(XX,ZZ-1,water);
                                        block(XX,ZZ+1,water)} < 2.
            

            %grass percentage
            %:- Grass = {block(XX,ZZ,grass)}, (100 * Grass) / (max_width * max_depth) < grass_percent.
            %:- Water = {block(XX,ZZ,water)}, (100 * Water) / (max_width * max_depth) < water_percent.
            
            %#show path/2.
            #show block/3.
            

            :- {block(_,_,grass)} == 0.
            

            ";

    
}
