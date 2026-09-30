namespace WebApplication3.Domain;

public record OrderItem(Guid ProductId, string Name, decimal Price, int Quantity);
