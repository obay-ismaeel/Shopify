using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopify.OrderService.Domain.Common.Idempotency;
public class IdempotencyKey
{
    public Guid Key { get; private set; }

    public Guid OrderId { get; private set; }

    public string ResponseBody { get; private set; } = default!;

    public DateTime CreatedAt { get; private set; }

    private IdempotencyKey() { }

    public static IdempotencyKey Create(Guid key, Guid orderId, string responseBody)
        => new()
        {
            Key = key,
            OrderId = orderId,
            ResponseBody = responseBody,
            CreatedAt = DateTime.UtcNow
        };
}
