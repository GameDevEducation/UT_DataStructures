#define OCTREE_TrackStats

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISpatialData3D
{
    Vector3 GetLocation();
    Bounds GetBounds();
    float GetRadius();
}

public class Octree : MonoBehaviour
{
    class Node
    {
        Bounds NodeBounds;
        Node[] Children;
        int Depth = -1;

        HashSet<ISpatialData3D> Data;

        public Node(Bounds InBounds, int InDepth = 0)
        {
            NodeBounds = InBounds;
            Depth = InDepth;
        }

        public void AddData(Octree Owner, ISpatialData3D Datum)
        {
            if (Children == null)
            {
                // is this the first time we're adding data to this node
                if (Data == null)
                    Data = new();

                // should we split AND are we able to split?
                if ((Data.Count + 1) >= Owner.PreferredMaxDataPerNode && CanSplit(Owner))
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

        void SplitNode(Octree Owner)
        {
            Vector3 ChildSize = NodeBounds.extents;
            Vector3 Offset = ChildSize / 2f;
            int NewDepth = Depth + 1;

#if OCTREE_TrackStats
            Owner.NewNodesCreated(8, NewDepth);
#endif // OCTREE_TrackStats

            Children = new Node[8]
            {
                new Node(new Bounds(NodeBounds.center + new Vector3(-Offset.x, -Offset.y,  Offset.z), ChildSize), NewDepth),
                new Node(new Bounds(NodeBounds.center + new Vector3( Offset.x, -Offset.y,  Offset.z), ChildSize), NewDepth),
                new Node(new Bounds(NodeBounds.center + new Vector3(-Offset.x, -Offset.y, -Offset.z), ChildSize), NewDepth),
                new Node(new Bounds(NodeBounds.center + new Vector3( Offset.x, -Offset.y, -Offset.z), ChildSize), NewDepth),
                new Node(new Bounds(NodeBounds.center + new Vector3(-Offset.x,  Offset.y,  Offset.z), ChildSize), NewDepth),
                new Node(new Bounds(NodeBounds.center + new Vector3( Offset.x,  Offset.y,  Offset.z), ChildSize), NewDepth),
                new Node(new Bounds(NodeBounds.center + new Vector3(-Offset.x,  Offset.y, -Offset.z), ChildSize), NewDepth),
                new Node(new Bounds(NodeBounds.center + new Vector3( Offset.x,  Offset.y, -Offset.z), ChildSize), NewDepth)
            };

            foreach(var Datum in Data)
            {
                AddDataToChildren(Owner, Datum);
            }

            Data = null;
        }

        void AddDataToChildren(Octree Owner, ISpatialData3D Datum)
        {
            foreach(var Child in Children)
            {
                if (Child.Overlaps(Datum.GetBounds()))
                    Child.AddData(Owner, Datum);
            }
        }

        bool Overlaps(Bounds Other)
        {
            return NodeBounds.Intersects(Other);
        }

        bool CanSplit(Octree Owner)
        {
            return NodeBounds.size.x >= Owner.MinimumNodeSize &&
                   NodeBounds.size.y >= Owner.MinimumNodeSize &&
                   NodeBounds.size.z >= Owner.MinimumNodeSize;
        }

        public void FindDataInBox(Bounds SearchBounds, HashSet<ISpatialData3D> OutFoundData, bool bExactBounds = true)
        {
            if (Children == null)
            {
                if (Data == null || Data.Count == 0)
                    return;

                // optimised check for a root node with no children
                if (Depth == 0 && bExactBounds)
                {
                    foreach(var Datum in Data)
                    {
                        if (SearchBounds.Intersects(Datum.GetBounds()))
                            OutFoundData.Add(Datum);
                    }

                    return;
                }

                OutFoundData.UnionWith(Data);

                return;
            }

            foreach(var Child in Children)
            {
                if (Child.Overlaps(SearchBounds))
                    Child.FindDataInBox(SearchBounds, OutFoundData, bExactBounds);
            }

            // if we're on the root node filter out data not within the search bounds
            if (Depth == 0 && bExactBounds)
            {
                OutFoundData.RemoveWhere(Datum =>
                {
                    return !SearchBounds.Intersects(Datum.GetBounds());
                });
            }
        }

        public void FindDataInRange(Vector3 SearchLocation, float SearchRange, HashSet<ISpatialData3D> OutFoundData)
        {
            if (Depth != 0)
            {
                throw new System.InvalidOperationException("FindDataInRange cannot be run on anything other than the root node.");
            }

            Bounds SearchBounds = new Bounds(SearchLocation, SearchRange * Vector3.one * 2f);

            FindDataInBox(SearchBounds, OutFoundData, false);

            OutFoundData.RemoveWhere(Datum =>
            {
                float TestRange = SearchRange + Datum.GetRadius();

                return (SearchLocation - Datum.GetLocation()).sqrMagnitude > (TestRange * TestRange);
            });
        }
    }

    [field: SerializeField] public int PreferredMaxDataPerNode { get; private set; } = 50;
    [field: SerializeField] public int MinimumNodeSize { get; private set; } = 5;

    Node RootNode;

    public void PrepareTree(Bounds InBounds)
    {
        RootNode = new Node(InBounds);

#if OCTREE_TrackStats
        MaxDepth = 0;
        NumNodes = 1;
#endif // OCTREE_TrackStats
    }

    public void AddData(ISpatialData3D Datum)
    {
        RootNode.AddData(this, Datum);
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
#if OCTREE_TrackStats
        Debug.Log($"Max Depth: {MaxDepth}");
        Debug.Log($"Num Nodes: {NumNodes}");
#endif // OCTREE_TrackStats
    }

    public HashSet<ISpatialData3D> FindDataInRange(Vector3 SearchLocation, float SearchRange)
    {
#if OCTREE_TrackStats
        var StopWatch = new System.Diagnostics.Stopwatch();
        StopWatch.Start();
#endif // OCTREE_TrackStats

        HashSet<ISpatialData3D> FoundData = new();
        RootNode.FindDataInRange(SearchLocation, SearchRange, FoundData);

#if OCTREE_TrackStats
        StopWatch.Stop();
        Debug.Log($"Search found {FoundData.Count} results in {StopWatch.ElapsedMilliseconds} ms");
#endif // OCTREE_TrackStats

        return FoundData;
    }

#if OCTREE_TrackStats
    int MaxDepth = -1;
    int NumNodes = 0;

    public void NewNodesCreated(int NumAdded, int NodeDepth)
    {
        NumNodes += NumAdded;
        MaxDepth = Mathf.Max(MaxDepth, NodeDepth);
    }
#endif // OCTREE_TrackStats
}
