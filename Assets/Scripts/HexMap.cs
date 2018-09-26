using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexMap : MonoBehaviour {

    public GameObject hexPrefab;

    public Mesh meshWater;
    public Mesh meshFlat;
    public Mesh meshHill;
    public Mesh meshMountain;

    public Material matOcean;
    public Material matPlains;
    public Material matGrasslands;
    public Material matMountains;
    public Material matDesert;

    public GameObject forestPrefab;
    public GameObject junglePrefab;

    // Height thresholds to determine tile type
    public float heightMountain = 1f;
    public float heightHill = 0.6f;
    public float heightFlat = 0.0f;

    public float moistureJungle = 0.66f;
    public float moistureForest = 0.33f;
    public float moistureGrasslands = 0.0f;
    public float moisturePlains = -0.5f;

    public readonly int numRows = 30;
    public readonly int numColumns = 60;

    public bool allowWrapEastWest = true;
    public bool allowWrapNorthSouth = false;

    private Hex[,] hexes;
    private Dictionary<Hex, GameObject> hexToGameObjectMap;

    public Hex GetHexAt(int x, int y)
    {
        if (hexes == null)
        {
            Debug.LogError("Hexes array not yet instantiated");
            return null;
        }

        if (allowWrapEastWest)
        {
            x = x % numColumns;
            if (x < 0)
            {
                x += numColumns;
            }
        }
        if (allowWrapNorthSouth)
        {
            y = y % numRows;
            if (y < 0)
            {
                y += numRows;
            }
        }

        return hexes[x, y];
    }

    // Use this for initialization
    void Start () {
        GenerateMap();
	}

    virtual public void GenerateMap()
    {
        hexes = new Hex[numColumns, numRows];
        hexToGameObjectMap = new Dictionary<Hex, GameObject>();

        for (int column = 0; column < numColumns; column++)
        {
            for (int row = 0; row < numRows; row++)
            {
                // Instantiate a hex
                Hex h = new Hex(this, column, row);
                h.elevation = -0.5f;

                hexes[column, row] = h;

                Vector3 pos = h.PositionFromCamera(
                    Camera.main.transform.position,
                    numRows,
                    numColumns
                );

                GameObject hexGO = (GameObject)Instantiate(
                    hexPrefab, 
                    pos,
                    Quaternion.identity,
                    this.transform
                );

                hexToGameObjectMap[h] = hexGO;

                hexGO.name = string.Format("{0},{1}", column, row);
                hexGO.GetComponent<HexComponent>().hex = h;
                hexGO.GetComponent<HexComponent>().hexMap = this;

                hexGO.GetComponentInChildren<TextMesh>().text = string.Format("{0},{1}", column, row);
            }
        }

        UpdateHexVisuals();

        //StaticBatchingUtility.Combine(this.gameObject);
    }

    public void UpdateHexVisuals()
    {
        for (int column = 0; column < numColumns; column++)
        {
            for (int row = 0; row < numRows; row++)
            {
                Hex h = hexes[column, row];
                GameObject hexGO = hexToGameObjectMap[h];

                MeshRenderer mr = hexGO.GetComponentInChildren<MeshRenderer>();
                MeshFilter mf = hexGO.GetComponentInChildren<MeshFilter>();

                if (h.elevation >= heightFlat && h.elevation < heightMountain)
                {
                    if (h.moisture >= moistureJungle)
                    {
                        mr.material = matGrasslands;
                        // Spawn jungle
                        Vector3 treePos = hexGO.transform.position;
                        if (h.elevation > heightHill)
                        {
                            treePos.y += 0.2f;
                        }
                        GameObject.Instantiate(junglePrefab, treePos, Quaternion.identity, hexGO.transform);
                    }
                    else if (h.moisture >= moistureForest)
                    {
                        mr.material = matGrasslands;
                        // Spawn forest
                        Vector3 treePos = hexGO.transform.position;
                        if(h.elevation > heightHill)
                        {
                            treePos.y += 0.2f;
                        }
                        GameObject.Instantiate(forestPrefab, treePos, Quaternion.identity, hexGO.transform);
                    }
                    else if (h.moisture >= moistureGrasslands)
                    {
                        mr.material = matGrasslands;
                    }
                    else if (h.moisture >= moisturePlains)
                    {
                        mr.material = matPlains;
                    }
                    else
                    {
                        mr.material = matDesert;
                    }
                }

                if (h.elevation >= heightMountain)
                {
                    mr.material = matMountains;
                    mf.mesh = meshMountain;
                }
                else if (h.elevation >= heightHill)
                {
                    mf.mesh = meshHill;
                }
                else if (h.elevation >= heightFlat)
                {
                    mf.mesh = meshFlat;
                }
                else
                {
                    mr.material = matOcean;
                    mf.mesh = meshFlat;
                }
            }
        }
    }

    public Hex[] GetHexesWithinRangeOf(Hex centerHex, int range)
    {
        List<Hex> results = new List<Hex>();

        for(int dx = -range; dx < range-1; dx++)
        {
            for (int dy = Mathf.Max(-range+1, -dx-range); dy < Mathf.Min(range, -dx+range-1); dy++)
            {
                results.Add(GetHexAt(centerHex.Q + dx, centerHex.R + dy));
            }
        }

        return results.ToArray();
    }
}
