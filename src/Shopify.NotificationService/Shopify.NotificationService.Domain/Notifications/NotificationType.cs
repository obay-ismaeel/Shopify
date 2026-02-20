using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopify.NotificationService.Domain.Notifications;
public enum NotificationType
{
    OrderConfirmed = 0,
    OrderRejected = 1
}