namespace Order.Api.Dtos;

public record OrderDto(int Id, DateTime CreatedAt, List<OrderItemDto> Items, decimal GrossTotal, decimal DiscountRate, decimal DiscountAmount, decimal NetTotal);
