using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using QPath;

// The Hex class defines the grid position, world-space
// position, neighbors, etc... of a hex tile. However, it
// does NOT interact directly with Unity in any way

public class Hex: IQPathTile {

    public Hex(HexMap hexMap, int q, int r)
    {
        this.HexMap = hexMap;
        this.Q = q;
        this.R = r;
        this.S = -(q + r);

        this.units = new HashSet<Unit>();
    }

    // Q + R + S = 0
    // S = -(Q + R)

    public readonly int Q;  // Column
    public readonly int R;  // Row
    public readonly int S;

    // Data for map generation and in-game effects
    public float Elevation;
    public float Moisture;

    public enum TERRAIN_TYPE { PLAINS, GRASSLANDS, FLOODPLAINS, DESERT, LAKE, OCEAN };
    public enum ELEVATION_TYPE { FLAT, HILL, MOUNTAIN, WATER };
    public enum FEATURE_TYPE { NONE, FOREST, RAINFOREST, MARSH };

    public TERRAIN_TYPE TerrainType { get; set; }
    public ELEVATION_TYPE ElevationType { get; set; }
    public FEATURE_TYPE FeatureType { get; set; }

    public readonly HexMap HexMap;

    static readonly float WIDTH_MULTIPLIER = Mathf.Sqrt(3) / 2;

    float radius = 1f;

    HashSet<Unit> units;

    public override string ToString()
    {
        return Q + ", " + R;
    }

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

    public static float CostEstimate(IQPathTile a, IQPathTile b)
    {
        return Distance((Hex)a, (Hex)b);
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

    // Returns the most common movement cost for this tile, not considering special unit rules
    public int BaseMovementCost(bool isHillWalker, bool isForestWalker, bool isFlyer)
    {
        if ((ElevationType == ELEVATION_TYPE.MOUNTAIN || ElevationType == ELEVATION_TYPE.WATER) && isFlyer == false)
            return -99;

        int moveCost = 1;

        if ( ElevationType == ELEVATION_TYPE.HILL && isHillWalker == false )
            moveCost += 1;

        if ((FeatureType == FEATURE_TYPE.FOREST || FeatureType == FEATURE_TYPE.RAINFOREST) && isForestWalker == false)
            moveCost += 1;

        return moveCost;
    }

    Hex[] neighbours;

    #region IQPathTile implementation
    public IQPathTile[] GetNeighbours() {

        if (this.neighbours != null)
            return this.neighbours;

        List<Hex> neighboursIncludingNulls = new List<Hex>();

        neighboursIncludingNulls.Add(HexMap.GetHexAt(Q + 1, R + 0));
        neighboursIncludingNulls.Add(HexMap.GetHexAt(Q - 1, R + 0));
        neighboursIncludingNulls.Add(HexMap.GetHexAt(Q + 0, R + 1));
        neighboursIncludingNulls.Add(HexMap.GetHexAt(Q + 0, R - 1));
        neighboursIncludingNulls.Add(HexMap.GetHexAt(Q + 1, R - 1));
        neighboursIncludingNulls.Add(HexMap.GetHexAt(Q - 1, R + 1));

        List<Hex> neighbours = new List<Hex>();

        foreach(Hex h in neighboursIncludingNulls)
        {
            if(h != null)
                neighbours.Add(h);
        }

        // Store array so we don't have to calculate it every time
        this.neighbours = neighbours.ToArray();

        return this.neighbours;
    }

    public float AggregateCostToEnter(float costSoFar, IQPathTile sourceTile, IQPathUnit unit) {
        return ((Unit)(unit)).AggregateTurnsToEnterHex(this, costSoFar);
    }
    #endregion
}
