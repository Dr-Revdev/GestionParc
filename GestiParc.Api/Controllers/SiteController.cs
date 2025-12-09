using System.Collections.Generic;
using GestiParc.Core.DTOs;
using GestiParc.Core.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GestiParc.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SiteController : ControllerBase
{
    private readonly ISiteRepository _siteRepository;

    public SiteController(ISiteRepository siteRepository)
    {
        _siteRepository = siteRepository;
    }

    // GET /api/equipe
    [HttpGet]
    public ActionResult<List<SiteDto>> GetAll()
    {
        var items = _siteRepository.GetAll();
        return Ok(items);
    }

    // GET /api/equipe/{id}
    [HttpGet("{id}")]
    public ActionResult<SiteDto> GetById(int id)
    {
        var item = _siteRepository.GetById(id);
        if (item == null)
            return NotFound();
        
        return Ok(item);
    }

    // POST /api/equipe
    [HttpPost]
    public IActionResult Create([FromBody] SiteDto dto)
    {
        if (dto == null)
            return BadRequest("Payload vide.");

        var newId = _siteRepository.Insert(dto.Name);
        var created = new SiteDto(newId, dto.Name);

        return CreatedAtAction(nameof(GetById), new { id = newId }, created);
    }

    // PUT /api/equipe/{id}
    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] SiteDto dto)
    {
        if (dto == null)
            return BadRequest("Payload vide.");
        
        if (id != dto.Id)
            return BadRequest("L'ID de l'URL ne correspond pas à l'ID de l'équipe.");
        
        var existing = _siteRepository.GetById(id);
        if (existing == null)
            return NotFound();

        _siteRepository.Update(dto);

        return NoContent();
    }

    // DELETE /api/equipe/{id}
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var existing = _siteRepository.GetById(id);
        if (existing == null)
            return NotFound();
        
        _siteRepository.Delete(id);

        return NoContent();
    }
}