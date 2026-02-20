using MediatR;
using Shopify.NotificationService.Domain.Notifications;

namespace Shopify.NotificationService.Application.Notifications.Commands;
public record SendNotificationCommand(
    Guid OrderId,
    NotificationType Type,
    string Message) : IRequest<SendNotificationResult>;

public record SendNotificationResult(
    Guid NotificationId,
    Guid OrderId,
    string Type,
    string Status,
    bool WasDuplicate);
