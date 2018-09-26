// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Petabridge, LLC">
//      Copyright (C) 2015 - 2018 Petabridge, LLC <https://petabridge.com>
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.IO;
using Akka.Actor;
using Akka.Configuration;

namespace Phobos.Samples.ClusterPing
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var configuration = ConfigurationFactory.ParseString(File.ReadAllText("demo.conf"));
            var ports = new[] {2551, 2552, 0, 0};
            foreach (var port in ports)
                StartActorSystem(configuration, port);

            Console.WriteLine("Press enter to exit");
            var line = Console.ReadLine();
        }

        public static void StartActorSystem(Config baseConfig, int port)
        {
            //Override the configuration of the port
            var config =
                ConfigurationFactory.ParseString("akka.remote.dot-netty.tcp.port=" + port)
                    .WithFallback(baseConfig);

            //create an Akka system
            var system = ActorSystem.Create("phobosdemo", config);

            var broadcastActor = system.ActorOf(Props.Create(() => new RandomBroadcastActor()), "random");
            var pinger = system.ActorOf(Props.Create(() => new PingGenerator(broadcastActor)));
        }
    }
}