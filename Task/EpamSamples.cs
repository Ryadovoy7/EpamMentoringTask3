using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using SampleSupport;
using Task.Data;
using CustomerExtensions;


namespace SampleQueries
{
	[Title("Epam Tasks")]
	[Prefix("Epam")]
	public class EpamSamples : SampleHarness
	{

		private DataSource dataSource = new DataSource();

		[Category("Tasks")]
		[Title("Epam Task 1")]
		[Description("Выдайте список всех клиентов, чей суммарный оборот (сумма всех заказов) превосходит некоторую величину X. " +
			"Продемонстрируйте выполнение запроса с различными X (подумайте, можно ли обойтись без копирования запроса несколько раз)")]
		public void Epam1()
		{
			decimal OrderTotalOneHundredThousand = 100000;
			decimal OrderTotalOneHundredTenThousand = 110000;
			// Способ 1 - делегат
			var customerOrderTotalQuery = new Func<decimal, IEnumerable<Customer>>(
				orderSum => from c in dataSource.Customers
							where c.Orders.Sum(o => o.Total) > orderSum
							select c);
			var printCustomersWithOrderTotalSum = new Action<IEnumerable<Customer>, decimal>(
				(customers, orderTotalSum) => {
					Console.WriteLine("Order total sum must be larger than: " + orderTotalSum);

					foreach (var c in customers)
					{
						ObjectDumper.Write(c);
						Console.WriteLine("Order Total Sum: " + c.Orders.Sum(o => o.Total));
					}

					Console.WriteLine();
				});


			var customers1 = customerOrderTotalQuery(OrderTotalOneHundredThousand);
			printCustomersWithOrderTotalSum(customers1, OrderTotalOneHundredThousand);
			
			customers1 = customerOrderTotalQuery(OrderTotalOneHundredTenThousand);
			printCustomersWithOrderTotalSum(customers1, OrderTotalOneHundredTenThousand);

			// Способ 2 - вынести в функцию
			var customers2 = CustomersOrderTotalSumLargerThanX(OrderTotalOneHundredThousand);
			printCustomersWithOrderTotalSum(customers2, OrderTotalOneHundredThousand);
			customers2 = CustomersOrderTotalSumLargerThanX(OrderTotalOneHundredTenThousand);
			printCustomersWithOrderTotalSum(customers2, OrderTotalOneHundredTenThousand);

			// Способ 3
			var customers3 = dataSource.Customers.CustomersOrderTotalSumLargerThanXExt(OrderTotalOneHundredThousand);
			printCustomersWithOrderTotalSum(customers3, OrderTotalOneHundredThousand);
		}

		private IEnumerable<Customer> CustomersOrderTotalSumLargerThanX(decimal x)
        {
            return from c in dataSource.Customers
				   where c.Orders.Sum(o => o.Total) > x
                   select c;
        }

		[Category("Tasks")]
		[Title("Epam Task 1 With Methods")]
		[Description("Выдайте список всех клиентов, чей суммарный оборот (сумма всех заказов) превосходит некоторую величину X. " +
			"Продемонстрируйте выполнение запроса с различными X (подумайте, можно ли обойтись без копирования запроса несколько раз)")]
		public void Epam1Methods()
		{
			decimal OrderTotalOneHundredThousand = 100000;
			decimal OrderTotalOneHundredTenThousand = 110000;
			// Только один способ, linq через методы достаточно продемонстрирован в нем
			var customerOrderTotalQuery = new Func<decimal, IEnumerable<Customer>>(
				orderSum => dataSource.Customers.Where(o => o.Orders.Sum(ord => ord.Total) > orderSum)) ;
			var printCustomersWithOrderTotalSum = new Action<IEnumerable<Customer>, decimal>(
				(customers, orderTotalSum) => {
					Console.WriteLine("Order total sum must be larger than: " + orderTotalSum);

					foreach (var c in customers)
					{
						ObjectDumper.Write(c);
						Console.WriteLine("Order Total Sum: " + c.Orders.Sum(o => o.Total));
					}

					Console.WriteLine();
				});


			var customers1 = customerOrderTotalQuery(OrderTotalOneHundredThousand);
			printCustomersWithOrderTotalSum(customers1, OrderTotalOneHundredThousand);

			customers1 = customerOrderTotalQuery(OrderTotalOneHundredTenThousand);
			printCustomersWithOrderTotalSum(customers1, OrderTotalOneHundredTenThousand);
		}

		[Category("Tasks")]
		[Title("Epam Task 2")]
		[Description("Для каждого клиента составьте список поставщиков, находящихся в той же стране и том же городе. " +
			"Сделайте задания с использованием группировки и без.")]
		public void Epam2()
		{
			// Способ 1 - с группировкой
			var customersGrouped = from c in dataSource.Customers
								   join supplier in (from s in dataSource.Suppliers
													 group s by new { s.City, s.Country })
								   on new { c.City, c.Country } equals supplier.Key into ccJoin
								   orderby c.CustomerID
								   from suppliers in ccJoin.DefaultIfEmpty()
								   select new { Customer = c, Suppliers = suppliers?.ToList() ?? new List<Supplier>() };

			Console.WriteLine("WITH GROUPING:\r\n");

			foreach (var pair in customersGrouped)
			{
				ObjectDumper.Write(pair.Customer);
				foreach (var supplier in pair.Suppliers)
					ObjectDumper.Write(supplier);
				Console.WriteLine();
			}

			// Способ 2 - без группировки
			var customersUngrouped = from c in dataSource.Customers
									 select new
									 {
										 Customer = c,
										 Suppliers = from s in dataSource.Suppliers
													 where s.City == c.City && s.Country == c.Country
													 select s
									 };
			Console.WriteLine("WITHOUT GROUPING:\r\n");

			foreach (var pair in customersUngrouped)
			{
				ObjectDumper.Write(pair.Customer);
				foreach (var supplier in pair.Suppliers)
					ObjectDumper.Write(supplier);
				Console.WriteLine();
			}
		}

		[Category("Tasks")]
		[Title("Epam Task 2 With Methods")]
		[Description("Для каждого клиента составьте список поставщиков, находящихся в той же стране и том же городе. " +
		"Сделайте задания с использованием группировки и без.")]
		public void Epam2Methods()
		{
			//// Способ 1 - с группировкой
			var customersGrouped = dataSource.Customers.GroupJoin(
					dataSource.Suppliers.
					GroupBy(
						s => new { s.City, s.Country }),
					c => new { c.City, c.Country },
					s => s.Key,
					(c, s) => new { Customer = c, Suppliers = s.ToList() })
				.OrderBy(g => g.Customer.CustomerID);

			Console.WriteLine("WITH GROUPING:\r\n");

			foreach (var pair in customersGrouped)
			{
				ObjectDumper.Write(pair.Customer);
				foreach (var supplier in pair.Suppliers)
					ObjectDumper.Write(supplier);
				Console.WriteLine();
			}

			// Способ 2 - без группировки
			var customersUngrouped = dataSource.Customers.Select(
				c => new { 
					Customer = c, 
					Suppliers = dataSource.Suppliers
						.Where(s => s.City == c.City && s.Country == c.Country) 
				});
			Console.WriteLine("WITHOUT GROUPING:\r\n");

			foreach (var pair in customersUngrouped)
			{
				ObjectDumper.Write(pair.Customer);
				foreach (var supplier in pair.Suppliers)
					ObjectDumper.Write(supplier);
				Console.WriteLine();
			}
		}

		[Category("Tasks")]
		[Title("Epam Task 3")]
		[Description("Найдите всех клиентов, у которых были заказы, превосходящие по сумме величину X")]
		public void Epam3()
		{
			decimal x = 3000;
			var customers = from c in dataSource.Customers
							where c.Orders.Any(o => o.Total > x)
							select c;

			foreach (var c in customers)
			{
				ObjectDumper.Write(c);

				Console.WriteLine("Orders: ");
                foreach (var order in c.Orders)
                    ObjectDumper.Write(order);
			}
		}

		[Category("Tasks")]
		[Title("Epam Task 3 With Methods")]
		[Description("Найдите всех клиентов, у которых были заказы, превосходящие по сумме величину X")]
		public void Epam3Methods()
		{
			decimal x = 3000;
			var customers = dataSource.Customers.Where(c => c.Orders.Any(o => o.Total > x));

			foreach (var c in customers)
			{
				ObjectDumper.Write(c);

				Console.WriteLine("Orders: ");
				foreach (var order in c.Orders)
					ObjectDumper.Write(order);
			}
		}

		[Category("Tasks")]
		[Title("Epam Task 4")]
		[Description("Выдайте список клиентов с указанием, начиная с какого месяца какого года они стали клиентами " +
			"(принять за таковые месяц и год самого первого заказа)")]
		public void Epam4()
		{
			var customers = from c in dataSource.Customers
							select new { CustomerID = c.CustomerID, Date = c.Orders.OrderBy(o => o.OrderDate).FirstOrDefault()?.OrderDate.ToString("MM.yyyy") ?? "No orders"};

			foreach (var c in customers)
			{
				Console.WriteLine(c.CustomerID);
				Console.WriteLine(c.Date);
			}
		}

		[Category("Tasks")]
		[Title("Epam Task 4 With Methods")]
		[Description("Выдайте список клиентов с указанием, начиная с какого месяца какого года они стали клиентами " +
			"(принять за таковые месяц и год самого первого заказа)")]
		public void Epam4Methods()
		{
			var customers = dataSource.Customers
				.Select(c => new { 
					CustomerID = c.CustomerID, 
					Date = c.Orders.OrderBy(o => o.OrderDate)
						.FirstOrDefault()?.OrderDate.ToString("MM.yyyy") ?? "No orders" 
				});

			foreach (var c in customers)
			{
				Console.WriteLine(c.CustomerID);
				Console.WriteLine(c.Date);
			}
		}

		[Category("Tasks")]
		[Title("Epam Task 5")]
		[Description("Сделайте предыдущее задание, но выдайте список отсортированным по году, месяцу, " +
			"оборотам клиента (от максимального к минимальному) и имени клиента")]
		public void Epam5()
		{
			var customers = from c in dataSource.Customers
							let firstOrderDate = c.Orders.Length > 0 ? c.Orders.Min(o => o.OrderDate) : DateTime.MinValue
							orderby firstOrderDate.Year, 
									firstOrderDate.Month, 
									c.Orders.Sum(o => o.Total) descending, 
									c.CustomerID
							select new { Customer = c, Date = firstOrderDate != DateTime.MinValue ? firstOrderDate.ToString("MM.yyyy") : "No orders" };

			foreach (var c in customers)
			{
				ObjectDumper.Write(c.Customer);
				Console.WriteLine(c.Date);
				Console.WriteLine("Order Total Sum: " + c.Customer.Orders.Sum(o => o.Total));
				Console.WriteLine("Orders: ");
				foreach (var order in c.Customer.Orders)
					ObjectDumper.Write(order);
			}
		}

		[Category("Tasks")]
		[Title("Epam Task 5 With Methods")]
		[Description("Сделайте предыдущее задание, но выдайте список отсортированным по году, месяцу, " +
			"оборотам клиента (от максимального к минимальному) и имени клиента")]
		public void Epam5Methods()
		{
			var customers = dataSource.Customers
				.Select(c => new { c, firstOrderDate = c.Orders.Length > 0 ? c.Orders.Min(o => o.OrderDate) : DateTime.MinValue })
				.OrderBy(cf => cf.firstOrderDate.Year)
				.ThenBy(cf => cf.firstOrderDate.Month)
				.ThenByDescending(cf => cf.c.Orders.Sum(o => o.Total))
				.ThenBy(cf => cf.c.CustomerID)
				.Select(cf => new { 
					Customer = cf.c, 
					Date = cf.firstOrderDate != DateTime.MinValue ? cf.firstOrderDate.ToString("MM.yyyy") : "No orders" 
				});

			foreach (var c in customers)
			{
				ObjectDumper.Write(c.Customer);
				Console.WriteLine(c.Date);
				Console.WriteLine("Order Total Sum: " + c.Customer.Orders.Sum(o => o.Total));
				Console.WriteLine("Orders: ");
				foreach (var order in c.Customer.Orders)
					ObjectDumper.Write(order);
			}
		}

		[Category("Tasks")]
		[Title("Epam Task 6")]
		[Description("Укажите всех клиентов, у которых указан нецифровой почтовый код или не заполнен регион " +
			"или в телефоне не указан код оператора (для простоты считаем, что это равнозначно «нет круглых скобочек в начале»).")]
		public void Epam6()
		{
			var customers = from c in dataSource.Customers
							where String.IsNullOrWhiteSpace(c.PostalCode)
							|| !c.PostalCode.Any(char.IsLetter) 
							|| String.IsNullOrWhiteSpace(c.Region) 
							|| !c.Phone.StartsWith("(")
							select c;

			foreach (var c in customers)
				ObjectDumper.Write(c);
		}

		[Category("Tasks")]
		[Title("Epam Task 6 With Methods")]
		[Description("Укажите всех клиентов, у которых указан нецифровой почтовый код или не заполнен регион " +
			"или в телефоне не указан код оператора (для простоты считаем, что это равнозначно «нет круглых скобочек в начале»).")]
		public void Epam6Methods()
		{
			var customers = dataSource.Customers.Where(c => String.IsNullOrWhiteSpace(c.PostalCode)
							|| !c.PostalCode.Any(char.IsLetter)
							|| String.IsNullOrWhiteSpace(c.Region)
							|| !c.Phone.StartsWith("("));

			foreach (var c in customers)
				ObjectDumper.Write(c);
		}


		[Category("Tasks")]
		[Title("Epam Task 7")]
		[Description("Сгруппируйте все продукты по категориям, внутри – по наличию на складе, " +
			"внутри последней группы отсортируйте по стоимости.")]
		public void Epam7()
		{
			var productsGroupedByCategory = from p in dataSource.Products
							group p by p.Category into pCatGroup
							select new
							{
								Category = pCatGroup.Key,
								ProductsGroupedByUnits = from p in pCatGroup
														 group p by p.UnitsInStock into pUnitsGroup
														 select new
														 {
															 UnitsInStock = pUnitsGroup.Key,
															 Products = from p in pUnitsGroup
																		orderby p.UnitPrice
																		select p
														 }
							};

			foreach (var prodCatGroup in productsGroupedByCategory)
            {
				Console.WriteLine($"Category: {prodCatGroup.Category}");
				foreach (var prodUnitGroup in prodCatGroup.ProductsGroupedByUnits)
                {
					Console.WriteLine($"Units in stock: {prodUnitGroup.UnitsInStock}");
					foreach (var product in prodUnitGroup.Products)
                    {
						ObjectDumper.Write(product);
					}
                }
            }			
		}

		[Category("Tasks")]
		[Title("Epam Task 7 With Methods")]
		[Description("Сгруппируйте все продукты по категориям, внутри – по наличию на складе, " +
			"внутри последней группы отсортируйте по стоимости.")]
		public void Epam7Methods()
		{
			var productsGroupedByCategory = dataSource.Products
				.GroupBy(p => p.Category)
				.Select(gp => new
				{
					Category = gp.Key,
					ProductsGroupedByUnits = gp
					   .GroupBy(p => p.UnitsInStock)
					   .Select(pUnitsGroup => new
					   {
						   UnitsInStock = pUnitsGroup.Key,
						   Products = pUnitsGroup.OrderBy(pug => pug.UnitPrice)
					   })
				});

			foreach (var prodCatGroup in productsGroupedByCategory)
			{
				Console.WriteLine($"Category: {prodCatGroup.Category}");
				foreach (var prodUnitGroup in prodCatGroup.ProductsGroupedByUnits)
				{
					Console.WriteLine($"Units in stock: {prodUnitGroup.UnitsInStock}");
					foreach (var product in prodUnitGroup.Products)
					{
						ObjectDumper.Write(product);
					}
				}
			}
		}

		[Category("Tasks")]
		[Title("Epam Task 8")]
		[Description("Сгруппируйте товары по группам «дешевые», «средняя цена», «дорогие». " +
			"Границы каждой группы задайте сами")]
		public void Epam8()
		{
			// уже сделан методом
			var productsPriceRangeGroup = dataSource.Products.GroupBy(p =>
			{
				if (p.UnitPrice <= 5)
					return "Дешевые";
				else if (p.UnitPrice <= 10)
					return "Средняя цена";
				else
					return "Дорогие";
			});

			foreach (var productGroup in productsPriceRangeGroup)
			{
				Console.WriteLine(productGroup.Key);
				foreach (var p in productGroup)
					ObjectDumper.Write(p);
			}
		}

		[Category("Tasks")]
		[Title("Epam Task 9")]
		[Description("Рассчитайте среднюю прибыльность каждого города " +
			"(среднюю сумму заказа по всем клиентам из данного города) и среднюю интенсивность " +
			"(среднее количество заказов, приходящееся на клиента из каждого города)")]
		public void Epam9()
		{
			var customerCityGroup = from c in dataSource.Customers
									group c by c.City into g
									select new {City = g.Key, 
										Customers = g, 
										AvgProfitability = (from customer in g
														   select customer.Orders.Sum(o => o.Total)).Average(),
										AvgIntensity = (from customer in g
														select customer.Orders.Count()).Average()
									};

			foreach (var g in customerCityGroup) // теперь только для вывода
            {
				Console.WriteLine(g.City);
				Console.WriteLine($"Средняя прибыльность: {g.AvgProfitability}");
				Console.WriteLine($"Средняя интенсивность: {g.AvgIntensity}");
			}			
		}

		[Category("Tasks")]
		[Title("Epam Task 9 With Methods")]
		[Description("Рассчитайте среднюю прибыльность каждого города " +
			"(среднюю сумму заказа по всем клиентам из данного города) и среднюю интенсивность " +
			"(среднее количество заказов, приходящееся на клиента из каждого города)")]
		public void Epam9Methods()
		{
			var customerCityGroup = dataSource.Customers
				.GroupBy(c => c.City)
				.Select(g => new
				{
					City = g.Key,
					Customers = g,
					AvgProfitability = g.Select(c => c.Orders.Sum(o => o.Total)).Average(),
					AvgIntensity = g.Select(c => c.Orders.Count()).Average()
				});

			foreach (var g in customerCityGroup) // теперь только для вывода
			{
				Console.WriteLine(g.City);
				Console.WriteLine($"Средняя прибыльность: {g.AvgProfitability}");
				Console.WriteLine($"Средняя интенсивность: {g.AvgIntensity}");
			}
		}

		[Category("Tasks")]
		[Title("Epam Task 10")]
		[Description("Сделайте среднегодовую статистику активности клиентов по месяцам (без учета года), " +
			"статистику по годам, по годам и месяцам (т.е. когда один месяц в разные годы имеет своё значение).")]
		public void Epam10()
		{
			var customerWithStatistics = from c in dataSource.Customers
										 select new
										 {
											 Customer = c,
											 OrderMonthGroup = (from o in c.Orders
																group o by o.OrderDate.Month into monthGroup
																orderby monthGroup.Key
																select new { Date = monthGroup.Key, OrderCount = monthGroup.Count() }),
											 OrderYearGroup = (from o in c.Orders
															   group o by o.OrderDate.Year into yearGroup
															   orderby yearGroup.Key
															   select new { Date = yearGroup.Key, OrderCount = yearGroup.Count() }),
											 OrderYearAndMonthGroup = (from o in c.Orders
																	   group o by new { Year = o.OrderDate.Year, Month = o.OrderDate.Month } into yearMonthGroup
																	   orderby yearMonthGroup.Key.Year, yearMonthGroup.Key.Month
																	   select new
																	   {
																		   Date = String.Join(".", yearMonthGroup.Key.Year, yearMonthGroup.Key.Month),
																		   OrderCount = yearMonthGroup.Count()
																	   })
										 };

			foreach (var cws in customerWithStatistics)
			{
				ObjectDumper.Write(cws.Customer);
	
				foreach (var g in cws.OrderMonthGroup)
					Console.WriteLine($"Месяц: {g.Date}\r\nЧисло сделок: {g.OrderCount}");
				
				foreach (var g in cws.OrderYearGroup)
					Console.WriteLine($"Год: {g.Date}\r\nЧисло сделок: {g.OrderCount}");				

				foreach (var g in cws.OrderYearAndMonthGroup)
					Console.WriteLine($"Год и месяц: {g.Date}\r\nЧисло сделок: {g.OrderCount}");
			}
		}

		[Category("Tasks")]
		[Title("Epam Task 10 With Methods")]
		[Description("Сделайте среднегодовую статистику активности клиентов по месяцам (без учета года), " +
			"статистику по годам, по годам и месяцам (т.е. когда один месяц в разные годы имеет своё значение).")]
		public void Epam10Methods()
		{
			var customerWithStatistics = dataSource.Customers.Select(c => new
			{
				Customer = c,
				OrderMonthGroup = c.Orders
					.GroupBy(o => o.OrderDate.Month)
					.OrderBy(g => g.Key)
					.Select(g => new
					{
						Date = g.Key,
						OrderCount = g.Count()
					}),
				OrderYearGroup = c.Orders
					.GroupBy(o => o.OrderDate.Year)
					.OrderBy(g => g.Key)
					.Select(g => new
					{
						Date = g.Key,
						OrderCount = g.Count()
					}),
				OrderYearAndMonthGroup = c.Orders
					.GroupBy(o => new { Year = o.OrderDate.Year, Month = o.OrderDate.Month })
					.OrderBy(g => g.Key.Year)
					.ThenBy(g => g.Key.Month)
					.Select(g => new
					{
						Date = String.Join(".", g.Key.Year, g.Key.Month),
						OrderCount = g.Count()
					})
			});

			foreach (var cws in customerWithStatistics)
			{
				ObjectDumper.Write(cws.Customer);

				foreach (var g in cws.OrderMonthGroup)
					Console.WriteLine($"Месяц: {g.Date}\r\nЧисло сделок: {g.OrderCount}");

				foreach (var g in cws.OrderYearGroup)
					Console.WriteLine($"Год: {g.Date}\r\nЧисло сделок: {g.OrderCount}");

				foreach (var g in cws.OrderYearAndMonthGroup)
					Console.WriteLine($"Год и месяц: {g.Date}\r\nЧисло сделок: {g.OrderCount}");
			}
		}
	}	
}
