using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
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

        private HierarchialPathfinding _hierarchialPathfinding;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _hierarchialPathfinding = new HierarchialPathfinding(transform.position, 
                                                                 _chunkAmountX,
                                                                 _chunkAmountY, 
                                                                 _eachChunkWidth, 
                                                                 _eachChunkHeight, 
                                                                 _nodeSize, 
                                                                 _fillSize, 
                                                                 _obstacleMask);
        }

        private void OnDrawGizmosSelected()
        {
            if (_hierarchialPathfinding != null)
            {
                bool isInPlayMode = Application.isPlaying && _hierarchialPathfinding.Chunks != null;
                _hierarchialPathfinding.OnDrawGizmosSelected(isInPlayMode);
            }
        }

        private void OnValidate()
        {
            if(_hierarchialPathfinding != null)
            {
                for(int i = 0; i < _hierarchialPathfinding.ChunkAmountX;  i++)
                {
                    for(int j = 0; j < _hierarchialPathfinding.ChunkAmountY;  j++)
                    {
                        _hierarchialPathfinding.Chunks[i,j].NodeSize = _nodeSize;
                        _hierarchialPathfinding.NodeSize = _nodeSize;
                        _hierarchialPathfinding.FillSize = _fillSize;
                        _hierarchialPathfinding.ObstacleLayerMask = _obstacleMask;

                    }
                }
            }
        }
        
    }
}
