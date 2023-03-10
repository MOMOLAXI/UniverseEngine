using System.Collections.Generic;

namespace Universe
{
    internal static class EntityIDGenerator
    {
        static readonly Dictionary<uint, uint> s_TypeSerials = new();

        public static EntityID Get(uint type)
        {
            EntityID id;
            if (s_TypeSerials.TryGetValue(type, out uint serial))
            {
                serial++;
                id = new(type, serial);
            }
            else
            {
                serial = 0;
                id = new(type, serial);
            }

            s_TypeSerials[type] = serial;

            return id;
        }
    }
}