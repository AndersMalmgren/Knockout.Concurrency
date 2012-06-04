using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Knockout.Concurrency.Demo.Common;
using Knockout.Concurrency.Demo.Common.Extensions;
using Knockout.Concurrency.Demo.Events;
using Knockout.Concurrency.Demo.Models;

namespace Knockout.Concurrency.Demo.Controllers
{
    public class PushController : SessionController, IHandle<IEnumerable<DequeueEvent<Dog>>>
    {
        private readonly IEventAggregator eventAggregator;
        private readonly IEventQueue<Dog> dogEventQueue;

        public PushController(IEventAggregator eventAggregator, IEventQueue<Dog> dogEventQueue, IConfig config)
        {
            this.eventAggregator = eventAggregator;
            this.dogEventQueue = dogEventQueue;
            AsyncManager.Timeout = (int)config.LongtimePollingTimeout.TotalMilliseconds;
        }

        public void DogSavedAsync(int id, Guid sessionId)
        {
            this.SessionId = sessionId;


            if (!dogEventQueue.AliveSession(sessionId))
                dogEventQueue.CreateSession(sessionId, m => m.Id == id);
            
            AsyncManager.OutstandingOperations.Increment();
            
            var messages = dogEventQueue.Dequeue(SessionId);
            if (messages.Any())
            {
                SetMessages(messages);
            }
            else
            {
                eventAggregator.Subscribe(this);
            }
        }
        
        public ActionResult DogSavedCompleted(IEnumerable<Dog> messages)
        {
            return new
            {
                SessionId,
                Data = messages
            }.AsJson();
        }

        protected override void EndExecute(IAsyncResult asyncResult)
        {
            eventAggregator.Unsubscribe(this);

            base.EndExecute(asyncResult);
        }

        private void SetMessages(IEnumerable<Dog> messages)
        {
            AsyncManager.Parameters["messages"] = messages;
            AsyncManager.OutstandingOperations.Decrement();
        }

        public void Handle(IEnumerable<DequeueEvent<Dog>> message)
        {
            var dequeueEvent = message.SingleOrDefault(de => de.SessionId == SessionId);
            if (dequeueEvent != null)
            {
                eventAggregator.Unsubscribe(this);

                var relevantMessages = dequeueEvent.Dequeue();
                if (relevantMessages.Any())
                    SetMessages(relevantMessages);
            }
        }
    }
}
