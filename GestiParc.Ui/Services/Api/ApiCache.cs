using GestiParc.Core.DTOs;

namespace GestiParc.Ui.Services.Api;

/// <summary>
/// Cache statique pour les données qui changent rarement (types, sites, équipes)
/// Réduit le nombre d'appels API et améliore les performances
/// </summary>
public static class ApiCache
{
    private static List<EquipmentTypeDto>? _types;
    private static List<SiteDto>? _sites;
    private static List<EquipeDto>? _equipes;
    
    private static DateTime _typesExpiry = DateTime.MinValue;
    private static DateTime _sitesExpiry = DateTime.MinValue;
    private static DateTime _equipesExpiry = DateTime.MinValue;
    
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private static readonly object _lock = new object();

    // Getters avec vérification d'expiration
    public static List<EquipmentTypeDto>? GetTypes()
    {
        lock (_lock)
        {
            return DateTime.Now < _typesExpiry ? _types : null;
        }
    }

    public static List<SiteDto>? GetSites()
    {
        lock (_lock)
        {
            return DateTime.Now < _sitesExpiry ? _sites : null;
        }
    }

    public static List<EquipeDto>? GetEquipes()
    {
        lock (_lock)
        {
            return DateTime.Now < _equipesExpiry ? _equipes : null;
        }
    }

    // Setters avec durée d'expiration
    public static void SetTypes(List<EquipmentTypeDto> types)
    {
        lock (_lock)
        {
            _types = types;
            _typesExpiry = DateTime.Now.Add(CacheDuration);
        }
    }

    public static void SetSites(List<SiteDto> sites)
    {
        lock (_lock)
        {
            _sites = sites;
            _sitesExpiry = DateTime.Now.Add(CacheDuration);
        }
    }

    public static void SetEquipes(List<EquipeDto> equipes)
    {
        lock (_lock)
        {
            _equipes = equipes;
            _equipesExpiry = DateTime.Now.Add(CacheDuration);
        }
    }

    // Invalidation du cache
    public static void InvalidateTypes()
    {
        lock (_lock)
        {
            _types = null;
            _typesExpiry = DateTime.MinValue;
        }
    }

    public static void InvalidateSites()
    {
        lock (_lock)
        {
            _sites = null;
            _sitesExpiry = DateTime.MinValue;
        }
    }

    public static void InvalidateEquipes()
    {
        lock (_lock)
        {
            _equipes = null;
            _equipesExpiry = DateTime.MinValue;
        }
    }

    public static void InvalidateAll()
    {
        lock (_lock)
        {
            _types = null;
            _sites = null;
            _equipes = null;
            _typesExpiry = DateTime.MinValue;
            _sitesExpiry = DateTime.MinValue;
            _equipesExpiry = DateTime.MinValue;
        }
    }
}
