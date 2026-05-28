using AuctionApp.Data;
using AuctionApp.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionApp.Controllers;

[ApiController]
[Route("api/tags")]
public class TagsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<List<TagBrief>> GetAll() =>
        await db.Tags
            .OrderBy(t => t.Name)
            .Select(t => new TagBrief(t.Id, t.Name))
            .ToListAsync();
}
