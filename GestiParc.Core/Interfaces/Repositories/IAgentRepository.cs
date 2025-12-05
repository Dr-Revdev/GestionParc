using GestiParc.Core.DTOs;

namespace GestiParc.Core.Interfaces.Repositories;

/// <summary>
/// Interface du repository pour la gestion des agents
/// </summary>
public interface IAgentRepository
{
    void Insert(AgentDto agent);
    void Update(AgentDto agent);
    void Delete(string idrh);
    AgentDto? GetById(string idrh);
    List<AgentDto> GetAll();
    List<AgentDto> GetByEquipe(int equipeId);
    List<AgentDto> GetBySite(int siteId);
    bool Exists(string idrh);
}
