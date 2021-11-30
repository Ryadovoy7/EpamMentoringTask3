using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using SampleSupport;
using Task.Data;


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
			// Способ 1 - делегат
			var customerOrderTotalQuery = new Func<decimal, IEnumerable<Customer>>(
				orderSum => from c in dataSource.Customers
							where c.Orders.Sum(o => o.Total) > orderSum
							select c);

			decimal x = 100000;
			var customers = customerOrderTotalQuery(x);
			PrintCustomersWithOrderTotalSum(customers, x);
			x = 110000;
			customers = customerOrderTotalQuery(x);
			PrintCustomersWithOrderTotalSum(customers, x);

			// Способ 2 - вынести в функцию
			QueryFuncEpam1(100000);
			QueryFuncEpam1(110000);
		}

		private void QueryFuncEpam1(decimal x)
        {
			var customers = from c in dataSource.Customers
							where c.Orders.Sum(o => o.Total) > x
							select c;

			PrintCustomersWithOrderTotalSum(customers, x);
        }

        private void PrintCustomersWithOrderTotalSum(IEnumerable<Customer> customers, decimal x)
        {
            Console.WriteLine("Order total sum must be larger than: " + x);

            foreach (var c in customers)
            {
                ObjectDumper.Write(c);
                Console.WriteLine("Order Total Sum: " + c.Orders.Sum(o => o.Total));
            }

            Console.WriteLine();
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
									 select new { Customer = c, Suppliers = from s in dataSource.Suppliers
																			where s.City == c.City && s.Country == c.Country
																			select s};
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
		[Title("Epam Task 4")]
		[Description("Выдайте список клиентов с указанием, начиная с какого месяца какого года они стали клиентами " +
			"(принять за таковые месяц и год самого первого заказа)")]
		public void Epam4()
		{
			var customers = from c in dataSource.Customers
							select new { Customer = c, Date = c.Orders.OrderBy(o => o.OrderDate).FirstOrDefault()?.OrderDate.ToString("MM.yyyy") ?? "No orders"};

			foreach (var c in customers)
			{
				ObjectDumper.Write(c.Customer);
				Console.WriteLine(c.Date);
				Console.WriteLine("Orders: ");
				foreach (var order in c.Customer.Orders)
					ObjectDumper.Write(order);
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
		[Title("Epam Task 8")]
		[Description("Сгруппируйте товары по группам «дешевые», «средняя цена», «дорогие». " +
			"Границы каждой группы задайте сами")]
		public void Epam8()
		{
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
									select new {City = g.Key, Customers = g};

			foreach (var g in customerCityGroup)
            {
				Console.WriteLine(g.City);
				var avgProfitability = (from c in g.Customers
									   select c.Orders.Sum(o => o.Total)).Average();
				Console.WriteLine($"Средняя прибыльность: {avgProfitability}");
				var avgIntensity = (from c in g.Customers
									select c.Orders.Count()).Average();
				Console.WriteLine($"Средняя интенсивность: {avgIntensity}");
			}			
		}

		[Category("Tasks")]
		[Title("Epam Task 10")]
		[Description("Сделайте среднегодовую статистику активности клиентов по месяцам (без учета года), " +
			"статистику по годам, по годам и месяцам (т.е. когда один месяц в разные годы имеет своё значение).")]
		public void Epam10()
		{
			foreach (var customer in dataSource.Customers)
			{
				ObjectDumper.Write(customer);
				var orderMonthGroup = from o in customer.Orders
									  group o by o.OrderDate.Month into monthGroup
									  orderby monthGroup.Key
									  select new { Date = monthGroup.Key, OrderCount = monthGroup.Count() };

				foreach (var g in orderMonthGroup)
					Console.WriteLine($"Месяц: {g.Date}\r\nЧисло сделок: {g.OrderCount}");

				var orderYearGroup = from o in customer.Orders
									 group o by o.OrderDate.Year into yearGroup
									 orderby yearGroup.Key
									 select new { Date = yearGroup.Key, OrderCount = yearGroup.Count() };

				foreach (var g in orderYearGroup)
					Console.WriteLine($"Год: {g.Date}\r\nЧисло сделок: {g.OrderCount}");

				var orderYearAndMonthGroup = from o in customer.Orders
											 group o by new { Year = o.OrderDate.Year, Month = o.OrderDate.Month } into yearMonthGroup
											 orderby yearMonthGroup.Key.Year, yearMonthGroup.Key.Month
											 select new { Date = String.Join(".", yearMonthGroup.Key.Year, yearMonthGroup.Key.Month), 
												 OrderCount = yearMonthGroup.Count() };

				foreach (var g in orderYearAndMonthGroup)
					Console.WriteLine($"Год и месяц: {g.Date}\r\nЧисло сделок: {g.OrderCount}");
			}
		}

	}	
}
