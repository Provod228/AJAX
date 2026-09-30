namespace WebApplication3.Application.Common;

public sealed record Error(string Code, string Message, ErrorType Type)
{
    public static Error Validation(string code, string msg) => new(code, msg, ErrorType.Validation);
    public static Error NotFound(string code, string msg) => new(code, msg, ErrorType.NotFound);
    public static Error Conflict(string code, string msg) => new(code, msg, ErrorType.Conflict);
    public static Error Forbidden(string code, string msg) => new(code, msg, ErrorType.Forbidden);
    public static Error Failure(string code, string msg) => new(code, msg, ErrorType.Failure);
}
