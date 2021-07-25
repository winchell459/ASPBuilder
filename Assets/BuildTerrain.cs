﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASPBuilder;

//[CreateAssetMenu(fileName = "ASPRun", menuName = "ScriptableObjects/ASPBuilder/new Terrain Build", order = 1)]
public class BuildTerrain : ASPRun
{
    public double SolveTime;
    public int width = 10, depth = 10, height = 10, cpus = 1, waterPercent = 30, grassPercent = 30;
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
        return terrain;
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
        return ASPRunAction.hold;
    }
    public override ASPRunAction TIMEOUT()
    {
        throw new System.NotImplementedException();
    }
    public override ASPRunAction ERROR()
    {
        throw new System.NotImplementedException();
    }


    
    

    string terrain = @"
            #const max_width = 10.
            #const max_depth = 10.
            #const step_count = 10.
            #const water_percent = 30.
            #const grass_percent = 30.

            width(1..max_width).
            depth(1..max_depth).
            height(1..step_count).

            block_types(grass;water;sand).

            1{block(XX,YY,ZZ,TYPE): height(YY), block_types(TYPE)}1 :- width(XX), depth(ZZ).
            start(1,1).

            %path(XX,ZZ) :- start(XX,ZZ).

            %path(XX,ZZ) :- path(XX - 1, ZZ), block(XX,YY,ZZ), block(XX - 1, Y2, ZZ), YY <= Y2+1, YY >= Y2-1.
            %path(XX,ZZ) :- path(XX + 1, ZZ), block(XX,YY,ZZ), block(XX + 1, Y2, ZZ), YY <= Y2+1, YY >= Y2-1.
            %path(XX,ZZ) :- path(XX, ZZ-1), block(XX,YY,ZZ), block(XX, Y2, ZZ-1), YY <= Y2+1, YY >= Y2-1.
            %path(XX,ZZ) :- path(XX, ZZ+1), block(XX,YY,ZZ), block(XX, Y2, ZZ+1), YY <= Y2+1, YY >= Y2-1.
            
            %:- block(XX,_,ZZ), not path(XX,ZZ).

            :- block(XX,Y1,ZZ,_), block(XX-1,Y2,ZZ,_), Y1 < Y2-1.
            :- block(XX,Y1,ZZ,_), block(XX-1,Y2,ZZ,_), Y1 > Y2+1.

            :- block(XX,Y1,ZZ,_), block(XX+1,Y2,ZZ,_), Y1 < Y2-1.
            :- block(XX,Y1,ZZ,_), block(XX+1,Y2,ZZ,_), Y1 > Y2+1.

            :- block(XX,Y1,ZZ,_), block(XX,Y2,ZZ-1,_), Y1 < Y2-1.
            :- block(XX,Y1,ZZ,_), block(XX,Y2,ZZ-1,_), Y1 > Y2+1.

            :- block(XX,Y1,ZZ,_), block(XX,Y2,ZZ+1,_), Y1 < Y2-1.
            :- block(XX,Y1,ZZ,_), block(XX,Y2,ZZ+1,_), Y1 > Y2+1.

            :- {block(_,YY,_,_): YY == 1} < 1.
            :- {block(_,YY,_,_): YY == step_count} < 1.


            %neghboring waters must have same height
            %:- block(XX,Y1,ZZ,water), block(XX-1,Y2,ZZ,water), block(XX+1,Y3,ZZ,water), Y3 == Y2, not Y1 == Y2.
            :- block(XX,Y1,ZZ,water), block(XX+1,Y2,ZZ,water), not Y1 == Y2.
            :- block(XX,Y1,ZZ,water), block(XX-1,Y2,ZZ,water), not Y1 == Y2.
            %:- block(XX,Y1,ZZ,water), block(XX,Y2,ZZ-1,water), block(XX,Y3,ZZ+1,water), Y3 == Y2, not Y1 == Y2.
            :- block(XX,Y1,ZZ,water), block(XX,Y2,ZZ+1,water), not Y1 == Y2.
            :- block(XX,Y1,ZZ,water), block(XX,Y2,ZZ-1,water), not Y1 == Y2.

            %:- block(XX,Y1,ZZ,water), block(XX-1,Y2,ZZ+1,water), not Y1 == Y2.
            %:- block(XX,Y1,ZZ,water), block(XX+1,Y2,ZZ+1,water), not Y1 == Y2.
            %:- block(XX,Y1,ZZ,water), block(XX+1,Y2,ZZ-1,water), not Y1 == Y2.
            %:- block(XX,Y1,ZZ,water), block(XX-1,Y2,ZZ+1,water), not Y1 == Y2.

            %neghboring waters must not be grass
            :- block(XX,Y1,ZZ,water), block(XX-1,Y2,ZZ,grass).
            :- block(XX,Y1,ZZ,water), block(XX+1,Y2,ZZ,grass).
            :- block(XX,Y1,ZZ,water), block(XX,Y2,ZZ-1,grass).
            :- block(XX,Y1,ZZ,water), block(XX,Y2,ZZ+1,grass).

            :- block(XX,Y1,ZZ,water), block(XX-1,Y2,ZZ-1,grass).
            :- block(XX,Y1,ZZ,water), block(XX-1,Y2,ZZ+1,grass).

            :- block(XX,Y1,ZZ,water), block(XX+1,Y2,ZZ-1,grass).
            :- block(XX,Y1,ZZ,water), block(XX+1,Y2,ZZ+1,grass).


            %water must not be higher then neighbor
            :- block(XX,Y1,ZZ,water), block(XX-1,Y2,ZZ,Type), Y1 > Y2-1, Type != water.
            :- block(XX,Y1,ZZ,water), block(XX+1,Y2,ZZ,Type), Y1 > Y2-1, Type != water.
            :- block(XX,Y1,ZZ,water), block(XX,Y2,ZZ-1,Type), Y1 > Y2-1, Type != water.
            :- block(XX,Y1,ZZ,water), block(XX,Y2,ZZ+1,Type), Y1 > Y2-1, Type != water.

            :- block(XX,Y1,ZZ,water), block(XX-1,Y2,ZZ+1,Type), Y1 > Y2-1, Type != water.
            :- block(XX,Y1,ZZ,water), block(XX+1,Y2,ZZ+1,Type), Y1 > Y2-1, Type != water.
            :- block(XX,Y1,ZZ,water), block(XX+1,Y2,ZZ-1,Type), Y1 > Y2-1, Type != water.
            :- block(XX,Y1,ZZ,water), block(XX-1,Y2,ZZ+1,Type), Y1 > Y2-1, Type != water.


            %sand cannot be surrounded by grass
            :- block(XX,_,ZZ,sand), {block(XX-1,_,ZZ,grass); block(XX+1,_,ZZ,grass);block(XX,_,ZZ-1,grass);block(XX,_,ZZ+1,grass)}==4.

            %grass cannot be surrounded by sand
            :- block(XX,_,ZZ,grass), {block(XX-1,_,ZZ,sand); block(XX+1,_,ZZ,sand)}==2.
            :- block(XX,_,ZZ,grass), {block(XX,_,ZZ-1,sand);block(XX,_,ZZ+1,sand)}==2.
            
            %sand must have a water or sand neighbor
            sand_depth(1..3).
            :- block(XX,Y1,ZZ,sand), {block(XX-Depth,_,ZZ, water): sand_depth(Depth);
                                       
                                        block(XX+Depth,_,ZZ,water): sand_depth(Depth);
                                        block(XX,_,ZZ-Depth,water): sand_depth(Depth);
                                        block(XX,_,ZZ+Depth,water): sand_depth(Depth)} < 1.

            %sand must have a water or sand neighbor
            
           % :- block(XX,Y1,ZZ,sand), {block(XX-1,_,ZZ, sand);
           %                             block(XX+1,_,ZZ,sand);
           %                             block(XX,_,ZZ-1,sand);
           %                             block(XX,_,ZZ+1,sand);
           %                             block(XX-1,_,ZZ, water);
           %                             block(XX+1,_,ZZ,water);
           %                             block(XX,_,ZZ-1,water);
           %                             block(XX,_,ZZ+1,water)} < 1.


            %water must have water neighbor
            :- block(XX,Y1,ZZ,water), {
                                        block(XX-1,_,ZZ, water);
                                        block(XX+1,_,ZZ,water);
                                        block(XX,_,ZZ-1,water);
                                        block(XX,_,ZZ+1,water)} < 2.
            

            %grass percentage
            %:- Grass = {block(XX,_,ZZ,grass)}, (100 * Grass) / (max_width * max_depth) < grass_percent.
            %:- Water = {block(XX,_,ZZ,water)}, (100 * Water) / (max_width * max_depth) < water_percent.
            
            %#show path/2.
            #show block/4.
            
            :- {block(_,_,_,grass)} == 0.
            :- {block(_,_,_,water)} == 0.

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
