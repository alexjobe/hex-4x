using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QPath
{
    /* 
     *  Tile[] ourPath = QPath.FindPath( world, unit, startTile, endTile );
     * 
     *  unit is the object that is trying to path between tiles. It might have special
     *  logic based on its movement type and the types of tiles being moved through
     * 
     *  Our tiles need to be able to return the following information:
     *      1) A list of neighbours
     *      2) The aggregate cost to enter this tile from another tile
     * 
     * 
     */

    public class QPath
    {
        public static IQPathTile[] FindPath(
            IQPathWorld world, 
            IQPathUnit unit, 
            IQPathTile startTile, 
            IQPathTile endTile,
            CostEstimateDelegate costEstimateFunc
        )
        {
            if (world == null || unit == null || startTile == null || endTile == null)
            {
                Debug.LogError("Null values passed to QPath::FindPath");
                return null;
            }

            // Call on path solver (could have different types)
            QPath_AStar resolver = new QPath_AStar(world, unit, startTile, endTile, costEstimateFunc);

            resolver.DoWork();

            return resolver.GetList();
        }
    }

    public delegate float CostEstimateDelegate(IQPathTile a, IQPathTile b);
}
