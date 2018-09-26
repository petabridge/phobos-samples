// -----------------------------------------------------------------------
// <copyright file="MyActor.cs" company="Petabridge, LLC">
//      Copyright (C) 2015 - 2018 Petabridge, LLC <https://petabridge.com>
// </copyright>
// -----------------------------------------------------------------------

using Akka.Actor;
using Akka.Event;
using Akka.Util;

namespace Phobos.Samples.Basic
{
    public class MyActor : ReceiveActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();

        public MyActor()
        {
            Receive<MyMsg>(myMsg =>
            {
                _log.Info("Received {0}", myMsg.Msg);
                if (ThreadLocalRandom.Current.Next(0, 2) == 0)
                    Context.ActorSelection("/user/fake").Tell(myMsg.Msg);
            });
        }

        public class MyMsg
        {
            public MyMsg(string msg)
            {
                Msg = msg;
            }

            public string Msg { get; }
        }
    }
}