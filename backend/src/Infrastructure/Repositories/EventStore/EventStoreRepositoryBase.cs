using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Common.Domain;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace Infrastructure.Repositories.EventStore
{
    public abstract class EventStoreRepositoryBase
    {
        protected ESConnectionContext _connectionContext;

        public EventStoreRepositoryBase(ESConnectionContext connectionContext)
        {
            _connectionContext = connectionContext;
        }

        protected abstract string GetStreamId(Guid entityId);

        protected void AppendPendingEventsToStream(IReadOnlyCollection<Event> eventsToAppend, long expectedVersion, Guid entityId)
        {
            var events = new List<EventData>();

            foreach (var @event in eventsToAppend)
            {
                var json = JsonConvert.SerializeObject(@event, new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All,
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc
                });
                var bytes = Encoding.UTF8.GetBytes(json);
                var eventData = new EventData(Guid.NewGuid(), @event.EventName, true, bytes, null);
                events.Add(eventData);
            }

            var writeResult = _connectionContext.Connection.AppendToStreamAsync(GetStreamId(entityId),
                expectedVersion, events, null).Result;
        }

        protected IReadOnlyCollection<Event> ReadEvents(Guid entityId, long lastEventVersion = -2)
        {
            var streamEvents = new List<ResolvedEvent>();

            StreamEventsSlice currentSlice;
            var nextSliceStart = StreamPosition.Start;
            do
            {
                currentSlice =
                    _connectionContext.Connection.ReadStreamEventsForwardAsync(GetStreamId(entityId), nextSliceStart,
                            200, false)
                        .Result;
                if (currentSlice.Status == SliceReadStatus.StreamDeleted || currentSlice.Status == SliceReadStatus.StreamNotFound)
                {
                    return null;
                }
                nextSliceStart = (int)currentSlice.NextEventNumber;

                if (lastEventVersion != -2)
                {
                    var resolved = currentSlice.Events.Where(e => e.OriginalEventNumber <= lastEventVersion);
                    streamEvents.AddRange(resolved);
                    if (resolved.Count() != currentSlice.Events.Length)
                    {
                        break;
                    }
                }
                else
                {
                    streamEvents.AddRange(currentSlice.Events);

                }
            } while (!currentSlice.IsEndOfStream);
            
            var eventList = streamEvents.Select(e =>
            {
                var json = Encoding.UTF8.GetString(e.Event.Data);
                return JsonConvert.DeserializeObject<Event>(json, new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All,
                    FloatParseHandling = FloatParseHandling.Decimal
                });
            }).ToList();

            return eventList;
        }
    }
}