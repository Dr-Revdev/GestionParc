using GestiParc.Core.DTOs;
using GestiParc.Core.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestiParc.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AgentController : ControllerBase
{
    private readonly IAgentRepository _agentRepository;

    public AgentController(IAgentRepository agentRepository)
    {
        _agentRepository = agentRepository;
    }

    // GET /api/agent
    [HttpGet]
    public ActionResult<List<AgentDto>> GetAll()
    {
        var items = _agentRepository.GetAll();
        return Ok(items);
    }

    // GET /api/agent/{id}
    [HttpGet("{id}")]
    public ActionResult<AgentDto> GetById(string id)
    {
        var item = _agentRepository.GetById(id);
        if (item == null)
            return NotFound();

        return Ok(item);
    }

    // POST /api/agent
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public IActionResult Create([FromBody] AgentDto dto)
    {
        if (dto == null)
            return BadRequest("Payload vide.");
        
        _agentRepository.Insert(dto);

        return CreatedAtAction(nameof(GetById), new { id = dto.Idrh }, dto);
    }

    // PUT /api/agent/{id}
    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public IActionResult Update(string id, [FromBody] AgentDto dto)
    {
        if (dto == null)
            return BadRequest("Payload vide.");
        
        if (id != dto.Idrh)
            return BadRequest("L'ID de l'URL ne correspond pas à l'ID de l'Agent.");
        
        var existing = _agentRepository.GetById(id);
        if (existing == null)
            return NotFound();

        _agentRepository.Update(dto);

        return NoContent();
    }

    // DELETE /api/agent/{id}
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public IActionResult Delete(string id)
    {
        var existing = _agentRepository.GetById(id);
        if (existing == null)
            return NotFound();
        
        _agentRepository.Delete(id);

        return NoContent();
    }
}