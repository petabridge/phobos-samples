using System;
using System.Collections.Generic;
using System.Text;

namespace Phobos.Samples.Filtering
{
    internal static class Messages
    {
        public sealed class CheckStatus
        {
            public static readonly CheckStatus Instance = new CheckStatus();

            private CheckStatus()
            {
            }
        }
        public sealed class PlaceOrder
        {
            public static readonly PlaceOrder Instance = new PlaceOrder();

            private PlaceOrder()
            {
            }
        }

        public sealed class GetSupply
        {
            public GetSupply(int id, int quantity)
            {
                Id = id;
                Quantity = quantity;
            }

            public int Id { get; }
            public int Quantity { get; }
            
        }
        public class StoreStatus 
        {
            public StoreStatus(int cooper, int gold, int tin, bool stat)
            {
                Copper = cooper;
                Gold = gold;
                Tin = tin;
                Stat = stat;
                Time = DateTime.UtcNow;
            }

            public bool Stat { get; }
            public int Tin { get; }
            public int Copper { get; }
            public int Gold { get; }
            public DateTime Time { get; }
        }

        public class ReplaceOrder : IStatus
        {
            public ReplaceOrder(int id, int qty)
            {
                Time = DateTime.UtcNow;
                Id = id;
                Quantity = qty;
            }

            public int Id { get;}
            public int Quantity { get; }
            public DateTime Time { get; }
        }
        public class Order
        {
            public Order(int cooper, int gold, int tin)
            {
                Copper = cooper;
                Gold = gold;
                Tin = tin;
                Time = DateTime.UtcNow;
            }

            public int Tin { get; }
            public int Copper { get; }
            public int Gold { get; }
            public DateTime Time { get; }
        }
    }
}
