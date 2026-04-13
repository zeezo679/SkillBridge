using E_commerce.Shared.Common.Dto.Service;
using System;

namespace SoftBridge.Shared.Common.Dto.Service;

//Creating the DTO only for service so i can use it in the CategoryftoShowServiceDto ( I didnt start implementing the Service Management Service)
public class ServiceDto
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public Guid CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int DeliveryDays { get; set; }
    public string Status { get; set; } = string.Empty; // Pending, Approved, etc.
    public float AverageRating { get; set; }

    // List of Image URLs to display in the UI
    public List<ServiceImageDto> Images { get; set; } = new List<ServiceImageDto>();

}
