﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Cars
{
    class Program
    {
        static void Main(string[] args)
        {
            var cars = ProcessCars("fuel.csv");
            var manufacturers = ProcessManufacturers("manufacturers.csv");

            var query = cars.Where(c => c.Manufacturer == "BMW" && c.Year == 2016)
                            .OrderByDescending(c => c.Combined)
                            .ThenBy(c => c.Name)
                            .Select(c => new { c.Manufacturer, c.Name, c.Combined }); // select just these 3 properties

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

            // Joins with Query Syntax
            var query_FuelEfficientManufacturers_QuerySyntax =
                from car in cars
                join manufacturer in manufacturers on car.Manufacturer equals manufacturer.Name
                orderby car.Combined descending, car.Name ascending
                select new
                {
                    manufacturer.Headquarters,
                    car.Name,
                    car.Combined
                };
            foreach (var car in query_FuelEfficientManufacturers_QuerySyntax.Take(10))
            {
                Console.WriteLine($"HQ: {car.Headquarters} - CarName: {car.Name}  - MPG: {car.Combined}");
            }

            Console.WriteLine("***");
            // Joins with Method Syntax
            var query_FuelEfficientManufacturers_MethodSyntax =
                cars.Join(manufacturers, c => c.Manufacturer,
                                        m => m.Name, (c, m) => new  // project fields we need
                                        {
                                            m.Headquarters,
                                            c.Name,
                                            c.Combined
                                        })
                    .OrderByDescending(c => c.Combined)
                    .ThenBy(c => c.Name);
            foreach (var car in query_FuelEfficientManufacturers_MethodSyntax.Take(10))
            {
                Console.WriteLine($"HQ: {car.Headquarters} - CarName: {car.Name}  - MPG: {car.Combined}");
            }

            // Joins with a composite key requires we build an object.
            // Query Syntax approach

            Console.WriteLine("Joining with a composite key (query syntax)");
            var query_FuelEfficientManufacturers_CompositeKey_QuerySyntax =
                from car in cars
                join manufacturer in manufacturers
                    on new { car.Manufacturer, car.Year }
                        equals
                        new { Manufacturer = manufacturer.Name, manufacturer.Year }
                orderby car.Combined descending, car.Name ascending
                select new
                {
                    manufacturer.Headquarters,
                    car.Name,
                    car.Combined
                };
            foreach (var car in query_FuelEfficientManufacturers_CompositeKey_QuerySyntax.Take(10))
            {
                Console.WriteLine($"HQ: {car.Headquarters} - CarName: {car.Name}  - MPG: {car.Combined}");
            }

            Console.WriteLine("Joins with composite key  (Method Syntax)");
            // Joins with Method Syntax
            var query_FuelEfficientManufacturers_CompositeKey_MethodSyntax =
                cars.Join(manufacturers, c => new { c.Manufacturer, c.Year },
                                        m => new { Manufacturer = m.Name, m.Year }, (c, m) => new  // project fields we need
                                        {
                                            m.Headquarters,
                                            c.Name,
                                            c.Combined
                                        })
                    .OrderByDescending(c => c.Combined)
                    .ThenBy(c => c.Name);
            foreach (var car in query_FuelEfficientManufacturers_CompositeKey_MethodSyntax.Take(10))
            {
                Console.WriteLine($"HQ: {car.Headquarters} - CarName: {car.Name}  - MPG: {car.Combined}");
            }

            Console.WriteLine("***");
            FindTwoMostFuelEfficientCarsByManufacturer("Fuel.csv");

            Console.WriteLine("***"); 
            GroupJoinQuery("Fuel.csv");

            Console.WriteLine("***");
            GroupByCountryShowTopThreeCars("Fuel.csv", "Manufacturers.csv");

            Console.WriteLine("***");
            GetManufacturerStatistics("Fuel.csv", "Manufacturers.csv");
        }

        private static void FindTwoMostFuelEfficientCarsByManufacturer(string path)
        {
            // Find the 2 most fuel efficient cars for each Manufacturer

            // Use LINQ Query Syntax
            var cars = ProcessCars("fuel.csv");
            var query_GroupByManufacturerQuerySyntax =
                from car in cars
                group car by car.Manufacturer.ToUpper() into manufacturer
                orderby manufacturer.Key
                select manufacturer;

            foreach (var group in query_GroupByManufacturerQuerySyntax)
            {
                Console.WriteLine($"\n {group.Key}");
                foreach (var car in group.OrderByDescending(c => c.Combined).Take(2))
                {
                    Console.WriteLine($"\t{car.Name} has a combined fuel efficiency of {car.Combined} MPG");
                }
            }

            // Use LINQ Method Syntax
            var query_GroupByManufacturerMethodSyntax =
                cars.GroupBy(c => c.Manufacturer.ToUpper())
                    .OrderBy(g => g.Key);

            foreach (var group in query_GroupByManufacturerMethodSyntax)
            {
                Console.WriteLine($"\n {group.Key}");
                foreach (var car in group.OrderByDescending(c => c.Combined).Take(2))
                {
                    Console.WriteLine($"\t{car.Name} has a combined fuel efficiency of {car.Combined} MPG");
                }
            }


        }

        private static void GroupJoinQuery(string path)
        {
            var cars = ProcessCars("fuel.csv");
            var manufacturers = ProcessManufacturers("manufacturers.csv");
            
            // Use LINQ Query Syntax
            var queryGroupJoinQuerySyntax =
                from manufacturer in manufacturers
                join car in cars on manufacturer.Name equals car.Manufacturer
                into carGroup  // stores the grouping
                orderby manufacturer.Name
                select new // project these objects
                {
                    Manufacturer = manufacturer,
                    Cars = carGroup
                };
            foreach (var group in queryGroupJoinQuerySyntax)
            {
                Console.WriteLine($"\n {group.Manufacturer.Name}:{group.Manufacturer.Headquarters}");
                foreach (var car in group.Cars.OrderByDescending(c => c.Combined).Take(2))
                {
                    Console.WriteLine($"\t{car.Name} has a combined fuel efficiency of {car.Combined} MPG");
                }
            }

            // Use LINQ Method Syntax
            var queryGroupJoinMethodSyntax =
                manufacturers.GroupJoin(cars, m => m.Name, c => c.Manufacturer, (m, g) => new
                    {
                        Manufacturer = m,
                        Cars = g
                    })
                .OrderBy(m => m.Manufacturer.Name);
            foreach (var group in queryGroupJoinMethodSyntax)
            {
                Console.WriteLine($"\n {group.Manufacturer.Name}:{group.Manufacturer.Headquarters}");
                foreach (var car in group.Cars.OrderByDescending(c => c.Combined).Take(2))
                {
                    Console.WriteLine($"\t{car.Name} has a combined fuel efficiency of {car.Combined} MPG");
                }
            }
        }

        private static void GroupByCountryShowTopThreeCars(string FuelPath, string ManufacturerPath)
        {
            // Get some data
            var cars = ProcessCars(FuelPath);
            var manufacturers = ProcessManufacturers(ManufacturerPath);

            var queryGroupJoinQuerySyntax =
                from manufacturer in manufacturers
                join car in cars on manufacturer.Name equals car.Manufacturer
                into carGroup
                select new
                {
                    Manufacturer = manufacturer,
                    Cars = carGroup
                } into result
                group result by result.Manufacturer.Headquarters;

            foreach (var group in queryGroupJoinQuerySyntax)
            {
                Console.WriteLine($"\n {group.Key}");
                foreach (var car in group.SelectMany(g => g.Cars)
                                         .OrderByDescending(g => group)
                                         .Take(3))
                {
                    Console.WriteLine($"\t{car.Name} has a combined fuel efficiency of {car.Combined} MPG");
                }
            }

            Console.WriteLine("***");
            // Use LINQ Method Syntax
            var queryGroupJoinMethodSyntax =
                manufacturers.GroupJoin(cars, m => m.Name, c => c.Manufacturer, (m, g) => new
                {
                    Manufacturer = m,
                    Cars = g
                })
                .GroupBy(m => m.Manufacturer.Headquarters.ToUpper())
                .OrderBy(m => m.Key);

            foreach (var group in queryGroupJoinMethodSyntax)
            {
                Console.WriteLine($"\n {group.Key}");
                foreach (var car in group.SelectMany(g => g.Cars)
                                         .OrderByDescending(g => group)
                                         .Take(3))
                {
                    Console.WriteLine($"\t{car.Name} has a combined fuel efficiency of {car.Combined} MPG");
                }
            }
        }

        private static void GetManufacturerStatistics(string FuelPath, string ManufacturerPath)
        {
            // Get some data
            var cars = ProcessCars(FuelPath);
            var manufacturers = ProcessManufacturers(ManufacturerPath);

            var queryGetStatsQuerySyntax =
                from car in cars
                group car by car.Manufacturer 
                into carGroup
                select new
                {
                    Name = carGroup.Key,
                    Max = carGroup.Max(c => c.Combined),
                    Min = carGroup.Min(c => c.Combined),
                    Avg = carGroup.Average(c => c.Combined)
                } into result
                orderby result.Max descending
                select result;

            foreach (var result in queryGetStatsQuerySyntax)
            {
                Console.Write($"{result.Name}\t\t\t\t\t");
                Console.Write($"{result.Min}\t");
                Console.Write($"{result.Max}\t");
                Console.WriteLine("F02: {0}",result.Avg);
            }

            Console.WriteLine("***");
            // Use LINQ Method Syntax
            var queryGetStatsMethodSyntax =
                cars.GroupBy(c => c.Manufacturer)
                    .Select(g =>
                    {
                        var results = g.Aggregate(new CarStats(),
                            (acc, c) => acc.Accumulate(c),
                            acc => acc.Compute());
                        return new
                        {
                            Name = g.Key,
                            Avg = results.Average,
                            Min = results.Min,
                            Max = results.Max
                        };
                    })
                    .OrderByDescending(r => r.Max);

            foreach (var result in queryGetStatsMethodSyntax)
            {
                Console.Write($"{result.Name}\t");
                Console.Write($"{result.Min}\t");
                Console.Write($"{result.Max}\t");
                Console.WriteLine($"{result.Avg}\t");
            }
        }

        private static List<Manufacturer> ProcessManufacturers(string path)
        {
            var query = File.ReadAllLines(path)
                            .Where(line => line.Length > 1)
                            .Select(line =>
                            {
                                var columns = line.Split(',');
                                return new Manufacturer
                                {
                                    Name = columns[0],
                                    Headquarters = columns[1],
                                    Year = int.Parse(columns[2])
                                };
                            });
            return query.ToList();
        }

        
        private static List<Car> ProcessCars(string path)
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
