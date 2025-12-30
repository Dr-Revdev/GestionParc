using GestiParc.Core.DTOs;
using GestiParc.Core.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestiParc.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EquipmentTypeController : ControllerBase
{
    private readonly IEquipmentTypeRepository _equipmentTypeRepository;

    public EquipmentTypeController(IEquipmentTypeRepository equipmentTypeRepository)
    {
        _equipmentTypeRepository = equipmentTypeRepository;
    }

    // GET /api/equipmentType/{id}
    [HttpGet("{id}")]
    public ActionResult<EquipmentTypeDto> GetById(int id)
    {
        var item = _equipmentTypeRepository.GetById(id);
        if (item == null)
            return NotFound();
        
        return Ok(item);
    }

    // GET /api/equipmentType
    [HttpGet]
    public ActionResult<List<EquipmentTypeDto>> GetAll()
    {
        var items = _equipmentTypeRepository.GetAll();
        return Ok(items);
    }

    // POST /api/equipmentType
    [HttpPost]
    [Authorize(Policy = "UserOrAdmin")]
    public IActionResult Create([FromBody] EquipmentTypeDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Le nom est obligatoire.");
        
        var newId = _equipmentTypeRepository.Insert(dto.Name);
        var created = new EquipmentTypeDto(newId, dto.Name);

        return CreatedAtAction(nameof(GetById), new { id = newId }, created);
    }

    // PUT /api/equipmentType/{id}
    [HttpPut("{id}")]
    [Authorize(Policy = "UserOrAdmin")]
    public IActionResult Update(int id, [FromBody] EquipmentTypeDto dto)
    {
        if (dto == null)
            return BadRequest("Payload vide.");

        if (id !=dto.Id)
            return BadRequest("L'ID de l'URL ne correspond pas à l'ID du type d'équipement.");
        
        var existing = _equipmentTypeRepository.GetById(id);
        if (existing == null)
            return NotFound();
        
        _equipmentTypeRepository.Update(dto);

        return NoContent();
    }

    // DELETE /api/equipmentType/{id}
    [HttpDelete("{id}")]
    [Authorize(Policy = "UserOrAdmin")]
    public IActionResult Delete(int id)
    {
        var existing = _equipmentTypeRepository.GetById(id);
        if (existing == null)
            return NotFound();

        _equipmentTypeRepository.Delete(id);

        return NoContent();
    }
}
    
