using System.Collections.Generic;
using UnityEngine;

namespace App.Pathfinding
{
    public class HierarchialPathfindingManager : MonoBehaviour
    {
        

        [Header("Chunk Settings:")]
        [Min(2)]
        [SerializeField] private int _chunkAmountX = 10;
        [Min(2)]
        [SerializeField] private int _chunkAmountY = 10;

        [Header("Each Chunk Settings:")]
        [SerializeField] private int _eachChunkWidth = 10;
        [SerializeField] private int _eachChunkHeight = 10;
        [Min(0.5f)]
        [SerializeField] private float _nodeSize = 2.0f;
        [SerializeField] private LayerMask _obstacleMask;
        [Range(0.0f, 1.0f)]
        [SerializeField] private float _fillSize = 0.5f;
        private AstarGrid[,] _chunks;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            InitializeGrid();
        }

        

        private void OnDrawGizmosSelected()
        {
            Vector2 realOrigin = new Vector2(transform.position.x, transform.position.y);

            bool isInPlayMode = Application.isPlaying && _chunks != null;
            for (int i = 0; i < _chunkAmountX * _chunkAmountY; i++)
            {
                int chunkX = i / _chunkAmountY;
                int chunkY = i % _chunkAmountY;

                Vector2 chunkOrigin = realOrigin;

                AstarGrid chunk = isInPlayMode ? _chunks[chunkX, chunkY] : null;
                if(!isInPlayMode)
                {
                    chunkOrigin += new Vector2(chunkX * _eachChunkHeight * _nodeSize / 2.0f, -chunkY * _eachChunkWidth * _nodeSize / 2.0f);
                }
                else
                {
                    chunkOrigin += _chunks[chunkX, chunkY].Origin;
                }

                for (int j = 0; j < _eachChunkWidth * _eachChunkHeight; j++)
                {
                    int eachChunkX = j / _eachChunkHeight;
                    int eachChunkY = j % _eachChunkHeight;

                    Vector2 origin = chunkOrigin;
                    if(isInPlayMode)
                    {
                        origin += _chunks[chunkX, chunkY].GetWorldPositionCentre(eachChunkX, eachChunkY);
                    }
                    else
                    {
                        origin += new Vector2(eachChunkX * _nodeSize/2.0f, -eachChunkY * _nodeSize/2.0f);
                        //origin += new Vector2(_nodeSize/4.0f, -_nodeSize/4.0f);
                    }
                    
                    if(!isInPlayMode)
                    {
                        DrawSquare(Color.white, origin, _nodeSize, _fillSize);
                    }
                    else
                    {
                        DrawSquare(chunk.Nodes[eachChunkX, eachChunkY].Walkable ? Color.green : Color.white, origin, _nodeSize, _fillSize);
                    }
                }
            }
        }

        private void OnValidate()
        {
            if(_chunks != null)
            {
                for(int i = 0; i < _chunks.GetLength(0);  i++)
                {
                    for(int j = 0; j < _chunks.GetLength(1);  j++)
                    {
                        _chunks[i,j].NodeSize = _nodeSize;
                    }
                }
            }
        }

        private void InitializeGrid()
        {
            _chunks = new AstarGrid[_chunkAmountX, _chunkAmountY];
            Vector2 realOrigin = new Vector2(transform.position.x, transform.position.y);

            for (int i = 0; i < _chunks.GetLength(0); i++)
            {
                for (int j = 0; j < _chunks.GetLength(1); j++)
                {
                    Vector2 origin = realOrigin + new Vector2(i * _eachChunkHeight * _nodeSize / 4.0f, -j * _eachChunkWidth * _nodeSize / 4.0f);
                    _chunks[i, j] = new AstarGrid(origin, _eachChunkWidth, _eachChunkHeight, _nodeSize, _obstacleMask);
                }
            }
        }

        private void DrawSquare(Color color, Vector2 origin, float size, float scale = 1.0f)
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
    }
}
