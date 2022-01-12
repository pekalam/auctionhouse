using System;

namespace Core.DomainFramework
{

    [Serializable]
    public class InfrastructureException : Exception
    {
        public InfrastructureException() { }
        public InfrastructureException(string message) : base(message) { }
        public InfrastructureException(string message, Exception inner) : base(message, inner) { }
        protected InfrastructureException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
