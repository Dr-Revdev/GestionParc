namespace GestiParc.Ui.Views.Loan.Models;

/// <summary>
/// Représente un agent sélectionnable dans l'interface
/// </summary>
public class AgentItem
{
    public required string Idrh { get; set; }
    public required string DisplayName { get; set; }
    public override string ToString() => DisplayName;
}
