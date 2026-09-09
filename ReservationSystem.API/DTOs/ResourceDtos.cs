using System.ComponentModel.DataAnnotations;
using ReservationSystem.API.Enums;

namespace ReservationSystem.API.DTOs;

public class CreateResourceDto
{
    [Required]
    [StringLength(100)]
    public required string Name { get; set; }

    [Required]
    public ResourceType Type { get; set; }

    [Range(1, 1000)]
    public int Capacity { get; set; }
}

public class UpdateResourceDto
{
    [Required]
    [StringLength(100)]
    public required string Name { get; set; }

    [Required]
    public ResourceType Type { get; set; }

    [Range(1, 1000)]
    public int Capacity { get; set; }

    public bool IsActive { get; set; }
}

public class ResourceResponseDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public ResourceType Type { get; set; }
    public int Capacity { get; set; }
    public bool IsActive { get; set; }
}
