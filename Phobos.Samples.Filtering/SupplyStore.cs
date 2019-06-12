using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using static Phobos.Samples.Filtering.Messages;
using Akka.Actor;
using Akka.Event;

namespace Phobos.Samples.Filtering
{
    public class SupplyStore : ReceiveActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();
        public SupplyStore(int tin, int copper, int gold)
        {
            Gold = gold;
            Tin = tin;
            Copper = copper;
            Open();
        }

        public void Open()
        {
            Receive<CheckStatus>(_ =>
            {
                if (Copper < 10 || Gold < 10 || Tin < 10)
                    Sender.Tell(new StoreStatus(Copper, Gold, Tin, false));
                Sender.Tell(new StoreStatus(Copper, Gold, Tin, true));
            });
            Receive<Order>(o =>
            {

                if (Copper < o.Copper)
                {
                    _log.Info("Cannot complete Copper order, need to resupply.");
                    Sender.Tell(new ReplaceOrder(1, o.Copper));
                }
                else if (Gold < o.Gold)
                {
                    _log.Info("Cannot complete Gold order, need to resupply.");
                    Sender.Tell(new ReplaceOrder(1, o.Gold));
                }
                else if (Tin < o.Tin)
                {
                    _log.Info("Cannot complete Tin order, need to resupply.");
                    Sender.Tell(new ReplaceOrder(1, o.Tin));
                }
                else
                {
                    _log.Info("Order Fulfilled");
                    Copper -= o.Copper;
                    Gold -= o.Gold;
                    Tin -= o.Tin;
                }
            });
            Receive<GetSupply>(s =>
            {
                _log.Info("New Stock");
                switch (s.Id)
                {
                    case 1:
                        Copper += s.Quantity;
                        return;
                    case 2:
                        Gold += s.Quantity;
                        return;
                    case 3:
                        Tin += s.Quantity;
                        return;
                    default:
                        _log.Warning($"Supply id {s.Id} does not exist");
                        return;

                }

            });
        }

        public int Gold { get; set; }
        public int Tin { get; set; }
        public int Copper { get; set; }
        public double Revenue { get; set; }
    }
}
