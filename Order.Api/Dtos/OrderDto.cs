namespace Order.Api.Dtos;

public record OrderDto(int Id, DateTime CreatedAt, List<OrderItemDto> Items);
