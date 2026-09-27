using System;
using UnityEngine;

namespace App.Pathfinding
{
    public class EntranceEdge : IEquatable<EntranceEdge>
    {

        public AstarGrid FromChunk { get; private set; }
        public AstarNode From { get; set; }

        public AstarGrid ToChunk { get; private set; }

        public AstarNode To { get; set; }

        public EntranceEdge(AstarGrid chunk, AstarNode from, AstarNode to, AstarGrid toChunk)
        {
            this.FromChunk = chunk;
            this.From = from;
            this.To = to;
            ToChunk = toChunk;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(FromChunk.GetHashCode(), From.GetHashCode(), To.GetHashCode(), ToChunk.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as EntranceEdge);
        }

        public bool Equals(EntranceEdge other)
        {
            if(other  == null)
            {
                return false;
            }

            return this.FromChunk == other.FromChunk && 
                   this.From == other.From && 
                   this.To == other.To &&
                   this.ToChunk == other.ToChunk;
        }
    }
}
