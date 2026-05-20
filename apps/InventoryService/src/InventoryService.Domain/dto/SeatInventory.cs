namespace InventoryService.Domain.Models;
public class SeatInventoryDto
{
    public Guid     EventId { get; set; }
    public Guid     SeatId  { get; set; }
    public string   Status  { get; set; } = string.Empty;
    public int      Version { get; set; } = 1;
    public  
}