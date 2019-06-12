using System;
using System.IO;
using Akka.Actor;
using Akka.Configuration;

namespace Phobos.Samples.Filtering
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = ConfigurationFactory.ParseString(File.ReadAllText("demo.conf"));
            var actorSystem = ActorSystem.Create("MySys", configuration);
            var iZStore = actorSystem.ActorOf(Props.Create(() => new SupplyStore(100, 100, 100)),"store");
            var fuber = actorSystem.ActorOf(Props.Create(() => new Manager(iZStore)), "manager");
            var orion = actorSystem.ActorOf(Props.Create(() => new Customer(iZStore,fuber)), "customer");
           
            
            actorSystem.WhenTerminated.Wait();
        }
    }
}
