using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopify.OrderService.Application.Common.Exceptions;
public class ValidationException(IEnumerable<ValidationError> errors)
    : Exception("One or more validation failures have occurred.")
{
    public IReadOnlyList<ValidationError> Errors { get; } = errors.ToList();
}

public record ValidationError(string PropertyName, string ErrorMessage);