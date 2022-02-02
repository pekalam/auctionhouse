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

    public class ConcurrencyException : InfrastructureException
    {
        public ConcurrencyException(string message) : base(message)
        {
        }

        public ConcurrencyException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    /// <summary>
    /// Occurs when concurrency issues are detected when performing inserts. It can be for example an unique contraint violation due to message delivered twice by
    /// an event outbox. Commands should handle it to be idempotent. Occurence of this exception should not happen frequently.
    /// </summary>
    public class ConcurrentInsertException : InfrastructureException
    {
        public ConcurrentInsertException(string message) : base(message)
        {
        }

        public ConcurrentInsertException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
