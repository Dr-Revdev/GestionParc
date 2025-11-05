using System.Collections;
using System.Windows.Forms;

namespace ProjetParc.Views;

/// <summary>
/// Permet de trier les colonnes des ListView quand on clique sur l'en-tête
/// Gère le tri alphabétique et numérique (détecte automatiquement)
/// </summary>
public class ListViewColumnSorter : IComparer
{
    private int _columnToSort;
    private SortOrder _orderOfSort;

    /// <summary>
    /// Constructeur - par défaut pas de tri au départ
    /// </summary>
    public ListViewColumnSorter()
    {
        _columnToSort = 0;
        _orderOfSort = SortOrder.None;
    }

    /// <summary>
    /// Compare deux lignes du ListView - c'est la méthode appelée automatiquement pour le tri
    /// Essaie d'abord en numérique, sinon en alphabétique
    /// </summary>
    public int Compare(object x, object y)
    {
        if (x is not ListViewItem itemX || y is not ListViewItem itemY)
            return 0;

        string textX = _columnToSort == 0 
            ? itemX.Text 
            : (_columnToSort < itemX.SubItems.Count ? itemX.SubItems[_columnToSort].Text : "");
        
        string textY = _columnToSort == 0 
            ? itemY.Text 
            : (_columnToSort < itemY.SubItems.Count ? itemY.SubItems[_columnToSort].Text : "");

        int compareResult;

        // Tentative de comparaison numérique
        if (int.TryParse(textX, out int numX) && int.TryParse(textY, out int numY))
        {
            compareResult = numX.CompareTo(numY);
        }
        else
        {
            // Comparaison alphabétique (insensible à la casse)
            compareResult = string.Compare(textX, textY, StringComparison.OrdinalIgnoreCase);
        }

        // Inverse le résultat si tri décroissant
        if (_orderOfSort == SortOrder.Descending)
            compareResult = -compareResult;

        return compareResult;
    }

    /// <summary>
    /// Change la colonne de tri - si on clique 2 fois sur la même, ça inverse l'ordre
    /// </summary>
    /// <param name="column">Numéro de la colonne (0 = première)</param>
    public void SetSortColumn(int column)
    {
        if (column == _columnToSort)
        {
            // Inverse l'ordre si on clique sur la même colonne
            _orderOfSort = _orderOfSort == SortOrder.Ascending 
                ? SortOrder.Descending 
                : SortOrder.Ascending;
        }
        else
        {
            // Nouvelle colonne : tri croissant par défaut
            _columnToSort = column;
            _orderOfSort = SortOrder.Ascending;
        }
    }

    /// <summary>
    /// L'ordre de tri actuel (Ascending/Descending/None)
    /// </summary>
    public SortOrder Order
    {
        get => _orderOfSort;
        set => _orderOfSort = value;
    }

    /// <summary>
    /// Numéro de la colonne qu'on est en train de trier
    /// </summary>
    public int SortColumn
    {
        get => _columnToSort;
        set => _columnToSort = value;
    }
}
