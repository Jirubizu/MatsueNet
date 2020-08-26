using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace MatsueNet.Services
{
    public class RandomService : Random
    {
        private readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

        public override int Next(int minValue, int maxValue)
        {
            var bytes = new byte[4];
            _rng.GetBytes(bytes);
            var scale = BitConverter.ToUInt32(bytes, 0);

            return (int)(minValue + (maxValue - minValue) * (scale / (uint.MaxValue + 1.0)));
        }

        public override int Next(int max)
        {
            return Next(0, max);
        }

        public float NextFloat()
        {
            var bytes = new byte[4];
            _rng.GetBytes(bytes);
            return BitConverter.ToSingle(bytes, 0);
        }

        public T Pick<T>(T[] array)
        {
            return array[this.Next(0, array.Length - 1)];
        }

        public T Pick<T>(List<T> list)
        {
            return list[this.Next(0, list.Count - 1)];
        }

        public T Pick<T>(JArray array)
        {
            return array[this.Next(0, array.Count - 1)].Value<T>();
        }
        
        
        public bool Chance(int likelihood)
        {
            return this.Sample() * 100 < likelihood;
        }

        public int Dice(int sides)
        {
            return this.Next(1, sides);
        }
    }
}