namespace RfidWarehouseApi.DTOs;

public class CartItemDto
{
    public int ItemId { get; set; }
    public string RfidUid { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public DateTime ScannedAt { get; set; }
}

public class SessionCartDto
{
    public int UserId { get; set; }
    public List<CartItemDto> Items { get; set; } = new();
    public DateTime SessionStarted { get; set; }
}
