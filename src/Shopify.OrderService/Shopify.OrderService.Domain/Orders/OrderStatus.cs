using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopify.OrderService.Domain.Orders;
public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Cancelled = 2
}