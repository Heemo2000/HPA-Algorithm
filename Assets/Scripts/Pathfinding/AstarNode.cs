using UnityEngine;

namespace App.Pathfinding
{
    public class AstarNode
    {
        public Vector2Int PositionInGrid { get; private set; }
        public int GCost { get; set; }
        public int HCost { get; set; }
        public int FCost { get => GCost + HCost; }
        public AstarNode Parent { get; set; }
        public bool Walkable { get; set; }
        public float Size { get; private set; }

        public AstarNode(Vector2Int positionInGrid, float size)
        {
            PositionInGrid = positionInGrid;
            GCost = 0;
            HCost = 0;
            Parent = null;
            Walkable = false;
            Size = size;
        }
    }
}
