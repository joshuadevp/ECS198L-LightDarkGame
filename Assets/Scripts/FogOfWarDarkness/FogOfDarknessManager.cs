using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogOfDarknessManager : MonoBehaviour
{
    [SerializeField]
    Vector2Int mapHeightAndWidth; // Height and width of points of darkness.
    [SerializeField]
    float distanceBetweenPoints; // Distance between points of darkness, assumes all space between two points contains darkness.
    [SerializeField]
    Vector2 activeHeightAndWidth; // Height and width of set of active points of darkness in world space.
    [SerializeField]
    Transform activeCenter; // Center point where we want darkness to be active around
    [SerializeField]
    GameObject darknessPrefab;
    [SerializeField]
    float pointHeight;
    private DarknessPoint[,] darknessArray;
    private HashSet<DarknessPoint> activePoints; // set of active points

    // Start is called before the first frame update
    void Start()
    {
        InitAllDarkness();
    }

    // Update is called once per frame
    void Update()
    {
        DeactivatePoints();
        ActivatePoints();
    }

    // Returns true if the given world point is within darkness
    public bool IsDarkness(Vector3 loc)
    {
        Vector2Int index = worldToIndex(new Vector2(loc.x, loc.z));
        if (index.x == -1)
        {
            return false;
        }
        return IsDarknessIndex(index);
    }

    // Returns if index has darkness
    private bool IsDarknessIndex(Vector2Int index)
    {
        return darknessArray[index.x, index.y].IsActive();
    }

    // Creates a point of darkness at the given world coord if possible
    // Returns true on success
    public bool CreateDarknessPoint(Vector3 coordinates, DarknessSpec spec)
    {
        Vector2Int index = worldToIndex(new Vector2(coordinates.x, coordinates.z));
        if (index.x == -1) return false; // fail on invalid coord
        CreateDarknessPointIndex(index, spec);
        return true;
    }

    // (Re)Creates all darkness points and game objects
    // EXPENSIVE!!! Only use when initializing game
    public void InitAllDarkness()
    {
        // Reset data
        activePoints = new HashSet<DarknessPoint>();

        // Spawn darkness points
        darknessArray = new DarknessPoint[mapHeightAndWidth.x, mapHeightAndWidth.y];

        for (int x = 0; x < mapHeightAndWidth.x; x++)
        {
            for (int y = 0; y < mapHeightAndWidth.y; y++)
            {
                CreateDarknessPointIndex(new Vector2Int(x, y), new DarknessSpec());
            }
        }
    }

    // Creates a point of darkness given index in array and associated data
    // Returns created DarknessPoint
    private DarknessPoint CreateDarknessPointIndex(Vector2Int index, DarknessSpec spec)
    {
        GameObject obj = Instantiate(darknessPrefab, Get3DVector(indexToWorld(index)), Quaternion.identity);
        DarknessPoint point = new DarknessPoint();
        point.darknessObject = obj;
        point.Init(spec);
        point.SetActive(false);
        darknessArray[index.x, index.y] = point;
        return point;
    }

    // Converts world coordinates to index in internal array
    // Invalid conversion results in (-1,-1) return
    private Vector2Int worldToIndex(Vector2 loc)
    {
        if (loc.x < 0 || loc.x > mapHeightAndWidth.x * distanceBetweenPoints || loc.y < 0 || loc.x > mapHeightAndWidth.y * distanceBetweenPoints)
        {
            return new Vector2Int(-1, -1);
        }
        return new Vector2Int(Mathf.RoundToInt(loc.x / distanceBetweenPoints), Mathf.RoundToInt(loc.y / distanceBetweenPoints));
    }

    // Converts index to coordinates in world
    // Assumes any point within the bounds o
    private Vector2 indexToWorld(Vector2Int index)
    {
        return new Vector2(index.x * distanceBetweenPoints, index.y * distanceBetweenPoints);
    }

    // Darkness exists on a 2D plane but in a 3D game we need 3D coords
    // 
    private Vector3 Get3DVector(Vector2 loc)
    {
        return new Vector3(loc.x, pointHeight, loc.y);
    }

    // Returns true if index position is surrounded by darkness and thus cannot spread
    private bool IsSurrounded(Vector2Int index)
    {
        return darknessArray[clampIndexX(index.x + 1), index.y].IsActive() &&
        darknessArray[clampIndexX(index.x - 1), index.y].IsActive() &&
        darknessArray[index.x, clampIndexY(index.y + 1)].IsActive() &&
        darknessArray[index.x, clampIndexY(index.y - 1)].IsActive();
    }

    // Clamp index within array ranges
    private int clampIndexX(int x)
    {
        return Mathf.Clamp(x, 0, mapHeightAndWidth.x - 1);
    }
    private int clampIndexY(int y)
    {
        return Mathf.Clamp(y, 0, mapHeightAndWidth.y - 1);
    }

    private void DeactivatePoints()
    {
        // Copy points so we can remove items from active set
        DarknessPoint[] points = new DarknessPoint[activePoints.Count];
        activePoints.CopyTo(points);
        foreach (DarknessPoint p in points)
        {
            if (IsSurrounded(worldToIndex(p.position)))
            {
                p.SetActive(false);
                activePoints.Remove(p);
            }
        }
    }

    private void ActivatePoints()
    {
        Vector2Int center = worldToIndex(new Vector2(activeCenter.position.x, activeCenter.position.z));
        // Exit early if location outside grid
        if (center.x == -1)
        {
            return;
        }
        int centerX = center.x;
        int centerY = center.y;

        int indexX = Mathf.RoundToInt(activeHeightAndWidth.x / 2 * distanceBetweenPoints);
        int indexY = Mathf.RoundToInt(activeHeightAndWidth.y / 2 * distanceBetweenPoints);

        Vector2Int topRightIndex = new Vector2Int(clampIndexX(centerX + indexX), clampIndexY(centerY + indexY));
        Vector2Int bottomLeftIndex = new Vector2Int(clampIndexX(centerX - indexX), clampIndexY(centerY - indexY));

        for (int x = bottomLeftIndex.x; x <= topRightIndex.x; x++)
        {
            for (int y = bottomLeftIndex.y; y <= topRightIndex.y; y++)
            {
                darknessArray[x, y].SetActive(true);
                activePoints.Add(darknessArray[x, y]);
            }
        }
    }
}
