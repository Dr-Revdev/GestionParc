namespace ProjetParc.Views.Loan.Models;

/// <summary>
/// Représente un équipement sélectionnable dans l'interface
/// </summary>
public class EquipmentItem
{
    public string Id { get; set; }
    public string DisplayName { get; set; }
    public override string ToString() => DisplayName;
}
