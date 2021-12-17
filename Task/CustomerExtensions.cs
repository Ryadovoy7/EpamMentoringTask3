using System.Collections.Generic;
using System.Linq;
using Task.Data;

namespace CustomerExtensions
{
    public static class CustomerExtensions
    {
        public static IEnumerable<Customer> CustomersOrderTotalSumLargerThanXExt(this IEnumerable<Customer> customers, decimal x)
        {
            return from c in customers
                   where c.Orders.Sum(o => o.Total) > x
                   select c;
        }
    }
}
