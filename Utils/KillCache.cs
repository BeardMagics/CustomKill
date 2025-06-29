using System.Collections.Generic;
using Unity.Entities;
using CustomKill;

/* 
  Entire file has been modified to simply create the downer map and logic
  This is a more simplified approach in consideration of performance, logic, and clarity
  It also removes verbose or duplicate checking and assist logic that was breaking core systems
  Now should lock downer and allocate credit to original last hit attacker so when a kill takes place
  The person who got the "down" is now credited as the killer, while all others are added to assists
  through the PlayerHitStore and VampireDownedPatch systems
 */

namespace CustomKill.Utils
{
    public static class KillCache
    {
        private static readonly Dictionary<Entity, Entity> DownerMap = new();

        public static void SetDowner(Entity victim, Entity downer)
        {
            DownerMap[victim] = downer;
        }

        public static Entity GetDowner(Entity victim)
        {
            return DownerMap.TryGetValue(victim, out var downer) ? downer : Entity.Null;
        }

        public static void Clear(Entity victim)
        {
            DownerMap.Remove(victim);
        }
    }
}
