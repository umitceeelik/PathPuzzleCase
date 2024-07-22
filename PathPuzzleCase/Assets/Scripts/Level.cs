using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    /// <summary>
    /// starting tile of the level and its starting point
    /// </summary>
    [SerializeField] public Tile startingTile;
    [SerializeField] public int startingPoint;
    [SerializeField] private List<Tile> tiles;

    public List<Tile> Tiles => tiles;
}