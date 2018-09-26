// -----------------------------------------------------------------------
// <copyright file="MyParent.cs" company="Petabridge, LLC">
//      Copyright (C) 2015 - 2018 Petabridge, LLC <https://petabridge.com>
// </copyright>
// -----------------------------------------------------------------------

using System;
using Akka.Actor;

namespace Phobos.Samples.Basic
{
    public class MyParent : ReceiveActor
    {
        public MyParent()
        {
            Receive<string>(str =>
            {
                var child = Context.Child(Uri.EscapeUriString(str));
                if (child.IsNobody())
                    child = Context.ActorOf(Props.Create(() => new MyActor()), Uri.EscapeUriString(str));
                child.Tell(new MyActor.MyMsg(str));
            });
        }
    }
}