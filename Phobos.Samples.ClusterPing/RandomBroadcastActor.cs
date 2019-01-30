// -----------------------------------------------------------------------
// <copyright file="RandomBroadcastActor.cs" company="Petabridge, LLC">
//      Copyright (C) 2015 - 2018 Petabridge, LLC <https://petabridge.com>
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster;
using Akka.Event;
using Akka.Util;
using Phobos.Actor;

namespace Phobos.Samples.ClusterPing
{
    /// <summary>
    ///     Randomly broadcasts messages out to other member nodes in the cluster upon receiving a ping
    /// </summary>
    public sealed class RandomBroadcastActor : ReceiveActor, IWithUnboundedStash
    {
        private readonly Akka.Cluster.Cluster _cluster = Akka.Cluster.Cluster.Get(Context.System);
        private readonly ILoggingAdapter _log = Context.GetLogger();
        private readonly HashSet<IActorRef> _neighbors = new HashSet<IActorRef>();

        public RandomBroadcastActor()
        {
            WaitingClusterUp();
        }

        public IStash Stash { get; set; }

        public void WaitingClusterUp()
        {
            ReceiveAsync<ClusterEvent.MemberWeaklyUp>(async m =>
            {
                await AddMember(m.Member);
                BecomeTransmitting();
            }, m => !m.Member.Address.Equals(_cluster.SelfAddress));
            ReceiveAsync<ClusterEvent.MemberUp>(async m =>
            {
                await AddMember(m.Member);
                BecomeTransmitting();
            }, m => !m.Member.Address.Equals(_cluster.SelfAddress));
            Receive<ClusterEvent.IMemberEvent>(_ => { }); // ignore
            Receive<Terminated>(t => { _neighbors.Remove(t.ActorRef); });
            ReceiveAny(_ => Stash.Stash());
        }

        private async Task AddMember(Member m)
        {
            var tries = 2;
            while (tries > 0)
                try
                {
                    var remoteRef = await Context.ActorSelection(m.Address + "/user/random")
                        .ResolveOne(TimeSpan.FromSeconds(3));

                    _neighbors.Add(remoteRef);
                    Context.Watch(remoteRef);
                    return;
                }
                catch (Exception ex)
                {
                    Context.GetInstrumentation().ActiveSpan?
                        .Log($"Failed to resolve path for random actor on node {m.Address}")
                        .Log($"Exception: {ex}");
                    --tries;
                }
        }

        private void BecomeTransmitting()
        {
            Become(Transmitting);
            Stash.UnstashAll();
        }

        public void Transmitting()
        {
            Receive<RandomPing>(p =>
            {
                _log.Info("Received {0}", p);
                var next = p.Next();
                var start = DateTime.UtcNow;

                Context.GetInstrumentation().ActiveSpan
                    .SetTag("foo", "bar");

                var timing = Context.GetInstrumentation().Monitor.CreateTiming("process-time");

                foreach (var m in _neighbors)
                {
                    var value = ThreadLocalRandom.Current.Next(1, 10);
                    if (value < 8)
                        m.Tell(next);
                    else // deadletter
                        Context.ActorSelection("/user/fakeactor").Tell(next);
                }

                var stop = DateTime.UtcNow;
                timing.Record((long) (stop - start).TotalMilliseconds);
            }, p => p.HasPings && p.Owned);

            Receive<RandomPing>(p =>
            {
                _log.Info("Received {0}", p);
                var next = p.Reply();
                Context.GetInstrumentation().ActiveSpan
                    .Log($"Attempting to send ping [{p.Message}] with {next.RemainingPings} remaining hops")
                    .Log(p.Message);

                Sender.Tell(next);
            }, p => p.HasPings);

            Receive<RandomPing>(p =>
            {
                _log.Info("Received {0}", p);
                // time to terminate this ping
                Context.GetInstrumentation().ActiveSpan.Log("ping terminated");
            });

            ReceiveAsync<ClusterEvent.MemberWeaklyUp>(async m =>
            {
                await AddMember(m.Member);
            }, m => !m.Member.Address.Equals(_cluster.SelfAddress));

            ReceiveAsync<ClusterEvent.MemberUp>(async m =>
            {
                await AddMember(m.Member);
            }, m => !m.Member.Address.Equals(_cluster.SelfAddress));
            
            Receive<ClusterEvent.IMemberEvent>(_ => { }); // ignore
            Receive<Terminated>(t => { _neighbors.Remove(t.ActorRef); });
        }

        protected override void PreStart()
        {
            if (!Context.GetInstrumentation().InstrumentationSettings.MonitorMailbox)
            {
                _log.Error("No mailbox monitoring found.");
                _log.Error("Config: {0}", Context.GetInstrumentation().InstrumentationSettings);
                throw new ApplicationException("FATAL - MAILBOX MONITORING MUST BE ON");
            }

            _cluster.Subscribe(Self, ClusterEvent.SubscriptionInitialStateMode.InitialStateAsEvents,
                typeof(ClusterEvent.IMemberEvent));
        }

        protected override void PostStop()
        {
            _cluster.Unsubscribe(Self);
        }
    }
}