using System;

namespace Cars
{
    // Accumulator class to hold stats for a car
    public class CarStats
    {
        public int Max { get; set; }
        public int Min { get; set; }
        public double Average { get; set; }
        public int Total { get; set; }
        public int Count { get; set; }

        public CarStats()
        {
            Max = Int32.MinValue;
            Min = Int32.MaxValue;
        }

        public CarStats Accumulate(Car car)
        {
            Total += car.Combined;
            Count += 1;
            Max = Math.Max(Max, car.Combined);
            Min = Math.Min(Min, car.Combined);
            return this;
        }

        public CarStats Compute()
        {
            Average = Total / Count;
            return this;
        }
    }
}
