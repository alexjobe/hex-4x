using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QPath
{
    public class QPath_AStar {

        public QPath_AStar(
            IQPathWorld world, 
            IQPathUnit unit, 
            IQPathTile startTile, 
            IQPathTile endTile,
            CostEstimateDelegate costEstimateFunc
        )
        {
            // Setup
            this.world = world;
            this.unit = unit;
            this.startTile = startTile;
            this.endTile = endTile;
            this.costEstimateFunc = costEstimateFunc;

        }

        IQPathWorld world;
        IQPathUnit unit;
        IQPathTile startTile;
        IQPathTile endTile;
        CostEstimateDelegate costEstimateFunc;

        Queue<IQPathTile> path;

        public void DoWork()
        {
            path = new Queue<IQPathTile>();

            HashSet<IQPathTile> closedSet = new HashSet<IQPathTile>();

            PathfindingPriorityQueue<IQPathTile> openSet = new PathfindingPriorityQueue<IQPathTile>();
            openSet.Enqueue(startTile, 0);

            Dictionary<IQPathTile, IQPathTile> cameFrom = new Dictionary<IQPathTile, IQPathTile>();

            Dictionary<IQPathTile, float> gScore = new Dictionary<IQPathTile, float>();
            gScore[startTile] = 0;
            
            Dictionary<IQPathTile, float> fScore = new Dictionary<IQPathTile, float>();
            fScore[startTile] = costEstimateFunc(startTile, endTile);

            while (openSet.Count > 0)
            {
                IQPathTile current = openSet.Dequeue();

                // Check to see if we are there.
                if (current == endTile)
                {
                    ReconstructPath(cameFrom, current);
                    return;
                }

                closedSet.Add(current);

                foreach (IQPathTile edgeNeighbour in current.GetNeighbours())
                {
                    IQPathTile neighbour = edgeNeighbour;

                    if (closedSet.Contains(neighbour))
                    {
                        continue; // ignore this already completed neighbor
                    }

                    float total_pathfinding_cost_to_neighbour =
                        neighbour.AggregateCostToEnter(gScore[current], current, unit);

                    if (total_pathfinding_cost_to_neighbour < 0)
                    {
                        // Values less than zero represent an invalid/impassable tile
                        continue; 
                    }
                    //Debug.Log(total_pathfinding_cost_to_neighbor);

                    float tentative_g_score = total_pathfinding_cost_to_neighbour;

                    // Is the neighbour already in the open set?
                    //   If so, and if this new score is worse than the old score,
                    //   discard this new result.
                    if (openSet.Contains(neighbour) && tentative_g_score >= gScore[neighbour])
                    {
                        continue;
                    }

                    // This is either a new tile or we just found a cheaper route to it
                    cameFrom[neighbour] = current;
                    gScore[neighbour] = tentative_g_score;
                    fScore[neighbour] = gScore[neighbour] + costEstimateFunc(neighbour, endTile);

                    openSet.EnqueueOrUpdate(neighbour, fScore[neighbour]);
                } // foreach neighbour
            } // while

        }

        private void ReconstructPath(
            Dictionary<IQPathTile, IQPathTile> cameFrom,
            IQPathTile current)
        {
            // So at this point, current IS the goal.
            // So what we want to do is walk backwards through the Came_From
            // map, until we reach the "end" of that map...which will be
            // our starting node!
            Queue<IQPathTile> totalPath = new Queue<IQPathTile>();
            totalPath.Enqueue(current); // This "final" step is the path is the goal!

            while (cameFrom.ContainsKey(current))
            {
                /*    Came_From is a map, where the
            *    key => value relation is real saying
            *    some_node => we_got_there_from_this_node
            */

                current = cameFrom[current];
                totalPath.Enqueue(current);
            }

            // At this point, total_path is a queue that is running
            // backwards from the END tile to the START tile, so let's reverse it.
            path = new Queue<IQPathTile>(totalPath.Reverse());
        }

        public IQPathTile[] GetList()
        {
            return path.ToArray();
        }
        
    }
}
