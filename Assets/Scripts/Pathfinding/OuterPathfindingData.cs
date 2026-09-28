using UnityEngine;

namespace App.Pathfinding
{
    public class OuterPathfindingData
    {
        public AstarGrid Chunk { get; set; }
        public AstarNode From { get; set; }
        public AstarNode To { get; set; }

        public OuterPathfindingData(AstarGrid chunk, AstarNode from, AstarNode to)
        {
            Chunk = chunk;
            From = from;
            To = to;
        }
    }
}
