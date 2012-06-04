using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Knockout.Concurrency.Demo.Common;
using Knockout.Concurrency.Demo.Common.Extensions;

namespace Knockout.Concurrency.Demo.Events
{
    public interface IEventQueue<T> where T : class
    {
        void CreateSession(Guid sessionId, Predicate<T> predicate);
        IEnumerable<T> Dequeue(Guid sessionId);
        bool AliveSession(Guid sessionId);
    }

    public class EventQueue<T> : IEventQueue<T>, IHandle<Message<T>>, IHandle<IEnumerable<Message<T>>> where T : class
    {
        private readonly IEventAggregator eventAggregator;
        private readonly IConfig config;
        private readonly Dictionary<Guid, EventSession> sessions;
        private readonly TimeSpan watchdogTimeout;

        public EventQueue(IEventAggregator eventAggregator, IConfig config)
        {
            this.watchdogTimeout = config.LongtimePollingTimeout.Add(TimeSpan.FromMinutes(2));
            this.eventAggregator = eventAggregator;
            this.config = config;
            sessions = new Dictionary<Guid, EventSession>();
            eventAggregator.Subscribe(this);
        }

        public bool AliveSession(Guid sessionId)
        {
            return sessions.ContainsKey(sessionId);
        }

        public void CreateSession(Guid sessionId, Predicate<T> predicate)
        {
            lock (sessions)
            {
                var session = new EventSession
                {
                    Id = sessionId,
                    Watchdog = new Timer(SessionTimeOut, sessionId, watchdogTimeout, TimeSpan.FromMilliseconds(-1)),
                    Predicate = predicate,
                    MessageQueue = new Queue<Message<T>>()
                };

                sessions[sessionId] = session;
            }
        }

        private void SessionTimeOut(object state)
        {
            lock (sessions)
            {
                var id = (Guid)state;
                GetSession(id).Dispose();
                sessions.Remove(id);
            }
        }

        public IEnumerable<T> Dequeue(Guid sessionId)
        {
            lock (sessions)
            {
                var session = GetSession(sessionId);
                session.Watchdog.Change(watchdogTimeout, TimeSpan.FromMilliseconds(-1));

                var messages = new List<T>();
                while(session.MessageQueue.Count > 0)
                {
                    messages.Add(session.MessageQueue.Dequeue().Data);
                }
                
                return messages.Where(m => true);
            }
        }

        public void Handle(Message<T> message)
        {
            lock (sessions)
            {
                var affectedSessions = ListAffectedSessions(message);
                affectedSessions.ForEach(s => s.MessageQueue.Enqueue(message));

                var dequeueEvents = affectedSessions.Select(s => new DequeueEvent<T>(s.Id, this.Dequeue));
                if(dequeueEvents.Any())
                    eventAggregator.Publish(dequeueEvents);
            }
        }

        public void Handle(IEnumerable<Message<T>> messages)
        {
            lock (sessions)
            {
                var affectedSessions = new List<DequeueEvent<T>>();
                foreach(var message in messages)
                {
                    var targets = ListAffectedSessions(message);
                    targets.ForEach(t => t.MessageQueue.Enqueue(message));
                    targets
                        .Where(t => !affectedSessions.Any(s => t.Id == s.SessionId))
                        .Select(s => new DequeueEvent<T>(s.Id, this.Dequeue))
                        .ForEach(affectedSessions.Add);
                }

                eventAggregator.Publish(affectedSessions);
            }
        }

        private IEnumerable<EventSession> ListAffectedSessions(Message<T> message)
        {
            return sessions.Where(s => s.Value.Predicate(message.Data) && s.Value.Id != message.SessionId).Select(s => s.Value).ToList();
        }
        
        private class EventSession : IDisposable
        {
            public Guid Id { get; set; }
            public Timer Watchdog { get; set; }
            public Queue<Message<T>> MessageQueue { get; set; }
            public Predicate<T> Predicate { get; set; }

            public void Dispose()
            {
                Watchdog.Dispose();
            }
        }

        private EventSession GetSession(Guid sessionId)
        {
            if(!sessions.ContainsKey(sessionId))
                throw new Exception("Long time polling session not found in session repository");

            return sessions[sessionId];
        }
    }

    public class DequeueEvent<T> where T : class
    {
        private readonly Func<Guid, IEnumerable<T>> dequeue;
        public Guid SessionId { get; private set; }

        public DequeueEvent(Guid sessionId, Func<Guid, IEnumerable<T>> dequeue)
        {
            this.SessionId = sessionId;
            this.dequeue = dequeue;
        }

        public IEnumerable<T> Dequeue()
        {
            return dequeue(SessionId);
        }
    }
}
