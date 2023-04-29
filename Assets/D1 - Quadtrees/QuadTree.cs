using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISpatialData2D
{
}

public class QuadTree : MonoBehaviour
{
    public void PrepareTree(Rect Bounds)
    {
    }

    public void AddData(ISpatialData2D Datum)
    {
    }

    public void AddData(List<ISpatialData2D> Data)
    {
    }

    public void ShowStats()
    {
    }

    public HashSet<ISpatialData2D> FindDataInRange(Vector2 SearchLocation, float SearchRange)
    {
        return null;
    }
}
