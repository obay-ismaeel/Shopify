using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopify.OrderService.Domain.Common.Exceptions;
public class OrderDomainException(string message) : Exception(message);
