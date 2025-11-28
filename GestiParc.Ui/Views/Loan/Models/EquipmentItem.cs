namespace GestiParc.Ui.Views.Loan.Models;

/// <summary>
/// Représente un équipement sélectionnable dans l'interface
/// </summary>
public class EquipmentItem
{
    public required string Id { get; set; }
    public required string DisplayName { get; set; }
    public override string ToString() => DisplayName;
}
