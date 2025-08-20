using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Maze
{
    public const int PathTile = 0;
    public const int WallTile = 1;

    private static readonly Vector3Int[] Directions =
    {
        new(0, 0, 2),
        new(0, 0, -2),
        new(2, 0, 0),
        new(-2, 0, 0),
        new(0, 2, 0),
        new(0, -2, 0)
    };

    private PathCell[,,] _cellInfos;
    private PathCell _farthestPathCell;

    public Maze(int width, int height, int depth, float spikesDensity, int seed)
    {
        Random.InitState(seed);
        InitializeMaze(width, height, depth);
        GenerateMazePath();

        FindFarthestCell();
        PlaceInteractiveElements();
        PlaceSpikes(spikesDensity);
    }

    public List<SpikeTrap> Spikes { get; } = new();

    public MazeElements Elements { get; private set; }

    public int[,,] Tiles { get; private set; }

    private void GenerateMazePath()
    {
        var startCell = new Vector3Int(1, 1, 1);
        Tiles[startCell.x, startCell.y, startCell.z] = PathTile;
        _cellInfos[startCell.x, startCell.y, startCell.z] = new PathCell(startCell, 0, false, null);

        var previousCells = new Stack<Vector3Int>();
        previousCells.Push(startCell);

        while (previousCells.Count > 0) GenerateNextPath(previousCells);
    }

    private void InitializeMaze(int width, int height, int depth)
    {
        Tiles = new int[width, height, depth];
        _cellInfos = new PathCell[width, height, depth];

        for (var x = 0; x < width; x++)
        for (var y = 0; y < height; y++)
        for (var z = 0; z < depth; z++)
        {
            Tiles[x, y, z] = WallTile;
            _cellInfos[x, y, z] = new PathCell(new Vector3Int(x, y, z), -1, false, null);
        }
    }

    private void GenerateNextPath(Stack<Vector3Int> previousCells)
    {
        var currentCell = previousCells.Peek();
        var availableDirections = GetShuffledDirections();

        var hasMoved = false;
        foreach (var direction in availableDirections)
        {
            var newCell = currentCell + direction;
            if (CanAddPath(newCell))
            {
                AddPathToNewCell(currentCell, newCell);
                var distance = _cellInfos[currentCell.x, currentCell.y, currentCell.z].Distance + 1;
                _cellInfos[newCell.x, newCell.y, newCell.z] = new PathCell(
                    new Vector3Int(newCell.x, newCell.y, newCell.z),
                    distance,
                    false,
                    _cellInfos[currentCell.x, currentCell.y, currentCell.z]);
                previousCells.Push(newCell);
                hasMoved = true;
                break;
            }
        }

        if (!hasMoved)
        {
            _cellInfos[currentCell.x, currentCell.y, currentCell.z].IsDeadEnd =
                IsDeadEnd(_cellInfos[currentCell.x, currentCell.y, currentCell.z].Position);
            previousCells.Pop();
        }
    }

    private static IEnumerable<Vector3Int> GetShuffledDirections()
    {
        var directions = (Vector3Int[])Directions.Clone();
        for (var i = 0; i < directions.Length; i++)
        {
            var randomIndex = Random.Range(i, directions.Length);
            (directions[i], directions[randomIndex]) = (directions[randomIndex], directions[i]);
        }

        return directions;
    }

    private bool CanAddPath(Vector3Int cell)
    {
        return cell.x >= 0 && cell.x < Tiles.GetLength(0) &&
               cell.y >= 0 && cell.y < Tiles.GetLength(1) &&
               cell.z >= 0 && cell.z < Tiles.GetLength(2) &&
               Tiles[cell.x, cell.y, cell.z] == WallTile;
    }

    private void AddPathToNewCell(Vector3Int currentCell, Vector3Int newCell)
    {
        Tiles[newCell.x, newCell.y, newCell.z] = PathTile;
        var midCell = currentCell + (newCell - currentCell) / 2;
        Tiles[midCell.x, midCell.y, midCell.z] = PathTile;
    }

    private bool IsDeadEnd(Vector3Int cell)
    {
        var count = 0;

        foreach (var direction in Directions)
        {
            var neighbor = cell + direction / 2;
            if (neighbor.x >= 0 && neighbor.x < Tiles.GetLength(0) &&
                neighbor.y >= 0 && neighbor.y < Tiles.GetLength(1) &&
                neighbor.z >= 0 && neighbor.z < Tiles.GetLength(2) &&
                Tiles[neighbor.x, neighbor.y, neighbor.z] == PathTile)
                count++;
        }

        return count == 1;
    }

    private void FindFarthestCell()
    {
        var maxDistance = -1;

        for (var x = 0; x < _cellInfos.GetLength(0); x++)
        for (var y = 0; y < _cellInfos.GetLength(1); y++)
        for (var z = 0; z < _cellInfos.GetLength(2); z++)
        {
            var cellInfo = _cellInfos[x, y, z];
            if (cellInfo.Distance > maxDistance)
            {
                maxDistance = cellInfo.Distance;
                _farthestPathCell = cellInfo;
            }
        }
    }

    private void PlaceInteractiveElements()
    {
        var possibleKeyLocations = new List<Vector3Int>();

        for (var x = 0; x < _cellInfos.GetLength(0); x++)
        for (var y = 0; y < _cellInfos.GetLength(1); y++)
        for (var z = 0; z < _cellInfos.GetLength(2); z++)
        {
            var cell = new Vector3Int(x, y, z);
            var cellInfo = _cellInfos[x, y, z];

            if (cellInfo.IsDeadEnd && 1 < cellInfo.Distance && cellInfo.Distance < _farthestPathCell.Distance - 1)
                possibleKeyLocations.Add(cell);
        }

        Vector3Int keyCell;
        if (possibleKeyLocations.Count > 0)
            keyCell = possibleKeyLocations[Random.Range(0, possibleKeyLocations.Count)];
        else
            keyCell = ChooseAnyPathCellCloserThan(_farthestPathCell);

        var doorCell = FindDoorLocationOnPathToEnd(keyCell);

        // Ensure key and door don't overlap
        while (doorCell == keyCell)
        {
            keyCell = ChooseAnyPathCellCloserThan(_farthestPathCell);
            doorCell = FindDoorLocationOnPathToEnd(keyCell);
        }

        Elements = new MazeElements(keyCell, doorCell, _farthestPathCell.Position);
    }

    private Vector3Int ChooseAnyPathCellCloserThan(PathCell targetCell)
    {
        var possibleCells = new List<Vector3Int>();
        var maxDistance = targetCell.Distance;

        for (var x = 0; x < _cellInfos.GetLength(0); x++)
        for (var y = 0; y < _cellInfos.GetLength(1); y++)
        for (var z = 0; z < _cellInfos.GetLength(2); z++)
        {
            var cell = new Vector3Int(x, y, z);
            var distance = _cellInfos[cell.x, cell.y, cell.z].Distance;
            if (1 < distance && distance < maxDistance - 1 && Tiles[cell.x, cell.y, cell.z] == PathTile)
                possibleCells.Add(cell);
            ;
        }

        return possibleCells[Random.Range(0, possibleCells.Count)];
    }

    private Vector3Int FindDoorLocationOnPathToEnd(Vector3Int keyCell)
    {
        var keyCellDistance = _cellInfos[keyCell.x, keyCell.y, keyCell.z].Distance;
        var doorDistanceFromEnd = Random.Range(0, _farthestPathCell.Distance - keyCellDistance);
        var doorCell = _farthestPathCell.PreviousCell;

        for (var i = 0; i < doorDistanceFromEnd; i++)
        {
            var cell = doorCell.PreviousCell;
            doorCell = cell;
        }

        return doorCell.Position;
    }

    private void PlaceSpikes(float spikeChance = 0.05f)
    {
        Spikes.Clear();

        for (var x = 1; x < Tiles.GetLength(0); x++)
        for (var y = 1; y < Tiles.GetLength(1); y++)
        for (var z = 2; z < Tiles.GetLength(2); z++)
            if (Tiles[x, y, z] == PathTile)
            {
                var pathCell = new Vector3Int(x, y, z);
                Vector3Int[] faceOffsets =
                {
                    Vector3Int.right, Vector3Int.left, Vector3Int.up, Vector3Int.down, Vector3Int.forward,
                    Vector3Int.back
                };

                foreach (var offset in faceOffsets)
                {
                    var adjacentWallCell = pathCell + offset;

                    if (Tiles[adjacentWallCell.x, adjacentWallCell.y, adjacentWallCell.z] == WallTile &&
                        Random.value < spikeChance)
                        Spikes.Add(new SpikeTrap(adjacentWallCell, -offset));
                }
            }
    }
}

public record PathCell(Vector3Int Position, int Distance, bool IsDeadEnd, PathCell PreviousCell)
{
    public Vector3Int Position { get; } = Position;
    public int Distance { get; } = Distance;
    public bool IsDeadEnd { get; set; } = IsDeadEnd;
    public PathCell PreviousCell { get; } = PreviousCell;
}

public class SpikeTrap
{
    public SpikeTrap(Vector3Int position, Vector3 direction)
    {
        Position = position;
        Direction = direction;
    }

    public Vector3Int Position { get; }
    public Vector3 Direction { get; }
}

public record MazeElements(Vector3Int KeyCell, Vector3Int DoorCell, Vector3Int EndCell)
{
    public Vector3Int KeyCell { get; } = KeyCell;
    public Vector3Int DoorCell { get; } = DoorCell;
    public Vector3Int EndCell { get; } = EndCell;
}