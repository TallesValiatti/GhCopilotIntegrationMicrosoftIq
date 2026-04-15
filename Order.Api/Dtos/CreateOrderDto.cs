namespace Order.Api.Dtos;

public record CreateOrderDto(List<CreateOrderItemDto> Items);
