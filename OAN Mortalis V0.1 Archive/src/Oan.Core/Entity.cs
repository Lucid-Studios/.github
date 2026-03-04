using System;
using System.Collections.Generic;
using System.Numerics;

namespace Oan.Core
{
    public class Entity
    {
        public string Id { get; set; }
        public string Type { get; set; } // "Agent", "Prop", "Zone"
        public Vector3 Position { get; set; }
        public Dictionary<string, object> State { get; set; } = new Dictionary<string, object>();

        public Entity(string id, string type)
        {
            Id = id;
            Type = type;
        }
    }
}
