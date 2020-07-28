using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;


namespace Cars
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //var cars = ProcessCars("fuel.csv");
            //var manufacturers = ProcessManufacturers("manufacturers.csv");

            //GetTopBMWsForYear(cars, 2016);
            //DemonstrateQuantifiers(cars);
            //DemonstrateProjections(cars);
            //DemonstrateJoins(cars, manufacturers);
            //DemonstrateCompositeJoins(cars, manufacturers);
            //FindTwoMostFuelEfficientCarsByManufacturer(cars);
            //GroupJoinQuery(cars, manufacturers);
            //GroupByCountryShowTopThreeCars(cars, manufacturers);
            //GetManufacturerStatistics(cars);

            //CreateElementBasedXMLDocument(cars);
            //CreateAttributeBasedXMLDocument(cars);
            //CreateAttributeBasedXMLDocument_v2(cars);
            //QueryXML();
            //CreateXMLWithNamespace(cars);
            //QueryXMLWithNamespace();




            InsertDataIntoDatabase();
            ShowTenMostFuelEfficientCarsFromDatabase();



        }

        private static void ShowTenMostFuelEfficientCarsFromDatabase()
        {
            Console.WriteLine("The 10 most fuel efficient cars are as follows: ");
            var carDB = new CarContext();
            
            // LINQ Query Method syntax
            var query =
                    from car in carDB.Cars
                    orderby car.Combined descending, car.Name ascending
                    select car;

            // LINQ Extension Method syntax
            var query2 =
                carDB.Cars
                    .OrderByDescending(c => c.Combined)
                    .ThenBy(c => c.Name)
                    .Take(10);
            
            foreach (var car in query2)
            {
                Console.WriteLine($"{car.Name}: {car.Combined}");
            }
        }

        private static void InsertDataIntoDatabase()
        {
            // this method will grab data out of the csv file and,
            // put it into the sql server database
            var cars = ProcessCars("fuel.csv");

            var CarDB = new CarContext();
            CarDB.Database.EnsureCreated();  // WARN: don't use EnsureCreated() in real application!!

            if (!CarDB.Cars.Any())
            {
                // If there are no cars, then add them
                foreach (var car in cars)
                {
                    CarDB.Cars.Add(car);
                }
                // Inserts actually occur when you call SaveChanges();
                CarDB.SaveChanges();

            }



        }

        private static void QueryXMLWithNamespace()
        {

            var ns = (XNamespace)"http://pluralsight.com/cars/2016";  // namespace used
            var document = XDocument.Load("fuel.xml");

            // Find all cars manufactured by BMW, within the namespace.
            // Elements() is an IEnumerable<>, so we can use LINQ to query
            // It explicitly navigates to each car vs. using Decendents("Car"), because 
            // we want to guard against document being reorganized in the future.  
            // The specs said it should look this way.

            // Also notice how we're using null coalescence, in case Elements don't exist, or, 
            // elements are not in the "ns" namespace because the document changed.

            var query =
                from element in document.Element(ns + "Cars")?.Elements(ns + "Car") ?? Enumerable.Empty<XElement>()  // namespace used.
                where element.Attribute("Manufacturer")?.Value == "BMW"
                select element.Attribute("Name").Value;

            foreach (var name in query)
            {
                Console.WriteLine(name);
            }
        }
        private static void CreateXMLWithNamespace(List<Car> carsData)
        {
            // Create an Attribute-based XML document WITH namespace
            var document = new XDocument();
            var ns = (XNamespace)"http://pluralsight.com/cars/2016";  // namespace used
            var cars = new XElement(ns + "Cars",                        // namespace used
                from row in carsData
                select new XElement(ns + "Car",                         // namespace used
                            new XAttribute("Name", row.Name),
                            new XAttribute("Combined", row.Combined),
                            new XAttribute("Manufacturer", row.Manufacturer))
                );
            document.Add(cars);
            document.Save("fuel.xml");

            // Creates output like:
            //<? xml version = "1.0" encoding = "utf-8" ?>
            //< Cars xmlns = "http://pluralsight.com/cars/2016" >
            //  < Car Name = "4C" Combined = "28" Manufacturer = "ALFA Romeo" />
            //  < Car Name = "V12 Vantage S" Combined = "14" Manufacturer = "Aston Martin Lagonda Ltd" />
        }
        private static void QueryXML()
        {
            var document = XDocument.Load("fuel.xml");

            // Find all cars manufactured by BMW
            // Elements() is an IEnumerable<>, so we can use LINQ to query
            // It explicitly navigates to each car vs. using Decendents("Car"), because 
            // we want to guard against document being reorganized in the future.  
            // The specs said it should look this way.

            var query =
                from element in document.Elements("Cars").Elements("Car")
                where element.Attribute("Manufacturer")?.Value == "BMW"
                select element.Attribute("Name").Value;

            foreach (var name in query)
            {
                Console.WriteLine(name);
            }
        }
        private static void CreateAttributeBasedXMLDocument_v2(List<Car> carsData)
        {
            // Create an Attribute-based XML document 
            var document = new XDocument();

            // This implements a shorter approach too vs. the Element-based method.
            // Note: This is a more dense implementation, declaritive, and eliminates the foreach loop
            var cars = new XElement("Cars",
                from row in carsData
                select new XElement("Car",
                            new XAttribute("Name", row.Name),
                            new XAttribute("Combined", row.Combined),
                            new XAttribute("Manufacturer", row.Manufacturer))
                );
            document.Add(cars);
            document.Save("fuel.xml");
        }
        private static void CreateAttributeBasedXMLDocument(List<Car> carsData)
        {
            // Create an Attribute-based XML document 

            var document = new XDocument();
            var cars = new XElement("Cars");

            // This implements a shorter approach too vs. the Element-based method.
            foreach (var row in carsData)
            {
                // var name = new XElement("Name", row.Name);
                // var combined = new XElement("Combined", row.Combined);

                // add name and combined attributes 
                // when building the element, aka "functional construction"
                // var car = new XElement("Car", name, combined); 

                // more dense implementation

                var car = new XElement("Car",
                    new XAttribute("Name", row.Name),
                    new XAttribute("Combined", row.Combined),
                    new XAttribute("Manufacturer", row.Manufacturer)
                    );
                cars.Add(car);
            }
            document.Add(cars);
            document.Save("fuel.xml");
        }
        private static void CreateElementBasedXMLDocument(List<Car> carsData)
        {

            // Create an Element-based XML document 
            var document = new XDocument();
            var cars = new XElement("Cars");

            // This is the long way to create an element for each car
            foreach (var row in carsData)
            {
                var car = new XElement("Car");
                var name = new XElement("Name", row.Name);
                var combined = new XElement("Combined", row.Combined);
                car.Add(name);
                car.Add(combined);
                cars.Add(car);
            }
            document.Add(cars);
            document.Save("fuel.xml");
        }
        private static void DemonstrateCompositeJoins(List<Car> cars, List<Manufacturer> manufacturers)
        {

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

        }
        private static void DemonstrateJoins(List<Car> cars, List<Manufacturer> manufacturers)
        {
            Console.WriteLine("*** Joining cars and manufacturers with LINQ ***");

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
        }
        private static void DemonstrateProjections(List<Car> cars)
        {
            Console.WriteLine("*** Demonstrating Projections and SelectMany() ***");

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
        private static void DemonstrateQuantifiers(List<Car> cars)
        {
            Console.WriteLine("*** Demonstrating Quantifiers***");

            // Quantifiers

            var result = cars.Any(c => c.Manufacturer == "Ford"); // Any Fords? (True)
            Console.WriteLine("Any Fords? " + result);

            result = cars.All(c => c.Manufacturer == "Ford"); // Are all Cars Fords? (False)
            Console.WriteLine("Are all cars fords? " + result);
        }
        private static void GetTopBMWsForYear(List<Car> cars, int year)
        {
            Console.WriteLine("*** Get Top BMWs for 2016 ***");
            var query = cars.Where(c => c.Manufacturer == "BMW" && c.Year == year)
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
            var topCar = cars.Where(c => c.Manufacturer == "BMW" && c.Year == year)
                            .OrderByDescending(c => c.Combined)
                            .ThenBy(c => c.Name)
                            .Select(c => c)
                            .FirstOrDefault();  // executes query, makes query concrete. might return null

            Console.WriteLine($"Top car: {topCar.Name}");
        }
        private static void FindTwoMostFuelEfficientCarsByManufacturer(List<Car> cars)
        {
            Console.WriteLine("*** Find the 2 most fuel efficient cars for each Manufacturer ***");

            // Use LINQ Query Syntax
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
        private static void GroupJoinQuery(List<Car> cars, List<Manufacturer> manufacturers)
        {
            Console.WriteLine("*** Using GroupJoin() LINQ Extension Method ***");

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
        private static void GroupByCountryShowTopThreeCars(List<Car> cars, List<Manufacturer> manufacturers)
        {
            Console.WriteLine("*** Group By Headquarters Country, then show Top 3 Cars ***");

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
        private static void GetManufacturerStatistics(List<Car> cars)
        {
            Console.WriteLine("*** Get Manufacturer Statistics *** ");

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
                Console.WriteLine("F02: {0}", result.Avg);
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
            Console.WriteLine("*** Reading all Manufacturers from file ***");
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
            Console.WriteLine("*** Reading all Cars from file ***");
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
