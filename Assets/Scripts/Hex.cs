using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The Hex class defines the grid position, world-space
// position, neighbors, etc... of a hex tile. However, it
// does NOT interact directly with Unity in any way

public class Hex{

    public Hex(int q, int r)
    {
        this.Q = q;
        this.R = r;
        this.S = -(q + r);
    }

    // Q + R + S = 0
    // S = -(Q + R)

    public readonly int Q;  // Column
    public readonly int R;  // Row
    public readonly int S;

    static readonly float WIDTH_MULTIPLIER = Mathf.Sqrt(3) / 2;

    //Returns the world-space position of this hex
    public Vector3 Position()
    {
        float radius = 1f;
        float height = radius * 2;
        float width = WIDTH_MULTIPLIER * height;

        float vert = height * 0.75f; // vertical spacing
        float horiz = width;         // horizontal spacing

        return new Vector3(
            horiz * (this.Q + this.R/2f),
            0,
            vert * this.R
        );
    }
}
