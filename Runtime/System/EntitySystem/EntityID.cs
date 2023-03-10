using System.Runtime.InteropServices;
using System;

namespace Universe
{
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct EntityID : IComparable<EntityID>, IEquatable<EntityID>
    {
        [field: FieldOffset(0)]
        public uint Ident;

        [field: FieldOffset(4)]
        public uint Serial;

        [field: FieldOffset(0)]
        public long Value;

        const string FORMAT = "[{0}-{1}]";

        public static EntityID None = new(0, 0);
        public bool IsZero => Ident == 0 && Serial == 0;

        public EntityID(uint ident, uint serial) : this()
        {
            Ident = ident;
            Serial = serial;
        }

        public EntityID(long value) : this()
        {
            Ident = 0;
            Serial = 0;
            Value = value;
        }

        public static implicit operator long(EntityID obj)
        {
            return obj.Value;
        }

        public static bool operator ==(EntityID a, EntityID b)
        {
            return a.Value == b.Value;
        }

        public static bool operator !=(EntityID a, EntityID b)
        {
            return a.Value != b.Value;
        }

        public override string ToString()
        {

            return string.Format(FORMAT, Ident.ToString(), Serial.ToString());
        }

        public override bool Equals(object obj)
        {
            return obj != null && Value == ((EntityID)obj).Value;
        }

        public override int GetHashCode()
        {
            return (int)Serial;
        }

        int IComparable<EntityID>.CompareTo(EntityID other)
        {
            return Value.CompareTo(other.Value);
        }

        bool IEquatable<EntityID>.Equals(EntityID other)
        {
            return Value == other.Value;
        }
    }
}