using System;
using System.Collections.Generic;
using System.Text;
using Akka.Actor;
using Akka.Event;
using static Phobos.Samples.Filtering.Messages;

namespace Phobos.Samples.Filtering
{
    public class Manager : ReceiveActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();
        private readonly IActorRef _store;
        private ICancelable _interval;
        public Manager(IActorRef store)
        {
            _store = store;

            Receive<StoreStatus>(s =>
            {
                if (s.Stat)
                {
                    _log.Info("Supplies look good!");
                    _log.Info($"Copper:{s.Copper} Gold:{s.Gold} Tin:{s.Tin}");
                }
                else
                {
                    
                    _log.Warning("Supplies running low updating stock");
                    if (s.Copper < 10)
                        _store.Tell(new GetSupply(1, 50));
                    if (s.Gold < 10)
                        _store.Tell(new GetSupply(2, 50));
                    if (s.Tin < 10)
                        _store.Tell(new GetSupply(3, 50));
                }

            });
            Receive<CheckStatus>(_ =>
            {
                _log.Info("Checking Store Status");
                _store.Tell(CheckStatus.Instance);
            });
            Receive<ReplaceOrder>(r =>
            {
                switch (r.Id)
                {
                    case 1:
                        _store.Tell(new GetSupply(1, r.Id));
                        return;
                    case 2:
                        _store.Tell(new GetSupply(2, r.Id));
                        return;
                    case 3:
                        _store.Tell(new GetSupply(3, r.Id));
                        return;
                    default:
                        _log.Warning($"Supply id {r.Id} does not exist");
                        return;

                }
            });
        }

        protected override void PreStart()
        {
            _interval = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromSeconds(0),
                TimeSpan.FromSeconds(5), Self, CheckStatus.Instance, ActorRefs.NoSender);
        }
    }

    
}
