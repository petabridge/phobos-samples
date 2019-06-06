using System;
using System.Collections.Generic;
using System.Text;
using static Phobos.Samples.Filtering.Messages;
using Akka.Actor;

namespace Phobos.Samples.Filtering
{
    public class Customer : ReceiveActor
    {
        private ICancelable _interval;
        private readonly IActorRef _store;
        private readonly IActorRef _manager;
        private readonly Random _rnd = new Random();

        public Customer(IActorRef store, IActorRef manager)
        {
            _store = store;
            _manager = manager;
            Ready();
        }
        public void Ready() { 

        Receive<PlaceOrder>(_ =>
            {
                _store.Tell(new Order(_rnd.Next(0,20), _rnd.Next(0, 20), _rnd.Next(0, 20)));
            });
        Receive<ReplaceOrder>(r => { _manager.Forward(r); });
        }
        protected override void PreStart()
        {
            _interval = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(1), Self, PlaceOrder.Instance, ActorRefs.NoSender);
        }
    }
}
