using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservationSystem.API.Data;
using ReservationSystem.API.DTOs;
using ReservationSystem.API.Enums;
using ReservationSystem.API.Models;

namespace ReservationSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResourcesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ResourcesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetResources([FromQuery] ResourceType? type, [FromQuery] int? minCapacity)
    {
        var query = _context.Resources.AsQueryable();

        query = query.Where(r => r.IsActive);

        if (type.HasValue)
        {
            query = query.Where(r => r.Type == type.Value);
        }

        if (minCapacity.HasValue)
        {
            query = query.Where(r => r.Capacity >= minCapacity.Value);
        }

        var resources = await query
            .Select(r => new ResourceResponseDto
            {
                Id = r.Id,
                Name = r.Name,
                Type = r.Type,
                Capacity = r.Capacity,
                IsActive = r.IsActive
            })
            .ToListAsync();

        return Ok(resources);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetResourceById(int id)
    {
        var resource = await _context.Resources.FindAsync(id);
        if (resource == null)
        {
            return NotFound(new { message = $"Resource with id {id} not found." });
        }

        return Ok(new ResourceResponseDto
        {
            Id = resource.Id,
            Name = resource.Name,
            Type = resource.Type,
            Capacity = resource.Capacity,
            IsActive = resource.IsActive
        });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateResource([FromBody] CreateResourceDto dto)
    {
        var resource = new Resource
        {
            Name = dto.Name,
            Type = dto.Type,
            Capacity = dto.Capacity,
            IsActive = true
        };

        _context.Resources.Add(resource);
        await _context.SaveChangesAsync();

        var response = new ResourceResponseDto
        {
            Id = resource.Id,
            Name = resource.Name,
            Type = resource.Type,
            Capacity = resource.Capacity,
            IsActive = resource.IsActive
        };

        return CreatedAtAction(nameof(GetResourceById), new { id = resource.Id }, response);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateResource(int id, [FromBody] UpdateResourceDto dto)
    {
        var resource = await _context.Resources.FindAsync(id);
        if (resource == null)
        {
            return NotFound(new { message = $"Resource with id {id} not found." });
        }

        resource.Name = dto.Name;
        resource.Type = dto.Type;
        resource.Capacity = dto.Capacity;
        resource.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();

        return Ok(new ResourceResponseDto
        {
            Id = resource.Id,
            Name = resource.Name,
            Type = resource.Type,
            Capacity = resource.Capacity,
            IsActive = resource.IsActive
        });
    }
}
