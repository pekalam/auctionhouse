using Core.Common.Domain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Core.DomainFramework
{
    public class GuidId<T> : ValueObject where T : GuidId<T> // used CRTP to prevent unintended implicit reference conversions when passed as param
    {
        public Guid Value { get; }

        public GuidId(Guid value)
        {
            Debug.Assert(typeof(T).AssemblyQualifiedName == GetType().AssemblyQualifiedName);
            Value = value;
        }
        //TODO remove implicit operators for beter safety - avoid unintended invalid comparsion when using == operator
        public static implicit operator Guid(GuidId<T> id) => id.Value;
        // not defininng impliicit Guid->GuidId by purpose. User should use factory method
        public override string ToString() => Value.ToString();

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
