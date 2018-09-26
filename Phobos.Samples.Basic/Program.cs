// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Petabridge, LLC">
//      Copyright (C) 2015 - 2018 Petabridge, LLC <https://petabridge.com>
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.IO;
using Akka.Actor;
using Akka.Configuration;

namespace Phobos.Samples.Basic
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var configuration = ConfigurationFactory.ParseString(File.ReadAllText("demo.conf"));
            var actorSystem = ActorSystem.Create("MySys", configuration);


            var myActor = actorSystem.ActorOf(Props.Create(() => new MyParent()), "recv");
            Console.WriteLine("Type any words to begin.");
            Console.WriteLine("Type /exit to quit at any time.");
            var line = Console.ReadLine();
            while (!string.IsNullOrEmpty(line) && !line.Equals("/exit"))
            {
                myActor.Tell(line);
                line = Console.ReadLine();
            }

            CoordinatedShutdown.Get(actorSystem).Run().Wait(TimeSpan.FromSeconds(5));
        }
    }
}