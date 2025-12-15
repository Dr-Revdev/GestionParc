using GestiParc.Core.DTOs;
using GestiParc.Core.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GestiParc.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EquipmentController : ControllerBase
{
    private readonly IEquipmentRepository _equipmentRepository;

    public EquipmentController(IEquipmentRepository equipmentRepository)
    {
        _equipmentRepository = equipmentRepository;
    }

    // GET /api/equipment
    [HttpGet]
    public ActionResult<List<EquipmentDto>> GetAll()
    {
        var items = _equipmentRepository.GetAll();
        return Ok(items);
    }

    // GET /api/equipment/{id}
    [HttpGet("{id}")]
    public ActionResult<EquipmentDto> GetById(string id)
    {
        var item = _equipmentRepository.GetById(id);
        if (item == null)
            return NotFound();
        
        return Ok(item);
    }

    // POST /api/equipment
    [HttpPost]
    public IActionResult Create([FromBody] EquipmentDto dto)
    {
        if (dto == null)
            return BadRequest("Payload vide.");

        _equipmentRepository.Insert(dto);

        return CreatedAtAction(nameof(GetById), new { id = dto.IdEquipement }, dto);
    }

    // PUT /api/equipment/{id}
    [HttpPut("{id}")]
    public IActionResult Update(string id, [FromBody] EquipmentDto dto)
    {
        if (dto == null)
            return BadRequest("Payload vide.");

        if (id != dto.IdEquipement)
            return BadRequest("L'ID de l'URL ne correspond pas à l'ID de l'équipement.");
        
        var existing = _equipmentRepository.GetById(id);
        if (existing == null)
            return NotFound();

        _equipmentRepository.Update(dto);

        return NoContent();
    }

    // DELETE /api/equipment/{id}
    [HttpDelete("{id}")]
    public IActionResult Delete(string id)
    {
        
        var existing = _equipmentRepository.GetById(id);
        if (existing == null)
            return NotFound();
        
        _equipmentRepository.Delete(id);

        return NoContent();
    }

}

