#define QUADTREE_TrackStats

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface ISpatialData2D
{
    Vector2 GetLocation();
    Rect GetBounds();
    float GetRadius();
}

public class QuadTree : MonoBehaviour
{
    class Node
    {
        Rect Bounds;
        Node[] Children;
        int Depth = -1;

        HashSet<ISpatialData2D> Data;

        public Node(Rect InBounds, int InDepth = 0)
        {
            Bounds = InBounds;
            Depth = InDepth;
        }

        public void AddData(QuadTree Owner, ISpatialData2D Datum)
        {
            if (Children == null)
            {
                // first data for node?
                if (Data == null)
                    Data = new();

                // reached the split point and permitted to?
                if (((Data.Count + 1) >= Owner.PreferredMaxDataPerNode) && CanSplit(Owner))
                {
                    SplitNode(Owner);

                    AddDataToChildren(Owner, Datum);
                }
                else
                    Data.Add(Datum);

                return;
            }

            AddDataToChildren(Owner, Datum);
        }

        bool CanSplit(QuadTree Owner)
        {
            return (Bounds.width >= (Owner.MinimumNodeSize * 2)) &&
                   (Bounds.height >= (Owner.MinimumNodeSize * 2));
        }

        void SplitNode(QuadTree Owner)
        {
            float HalfWidth = Bounds.width / 2f;
            float HalfHeight = Bounds.height / 2f;
            int NewDepth = Depth + 1;

#if QUADTREE_TrackStats
            Owner.NewNodesCreated(4, NewDepth);
#endif // QUADTREE_TrackStats

            Children = new Node[4]
            {
                new Node(new Rect(Bounds.xMin, Bounds.yMin, HalfWidth, HalfHeight), NewDepth),
                new Node(new Rect(Bounds.xMin + HalfWidth, Bounds.yMin, HalfWidth, HalfHeight), NewDepth),
                new Node(new Rect(Bounds.xMin, Bounds.yMin + HalfHeight, HalfWidth, HalfHeight), NewDepth),
                new Node(new Rect(Bounds.xMin + HalfWidth, Bounds.yMin + HalfHeight, HalfWidth, HalfHeight), NewDepth)
            };

            // distribute the data
            foreach(var Datum in Data)
            {
                AddDataToChildren(Owner, Datum);
            }

            Data = null;
        }

        void AddDataToChildren(QuadTree Owner, ISpatialData2D Datum)
        {
            foreach(var Child in Children)
            {
                if (Child.Overlaps(Datum.GetBounds()))
                    Child.AddData(Owner, Datum);
            }
        }

        bool Overlaps(Rect Other)
        {
            return Bounds.Overlaps(Other);
        }

        public void FindDataInBox(Rect SearchRect, HashSet<ISpatialData2D> OutFoundData)
        {
            if (Children == null)
            {
                if (Data == null || Data.Count == 0)
                    return;

                OutFoundData.UnionWith(Data);

                return;
            }

            foreach(var Child in Children)
            {
                if (Child.Overlaps(SearchRect))
                    Child.FindDataInBox(SearchRect, OutFoundData);
            }
        }

        public void FindDataInRange(Vector2 SearchLocation, float SearchRange, HashSet<ISpatialData2D> OutFoundData)
        {
            if (Depth != 0)
            {
                throw new System.InvalidOperationException("FindDataInRange cannot be run on anything other than the root node.");
            }

            Rect SearchRect = new Rect(SearchLocation.x - SearchRange, SearchLocation.y - SearchRange,
                                       SearchRange * 2f, SearchRange * 2f);

            FindDataInBox(SearchRect, OutFoundData);

            OutFoundData.RemoveWhere(Datum => {
                float TestRange = SearchRange + Datum.GetRadius();

                return (SearchLocation - Datum.GetLocation()).sqrMagnitude > (TestRange * TestRange);
                });
        }
    }

    [field: SerializeField] public int PreferredMaxDataPerNode { get; private set; } = 50;
    [field: SerializeField] public int MinimumNodeSize { get; private set; } = 2;

    Node RootNode;

    public void PrepareTree(Rect Bounds)
    {
        RootNode = new Node(Bounds);

#if QUADTREE_TrackStats
        NumNodes = 0;
        MaxDepth = -1;
#endif // QUADTREE_TrackStats
    }

    public void AddData(ISpatialData2D Datum)
    {
        RootNode.AddData(this, Datum);
    }

    public void AddData(List<ISpatialData2D> Data)
    {
        foreach(ISpatialData2D Datum in Data)
        {
            AddData(Datum);
        }
    }

    public void ShowStats()
    {
#if QUADTREE_TrackStats
        Debug.Log($"Max Depth: {MaxDepth}");
        Debug.Log($"Num Nodes: {NumNodes}");
#endif // QUADTREE_TrackStats
    }

    public HashSet<ISpatialData2D> FindDataInRange(Vector2 SearchLocation, float SearchRange)
    {
#if QUADTREE_TrackStats
        var StopWatch = new System.Diagnostics.Stopwatch();
        StopWatch.Start();
#endif // QUADTREE_TrackStats

        HashSet<ISpatialData2D> FoundData = new();
        RootNode.FindDataInRange(SearchLocation, SearchRange, FoundData);

#if QUADTREE_TrackStats
        StopWatch.Stop();
        Debug.Log($"Search found {FoundData.Count} results in {StopWatch.ElapsedMilliseconds} ms");
#endif // QUADTREE_TrackStats

        return FoundData;
    }

#if QUADTREE_TrackStats
    int MaxDepth = -1;
    int NumNodes = 0;

    public void NewNodesCreated(int NumAdded, int NodeDepth)
    {
        NumNodes += NumAdded;
        MaxDepth = Mathf.Max(MaxDepth, NodeDepth);
    }
#endif // QUADTREE_TrackStats
}
