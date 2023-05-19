using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shared
{
    public static readonly string TileDetectorName = "TileDetector";

    public enum TileType
    {
        Default,
        Selected,
        Available,
        MoveTo
    }

    public enum TeamType
    {
        White,
        Black
    }

}
