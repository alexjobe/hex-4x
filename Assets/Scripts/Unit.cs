using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit {

    public string Name = "Dwarf";
    public int HitPoints = 100;
    public int Strength = 8;
    public int Movement = 2;
    public int MovementRemaining = 2;

	public Hex Hex { get; protected set; }

    public delegate void UnitMovedDelegate(Hex oldHex, Hex newHex);
    public event UnitMovedDelegate OnUnitMoved;

    Queue<Hex> hexPath;

    public void SetHex( Hex newHex )
    {
        Hex oldHex = Hex;
        if(Hex != null)
        {
            Hex.RemoveUnit(this);
        }

        Hex = newHex;
        Hex.AddUnit(this);

        if(OnUnitMoved != null)
        {
            OnUnitMoved(oldHex, newHex);
        }
    }

    public void SetHexPath( Hex[] hexPath)
    {
        this.hexPath = new Queue<Hex>(hexPath);
    }

    public void DoTurn()
    {
        // Do queued move
        if(hexPath == null || hexPath.Count == 0)
        {
            return;
        }
        Hex oldHex = Hex;
        Hex newHex = hexPath.Dequeue();

        SetHex(newHex);
    }
}
