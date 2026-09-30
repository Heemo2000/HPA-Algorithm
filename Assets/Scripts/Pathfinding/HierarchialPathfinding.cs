using UnityEngine;
using System.Collections.Generic;





#if UNITY_EDITOR
using UnityEditor;
#endif
namespace App.Pathfinding
{
    public class HierarchialPathfinding
    {
        private static int DiagonalCost = 14;
        #region Properties

        public Vector2 RealOrigin { get; set; }
        public int ChunkAmountX { get; set; }
        public int ChunkAmountY { get; set; }
        public int EachChunkWidth { get; set; }
        public int EachChunkHeight { get; set; }
        public float NodeSize { get; set; }
        public LayerMask ObstacleLayerMask { get; set; }
        public float FillSize { get; set; }

        public AstarGrid[,] Chunks { get => _chunks; private set => _chunks = value; }

        public float MinWalkabilityPercent { get => _minWalkabilityPercent;
                                             set
                                             { 
                                                _minWalkabilityPercent = value;
                                                SetWalkabilityPercentForChunks();
                                             }
                                           }
        
        #endregion
        #region Private Fields
        private AstarGrid[,] _chunks;
        private float _minWalkabilityPercent;
        
        #if UNITY_EDITOR
        private GUIStyle _fromGUIStyle = null;
        private Texture2D _fromGUIBGTex = null;

        private GUIStyle _toGUIStyle = null;
        private Texture2D _toGUIBGTex = null;

        #endif
        #endregion

        #region Public Methods and constructors
        public HierarchialPathfinding(Vector2 realOrigin,
                                      int chunkAmountX,
                                      int chunkAmountY,
                                      int eachChunkWidth,
                                      int eachChunkHeight,
                                      float nodeSize,
                                      float fillSize,
                                      LayerMask obstacleLayerMask,
                                      float minWalkabilityPercent = 0.4f)
        {
            RealOrigin = realOrigin;
            ChunkAmountX = chunkAmountX;
            ChunkAmountY = chunkAmountY;
            EachChunkWidth = eachChunkWidth;
            EachChunkHeight = eachChunkHeight;
            NodeSize = nodeSize;
            FillSize = fillSize;
            ObstacleLayerMask = obstacleLayerMask;
            Setup();
            MinWalkabilityPercent = minWalkabilityPercent;
        }

        public List<Vector2> FindPath(Vector2 startPosition, Vector2 endPosition)
        {
            GetChunkXY(startPosition, out int startPositionX, out int startPositionY);
            
            if(startPositionX < 0 || startPositionY < 0 || 
               startPositionX >= ChunkAmountX || startPositionY >= ChunkAmountY)
            {
                return null;
            }

            GetChunkXY(endPosition, out int endPositionX, out int endPositionY);
            
            if(endPositionX <  0 || endPositionY < 0 ||
               endPositionX >= ChunkAmountX || endPositionY >= ChunkAmountY)
            {
                return null;
            }
            
            AstarGrid startGrid = _chunks[startPositionX, startPositionY];
            AstarGrid endGrid = _chunks[endPositionX, endPositionY];

            startGrid.GetXY(startPosition, out int startNodeX, out int startNodeY);
            endGrid.GetXY(endPosition, out int endNodeX, out int endNodeY);

            if ((startGrid != null && endGrid != null) && startGrid == endGrid)
            {
                return startGrid.FindPath(startGrid.Nodes[startNodeX, startNodeY], startGrid.Nodes[endNodeX, endNodeY]);
            }

            List<EntranceEdge> outerPath = FindOuterPath(_chunks[startPositionX, startPositionY],
                                                         _chunks[endPositionX, endPositionY]);

            if(outerPath == null)
            {
                return null;
            }

            //Add the edge for start position.
            

            EntranceEdge startingEdge = new EntranceEdge(null, null,
                                           startGrid.Nodes[startNodeX, startNodeY],
                                           startGrid);
            
            outerPath.Insert(0, startingEdge);

            //Add the edge for end position.

            EntranceEdge endingEdge = new EntranceEdge(endGrid,
                                           endGrid.Nodes[endNodeX, endNodeY],
                                           null,
                                           null);
            outerPath.Add(endingEdge);

            List<Vector2> result = new List<Vector2>();

            for (int i = 0; i < outerPath.Count - 1; i++)
            {
                EntranceEdge edge = outerPath[i];
                AstarGrid chunk = edge.ToChunk;
                List<Vector2> path = chunk.FindPath(edge.To, outerPath[i + 1].From);
                if(path == null)
                {
                    Debug.Log("Path is null for index " + i);
                }
                result.AddRange(path);
            }



            outerPath[outerPath.Count - 1].Parent = null;
            outerPath[1].Parent = null;

            return result;
        }

        public static void OnDrawGizmosSelectedStatic(int chunkAmountX,
                                                      int chunkAmountY,
                                                      int eachChunkWidth,
                                                      int eachChunkHeight,
                                                      float nodeSize,
                                                      float fillSize,
                                                      Vector2 realOrigin)
        {
            for (int i = 0; i < chunkAmountX * chunkAmountY; i++)
            {
                int chunkX = i / chunkAmountY;
                int chunkY = i % chunkAmountY;
                
                Vector2 chunkOrigin = realOrigin;
                chunkOrigin += new Vector2(chunkX * eachChunkHeight * nodeSize / 2.0f, -chunkY * eachChunkHeight * nodeSize / 2.0f);

                for (int j = 0; j < eachChunkWidth * eachChunkHeight; j++)
                {
                    int nodeIndexX = j / eachChunkHeight;
                    int nodeIndexY = j % eachChunkHeight;

                    Vector2 origin = chunkOrigin + new Vector2(nodeIndexX * nodeSize / 2.0f, -nodeIndexY * nodeSize / 2.0f);

                    DrawSquare(Color.white, origin, nodeSize, fillSize);
                }
            }
        }

        public void OnDrawGizmosSelected()
        {

            #if UNITY_EDITOR

            if (_fromGUIStyle == null)
            {
                _fromGUIStyle = new GUIStyle();
                _fromGUIBGTex = new Texture2D(1, 1);
                _fromGUIBGTex.SetPixel(0, 0, Color.white);
                _fromGUIBGTex.Apply();
                _fromGUIStyle.normal.background = _fromGUIBGTex;

                _fromGUIStyle.alignment = TextAnchor.MiddleCenter;
                _fromGUIStyle.padding = new RectOffset(8, 8, 4, 4);
                _fromGUIStyle.margin = new RectOffset(0, 0, 0, 0);

                _fromGUIStyle.fontSize = 10;
                _fromGUIStyle.fontStyle = FontStyle.Bold;
            }

            if (_toGUIStyle == null)
            {
                _toGUIStyle = new GUIStyle();
                _toGUIBGTex = new Texture2D(1, 1);
                _toGUIBGTex.SetPixel(0, 0, Color.lightBlue);
                _toGUIBGTex.Apply();
                _toGUIStyle.normal.background = _toGUIBGTex;

                _toGUIStyle.alignment = TextAnchor.MiddleCenter;
                _toGUIStyle.padding = new RectOffset(8, 8, 4, 4);
                _toGUIStyle.margin = new RectOffset(0, 0, 0, 0);

                _toGUIStyle.fontSize = 10;
                _toGUIStyle.fontStyle = FontStyle.Bold;
            }

            #endif
            for (int i = 0; i < ChunkAmountX * ChunkAmountY; i++)
            {
                int chunkX = i / ChunkAmountY;
                int chunkY = i % ChunkAmountY;

                AstarGrid chunk = _chunks[chunkX, chunkY];

                Vector2 chunkOrigin = chunk.Origin;

                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(chunkOrigin, Vector3.one * NodeSize/2.0f);

                for (int j = 0; j < EachChunkWidth * EachChunkHeight; j++)
                {
                    int nodeIndexX = j / EachChunkHeight;
                    int nodeIndexY = j % EachChunkHeight;

                    Vector2 origin = Vector3.zero;
                    origin = chunk.GetWorldPositionCentre(nodeIndexX, nodeIndexY);

                    DrawSquare(chunk.Nodes[nodeIndexX, nodeIndexY].Walkable ? Color.green : Color.red, origin, NodeSize, FillSize);
                }

                foreach (EntranceEdge edge in chunk.Entrances)
                {
                    AstarGrid fromChunk = edge.FromChunk;
                    AstarNode from = edge.From;
                    AstarNode to = edge.To;
                    AstarGrid toChunk = edge.ToChunk;

                    Vector2 fromPosition = fromChunk.GetWorldPositionCentre(from.PositionInGrid.x, from.PositionInGrid.y);
                    Vector2 toPosition = toChunk.GetWorldPositionCentre(to.PositionInGrid.x, to.PositionInGrid.y);

                    Gizmos.color = Color.magenta;
                    Gizmos.DrawLine(fromPosition, toPosition);

                    #if UNITY_EDITOR
                    Handles.Label(fromPosition, from.PositionInGrid.ToString() + "\n" + to.PositionInGrid.ToString(), _fromGUIStyle);
                    #endif

                    #if UNITY_EDITOR
                    Handles.Label(toPosition, to.PositionInGrid.ToString() + "\n" + to.PositionInGrid.ToString(), _toGUIStyle);
                    #endif
                }

            }
        }

        private List<EntranceEdge> FindOuterPath(AstarGrid startChunk, AstarGrid endChunk)
        {
            SetWalkabilityPercentForChunks();
            PopulateEntrances();

            if (!startChunk.Walkable || !endChunk.Walkable)
            {
                return null;
            }

            ClearChunkCostsAndEdgeParents();

            List<OuterPathfindingPartData> openSet = new List<OuterPathfindingPartData>();
            List<OuterPathfindingPartData> closeSet = new List<OuterPathfindingPartData>();

            openSet.Add(new OuterPathfindingPartData(startChunk, null));

            while(openSet.Count > 0)
            {
                OuterPathfindingPartData current = GetLeastCostPart(openSet);
                
                if(current.Chunk == endChunk)
                {
                    return RetracePath(current.Edge);
                }
                openSet.Remove(current);
                closeSet.Add(current);
                HashSet<EntranceEdge> neighbours = current.Chunk.Entrances;

                foreach(EntranceEdge neighbour in neighbours)
                {
                    if(!neighbour.ToChunk.Walkable || IsChunkExistsInCloseSet(closeSet, neighbour.ToChunk))
                    {
                        continue;
                    }
                    int newCostToNeighbour = current.Chunk.GCost + EuclideanHeuristic(current.Chunk, neighbour.ToChunk);

                    bool isNeighbourExists = IsChunkExistsInOpenSet(openSet, neighbour.ToChunk);
                    if (newCostToNeighbour < neighbour.ToChunk.GCost || !isNeighbourExists)
                    {
                        neighbour.ToChunk.GCost = newCostToNeighbour;
                        neighbour.ToChunk.HCost = EuclideanHeuristic(neighbour.ToChunk, endChunk);
                        neighbour.Parent = current.Edge;
                        if(!isNeighbourExists)
                        {
                            openSet.Add(new OuterPathfindingPartData(neighbour.ToChunk, neighbour));
                        }
                    }
                }
            }

            return null;
        }
        #endregion

        #region Private Methods
        
        private static void DrawSquare(Color color, Vector2 origin, float size, float scale = 1.0f)
        {
            //I don't know right now why this works.
            float scaledSize = size * scale / 2.0f;

            Vector2 topLeft = origin + new Vector2(-scaledSize / 2.0f, scaledSize / 2.0f);
            Vector2 topRight = origin + new Vector2(scaledSize / 2.0f, scaledSize / 2.0f);
            Vector2 bottomRight = origin + new Vector2(scaledSize / 2.0f, -scaledSize / 2.0f);
            Vector2 bottomLeft = origin + new Vector2(-scaledSize / 2.0f, -scaledSize / 2.0f);

            Gizmos.color = color;
            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
            Gizmos.DrawLine(bottomLeft, topLeft);
        }

        private void InitializeGrid()
        {
            _chunks = new AstarGrid[ChunkAmountX, ChunkAmountY];
            
            for (int i = 0; i < ChunkAmountX; i++)
            {
                for (int j = 0; j < ChunkAmountY; j++)
                {
                    Vector2 origin = RealOrigin + new Vector2(i * EachChunkWidth * (NodeSize / 2.0f), -j * EachChunkHeight * (NodeSize / 2.0f));

                    _chunks[i, j] = new AstarGrid(new Vector2Int(i,j), origin, EachChunkWidth, EachChunkHeight, NodeSize, ObstacleLayerMask, _minWalkabilityPercent);
                }
            }
        }

        private void PopulateEntrances()
        {
            for (int i = 0; i < ChunkAmountX * ChunkAmountY; i++)
            {
                int x = i / ChunkAmountY;
                int y = i % ChunkAmountY;

                _chunks[x, y].Entrances.Clear();
            }

            for (int i = 0; i < ChunkAmountX * ChunkAmountY; i++)
            {
                int x = i / ChunkAmountY;
                int y = i % ChunkAmountY;

                AstarGrid currentChunk = _chunks[x, y];
                
                //Top
                if(x - 1 >= 0 && !IsHorizontalEntranceExists(x-1, y, -1))
                {
                    //Top Left
                    if (y - 1 >= 0 && !IsDiagonalEntranceExists(x-1, y-1, -1, 1))
                    {
                        AstarGrid topLeftChunk = _chunks[x-1, y-1];
                        if (currentChunk.Nodes[0,0].Walkable && topLeftChunk.Nodes[topLeftChunk.Rows - 1, topLeftChunk.Columns - 1].Walkable)
                        {
                            _chunks[x, y].Entrances.Add(new EntranceEdge(currentChunk, currentChunk.Nodes[0, 0], topLeftChunk.Nodes[topLeftChunk.Rows - 1, topLeftChunk.Columns - 1], topLeftChunk));
                            _chunks[x - 1, y - 1].Entrances.Add(new EntranceEdge(topLeftChunk, topLeftChunk.Nodes[topLeftChunk.Rows - 1, topLeftChunk.Columns - 1], currentChunk.Nodes[0, 0], currentChunk));
                        }
                    }
                    Debug.Log($"There exists a top chunk for chunk at {x},{y}");
                    
                    
                    AstarGrid topChunk = _chunks[x - 1, y];

                    int entranceY = FindRandomWalkableNodeIndexY(currentChunk, topChunk, 1);

                    if(entranceY > -1)
                    {
                        _chunks[x, y].Entrances.Add(new EntranceEdge(currentChunk, currentChunk.Nodes[0, entranceY], topChunk.Nodes[topChunk.Rows - 1, entranceY], topChunk));

                        _chunks[x-1, y].Entrances.Add(new EntranceEdge(topChunk, topChunk.Nodes[topChunk.Rows - 1, entranceY], currentChunk.Nodes[0, entranceY], currentChunk));
                    }

                    //Top Right
                    if (y + 1 < ChunkAmountY && !IsDiagonalEntranceExists(x - 1, y + 1, -1, -1))
                    {
                        AstarGrid topRightChunk = _chunks[x - 1, y + 1];
                        if (currentChunk.Nodes[0, currentChunk.Columns - 1].Walkable && topRightChunk.Nodes[topRightChunk.Rows - 1, topRightChunk.Columns - 1].Walkable)
                        {
                            _chunks[x, y].Entrances.Add(new EntranceEdge(currentChunk,
                                                                     currentChunk.Nodes[0, currentChunk.Columns - 1],
                                                                     topRightChunk.Nodes[topRightChunk.Rows - 1, topRightChunk.Columns - 1],
                                                                     topRightChunk));

                            _chunks[x - 1, y + 1].Entrances.Add(new EntranceEdge(topRightChunk,
                                                                                 topRightChunk.Nodes[topRightChunk.Rows - 1, topRightChunk.Columns - 1],
                                                                                 currentChunk.Nodes[0, currentChunk.Columns - 1],
                                                                                 currentChunk));
                        }
                    }
                }


                //Right
                if (y + 1 < ChunkAmountY && !IsVerticalEntranceExists(x, y+1, -1))
                {

                    Debug.Log($"There exists a right chunk for chunk at {x},{y}");
                    AstarGrid rightChunk = _chunks[x, y + 1];
                    int entranceX = FindRandomWalkableNodeIndexX(currentChunk, rightChunk, 1);
                    if (entranceX > -1)
                    {
                        _chunks[x, y].Entrances.Add(new EntranceEdge(currentChunk, currentChunk.Nodes[entranceX, currentChunk.Columns - 1], rightChunk.Nodes[entranceX, 0], rightChunk));
                        _chunks[x, y + 1].Entrances.Add(new EntranceEdge(rightChunk, rightChunk.Nodes[entranceX, 0], currentChunk.Nodes[entranceX, currentChunk.Columns - 1], currentChunk));
                    }
                }
                

                //Bottom
                if(x + 1 < ChunkAmountX && !IsHorizontalEntranceExists(x+1, y, 1))
                {
                    //Bottom Left
                    if(y - 1 >= 0 && !IsDiagonalEntranceExists(x+1, y - 1, 1, 1))
                    {
                        AstarGrid bottomLeftChunk = _chunks[x + 1, y - 1];
                        if(currentChunk.Nodes[currentChunk.Rows - 1, 0].Walkable && bottomLeftChunk.Nodes[0, bottomLeftChunk.Columns - 1].Walkable)
                        {
                            _chunks[x, y].Entrances.Add(new EntranceEdge(currentChunk,
                                                                    currentChunk.Nodes[currentChunk.Rows - 1, 0],
                                                                    bottomLeftChunk.Nodes[0, bottomLeftChunk.Columns - 1],
                                                                    bottomLeftChunk));

                            _chunks[x + 1, y - 1].Entrances.Add(new EntranceEdge(bottomLeftChunk,
                                                                                 bottomLeftChunk.Nodes[0, bottomLeftChunk.Columns - 1],
                                                                                 currentChunk.Nodes[currentChunk.Rows - 1, 0],
                                                                                 currentChunk));
                        }
                    }

                    Debug.Log($"There exists a bottom chunk for chunk at {x},{y}");
                    AstarGrid bottomChunk = _chunks[x + 1, y];

                    int entranceY = FindRandomWalkableNodeIndexY(currentChunk, bottomChunk, -1);
                    if (entranceY > -1)
                    {
                        _chunks[x, y].Entrances.Add(new EntranceEdge(currentChunk,
                                                                    currentChunk.Nodes[currentChunk.Rows - 1, entranceY],
                                                                    bottomChunk.Nodes[0, entranceY], bottomChunk));

                        _chunks[x + 1, y].Entrances.Add(new EntranceEdge(bottomChunk,
                                                                         bottomChunk.Nodes[0, entranceY],
                                                                         currentChunk.Nodes[currentChunk.Rows - 1, entranceY], currentChunk));
                    }

                    //Bottom Right
                    if(y + 1 < ChunkAmountY && !IsDiagonalEntranceExists(x + 1, y + 1, -1, -1))
                    {
                        AstarGrid bottomRightChunk = _chunks[x + 1, y + 1];
                        if(currentChunk.Nodes[currentChunk.Rows - 1, currentChunk.Columns - 1].Walkable && bottomRightChunk.Nodes[0, 0].Walkable)
                        {
                            _chunks[x, y].Entrances.Add(new EntranceEdge(currentChunk,
                                                                    currentChunk.Nodes[currentChunk.Rows - 1, currentChunk.Columns - 1],
                                                                    bottomRightChunk.Nodes[0, 0],
                                                                    bottomRightChunk));

                            _chunks[x + 1, y + 1].Entrances.Add(new EntranceEdge(bottomRightChunk,
                                                                                 bottomRightChunk.Nodes[0, 0],
                                                                                 currentChunk.Nodes[currentChunk.Rows - 1, currentChunk.Columns - 1],
                                                                                 currentChunk));
                        }
                    }
                }
                

                //Left
                if (y - 1 >= 0 && !IsVerticalEntranceExists(x, y-1, -1))
                {
                    Debug.Log($"There exists a left chunk for chunk at {x},{y}");
                    AstarGrid leftChunk = _chunks[x, y - 1];
                    int entranceX = FindRandomWalkableNodeIndexX(currentChunk, leftChunk, -1);
                    if (entranceX > -1)
                    {
                        _chunks[x, y].Entrances.Add(new EntranceEdge(currentChunk, currentChunk.Nodes[entranceX, 0], leftChunk.Nodes[entranceX, leftChunk.Columns - 1], leftChunk));
                        _chunks[x, y - 1].Entrances.Add(new EntranceEdge(leftChunk, leftChunk.Nodes[entranceX, leftChunk.Columns - 1], currentChunk.Nodes[entranceX, 0], currentChunk));
                    }
                }
            }
        }

        private bool IsHorizontalEntranceExists(int x, int y, int direction)
        {
            AstarGrid chunk = _chunks[x, y];
            HashSet<EntranceEdge> entrances = chunk.Entrances;
            foreach(EntranceEdge entrance in entrances)
            {
                AstarNode from = entrance.From;

                if (direction == 1 && (from.PositionInGrid.x == 0))
                {
                    return true;
                }
                else if (direction == -1 && (from.PositionInGrid.x == chunk.Rows - 1))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsVerticalEntranceExists(int x, int y, int direction)
        {
            AstarGrid chunk = _chunks[x, y];
            HashSet<EntranceEdge> entrances = chunk.Entrances;
            foreach (EntranceEdge entrance in entrances)
            {
                AstarNode from = entrance.From;

                if (direction == 1 && (from.PositionInGrid.y == 0))
                {
                    return true;
                }
                else if (direction == -1 && (from.PositionInGrid.y == chunk.Columns - 1))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsDiagonalEntranceExists(int x, int y, int directionX, int directionY)
        {
            AstarGrid chunk = _chunks[x, y];
            HashSet<EntranceEdge> entrances = chunk.Entrances;
            foreach (EntranceEdge entrance in entrances)
            {
                AstarNode from = entrance.From;
                //Top Right
                if((directionX == -1 && directionY == 1) && (from.PositionInGrid.x == 0 && from.PositionInGrid.y == chunk.Columns - 1))
                {
                    return true;
                }
                //Bottom Right
                if((directionX == 1 && directionY == 1) && (from.PositionInGrid.x == chunk.Rows - 1 && from.PositionInGrid.y == chunk.Columns - 1))
                {
                    return false;
                }
                //Bottom Left
                if((directionX == -1 && directionY == -1) && (from.PositionInGrid.x == chunk.Rows - 1 && from.PositionInGrid.y == 0))
                {
                    return true;
                }
                //Top Left
                if((directionX == 1 && directionY == -1) && (from.PositionInGrid.x == 0 && from.PositionInGrid.y == 0))
                {
                    return true;
                }
            }

            return false;
        }
        private void Setup()
        {
            InitializeGrid();
            PopulateEntrances();
        }
        private int FindRandomWalkableNodeIndexY(AstarGrid chunk, AstarGrid other, int directionY)
        {
            int randomIndexY = 0;
            int maxTries = chunk.Columns;
            List<int> visitedIndices = new List<int>();
            switch (directionY)
            {
                //Top
                case 1:
                    randomIndexY = Random.Range(0, chunk.Columns);

                    while (!(chunk.Nodes[0, randomIndexY].Walkable &&
                           other.Nodes[other.Rows - 1, randomIndexY].Walkable) &&
                           maxTries > 0)
                    {
                        randomIndexY = Random.Range(0, chunk.Columns);
                        if(!visitedIndices.Contains(randomIndexY))
                        {
                            visitedIndices.Add(randomIndexY);
                            maxTries--;
                        }
                    }

                    if (maxTries <= 0)
                    {
                        randomIndexY = -1;
                    }
                    break;

                //Bottom
                case -1:
                    randomIndexY = Random.Range(0, chunk.Columns);
                    while (!(chunk.Nodes[chunk.Rows - 1, randomIndexY].Walkable
                           && other.Nodes[0, randomIndexY].Walkable)
                           && maxTries > 0)
                    {
                        randomIndexY = Random.Range(0, chunk.Columns);
                        if (!visitedIndices.Contains(randomIndexY))
                        {
                            visitedIndices.Add(randomIndexY);
                            maxTries--;
                        }
                    }

                    if (maxTries <= 0)
                    {
                        randomIndexY = -1;
                    }
                    break;
            }


            return randomIndexY;
        }

        private int FindRandomWalkableNodeIndexX(AstarGrid chunk, AstarGrid other, int directionX)
        {
            int randomIndexX = 0;
            int maxTries = chunk.Rows;
            List<int> visitedIndices = new List<int>();

            switch (directionX)
            {
                //Right
                case 1:
                    randomIndexX = Random.Range(0, chunk.Rows);

                    while (!(chunk.Nodes[randomIndexX, chunk.Columns - 1].Walkable &&
                           other.Nodes[randomIndexX, 0].Walkable) &&
                            maxTries > 0)
                    {
                        randomIndexX = Random.Range(0, chunk.Rows);
                        if(!visitedIndices.Contains(randomIndexX))
                        {
                            visitedIndices.Add(randomIndexX);
                            maxTries--;
                        }
                        
                    }

                    if (maxTries <= 0)
                    {
                        randomIndexX = -1;
                    }
                    break;
                //Left
                case -1:

                    randomIndexX = Random.Range(0, chunk.Rows);
                    while (!(chunk.Nodes[randomIndexX, 0].Walkable &&
                           other.Nodes[randomIndexX, other.Columns - 1].Walkable) && maxTries > 0)
                    {
                        randomIndexX = Random.Range(0, chunk.Rows);
                        maxTries--;
                    }
                    if (maxTries <= 0)
                    {
                        randomIndexX = -1;
                    }
                    break;
            }

            return randomIndexX;
        }
        
        private void GetChunkXY(Vector2 worldPosition, out int x, out int y)
        {
            Vector2 local = worldPosition - RealOrigin;
            float spacingX = EachChunkWidth *  NodeSize / 2.0f;
            float spacingY = -EachChunkHeight * NodeSize / 2.0f;

            x = Mathf.FloorToInt(local.x / spacingX);
            y = Mathf.FloorToInt(local.y / spacingY);
        }

        private bool IsChunkLeastWalkable(Vector2 position, 
                                          out int chunkX, 
                                          out int chunkY,
                                          out int inGridPosX,
                                          out int inGridPosY)
        {
            chunkX = -1;
            chunkY = -1;
            inGridPosX = -1;
            inGridPosY = -1;

            GetChunkXY(position, out chunkX, out chunkY);
            if (chunkX < 0 || chunkY < 0 || chunkX >= ChunkAmountX || chunkY >= ChunkAmountY)
            {    
                return false;
            }

            AstarGrid chunk = _chunks[chunkX, chunkY];
            chunk.GetXY(position, out inGridPosX, out inGridPosY);

            if (inGridPosX < 0 || inGridPosY < 0 ||
               inGridPosX >= chunk.Rows || inGridPosY >= chunk.Columns ||
               !chunk.Nodes[inGridPosX, inGridPosY].Walkable)
            {
                return false;
            }

            return true;
        }

        private List<EntranceEdge> RetracePath(EntranceEdge edge)
        {
            List<EntranceEdge> result = new List<EntranceEdge>();
            EntranceEdge currentEdge = edge;
            while(currentEdge != null)
            {
                result.Add(currentEdge);
                currentEdge = currentEdge.Parent;
            }

            result.Reverse();
            
            return result;
        }

        private void ClearChunkCostsAndEdgeParents()
        {
            for (int i = 0; i < ChunkAmountX; i++)
            {
                for(int j = 0; j < ChunkAmountY; j++)
                {
                    _chunks[i, j].GCost = 0;
                    _chunks[i, j].HCost = 0;
                    foreach(EntranceEdge edge in _chunks[i,j].Entrances)
                    {
                        edge.Parent = null;
                    }
                }
            }
        }

        private OuterPathfindingPartData GetLeastCostPart(List<OuterPathfindingPartData> openSet)
        {
            int leastCost = int.MaxValue;
            OuterPathfindingPartData leastCostPart = openSet[0];
            foreach(OuterPathfindingPartData part in openSet)
            {
                if(leastCost > part.Chunk.FCost || leastCost == part.Chunk.FCost)
                {
                    if(leastCost > part.Chunk.FCost)
                    {
                        leastCost = part.Chunk.FCost;
                    }
                    else
                    {
                        if(leastCost > part.Chunk.HCost)
                        {
                            leastCost = part.Chunk.HCost;
                        }
                    }

                    leastCostPart = part;
                }
            }

            return leastCostPart;
        }

        private int EuclideanHeuristic(AstarGrid chunkA, AstarGrid chunkB)
        {
            int dx = chunkB.PositionInChunksGrid.x - chunkA.PositionInChunksGrid.x;
            int dy = chunkB.PositionInChunksGrid.y - chunkA.PositionInChunksGrid.y;

            return DiagonalCost * Mathf.RoundToInt(Mathf.Sqrt(dx * dx + dy * dy));
        }
        
        private bool IsChunkExistsInOpenSet(List<OuterPathfindingPartData> openSet, AstarGrid chunkToFind)
        {
            return openSet.FindIndex((value) => value.Chunk == chunkToFind) != -1;
        }

        private bool IsChunkExistsInCloseSet(List<OuterPathfindingPartData> closeSet, AstarGrid chunkToFind)
        {
            return closeSet.FindIndex((value) => value.Chunk == chunkToFind) != -1;
        }
        
        private void SetWalkabilityPercentForChunks()
        {
            for (int i = 0; i < ChunkAmountX; i++)
            {
                for (int j = 0; j < ChunkAmountY; j++)
                {
                    _chunks[i, j].MinWalkabilityPercent = _minWalkabilityPercent;
                }
            }
        }
        #endregion
    }
}
