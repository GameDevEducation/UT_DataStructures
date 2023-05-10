using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctreeBridge : MonoBehaviour
{
    [SerializeField] Octree LinkedOctree;

    public void On3DBoundsCalculated(Bounds InBounds)
    {
        LinkedOctree.PrepareTree(InBounds);
    }

    public void OnItemSpawned(GameObject ItemGO)
    {
        LinkedOctree.AddData(ItemGO.GetComponent<ISpatialData3D>());
    }

    public void OnAllItemsSpawned(List<GameObject> Items)
    {
        // Intentionally turned off as only one of the add methods should be used

        //List<ISpatialData3D> SpatialItems = new List<ISpatialData3D>(Items.Count);
        //foreach (GameObject Item in Items)
        //{
        //    SpatialItems.Add(Item.GetComponent<ISpatialData3D>());
        //}

        //LinkedOctree.AddData(SpatialItems);

        LinkedOctree.ShowStats();
    }
}
