using GestiParc.Core.Domain.Entities;
using GestiParc.Core.DTOs;
using GestiParc.Core.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GestiParc.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UtilisateurController : ControllerBase
{
    private readonly IUtilisateurRepository _utilisateurRepository;

    public UtilisateurController(IUtilisateurRepository utilisateurRepository)
    {
        _utilisateurRepository = utilisateurRepository;
    }

    // GET /api/utilisateur
    [HttpGet]
    public ActionResult<List<UtilisateurDto>> GetAll()
    {
        var entities = _utilisateurRepository.GetAll();
        var dtos = entities.Select(EntityToDto).ToList();
        return Ok(dtos);
    }

    // GET /api/utilisateur/{id}
    [HttpGet("{id}")]
    public ActionResult<UtilisateurDto> GetById(int id)
    {
        var entity = _utilisateurRepository.GetById(id);
        if (entity == null)
            return NotFound();

        return Ok(EntityToDto(entity));
    }

    // POST /api/utilisateur/authentifier
    [HttpPost("authentifier")]
    public ActionResult<UtilisateurDto> Authentifier([FromBody] AuthRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Username et password requis.");

        var entity = _utilisateurRepository.Authentifier(request.Username, request.Password);
        
        if (entity == null)
            return Unauthorized();

        return Ok(EntityToDto(entity));
    }

    // POST /api/utilisateur
    [HttpPost]
    public IActionResult Create([FromBody] UtilisateurDto dto)
    {
        if (dto == null)
            return BadRequest("Payload vide.");

        var entity = DtoToEntity(dto);
        _utilisateurRepository.Insert(entity);

        return Ok(dto);
    }

    // PUT /api/utilisateur/{id}
    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] UtilisateurDto dto)
    {
        if (dto == null || dto.Id != id)
            return BadRequest();

        var existing = _utilisateurRepository.GetById(id);
        if (existing == null)
            return NotFound();

        var entity = DtoToEntity(dto);
        _utilisateurRepository.Update(entity);

        return NoContent();
    }

    // DELETE /api/utilisateur/{id}
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var existing = _utilisateurRepository.GetById(id);
        if (existing == null)
            return NotFound();

        _utilisateurRepository.Delete(id);

        return NoContent();
    }

    private static UtilisateurDto EntityToDto(Utilisateur entity)
    {
        return new UtilisateurDto
        {
            Id = entity.Id,
            Username = entity.Username,
            Nom = entity.Nom,
            Prenom = entity.Prenom,
            Role = entity.Role,
            DateCreation = entity.DateCreation,
            DerniereConnexion = entity.DerniereConnexion,
            Actif = entity.Actif
        };
    }

    private static Utilisateur DtoToEntity(UtilisateurDto dto)
    {
        return new Utilisateur
        {
            Id = dto.Id,
            Username = dto.Username,
            Nom = dto.Nom,
            Prenom = dto.Prenom,
            Role = dto.Role,
            DateCreation = dto.DateCreation,
            DerniereConnexion = dto.DerniereConnexion,
            Actif = dto.Actif
        };
    }
}

public class AuthRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}
