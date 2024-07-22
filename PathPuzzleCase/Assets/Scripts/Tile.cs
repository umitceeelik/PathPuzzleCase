using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Each tile in level.
/// </summary>
public class Tile : MonoBehaviour
{
    [SerializeField] private List<TilePathData> _tilePathDataList;
    [SerializeField] private List<Transform> _pointPositions;

    public List<TilePathData> GetPaths() => _tilePathDataList;
    public List<Transform> GetPoints() => _pointPositions;

    private void Awake()
    {
        for (int i = 1; i < transform.childCount; i++)
        {
            _pointPositions.Add(transform.GetChild(i));
        }
    }
}