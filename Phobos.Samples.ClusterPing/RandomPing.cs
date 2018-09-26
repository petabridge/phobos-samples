// -----------------------------------------------------------------------
// <copyright file="RandomPing.cs" company="Petabridge, LLC">
//      Copyright (C) 2015 - 2018 Petabridge, LLC <https://petabridge.com>
// </copyright>
// -----------------------------------------------------------------------

namespace Phobos.Samples.ClusterPing
{
    public class RandomPing
    {
        public RandomPing(string message, int remainingPings, bool owned = true)
        {
            Message = message;
            RemainingPings = remainingPings;
            Owned = owned;
        }

        public int RemainingPings { get; }

        public bool HasPings => RemainingPings > 0;

        public bool Owned { get; }

        public string Message { get; }

        public RandomPing Next()
        {
            return new RandomPing(Message, RemainingPings - 1, false);
        }

        public RandomPing Reply()
        {
            return new RandomPing(Message, RemainingPings - 1, true);
        }

        public override string ToString()
        {
            return $"{Message} [{RemainingPings} remaining]";
        }
    }
}