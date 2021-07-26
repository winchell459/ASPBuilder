using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASPBuilder;

public abstract class BuildTerrainASPRun : ASPRun
{
    public GameObject GrassPrefab, WaterPrefab, SandPrefab;
    protected void PlaceTerrain(Dictionary<string, List<List<string>>> solution, Transform buildLocation, string key)
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
            blockObj.transform.localPosition = new Vector3(x, y / 8, z);
        }
    }
}
