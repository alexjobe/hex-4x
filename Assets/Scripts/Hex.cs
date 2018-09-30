using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// The Hex class defines the grid position, world-space
// position, neighbors, etc... of a hex tile. However, it
// does NOT interact directly with Unity in any way

public class Hex {

    public Hex(HexMap hexMap, int q, int r)
    {
        this.HexMap = hexMap;
        this.Q = q;
        this.R = r;
        this.S = -(q + r);
    }

    // Q + R + S = 0
    // S = -(Q + R)

    public readonly int Q;  // Column
    public readonly int R;  // Row
    public readonly int S;

    // Data for map generation and in-game effects
    public float Elevation;
    public float Moisture;

    public readonly HexMap HexMap;

    static readonly float WIDTH_MULTIPLIER = Mathf.Sqrt(3) / 2;

    float radius = 1f;

    HashSet<Unit> units;

    //Returns the world-space position of this hex
    public Vector3 Position()
    {
        return new Vector3(
            HexHorizontalSpacing() * (this.Q + this.R / 2f),
            0,
            HexVerticalSpacing() * this.R
        );
    }

    public float HexHeight()
    {
        return radius * 2;
    }

    public float HexWidth()
    {
        return WIDTH_MULTIPLIER * HexHeight();
    }

    public float HexVerticalSpacing()
    {
        return HexHeight() * 0.75f;
    }

    public float HexHorizontalSpacing()
    {
        return HexWidth();
    }

    public Vector3 PositionFromCamera()
    {
        return HexMap.GetHexPosition(this);
    }

    public Vector3 PositionFromCamera(Vector3 cameraPosition, float numRows, float numColumns)
    {
        float mapHeight = numRows * HexVerticalSpacing();
        float mapWidth = numColumns * HexHorizontalSpacing();

        Vector3 position = Position();

        if (HexMap.AllowWrapEastWest)
        {
            float howManyWidthsFromCamera = (position.x - cameraPosition.x) / mapWidth;

            if (howManyWidthsFromCamera > 0)
                howManyWidthsFromCamera += 0.5f;
            else
                howManyWidthsFromCamera -= 0.5f;

            int howManyWidthsToFix = (int)howManyWidthsFromCamera;

            position.x -= howManyWidthsToFix * mapWidth;
        }

        if (HexMap.AllowWrapNorthSouth)
        {
            float howManyHeightsFromCamera = (position.z - cameraPosition.z) / mapHeight;

            if (howManyHeightsFromCamera > 0)
                howManyHeightsFromCamera += 0.5f;
            else
                howManyHeightsFromCamera -= 0.5f;

            int howManyHeightsToFix = (int)howManyHeightsFromCamera;

            position.z -= howManyHeightsToFix * mapHeight;
        }

        return position;
    }

    public static float Distance(Hex a, Hex b)
    {
        int dQ = Mathf.Abs(a.Q - b.Q);
        if (a.HexMap.AllowWrapEastWest)
        {
            if (dQ > a.HexMap.NumColumns / 2)
                dQ = a.HexMap.NumColumns - dQ;
        }

        int dR = Mathf.Abs(a.R - b.R);
        if (a.HexMap.AllowWrapNorthSouth)
        {
            if (dR > a.HexMap.NumRows / 2)
                dR = a.HexMap.NumRows - dR;
        }

        return 
            Mathf.Max(
                dQ,
                dR,
                Mathf.Abs(a.S - b.S)
            );
    }

    public void AddUnit(Unit unit)
    {
        if(units == null)
        {
            units = new HashSet<Unit>();
        }

        units.Add(unit);
    }

    public void RemoveUnit(Unit unit)
    {
        if (units != null)
        {
            units.Remove(unit);
        }
    }

    public Unit[] Units()
    {
        return units.ToArray();
    }
}
