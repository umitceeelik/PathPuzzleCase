using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelHealthChecker : MonoBehaviour
{
    [SerializeField] public Level level;

    public Dictionary<int, List<int>> finishingTiles;

    public static LevelHealthChecker Instance;

    private Vector3 startPointPos;

    private List<List<string>> allPaths;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void Start()
    {
        finishingTiles = new Dictionary<int, List<int>>();
        startPointPos = level.startingTile.transform.GetChild(level.startingPoint + 1).position; // + 1 because the first child is square.
        allPaths = new List<List<string>>(); // To debug the paths.
        CheckLevel();
    }

    public void CheckLevel()
    {
        Debug.Log("CheckLevel: Starting level check...");
        SetInitialFinishingTiles();
        bool solvable = IsLevelSolvable(level.startingTile, startPointPos, new List<string> { level.startingTile.name });
        Debug.Log("The Level is Solvable: " + solvable);

        Debug.Log("All the paths tried:");
        foreach (var path in allPaths)
        {
            Debug.Log(string.Join(" -> ", path));
        }
    }

    // Determines the win points at the beginning.
    public void SetInitialFinishingTiles()
    {
        Debug.Log("SetInitialFinishingTiles: Setting initial finishing tiles...");
        for (int i = level.Tiles.Count; i > level.Tiles.Count - 3; i--)
        {
            finishingTiles.Add(i, new List<int> { 0, 1 });
            finishingTiles.TryGetValue(i, out List<int> test);
            Debug.Log($"Tile {i} initialized with points: {test[0]}, {test[1]}");
        }
    }

    // To update the win points after rotating the finishing tiles.
    public void ChangeFinishingTiles(Tile clickedTile)
    {
        int tileNumber = level.Tiles.IndexOf(clickedTile) + 1;

        if (finishingTiles.ContainsKey(tileNumber))
        {
            switch (clickedTile.transform.localEulerAngles.z)
            {
                case 270:
                    finishingTiles[tileNumber] = new List<int> { 6, 7 };
                    break;
                case 180:
                    finishingTiles[tileNumber] = new List<int> { 4, 5 };
                    break;
                case 90:
                    finishingTiles[tileNumber] = new List<int> { 2, 3 };
                    break;
                case 0:
                    finishingTiles[tileNumber] = new List<int> { 0, 1 };
                    break;
            }
            finishingTiles.TryGetValue(tileNumber, out List<int> updatedPoints);
            Debug.Log($"Tile {tileNumber} rotated to {clickedTile.transform.localEulerAngles.z} degrees, new points: {updatedPoints[0]}, {updatedPoints[1]}");
        }
    }

    private bool IsLevelSolvable(Tile currentTile, Vector3 currentPoint, List<string> currentPath)
    {
        Debug.Log("IsLevelSolvable: Checking if the level is solvable...");
        HashSet<string> visitedTiles = new HashSet<string>();
        return IsPathToWin(currentTile, currentPoint, visitedTiles, currentPath);
    }

    // Checks the all tiles with rotating them, to check that is there any path to reach the win points.
    private bool IsPathToWin(Tile tile, Vector3 entryPointPose, HashSet<string> visitedTiles, List<string> currentPath)
    {
        // Tile name combined with entry and exit points
        //string tileKey = $"{tile.name}-{GetEntryPoint(tile, entryPointPose)}";
        string tileKey = tile.name + "_" + tile.transform.localEulerAngles.z;

        if (visitedTiles.Contains(tileKey))
            return false; // If we have already visited this tile with this entry point and rotation, return false to avoid cycles

        if (visitedTiles.Contains(tileKey))
        {
            Debug.Log($"IsPathToWin: Tile {tileKey} already visited.");
            return false;
        }

        visitedTiles.Add(tileKey);
        Debug.Log($"IsPathToWin: Visiting Tile {tileKey}...");

        for (int rotation = 0; rotation < 4; rotation++) // Only have 4 rotation for all tiles because 360/90 = 4.
        {
            int entryPoint = GetEntryPoint(tile, entryPointPose); // To get entry point depends on the starting pose or the exit point's pose of the last tile.
            
            if (entryPoint == -1) // If the entry point is -1, the path has finished on lose area or it is a DeadEnds.
            {
                Debug.Log($"IsPathToWin: Entry point not found for Tile {tile.name}.");
                return false;
            }

            Debug.Log($"IsPathToWin: Entry point for Tile {tile.name} is {entryPoint}.");

            foreach (var path in tile.GetPaths())
            {
                if (path.PointX == entryPoint || path.PointY == entryPoint) // Checks the tile's paths and match the entry point with the tile's point.
                {
                    int nextPoint = (path.PointX == entryPoint) ? path.PointY : path.PointX;
                    Debug.Log($"IsPathToWin: Found path from {entryPoint} to {nextPoint} on Tile {tile.name}.");

                    if (IsWinningTile(tile, nextPoint)) // checks if the tile and the point is in win points.
                    {
                        Debug.Log($"IsPathToWin: Winning tile found at Tile {tile.name}, Point {nextPoint}.");
                        currentPath.Add($"{tile.name} ({entryPoint} -> {nextPoint})");
                        allPaths.Add(new List<string>(currentPath));
                        return true;
                    }

                    (Tile nextTile, int nextEntryPoint) = GetNextTile(tile, nextPoint);
                    if (nextTile != null && nextEntryPoint != -1)
                    {
                        Debug.Log($"IsPathToWin: Moving to next Tile {nextTile.name} from Point {nextPoint}.");
                        currentPath.Add($"{tile.name} ({entryPoint} -> {nextPoint})");
                        if (IsPathToWin(nextTile, nextTile.transform.GetChild(nextEntryPoint + 1).position, visitedTiles, currentPath)) // Call the same function for the next Tile with it's entry point.
                            return true;
                        currentPath.RemoveAt(currentPath.Count - 1);
                    }
                }
            }

            RotateTile(tile, 90); // Rotates the tile to check the other paths of the tile.
        }

        Debug.Log($"IsPathToWin: No winning path found from Tile {tile.name}.");
        allPaths.Add(new List<string>(currentPath));
        return false;
    }

    // To get the point number depends on the tile and the point position.
    private int GetEntryPoint(Tile tile, Vector3 entryPointPose)
    {
        int entryPoint = tile.GetPoints().FindIndex(t => t.position == entryPointPose);

        return entryPoint;
    }

    // To check reaching win points.
    private bool IsWinningTile(Tile tile, int point)
    {
        int tileIndex = level.Tiles.IndexOf(tile) + 1;
        bool isWinning = finishingTiles.ContainsKey(tileIndex) && finishingTiles[tileIndex].Contains(point);
        Debug.Log($"IsWinningTile: Tile {tile.name} (Index {tileIndex}) with Point {point} is {(isWinning ? "a winning tile" : "not a winning tile")}.");
        return isWinning;
    }

    // To get next tile, gets the last tile's exit point and finds the current tile with entry point.
    private (Tile, int) GetNextTile(Tile currentTile, int exitPoint)
    {
        Vector3 exitPosition = currentTile.GetPoints()[exitPoint].position;

        foreach (Tile tile in level.Tiles)
        {
            if (tile == currentTile) continue;

            for (int j = 0; j < tile.GetPoints().Count; j++)
            {
                if (exitPosition == tile.GetPoints()[j].position)
                {
                    Debug.Log($"GetNextTile: Current Tile {currentTile.name}, Exit Point {exitPoint} -> Next Tile {tile.name}, Entry Point {j}");
                    return (tile, j);
                }
            }
        }

        Debug.Log($"GetNextTile: No next tile found for Current Tile {currentTile.name}, Exit Point {exitPoint}.");
        return (null, -1);
    }
    
    // Rotates Tile
    private void RotateTile(Tile tile, int angle)
    {
        tile.transform.Rotate(0, 0, angle);
        ChangeFinishingTiles(tile);
        Debug.Log($"RotateTile: Rotated Tile {tile.name} by {angle} degrees.");
    }
}
