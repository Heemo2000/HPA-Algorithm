using System;
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

        #region Node Related Properties
        public AstarNode[,] Nodes {  get; private set; }
        public Vector2Int PositionInChunksGrid { get; private set; }
        public Vector2 Origin { get; private set; }
        public int Rows { get; private set; }
        public int Columns { get; private set; }
        public float NodeSize {  get; set; }
        public LayerMask ObstacleMask { get; private set; }
        public HashSet<EntranceEdge> Entrances { get; private set; }

        #endregion

        public AstarGrid(Vector2Int positionInChunksGrid, Vector2 origin, int rows, int columns, float nodeSize, LayerMask obstacleMask)
        {
            Origin = origin;
            Rows = rows;
            Columns = columns;
            NodeSize = nodeSize;
            ObstacleMask = obstacleMask;
            PositionInChunksGrid = positionInChunksGrid;

            Nodes = new AstarNode[rows, columns];
            Entrances = new HashSet<EntranceEdge>();

            for(int i = 0; i < rows; i++)
            {
                for(int j = 0; j < columns; j++)
                {
                    Nodes[i, j] = new AstarNode(new Vector2Int(i, j), positionInChunksGrid, nodeSize);
                    Vector2 nodeWorldPosition = GetWorldPositionCentre(i, j);
                    Nodes[i, j].Walkable = Physics2D.OverlapCircle(nodeWorldPosition, nodeSize / 2.0f, obstacleMask.value) == null;
                }
            }

            _openSet = new List<AstarNode>();
            _closeSet = new List<AstarNode>();
            _result = new List<Vector2>();
        }

        public Vector2 GetWorldPositionCentre(int x, int y)
        {
            return Origin + new Vector2(x * NodeSize/2.0f, -y * NodeSize/2.0f);
        }

        public void GetXY(Vector2 position, out int x, out int y)
        {
            Vector2Int gridPosition = new Vector2Int((int)(position.x - Origin.x), (int)(position.y - Origin.y));
            
            gridPosition.x /= Mathf.CeilToInt(NodeSize/2.0f);
            gridPosition.y /= -Mathf.CeilToInt(NodeSize/2.0f);

            x = gridPosition.x;
            y = gridPosition.y;
        }

        public List<Vector2> FindPath(AstarNode startNode, AstarNode endNode)
        {
            
            if(!startNode.Walkable || !endNode.Walkable)
            {
                return null;
            }

            ClearCostsAndRemoveParent();

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

        private void ClearCostsAndRemoveParent()
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    Nodes[i, j].GCost = 0;
                    Nodes[i, j].HCost = 0;
                    Nodes[i, j].Parent = null;
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
                _result.Add(GetWorldPositionCentre(current.PositionInGrid.x, current.PositionInGrid.y));
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

                    if(x == 0 && y == 0)
                    {
                        continue;
                    }
                    else if(estimatedNeighbourX < 0 || estimatedNeighbourX >= Rows || estimatedNeighbourY < 0 || estimatedNeighbourY >= Columns)
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
            int dstX = Mathf.Abs(nodeA.PositionInGrid.x - nodeB.PositionInGrid.x);
            int dstY = Mathf.Abs(nodeA.PositionInGrid.y - nodeB.PositionInGrid.y);

            return DiagonalCost * Mathf.Max(dstX, dstY) + StraightCost * Mathf.Abs(dstX - dstY);
        }

        public override bool Equals(object obj)
        {
            if (obj is AstarGrid grid)
            {
                return Origin.x == grid.Origin.x &&
                       Origin.y == grid.Origin.y &&
                       Rows == grid.Rows &&
                       Columns == grid.Columns &&
                       NodeSize == grid.NodeSize;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Origin.x, Origin.y, Rows, Columns, NodeSize);
        }
    }
}
