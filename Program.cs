using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cars
{
    class Program
    {
        static void Main(string[] args)
        {
            var cars = ProcessFile("fuel.csv");
            var query = cars.Where(c => c.Manufacturer == "BMW" && c.Year == 2016)
                            .OrderByDescending(c => c.Combined)
                            .ThenBy(c => c.Name)
                            .Select(c => new { c.Manufacturer, c.Name, c.Combined}); // select just these 3 properties

            // Alternative LINQ Query Syntax
            // var query = 
            //      from car in cars
            //      orderby car.Combined descending, car.Name ascending
            //      select car;

            foreach (var car in query.Take(10))
            {
                Console.WriteLine($"Car Name: {car.Name} - MPG {car.Combined}");
            }
            var topCar = cars.Where(c => c.Manufacturer == "BMW" && c.Year == 2016)
                            .OrderByDescending(c => c.Combined)
                            .ThenBy(c => c.Name)
                            .Select(c => c)
                            .FirstOrDefault();  // executes query, makes query concrete. might return null

            Console.WriteLine($"Top car: {topCar.Name}");

            // Quantifiers
            var result = cars.Any(c => c.Manufacturer == "Ford"); // Any Fords? (True)
            Console.WriteLine(result);

            result = cars.All(c => c.Manufacturer == "Ford"); // Are all Cars Fords? (False)
            Console.WriteLine(result);

            // Projections - "Select" transforms to anything
            // .Select(c => new { c.Manufacturer, c.Name, c.Combined}); // select just these 3 properties

            // SelectMany() flattens a sequence of sequences 
            // for example, each char name is a sequence of characters.
            var carName = cars.SelectMany(c => c.Name)
                              .OrderBy(c => c);
            foreach (var character in carName)
            {
                Console.Write(character);
            }
        }

        private static List<Car> ProcessFile(string path)
        {
            var query = File.ReadAllLines(path)
                        .Skip(1) // skip header
                        .Where(line => line.Length > 1) // skip last line
                        .ToCar();

            return query.ToList();

            // Alternative LINQ Query Syntax
            //var query = from line in File.ReadAllLines(path).Skip(1)
            //            where line.Length > 1
            //            select Car.ParseFromCSV(line);
            //return query.ToList();

        }


    }
}
