using QPath;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : IQPathUnit {

    public string Name = "Dwarf";
    public int HitPoints = 100;
    public int Strength = 8;
    public int Movement = 2;
    public int MovementRemaining = 2;

    public Hex Hex { get; protected set; }

    public delegate void UnitMovedDelegate(Hex oldHex, Hex newHex);
    public event UnitMovedDelegate OnUnitMoved;

    // List of hexes to walk through (from pathfinder)
    // NOTE: First item is the hex we are standing on
    List<Hex> hexPath;

    // TODO: Move to config file
    const bool MOVEMENT_RULES_LIKE_CIV6 = false;

    public void SetHex(Hex newHex)
    {
        Hex oldHex = Hex;
        if (Hex != null)
        {
            Hex.RemoveUnit(this);
        }

        Hex = newHex;
        Hex.AddUnit(this);

        if (OnUnitMoved != null)
        {
            OnUnitMoved(oldHex, newHex);
        }
    }

    public void DUMMY_PATHFINDING_FUNC()
    {
        Hex[] p = QPath.QPath.FindPath<Hex>(
            Hex.HexMap,
            this,
            Hex,
            Hex.HexMap.GetHexAt(Hex.Q + 8, Hex.R),
            Hex.CostEstimate
        );

        Debug.Log("Got path of length: " + p.Length);

        SetHexPath(p);
    }

    public void ClearHexPath()
    {
        hexPath = new List<Hex>();
    }

    public void SetHexPath(Hex[] hexArray)
    {
        hexPath = new List<Hex>(hexArray);
    }

    public Hex[] GetHexPath()
    {
        return (this.hexPath == null) ? null : this.hexPath.ToArray();
    }

    // Returns true if we have movement left but nothing queued
    public bool UnitWaitingForOrder()
    {
        if (MovementRemaining > 0 && 
            (hexPath == null || hexPath.Count == 0)
            // TODO: Maybe we've been told to Fortify/Skip Turn/Alert
        )
        {
            return true;
        }

        return false;
    }

    // Processes one tile worth of movement for unit
    // Returns true if this should be called immediately again
    public bool DoMove()
    {
        // Do queued move
        if(hexPath == null || hexPath.Count == 0)
        {
            return false;
        }

        // Remove first hex from queue
        Hex nextHex = hexPath[1];

        int costToEnter = MovementCostToEnterHex( nextHex );

        if(costToEnter > MovementRemaining && MovementRemaining < Movement && MOVEMENT_RULES_LIKE_CIV6)
        {
            // We can't enter the hex this turn
            return false;
        }

        hexPath.RemoveAt(0);

        if (hexPath.Count == 1)
        {
            // The only hex left in the list is the one we are moving to, so clear the queue
            hexPath = null;
        }

        // Move to new hex
        SetHex(nextHex);

        return hexPath != null && MovementRemaining > 0;
    }

    public int MovementCostToEnterHex( Hex hex )
    {
        // TODO: Implement different movement traits

        return hex.BaseMovementCost( false, false, false );
    }

    public float AggregateTurnsToEnterHex(Hex hex, float turnsToDate)
    {
        // If you are trying to enter a tile with a movement cost greater than
        // your current remaining movement points, this will either result in a 
        // cheaper-than-expected turn cost (Civ5) or a more-expensive-than-expected
        // turn cost (Civ6)

        float baseTurnsToEnterHex = MovementCostToEnterHex(hex) / Movement; // Ex: Entering a forest is 1 turn

        if(baseTurnsToEnterHex < 0)
        {
            // Impassable terrain
            return -999;
        }

        if (baseTurnsToEnterHex > 1)
        {
            // Even if a hex costs 3 to enter and we have a max move of 2, you can always enter
            // it using a full turn of movement
            baseTurnsToEnterHex = 1;
        }

        float turnsRemaining = MovementRemaining / Movement; // Ex: If we are at 1/2 moves, then we have .5 turns left

        float turnsToDateWhole = Mathf.Floor(turnsToDate); // Ex: 4.33 becomes 4
        float turnsToDateFraction = turnsToDate - turnsToDateWhole; // Ex: 4.33 becomes 0.33

        if( (turnsToDateFraction > 0 && turnsToDateFraction < 0.01f) || turnsToDateFraction > 0.99f)
        {
            Debug.LogError("Floating point drift");

            if (turnsToDateFraction < 0.01f)
            {
                turnsToDateFraction = 0;
            }
            if (turnsToDateFraction > 0.99f)
            {
                turnsToDateWhole += 1;
                turnsToDateFraction = 0;
            }
        }

        float turnsUsedAfterThisMove = turnsToDate + baseTurnsToEnterHex;

        if(turnsUsedAfterThisMove > 1)
        {
            // We don't have enough movement to complete this move
            if (MOVEMENT_RULES_LIKE_CIV6)
            {
                if(turnsToDateFraction == 0)
                {
                    // We have full movement, but not enough to enter tile
                    // Ex: We have a max move of 2, but tile costs 3 to enter
                }
                else
                {
                    // We aren't allowed to enter the tile this move
                    // Sit idle for remainder of turn
                    turnsToDateWhole += 1;
                    turnsToDateFraction = 0;
                }

                // Now we know we are starting on a fresh turn
                turnsUsedAfterThisMove = baseTurnsToEnterHex;
                if (turnsUsedAfterThisMove > 1)
                    turnsUsedAfterThisMove = 1;
            }
            else
            {
                // Civ5 style movement rules allow us to enter a tile even if
                // we don't have enough movement left
                turnsUsedAfterThisMove = 1;
            }
        }

        // turnsUsedAfterThisMove is now some value from 0...1 (this includes the fractional
        // part of moves from previous turns)

        // Return total turn cost of turnsToDate + turns for this move
        return turnsToDateWhole + turnsUsedAfterThisMove;

    }

    #region IQPathUnit implementation
    //The turn cost to enter a hex (i.e. 0.5 turns if a movement cost is 1 and we have 2 max movement)
    public float CostToEnterHex(IQPathTile sourceTile, IQPathTile destinationTile)
    {
        return 1;
    }
    #endregion
}
