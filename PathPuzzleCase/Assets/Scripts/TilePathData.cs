using System;
using UnityEngine;


/// <summary>
/// Represents a tile path data in a tile
/// </summary>
[Serializable]
public class TilePathData
{
    [SerializeField] private int pointX;
    [SerializeField] private int pointY;
    
    public int PointX => pointX;
    public int PointY => pointY;
}
