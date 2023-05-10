using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISpatialData3D
{

}

public class Octree : MonoBehaviour
{
    public void PrepareTree(Bounds InBounds)
    {
    }

    public void AddData(ISpatialData3D Datum)
    {
    }

    public void AddData(List<ISpatialData3D> Data)
    {
        foreach(ISpatialData3D Datum in Data)
        {
            AddData(Datum);
        }
    }

    public void ShowStats()
    {
    }

    public HashSet<ISpatialData3D> FindDataInRange(Vector3 SearchLocation, float SearchRange)
    {
        return null;
    }
}
