using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Clingo;
namespace ASPBuilder
{
    public class Builder : MonoBehaviour
    {
        public ClingoSolver clingoSolver;

        public int width = 10, depth = 10, height = 10, cpus = 1;
        public GameObject BlockPrefab, WaterPrefab, SandPrefab;

        string terrain = @"
            #const max_width = 10.
            #const max_depth = 10.
            #const step_count = 10.

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
            :- block(XX,Y1,ZZ,water), block(XX-1,Y2,ZZ,water), not Y1 == Y2.
            :- block(XX,Y1,ZZ,water), block(XX+1,Y2,ZZ,water), not Y1 == Y2.
            :- block(XX,Y1,ZZ,water), block(XX,Y2,ZZ-1,water), not Y1 == Y2.
            :- block(XX,Y1,ZZ,water), block(XX,Y2,ZZ+1,water), not Y1 == Y2.

            :- block(XX,Y1,ZZ,water), block(XX-1,Y2,ZZ+1,water), not Y1 == Y2.
            :- block(XX,Y1,ZZ,water), block(XX+1,Y2,ZZ+1,water), not Y1 == Y2.
            :- block(XX,Y1,ZZ,water), block(XX+1,Y2,ZZ-1,water), not Y1 == Y2.
            :- block(XX,Y1,ZZ,water), block(XX-1,Y2,ZZ+1,water), not Y1 == Y2.

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
            %:- Grass = {block(XX,_,ZZ,grass)}, (100 * Grass) / (max_width * max_depth) < 10.
            
            %#show path/2.
            #show block/4.
            



            ";

        // Start is called before the first frame update
        void Start()
        {
            string parameters = $" -c max_width={width} -c max_depth={depth} -c step_count={height} --parallel-mode {cpus}";
            string path = ClingoUtil.CreateFile(terrain);
            clingoSolver.Solve(path, parameters);
        }

        bool waiting = true;
        bool solved = false;
        // Update is called once per frame
        void Update()
        {
            if(waiting && clingoSolver.SolverStatus == ClingoSolver.Status.SATISFIABLE)
            {
                
                waiting = false;
                solved = true;
            }else if (solved)
            {
                foreach (List<string> block in clingoSolver.answerSet["block"])
                {
                    int x = int.Parse(block[0]);
                    int z = int.Parse(block[2]);
                    float y = int.Parse(block[1]);
                    GameObject prefab = BlockPrefab;
                    if (block[3] == "water") prefab = WaterPrefab;
                    else if (block[3] == "sand") prefab = SandPrefab;
                    GameObject blockObj = Instantiate(prefab);
                    blockObj.transform.position = new Vector3(x, y / 10, z);
                }
                solved = false;
            }
        }
    }
}

