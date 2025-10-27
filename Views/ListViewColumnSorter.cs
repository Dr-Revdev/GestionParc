using System.Collections;
using System.Windows.Forms;

namespace ProjetParc.Views;

/// <summary>
/// Classe permettant de trier les colonnes d'un ListView par ordre alphabétique ou numérique.
/// Supporte le tri croissant et décroissant en cliquant sur l'en-tête de colonne.
/// </summary>
public class ListViewColumnSorter : IComparer
{
    private int _columnToSort;
    private SortOrder _orderOfSort;

    /// <summary>
    /// Initialise un nouveau trieur avec tri croissant sur la première colonne.
    /// </summary>
    public ListViewColumnSorter()
    {
        _columnToSort = 0;
        _orderOfSort = SortOrder.None;
    }

    /// <summary>
    /// Compare deux éléments ListViewItem selon la colonne et l'ordre définis.
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
    /// Définit la colonne à trier et inverse l'ordre si on clique sur la même colonne.
    /// </summary>
    /// <param name="column">Index de la colonne à trier.</param>
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
    /// Obtient ou définit l'ordre de tri actuel.
    /// </summary>
    public SortOrder Order
    {
        get => _orderOfSort;
        set => _orderOfSort = value;
    }

    /// <summary>
    /// Obtient ou définit la colonne actuellement triée.
    /// </summary>
    public int SortColumn
    {
        get => _columnToSort;
        set => _columnToSort = value;
    }
}
