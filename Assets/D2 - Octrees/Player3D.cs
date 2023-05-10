using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player3D : MonoBehaviour
{
    [SerializeField] Octree LinkedOctree;
    [SerializeField] float ObstacleSearchRange = 30f;

    Vector3 CachedPosition = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);

    HashSet<ISpatialData3D> NearbyObstacles;

    bool HasMoved
    {
        get
        {
            return !Mathf.Approximately((transform.position - CachedPosition).sqrMagnitude, 0f);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (HasMoved)
        {
            CachedPosition = transform.position;

            HighlightNearbyObstacles();
        }
    }

    void HighlightNearbyObstacles()
    {
        HashSet<ISpatialData3D> CandidateObstacles = LinkedOctree.FindDataInRange(CachedPosition, ObstacleSearchRange);

        // identify removals
        if (NearbyObstacles != null)
        {
            foreach(var OldObstacle in NearbyObstacles)
            {
                if (CandidateObstacles.Contains(OldObstacle))
                    continue;

                ProcessRemoveObstacle(OldObstacle);
            }
        }

        if (CandidateObstacles == null)
            return;

        // first time finding obstacles?
        if (NearbyObstacles == null)
        {
            NearbyObstacles = CandidateObstacles;
            foreach (var NewObstacle in NearbyObstacles)
                ProcessAddObstacle(NewObstacle);

            return;
        }

        // identify additions
        foreach(var NewObstacle in CandidateObstacles)
        {
            if (NearbyObstacles.Contains(NewObstacle))
                continue;

            ProcessAddObstacle(NewObstacle);
        }

        NearbyObstacles = CandidateObstacles;
    }

    void ProcessAddObstacle(ISpatialData3D AddedObstacle)
    {
        (AddedObstacle as Obstacle3D).AddHighlight();
    }

    void ProcessRemoveObstacle(ISpatialData3D RemovedObstacle)
    {
        (RemovedObstacle as Obstacle3D).RemoveHighlight();
    }
}
