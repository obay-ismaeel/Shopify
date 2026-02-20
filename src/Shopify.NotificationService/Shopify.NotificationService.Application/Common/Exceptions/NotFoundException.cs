using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopify.NotificationService.Application.Common.Exceptions;
public class NotFoundException(string entityName, object key)
    : Exception($"Entity '{entityName}' with key '{key}' was not found.");