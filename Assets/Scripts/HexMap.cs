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

    public readonly int numRows = 30;
    public readonly int numColumns = 60;

    bool allowWrapEastWest = true;
    bool allowWrapNorthSouth = false;

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
            x = x % numColumns;
        if (allowWrapNorthSouth)
            y = y % numRows;

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
                Hex h = new Hex(column, row);
                h.elevation = -1;

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

                if(h.elevation >= 0)
                {
                    mr.material = matGrasslands;
                }
                else
                {
                    mr.material = matOcean;
                }

                MeshFilter mf = hexGO.GetComponentInChildren<MeshFilter>();
                mf.mesh = meshWater;
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
                results.Add(hexes[centerHex.Q + dx, centerHex.R + dy]);
            }
        }

        return results.ToArray();
    }
}
