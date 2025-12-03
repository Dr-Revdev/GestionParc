using System.Collections.Generic;
using GestiParc.Core.DTOs;
using GestiParc.Core.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GestiParc.Api.Controllers
{
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
        [HttpPut]
        public IActionResult Update([FromBody] EquipmentDto dto)
        {
            if (dto == null)
                return BadRequest("Payload vide.");
            
            _equipmentRepository.Update(dto);

            return Update(nameof(GetById), new { id = dto.IdEquipement }, dto);
        }
    }
}
