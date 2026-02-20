using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopify.NotificationService.Domain.Notifications;
public enum NotificationStatus
{
    Pending = 0,
    Sent = 1,
    Failed = 2
}