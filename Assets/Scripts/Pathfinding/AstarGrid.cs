using System.Collections.Generic;
using UnityEngine;

namespace App.Pathfinding
{
    public class AstarGrid
    {
        private static int StraightCost = 10;
        private static int DiagonalCost = 14;

        private List<AstarNode> _openSet;
        private List<AstarNode> _closeSet;
        private List<Vector2> _result;

        public AstarNode[,] Nodes {  get; private set; }
        public Vector2 Origin { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public float NodeSize {  get; private set; }
        public LayerMask ObstacleMask { get; private set; }

        public AstarGrid(Vector2 origin, int width, int height, float nodeSize, LayerMask obstacleMask)
        {
            Origin = origin;
            Width = width;
            Height = height;
            NodeSize = nodeSize;
            ObstacleMask = obstacleMask;

            Nodes = new AstarNode[width, height];
            for(int i = 0; i < width; i++)
            {
                for(int j = 0; j < height; j++)
                {
                    Nodes[i, j] = new AstarNode(new Vector2Int(i, j), nodeSize);
                    Vector2 nodeWorldPosition = GetWorldPosition(i, j);
                    Nodes[i, j].Walkable = Physics2D.OverlapCircle(nodeWorldPosition, nodeSize / 2.0f, obstacleMask.value) == null;
                }
            }

            _openSet = new List<AstarNode>();
            _closeSet = new List<AstarNode>();
            _result = new List<Vector2>();
        }

        public Vector2 GetWorldPosition(int x, int y)
        {
            return Origin + new Vector2(x * Height * NodeSize,-y * Width * NodeSize);
        }

        public void GetXY(Vector2 position, out int x, out int y)
        {
            Vector2Int gridPosition = new Vector2Int((int)(position.x - Origin.x), (int)(position.y - Origin.y));
            
            gridPosition.x /= (Width * Mathf.CeilToInt(NodeSize));
            gridPosition.y /= (Height * Mathf.CeilToInt(NodeSize));

            x = gridPosition.x;
            y = gridPosition.y;
        }

        public List<Vector2> FindPath(AstarNode startNode, AstarNode endNode)
        {
            
            if(!startNode.Walkable || !endNode.Walkable)
            {
                return null;
            }

            ClearCosts();

            _openSet.Clear();
            _closeSet.Clear();

            _openSet.Add(startNode);

            while (_openSet.Count > 0)
            {
                AstarNode leastNode = GetLeastCostNode(ref _openSet);
                _openSet.Remove(leastNode);
                _closeSet.Add(leastNode);


                if (leastNode.PositionInGrid == endNode.PositionInGrid)
                {
                    return RetracePath(leastNode);
                }

                List<AstarNode> neighbours = GetNeighbours(leastNode);

                foreach(AstarNode neighbour in neighbours)
                {
                    if(!neighbour.Walkable || _closeSet.Contains(neighbour))
                    {
                        continue;
                    }

                    int newCostToNeighbour = leastNode.GCost + ManhattanDistance(leastNode, endNode);

                    if(newCostToNeighbour < neighbour.GCost || !_openSet.Contains(neighbour))
                    {
                        neighbour.GCost = newCostToNeighbour;
                        neighbour.HCost = ManhattanDistance(neighbour, endNode);
                        neighbour.Parent = leastNode;

                        if(!_openSet.Contains(neighbour))
                        {
                            _openSet.Add(neighbour);
                        }
                    }
                }

            }

            return null;
        }

        private void ClearCosts()
        {
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    Nodes[i, j].GCost = 0;
                    Nodes[i, j].HCost = 0;
                }
            }
        }

        private AstarNode GetLeastCostNode(ref List<AstarNode> openSet)
        {
            int lowestCost = int.MaxValue;
            AstarNode leastCostNode = null;

            foreach (AstarNode node in openSet)
            {
                if(lowestCost > node.FCost)
                {
                    lowestCost = node.FCost;
                    leastCostNode = node;
                }
            }

            return leastCostNode;
        }

        private List<Vector2> RetracePath(AstarNode node)
        {
            _result.Clear();

            AstarNode current = node;
            while (current != null)
            {
                _result.Add(GetWorldPosition(current.PositionInGrid.x, current.PositionInGrid.y));
                current = current.Parent;
            }

            return _result;
        }

        private List<AstarNode> GetNeighbours(AstarNode currentNode)
        {
            Vector2Int currentPosition = currentNode.PositionInGrid;
            List<AstarNode> neighbours = new List<AstarNode>();
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    int estimatedNeighbourX = currentPosition.x + x;
                    int estimatedNeighbourY = currentPosition.y + y;

                    if(estimatedNeighbourX < 0 || estimatedNeighbourX >= Width || estimatedNeighbourY < 0 || estimatedNeighbourY >= Height)
                    {
                        continue;
                    }

                    neighbours.Add(Nodes[estimatedNeighbourX, estimatedNeighbourY]);
                }
            }

            return neighbours;
        }

        private int ManhattanDistance(AstarNode nodeA,  AstarNode nodeB)
        {
            int dstX = Mathf.Abs(nodeA.PositionInGrid.x - nodeB.PositionInGrid.y);
            int dstY = Mathf.Abs(nodeA.PositionInGrid.y - nodeB.PositionInGrid.y);

            return DiagonalCost * Mathf.Max(dstX, dstY) + StraightCost * Mathf.Abs(dstX - dstY);
        }
    }
}
