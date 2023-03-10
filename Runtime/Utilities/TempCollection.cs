using System.Collections.Generic;

namespace Universe
{
    public static class TempList<T>
    {
        static readonly List<T> s_Value = new();

        public static List<T> Value
        {
            get
            {
                s_Value.Clear();
                return s_Value;
            }
        }
    }
}