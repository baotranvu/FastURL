using System;

namespace FastUrl.Domain.Exceptions;

/// <summary>
/// Ngoại lệ đại diện cho các vi phạm an ninh cửa ngõ API (Layer 2 Security Violation)
/// </summary>
public class SecurityViolationException : Exception
{
    public SecurityViolationException(string message) : base(message)
    {
    }

    public SecurityViolationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
