using System;
using UnityEngine;

namespace App.Pathfinding
{
    public class OuterPathfindingPartData : IEquatable<OuterPathfindingPartData>
    {
        public AstarGrid Chunk { get; private set; }
        public EntranceEdge Edge { get; private set; }

        public OuterPathfindingPartData(AstarGrid chunk, EntranceEdge edge)
        {
            Chunk = chunk;
            Edge = edge;
        }

        public bool Equals(OuterPathfindingPartData other)
        {
            if(other == null) return false;

            return Chunk.Equals(other.Chunk) && Edge.Equals(other.Edge);
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();

            if (Chunk != null)
            {
                hash.Add(Chunk.GetHashCode());
            }

            if(Edge != null)
            {
                hash.Add(Edge.GetHashCode());
            }

            return hash.ToHashCode();
        }
    }
}
