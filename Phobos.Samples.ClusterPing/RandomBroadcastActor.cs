// -----------------------------------------------------------------------
// <copyright file="RandomBroadcastActor.cs" company="Petabridge, LLC">
//      Copyright (C) 2015 - 2018 Petabridge, LLC <https://petabridge.com>
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Linq;
using Akka.Actor;
using Akka.Event;
using Akka.Routing;
using Akka.Util;
using Phobos.Actor;

namespace Phobos.Samples.ClusterPing
{
    /// <summary>
    ///     Randomly broadcasts messages out to other member nodes in the cluster upon receiving a ping
    /// </summary>
    public sealed class RandomBroadcastActor : ReceiveActor
    {
        private readonly Akka.Cluster.Cluster _cluster = Akka.Cluster.Cluster.Get(Context.System);
        private readonly ILoggingAdapter _log = Context.GetLogger();
        private IActorRef _router;

        public RandomBroadcastActor()
        {
           Transmitting();
        }

        public void Transmitting()
        {
            Receive<RandomPing>(p =>
            {
                _log.Info("Received {0}", p);
                var next = p.Next();

                Context.GetInstrumentation().ActiveSpan
                    .SetTag("foo", "bar");

                using (var timing = Context.GetInstrumentation().Monitor.StartTiming("process-time"))
                {
                    foreach (var m in Enumerable.Range(0, ThreadLocalRandom.Current.Next(1, 4)))
                    {
                        _router.Tell(next);
                    }
                }
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
        }

        protected override void PreStart()
        {
            if (!Context.GetInstrumentation().InstrumentationSettings.MonitorMailbox)
            {
                _log.Error("No mailbox monitoring found.");
                _log.Error("Config: {0}", Context.GetInstrumentation().InstrumentationSettings);
                throw new ApplicationException("FATAL - MAILBOX MONITORING MUST BE ON");
            }

            _router = Context.ActorOf(Props.Empty.WithRouter(FromConfig.Instance), "router");
        }

        protected override void PostStop()
        {
            _cluster.Unsubscribe(Self);
        }
    }
}