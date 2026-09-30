using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace App.Pathfinding
{
    public class HierarchialPathfindingTest : MonoBehaviour
    {
        [SerializeField] private HierarchialPathfindingManager _pathfindingManager;
        [SerializeField] private Transform _start;
        [SerializeField] private Transform _end;
        [Min(0.1f)]
        [SerializeField] private float _moveSpeed = 10.0f;

        private List<Vector2> _path = null;
        private void Update()
        {
            if(Keyboard.current != null)
            {
                //For moving start position.
                
                if(Keyboard.current.wKey.isPressed)
                {
                    _start.position += Vector3.up * _moveSpeed * Time.deltaTime;
                }
                else if(Keyboard.current.sKey.isPressed)
                {
                    _start.position += -Vector3.up * _moveSpeed * Time.deltaTime;
                }

                if(Keyboard.current.dKey.isPressed)
                {
                    _start.position += Vector3.right * _moveSpeed * Time.deltaTime;
                }
                else if(Keyboard.current.aKey.isPressed)
                {
                    _start.position -= Vector3.right * _moveSpeed * Time.deltaTime;
                }

                //For moving end position

                if (Keyboard.current.upArrowKey.isPressed)
                {
                    _end.position += Vector3.up * _moveSpeed * Time.deltaTime;
                }
                else if (Keyboard.current.downArrowKey.isPressed)
                {
                    _end.position += -Vector3.up * _moveSpeed * Time.deltaTime;
                }

                if (Keyboard.current.rightArrowKey.isPressed)
                {
                    _end.position += Vector3.right * _moveSpeed * Time.deltaTime;
                }
                else if (Keyboard.current.leftArrowKey.isPressed)
                {
                    _end.position -= Vector3.right * _moveSpeed * Time.deltaTime;
                }

                if(Keyboard.current.spaceKey.wasReleasedThisFrame)
                {
                   _path = _pathfindingManager.FindPath(_start.position, _end.position);
                }
            }
        }




        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellowGreen;
            if(_path != null)
            {
                if(_path.Count == 2)
                {
                    Gizmos.DrawLine(_path[0], _path[1]);
                }
                else
                {
                    for (int i = 1; i < _path.Count - 1; i++)
                    {
                        Gizmos.DrawLine(_path[i - 1], _path[i]);
                        Gizmos.DrawLine(_path[i], _path[i + 1]);
                    }
                }
            }
        }
    }
}
