// -----------------------------------------------------------------------
// <copyright file="PingGenerator.cs" company="Petabridge, LLC">
//      Copyright (C) 2015 - 2018 Petabridge, LLC <https://petabridge.com>
// </copyright>
// -----------------------------------------------------------------------

using System;
using Akka.Actor;
using Akka.Util;

namespace Phobos.Samples.ClusterPing
{
    public sealed class PingGenerator : ReceiveActor
    {
        private readonly IActorRef _localBroadcastActor;
        private int _pingCount;
        private ICancelable _pingGenerationTask;

        public PingGenerator(IActorRef localBroadcastActor)
        {
            _localBroadcastActor = localBroadcastActor;

            Receive<GeneratePing>(_ =>
            {
                var ping = new RandomPing("ping" + ++_pingCount, ThreadLocalRandom.Current.Next(1, 10));
                _localBroadcastActor.Tell(ping);
            });
        }

        protected override void PreStart()
        {
            _pingGenerationTask = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromSeconds(2.5),
                TimeSpan.FromSeconds(1), Self, GeneratePing.Instance, ActorRefs.NoSender);
        }

        protected override void PostStop()
        {
            _pingGenerationTask.Cancel();
        }

        /// <summary>
        ///     INTERNAL API.
        /// </summary>
        private sealed class GeneratePing
        {
            public static readonly GeneratePing Instance = new GeneratePing();

            private GeneratePing()
            {
            }
        }
    }
}