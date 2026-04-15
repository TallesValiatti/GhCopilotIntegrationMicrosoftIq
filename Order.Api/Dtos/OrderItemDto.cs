namespace Order.Api.Dtos;

public record OrderItemDto(int ProductId, string ProductName, int Quantity, decimal UnitPrice);
