namespace Core.Common.Domain
{
    public interface IInternalEventAdd
    {
        void AddEvent(Event @event);
    }

    /// <summary>
    /// Sublcass of <see cref="AggregateRoot{T}"/> can implement this interface to handle events sent from entities.
    /// </summary>
    public interface IInternalEventApply
    {
        void Apply(Event @event);
    }

    /// <summary>
    /// Temporary
    /// </summary>
    /// <typeparam name="TId"></typeparam>
    public class SimpleEntity<TId> where TId : ValueObject
    {
        public TId Id { get; protected set; }
    }

    public abstract class Entity<TId>
        where TId : ValueObject
    {
        private IInternalEventAdd _parentAdd;
        private IInternalEventApply _parentApply;


        /// <param name="parentAdd">In general usecase use <see cref="AddEvent(Event)"/>. Entity can use it to add event to parent ONLY if its not either applying event.</param>
        /// <param name="parentApply">In general usecase use <see cref="AddEvent(Event)"/>. Can use it to apply event to parent. Usage of this param should be consistent in all constructors.</param>
        protected Entity(IInternalEventAdd parentAdd, IInternalEventApply parentApply = null)
        {
            _parentAdd = parentAdd;
            _parentApply = parentApply;
        }

        public TId Id { get; protected set; }

        /// <summary>
        /// Called by entity when changing its state in method.
        /// </summary>
        /// <param name="event"></param>
        protected void AddEvent(Event @event)
        {
            _parentAdd.AddEvent(@event);
            _parentApply?.Apply(@event);
        }

        /// <summary>
        /// Called by parent when recreating entity state
        /// </summary>
        /// <param name="event"></param>
        internal void ApplyInternal(Event @event)
        {
            Apply(@event);
            _parentApply?.Apply(@event);
        }

        protected abstract void Apply(Event @event);
    }
}