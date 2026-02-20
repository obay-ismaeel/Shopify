using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopify.NotificationService.Application.Common.Interfaces;
public interface INotificationSender
{
    Task SendAsync(string message, CancellationToken cancellationToken = default);
}
