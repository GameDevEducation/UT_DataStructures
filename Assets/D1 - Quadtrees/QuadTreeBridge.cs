using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadTreeBridge : MonoBehaviour
{
    [SerializeField] QuadTree LinkedQuadTree;

    public void On2DBoundsCalculated(Rect Bounds)
    {
        LinkedQuadTree.PrepareTree(Bounds);
    }

    public void OnItemSpawned(GameObject ItemGO)
    {
        LinkedQuadTree.AddData(ItemGO.GetComponent<ISpatialData2D>());
    }

    public void OnAllItemsSpawned(List<GameObject> Items)
    {
        // Intentionally turned off as only one of the add methods should be used

        //List<ISpatialData2D> SpatialItems = new List<ISpatialData2D>(Items.Count);
        //foreach (GameObject Item in Items)
        //{
        //    SpatialItems.Add(Item.GetComponent<ISpatialData2D>());
        //}

        //LinkedQuadTree.AddData(SpatialItems);

        LinkedQuadTree.ShowStats();
    }
}
