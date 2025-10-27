namespace ProjetParc.Views.Loan.Models;

/// <summary>
/// Représente un agent sélectionnable dans l'interface
/// </summary>
public class AgentItem
{
    public string Idrh { get; set; }
    public string DisplayName { get; set; }
    public override string ToString() => DisplayName;
}
