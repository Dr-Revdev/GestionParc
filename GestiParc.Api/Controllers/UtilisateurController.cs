using GestiParc.Core.Domain.Entities;
using GestiParc.Core.DTOs;
using GestiParc.Core.Interfaces.Repositories;
using GestiParc.Api.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestiParc.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UtilisateurController : ControllerBase
{
    private readonly IUtilisateurRepository _utilisateurRepository;
    private readonly JwtTokenService _jwtTokenService;

    public UtilisateurController(IUtilisateurRepository utilisateurRepository, JwtTokenService jwtTokenService)
    {
        _utilisateurRepository = utilisateurRepository;
        _jwtTokenService = jwtTokenService;
    }

    // GET /api/utilisateur
    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    public ActionResult<List<UtilisateurDto>> GetAll()
    {
        var entities = _utilisateurRepository.GetAll();
        var dtos = entities.Select(EntityToDto).ToList();
        return Ok(dtos);
    }

    // GET /api/utilisateur/{id}
    [HttpGet("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public ActionResult<UtilisateurDto> GetById(int id)
    {
        var entity = _utilisateurRepository.GetById(id);
        if (entity == null)
            return NotFound();

        return Ok(EntityToDto(entity));
    }

    // POST /api/utilisateur/authentifier
    [HttpPost("authentifier")]
    [AllowAnonymous]
    public ActionResult<AuthResponseDto> Authentifier([FromBody] AuthRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Username et password requis.");

        var entity = _utilisateurRepository.Authentifier(request.Username, request.Password);
        
        if (entity == null)
            return Unauthorized();

        var token = _jwtTokenService.CreateToken(entity, out var expiresInSeconds);
        var response = new AuthResponseDto
        {
            User = EntityToDto(entity),
            Token = token,
            ExpiresIn = expiresInSeconds
        };

        return Ok(response);
    }

    // POST /api/utilisateur
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public IActionResult Create([FromBody] CreateUtilisateurRequestDto dto)
    {
        if (dto == null)
            return BadRequest("Payload vide.");

        if (string.IsNullOrWhiteSpace(dto.Username))
            return BadRequest("Username requis.");
        if (string.IsNullOrWhiteSpace(dto.Password) || string.IsNullOrWhiteSpace(dto.ConfirmPassword))
            return BadRequest("Password requis.");
        if (dto.Password != dto.ConfirmPassword)
            return BadRequest("Le mot de passe et sa confirmation ne correspondent pas.");

        if (!TryNormalizeRole(dto.Role, out var normalizedRole))
            return BadRequest("Role invalide (ADMIN/USER).");

        if (_utilisateurRepository.ExistsByUsername(dto.Username))
            return Conflict("Username déjà utilisé.");

        if (!TryValidatePassword(dto.Password, out var passwordError))
            return BadRequest(passwordError);

        var entity = new Utilisateur
        {
            Username = dto.Username.Trim(),
            Nom = dto.Nom?.Trim() ?? "",
            Prenom = dto.Prenom?.Trim() ?? "",
            Role = normalizedRole,
            Actif = dto.Actif
        };

        var newId = _utilisateurRepository.Create(entity, dto.Password);
        var created = _utilisateurRepository.GetById(newId);
        if (created == null)
            return Ok(new UtilisateurDto { Id = newId, Username = entity.Username, Nom = entity.Nom, Prenom = entity.Prenom, Role = entity.Role, Actif = entity.Actif });

        return Ok(EntityToDto(created));
    }

    // POST /api/utilisateur/changer-motdepasse
    [HttpPost("changer-motdepasse")]
    [Authorize(Policy = "UserOrAdmin")]
    public IActionResult ChangerMotDePasse([FromBody] ChangePasswordRequestDto dto)
    {
        if (dto == null)
            return BadRequest("Payload vide.");

        var sub = User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                  ?? User?.FindFirst("sub")?.Value;
        if (string.IsNullOrWhiteSpace(sub) || !int.TryParse(sub, out var userId))
            return Unauthorized();

        var user = _utilisateurRepository.GetById(userId);
        if (user == null || !user.Actif)
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(dto.OldPassword) || string.IsNullOrWhiteSpace(dto.NewPassword) || string.IsNullOrWhiteSpace(dto.ConfirmNewPassword))
            return BadRequest("Ancien/Nouveau/Confirmation requis.");
        if (dto.NewPassword != dto.ConfirmNewPassword)
            return BadRequest("Le nouveau mot de passe et sa confirmation ne correspondent pas.");
        if (dto.OldPassword == dto.NewPassword)
            return BadRequest("Le nouveau mot de passe doit être différent de l'ancien.");

        if (!_utilisateurRepository.CheckPassword(user.Id, dto.OldPassword))
            return Unauthorized();

        if (!TryValidatePassword(dto.NewPassword, out var passwordError))
            return BadRequest(passwordError);

        _utilisateurRepository.ChangePassword(user.Id, dto.NewPassword);
        return NoContent();
    }

    // POST /api/utilisateur/{id}/reset-motdepasse
    [HttpPost("{id}/reset-motdepasse")]
    [Authorize(Policy = "AdminOnly")]
    public ActionResult<ResetPasswordResponseDto> ResetMotDePasse(int id)
    {
        var existing = _utilisateurRepository.GetById(id);
        if (existing == null)
            return NotFound();

        var tempPassword = GenerateTemporaryPassword();
        _utilisateurRepository.ChangePassword(id, tempPassword);

        return Ok(new ResetPasswordResponseDto
        {
            UserId = id,
            TemporaryPassword = tempPassword
        });
    }

    // PUT /api/utilisateur/{id}/role
    [HttpPut("{id}/role")]
    [Authorize(Policy = "AdminOnly")]
    public IActionResult SetRole(int id, [FromBody] SetRoleRequestDto dto)
    {
        if (dto == null)
            return BadRequest("Payload vide.");

        var existing = _utilisateurRepository.GetById(id);
        if (existing == null)
            return NotFound();

        if (!TryNormalizeRole(dto.Role, out var normalizedRole))
            return BadRequest("Role invalide (ADMIN/USER).");

        _utilisateurRepository.SetRole(id, normalizedRole);
        return NoContent();
    }

    // PUT /api/utilisateur/{id}/actif
    [HttpPut("{id}/actif")]
    [Authorize(Policy = "AdminOnly")]
    public IActionResult SetActif(int id, [FromBody] SetActifRequestDto dto)
    {
        if (dto == null)
            return BadRequest("Payload vide.");

        var existing = _utilisateurRepository.GetById(id);
        if (existing == null)
            return NotFound();

        _utilisateurRepository.SetActif(id, dto.Actif);
        return NoContent();
    }

    // PUT /api/utilisateur/{id}
    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
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
    [Authorize(Policy = "AdminOnly")]
    public IActionResult Delete(int id)
    {
        var sub = User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                  ?? User?.FindFirst("sub")?.Value;
        if (string.IsNullOrWhiteSpace(sub) || !int.TryParse(sub, out var callerId))
            return Unauthorized();

        if (callerId == id)
            return BadRequest("Impossible de supprimer votre propre compte.");

        var existing = _utilisateurRepository.GetById(id);
        if (existing == null)
            return NotFound();

        if (string.Equals(existing.Role, "ADMIN", StringComparison.OrdinalIgnoreCase))
        {
            var adminCount = _utilisateurRepository
                .GetAll()
                .Count(u => string.Equals(u.Role, "ADMIN", StringComparison.OrdinalIgnoreCase));

            if (adminCount <= 1)
                return BadRequest("Impossible de supprimer le dernier administrateur.");
        }

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

    private static bool TryNormalizeRole(string? role, out string normalizedRole)
    {
        normalizedRole = (role ?? "").Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(normalizedRole))
        {
            normalizedRole = "USER";
            return true;
        }

        return normalizedRole is "ADMIN" or "USER";
    }

    private static bool TryValidatePassword(string password, out string error)
    {
        error = "";

        if (string.IsNullOrWhiteSpace(password))
        {
            error = "Mot de passe requis.";
            return false;
        }

        if (password.Length < 12)
        {
            error = "Le mot de passe doit contenir au moins 12 caractères.";
            return false;
        }

        if (!password.Any(char.IsUpper))
        {
            error = "Le mot de passe doit contenir au moins une majuscule.";
            return false;
        }

        if (!password.Any(char.IsLower))
        {
            error = "Le mot de passe doit contenir au moins une minuscule.";
            return false;
        }

        if (!password.Any(char.IsDigit))
        {
            error = "Le mot de passe doit contenir au moins un chiffre.";
            return false;
        }

        if (!password.Any(c => !char.IsLetterOrDigit(c)))
        {
            error = "Le mot de passe doit contenir au moins un caractère spécial.";
            return false;
        }

        return true;
    }

    private static string GenerateTemporaryPassword(int length = 16)
    {
        const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lower = "abcdefghijkmnopqrstuvwxyz";
        const string digits = "23456789";
        const string special = "!@#$%*_-+";

        if (length < 12)
            length = 12;

        var chars = new List<char>(length)
        {
            upper[RandomNumberGenerator.GetInt32(upper.Length)],
            lower[RandomNumberGenerator.GetInt32(lower.Length)],
            digits[RandomNumberGenerator.GetInt32(digits.Length)],
            special[RandomNumberGenerator.GetInt32(special.Length)]
        };

        var all = upper + lower + digits + special;
        while (chars.Count < length)
        {
            chars.Add(all[RandomNumberGenerator.GetInt32(all.Length)]);
        }

        // Shuffle (Fisher–Yates)
        for (var i = chars.Count - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        return new string(chars.ToArray());
    }
}
