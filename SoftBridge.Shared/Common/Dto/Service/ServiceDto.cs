using System;

namespace SoftBridge.Shared.Common.Dto.Service;

//Creating the DTO only for service 
public class ServiceDto
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public Guid CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public float AverageRating { get; set; }

    // one image only 
    public string ThumbnailUrl { get; set; } = string.Empty;

}
