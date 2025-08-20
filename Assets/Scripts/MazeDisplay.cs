using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MazeDisplay : MonoBehaviour
{
    private const int MESH_COUNT = 5; // North, South, East, West, Outer
    [SerializeField] private GameObject grabablePrefab;
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private GameObject keyPrefab;
    [SerializeField] private GameObject doorPrefab;
    [SerializeField] private GameObject endPrefab;
    [SerializeField] private GameObject spikeTrapPrefab;
    [SerializeField] private Material northWallMaterial;
    [SerializeField] private Material southWallMaterial;
    [SerializeField] private Material eastWallMaterial;
    [SerializeField] private Material westWallMaterial;
    [SerializeField] private Material outerWallMaterial;
    [SerializeField] private int givenWidth = 4;
    [SerializeField] private int givenHeight = 4;
    [SerializeField] private int givenDepth = 4;
    [SerializeField] private int seed = 123456;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private float spikesDensity = 0.01f;

    [SerializeField] private GameObject midGameUIPanel;

    [SerializeField] private GameObject NextPlayerPanel;
    [SerializeField] private GameObject NextPlayerName;
    private Mesh[] _directionalMeshes;

    private GameObject _grabable;
    private GameObject _mazeContainer;
    private int mazeDepth = 4;
    private int mazeHeight = 4;

    private int mazeWidth = 4;
    private float time;

    private Maze Maze { get; set; }

    private void Start()
    {
        time = 0.0f;
    }

    private void Update()
    {
        time += Time.deltaTime;
    }

    private void OnEnable()
    {
        GameEvents.OnEndReached += HandleEndReached;
    }

    private void OnDisable()
    {
        GameEvents.OnEndReached -= HandleEndReached;
    }

    public void UpdateParameters(int height, int width, int depth, int s)
    {
        givenHeight = height;
        givenWidth = width;
        givenDepth = depth;
        seed = s;
        DisplayNewlyCreatedMaze();
    }

    public float GetTime()
    {
        return time;
    }

    public void AddSpikesTime()
    {
        time += 3.0f;
    }

    private void HandleEndReached()
    {
        if (midGameUIPanel.GetComponent<MidGameUI>().NextPlayerSetUp())
        {
            NextPlayerName.GetComponent<TMP_Text>().text = midGameUIPanel.GetComponent<MidGameUI>().GetNextPlayerName();
            NextPlayerPanel.SetActive(true);
        }
        else
        {
            DestroyPuzzle();
            midGameUIPanel.GetComponent<MidGameUI>().SaveAndEnd();
        }
    }

    public void NextPlayerPuzzle()
    {
        NextPlayerPanel.SetActive(false);
        DisplayNewlyCreatedMaze();
    }

    public void DestroyPuzzle()
    {
        if (_grabable) Destroy(_grabable);
    }

    private void DisplayNewlyCreatedMaze()
    {
        time = 0.0f;
        mazeDepth = 2 * givenDepth + 1;
        mazeHeight = 2 * givenHeight + 1;
        mazeWidth = 2 * givenWidth + 1;

        if (_grabable) Destroy(_grabable);

        _grabable = Instantiate(grabablePrefab, Vector3.zero, Quaternion.identity);
        _mazeContainer = new GameObject("MazeContainer")
        {
            transform =
            {
                parent = _grabable.transform
            }
        };

        Maze = new Maze(mazeWidth, mazeHeight, mazeDepth, spikesDensity, seed);
        DisplayMaze();
        AdjustGrabableCollider();
        SpawnInteractiveElements();
        _grabable.transform.position = new Vector3(0, 1f, 0.5f);
    }

    private void DisplayMaze()
    {
        _directionalMeshes = new Mesh[MESH_COUNT];
        var meshesData = new List<Vector3>[MESH_COUNT];
        var meshesTriangles = new List<int>[MESH_COUNT];
        var meshesUVs = new List<Vector2>[MESH_COUNT];

        // Initialize lists
        for (var i = 0; i < MESH_COUNT; i++)
        {
            meshesData[i] = new List<Vector3>();
            meshesTriangles[i] = new List<int>();
            meshesUVs[i] = new List<Vector2>();
        }

        for (var x = 0; x < mazeWidth; x++)
        for (var y = 0; y < mazeHeight; y++)
        for (var z = 0; z < mazeDepth; z++)
            if (Maze.Tiles[x, y, z] == Maze.WallTile)
            {
                var position = new Vector3(x * cellSize, y * cellSize, z * cellSize);
                AddCubeFaces(position, x, y, z, meshesData, meshesTriangles, meshesUVs);
            }

        // Create separate GameObjects for each direction
        CreateDirectionalMesh("NorthWalls", meshesData[0], meshesTriangles[0], meshesUVs[0], northWallMaterial);
        CreateDirectionalMesh("SouthWalls", meshesData[1], meshesTriangles[1], meshesUVs[1], southWallMaterial);
        CreateDirectionalMesh("EastWalls", meshesData[2], meshesTriangles[2], meshesUVs[2], eastWallMaterial);
        CreateDirectionalMesh("WestWalls", meshesData[3], meshesTriangles[3], meshesUVs[3], westWallMaterial);
        CreateDirectionalMesh("OuterWalls", meshesData[4], meshesTriangles[4], meshesUVs[4], outerWallMaterial);
    }

    private void CreateDirectionalMesh(string name, List<Vector3> vertices, List<int> triangles, List<Vector2> uvs,
        Material material)
    {
        if (vertices.Count == 0) return;

        var meshObject = new GameObject(name);
        meshObject.transform.parent = _mazeContainer.transform;
        meshObject.tag = "Wall";

        var mesh = new Mesh
        {
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray(),
            uv = uvs.ToArray()
        };
        mesh.RecalculateNormals();

        var meshFilter = meshObject.AddComponent<MeshFilter>();
        var meshRenderer = meshObject.AddComponent<MeshRenderer>();
        var meshCollider = meshObject.AddComponent<MeshCollider>();

        meshFilter.mesh = mesh;
        meshRenderer.material = material;
        meshCollider.sharedMesh = mesh;
    }

    private void AddCubeFaces(Vector3 position, int x, int y, int z, List<Vector3>[] vertices, List<int>[] triangles,
        List<Vector2>[] uvs)
    {
        bool IsNeighbor(int nx, int ny, int nz)
        {
            return nx >= 0 && nx < mazeWidth && ny >= 0 && ny < mazeHeight && nz >= 0 && nz < mazeDepth &&
                   Maze.Tiles[nx, ny, nz] == Maze.WallTile;
        }

        bool IsOuter(int nx, int ny, int nz)
        {
            return nx < 0 || nx >= mazeWidth || ny < 0 || ny >= mazeHeight || nz < 0 || nz >= mazeDepth;
        }

        Vector3[] faceOffsets =
        {
            new(0, 0, 1), // North
            new(0, 0, -1), // South
            new(1, 0, 0), // East
            new(-1, 0, 0), // West
            new(0, 1, 0), // Top
            new(0, -1, 0) // Bottom
        };

        Vector3[][] faceVertices =
        {
            // North
            new Vector3[] { new(0, 0, 1), new(1, 0, 1), new(1, 1, 1), new(0, 1, 1) },
            // South
            new Vector3[] { new(1, 0, 0), new(0, 0, 0), new(0, 1, 0), new(1, 1, 0) },
            // East
            new Vector3[] { new(1, 1, 1), new(1, 0, 1), new(1, 0, 0), new(1, 1, 0) },
            // West
            new Vector3[] { new(0, 1, 0), new(0, 0, 0), new(0, 0, 1), new(0, 1, 1) },
            // Top
            new Vector3[] { new(0, 1, 1), new(1, 1, 1), new(1, 1, 0), new(0, 1, 0) },
            // Bottom
            new Vector3[] { new(0, 0, 0), new(1, 0, 0), new(1, 0, 1), new(0, 0, 1) }
        };

        Vector2[] faceUVs =
        {
            new(1, 1), new(1, 0), new(0, 0), new(0, 1)
        };

        // Process each face
        for (var i = 0; i < faceOffsets.Length; i++)
        {
            var nx = x + (int)faceOffsets[i].x;
            var ny = y + (int)faceOffsets[i].y;
            var nz = z + (int)faceOffsets[i].z;

            if (!IsNeighbor(nx, ny, nz))
            {
                // Determine if this is an outer wall or a directional inner wall
                int meshIndex;
                if (IsOuter(nx, ny, nz))
                    meshIndex = 4; // Outer walls
                else
                    // Only use directional materials for horizontal faces (not top/bottom)
                    meshIndex = i < 4 ? i : 4;

                var vertexIndex = vertices[meshIndex].Count;

                foreach (var v in faceVertices[i])
                    vertices[meshIndex].Add(position + v * cellSize);

                triangles[meshIndex].Add(vertexIndex + 0);
                triangles[meshIndex].Add(vertexIndex + 1);
                triangles[meshIndex].Add(vertexIndex + 2);
                triangles[meshIndex].Add(vertexIndex + 0);
                triangles[meshIndex].Add(vertexIndex + 2);
                triangles[meshIndex].Add(vertexIndex + 3);

                uvs[meshIndex].AddRange(faceUVs);
            }
        }
    }

    private void AdjustGrabableCollider()
    {
        var boxCollider = _grabable.GetComponent<BoxCollider>();

        var totalWidth = mazeWidth * cellSize;
        var totalHeight = mazeHeight * cellSize;
        var totalDepth = mazeDepth * cellSize;
        var centerOffset = new Vector3(
            mazeWidth * cellSize / 2f,
            mazeHeight * cellSize / 2f,
            mazeDepth * cellSize / 2f
        );

        boxCollider.size = new Vector3(totalWidth, totalHeight, totalDepth);
        boxCollider.center = centerOffset;
    }

    private void SpawnInteractiveElements()
    {
        InstantiateAndScale(ballPrefab, new Vector3(1, 1, 1));
        InstantiateAndScale(keyPrefab, Maze.Elements.KeyCell);
        InstantiateAndScale(doorPrefab, Maze.Elements.DoorCell, 1f);
        InstantiateAndScale(endPrefab, Maze.Elements.EndCell);
        DisplaySpikes();
    }

    private void InstantiateAndScale(GameObject prefab, Vector3 position, float scaleFactor = 0.8f)
    {
        var radius = cellSize * scaleFactor / 2f;
        position *= cellSize;
        position += new Vector3(radius, radius, radius);

        var instance = Instantiate(prefab, position, Quaternion.identity, _mazeContainer.transform);
        instance.transform.localScale = new Vector3(cellSize, cellSize, cellSize) * scaleFactor;
    }

    private void DisplaySpikes(float scaleFactor = 0.9f)
    {
        foreach (var spikeInfo in Maze.Spikes)
        {
            var worldPosition = new Vector3(
                spikeInfo.Position.x * cellSize + cellSize / 2,
                spikeInfo.Position.y * cellSize + cellSize / 2,
                spikeInfo.Position.z * cellSize + cellSize / 2
            );

            var offsetPosition = worldPosition + spikeInfo.Direction * (cellSize / 1.8f);

            var spikeTrap = Instantiate(spikeTrapPrefab, offsetPosition, Quaternion.identity, _mazeContainer.transform);

            spikeTrap.transform.up = spikeInfo.Direction;
            spikeTrap.transform.localScale = new Vector3(cellSize, cellSize, cellSize) * scaleFactor;
        }
    }
}