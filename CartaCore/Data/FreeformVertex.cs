using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace CartaCore.Data
{
    public class FreeformVertex : IComparable<FreeformVertex>, IEquatable<FreeformVertex>
    {
        public Guid Id { get; set; }
        public IDictionary<string, FreeformVertexProperty> Properties { get; set; }

        public int CompareTo([AllowNull] FreeformVertex other)
        {
            if (other is null)
                return 1;
            return Id.CompareTo(other.Id);
        }
        public bool Equals([AllowNull] FreeformVertex other)
        {
            if (other is null)
                return false;
            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(FreeformVertex lhs, FreeformVertex rhs)
        {
            return lhs.Equals(rhs);
        }
        public static bool operator !=(FreeformVertex lhs, FreeformVertex rhs)
        {
            return !lhs.Equals(rhs);
        }
    }
}