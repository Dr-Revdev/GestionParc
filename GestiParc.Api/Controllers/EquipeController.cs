using GestiParc.Core.DTOs;
using GestiParc.Core.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestiParc.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EquipeController : ControllerBase
{
    private readonly IEquipeRepository _equipeRepository;

    public EquipeController(IEquipeRepository equipeRepository)
    {
        _equipeRepository = equipeRepository;
    }

    // GET /api/equipe
    [HttpGet]
    public ActionResult<List<EquipeDto>> GetAll()
    {
        var items = _equipeRepository.GetAll();
        return Ok(items);
    }

    // GET /api/equipe/{id}
    [HttpGet("{id}")]
    public ActionResult<EquipeDto> GetById(int id)
    {
        var item = _equipeRepository.GetById(id);
        if (item == null)
            return NotFound();
        
        return Ok(item);
    }

    // POST /api/equipe
    [HttpPost]
    [Authorize(Policy = "UserOrAdmin")]
    public IActionResult Create([FromBody] EquipeDto dto)
    {
        if (dto == null)
            return BadRequest("Payload vide.");

        var newId = _equipeRepository.Insert(dto.Name);
        var created = new EquipeDto(newId, dto.Name);

        return CreatedAtAction(nameof(GetById), new { id = newId }, created);
    }

    // PUT /api/equipe/{id}
    [HttpPut("{id}")]
    [Authorize(Policy = "UserOrAdmin")]
    public IActionResult Update(int id, [FromBody] EquipeDto dto)
    {
        if (dto == null)
            return BadRequest("Payload vide.");
        
        if (id != dto.Id)
            return BadRequest("L'ID de l'URL ne correspond pas à l'ID de l'équipe.");
        
        var existing = _equipeRepository.GetById(id);
        if (existing == null)
            return NotFound();

        _equipeRepository.Update(dto);

        return NoContent();
    }

    // DELETE /api/equipe/{id}
    [HttpDelete("{id}")]
    [Authorize(Policy = "UserOrAdmin")]
    public IActionResult Delete(int id)
    {
        var existing = _equipeRepository.GetById(id);
        if (existing == null)
            return NotFound();
        
        _equipeRepository.Delete(id);

        return NoContent();
    }
}