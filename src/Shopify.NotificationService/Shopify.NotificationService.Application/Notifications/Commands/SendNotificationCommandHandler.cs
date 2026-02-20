using MediatR;
using Microsoft.Extensions.Logging;
using Shopify.NotificationService.Application.Common.Interfaces;
using Shopify.NotificationService.Domain.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopify.NotificationService.Application.Notifications.Commands;
public class SendNotificationCommandHandler : IRequestHandler<SendNotificationCommand, SendNotificationResult>
{
    private readonly INotificationRepository notificationRepository;
    private readonly INotificationSender notificationSender;
    private readonly IUnitOfWork unitOfWork;
    private readonly ILogger<SendNotificationCommandHandler> logger;

    public SendNotificationCommandHandler(
        INotificationRepository notificationRepository,
        INotificationSender notificationSender,
        IUnitOfWork unitOfWork,
        ILogger<SendNotificationCommandHandler> logger)
    {
        this.notificationRepository = notificationRepository;
        this.notificationSender = notificationSender;
        this.unitOfWork = unitOfWork;
        this.logger = logger;
    }

    public async Task<SendNotificationResult> Handle(
        SendNotificationCommand request,
        CancellationToken cancellationToken)
    {
        // Idempotency check — don't send the same notification twice
        var alreadySent = await notificationRepository.ExistsForOrderAsync(
            request.OrderId, request.Type, cancellationToken);

        if (alreadySent)
        {
            logger.LogWarning(
                "duplicate notification detected for order {OrderId} of type {Type}. Skipping.",
                request.OrderId, request.Type);

            return new SendNotificationResult(
                NotificationId: Guid.Empty,
                OrderId: request.OrderId,
                Type: request.Type.ToString(),
                Status: "Skipped",
                WasDuplicate: true);
        }

        var notification = Notification.Create(request.OrderId, request.Type, request.Message);
        await notificationRepository.AddAsync(notification, cancellationToken);

        try
        {
            await notificationSender.SendAsync(notification.Message, cancellationToken);
            notification.MarkAsSent();

            logger.LogInformation(
                "✅ Notification sent for order {OrderId} | Type: {Type} | Message: {Message}",
                request.OrderId, request.Type, request.Message);
        }
        catch (Exception ex)
        {
            notification.MarkAsFailed(ex.Message);

            logger.LogError(ex,
                "❌ Failed to send notification for order {OrderId}. Marked for retry.",
                request.OrderId);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new SendNotificationResult(
            NotificationId: notification.Id,
            OrderId: notification.OrderId,
            Type: notification.Type.ToString(),
            Status: notification.Status.ToString(),
            WasDuplicate: false);
    }
}