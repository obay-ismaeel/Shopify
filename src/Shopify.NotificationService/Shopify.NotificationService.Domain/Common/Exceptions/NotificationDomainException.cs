using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopify.NotificationService.Domain.Common.Exceptions;
public class NotificationDomainException(string message) : Exception(message);
