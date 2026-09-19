namespace midalware.Models;

/// <summary>
/// Модель формы обратной связи. Привязывается к полям HTML-формы.
/// </summary>
public class FeedbackForm
{
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Message { get; set; } = "";
}
