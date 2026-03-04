using System.Collections.Generic;
using Oan.Core;

namespace Oan.Runtime
{
    public class WorldState
    {
        public Dictionary<string, Entity> Entities { get; private set; } = new Dictionary<string, Entity>();
        public long Tick { get; private set; } = 0;

        public void AddEntity(Entity entity)
        {
            if (!Entities.ContainsKey(entity.Id))
            {
                Entities[entity.Id] = entity;
            }
        }

        public Entity? GetEntity(string id)
        {
            if (Entities.TryGetValue(id, out var entity))
            {
                return entity;
            }
            return null;
        }

        public void UpdateEntityState(string id, string key, object value)
        {
            if (Entities.TryGetValue(id, out var entity))
            {
                entity.State[key] = value;
            }
        }
        
        public void IncrementTick()
        {
            Tick++;
        }
    }
}
