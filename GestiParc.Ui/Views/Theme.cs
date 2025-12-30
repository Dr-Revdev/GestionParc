namespace GestiParc.Ui.Views;

/// <summary>
/// Gère tout le style de l'appli (couleurs, polices, etc.)
/// J'ai gardé que ce qui est vraiment utilisé pour pas surcharger
/// </summary>
public static class Theme
{
    // Palette de couleurs (uniquement les couleurs utilisées)
    public static class Colors
    {
        // Couleurs primaires
        public static readonly Color Primary = Color.FromArgb(52, 73, 94);
        public static readonly Color PrimaryDark = Color.FromArgb(44, 62, 80);
        public static readonly Color PrimaryLight = Color.FromArgb(69, 90, 100);
        
        // Couleurs secondaires
        public static readonly Color Secondary = Color.FromArgb(96, 125, 139);
        public static readonly Color SecondaryDark = Color.FromArgb(84, 110, 122);
        
        // Couleurs d'état (utilisées par StyleSuccessButton et StyleDangerButton)
        public static readonly Color Success = Color.FromArgb(92, 184, 92);
        public static readonly Color Danger = Color.FromArgb(217, 83, 79);
        
        // Couleurs de surface
        public static readonly Color Background = Color.FromArgb(245, 245, 245);
        public static readonly Color Surface = Color.White;
        public static readonly Color SurfaceHover = Color.FromArgb(250, 250, 250);
        
        // Bordures
        public static readonly Color Border = Color.FromArgb(220, 220, 220);
        
        // Couleurs de texte
        public static readonly Color TextPrimary = Color.FromArgb(33, 33, 33);
        public static readonly Color TextSecondary = Color.FromArgb(117, 117, 117);
        public static readonly Color TextOnPrimary = Color.White;
    }
    
    // Polices (uniquement les polices utilisées)
    public static class Fonts
    {
        private const string FontFamily = "Segoe UI";
        
        // Headers
        public static readonly Font H1 = new Font(FontFamily, 28f, FontStyle.Bold);
        public static readonly Font H3 = new Font(FontFamily, 20f, FontStyle.Bold);
        public static readonly Font H5 = new Font(FontFamily, 14f, FontStyle.Bold);
        
        // Body text
        public static readonly Font Body = new Font(FontFamily, 11f);
        public static readonly Font BodyLarge = new Font(FontFamily, 13f);
        public static readonly Font BodySmall = new Font(FontFamily, 10f);
        
        // Spécial
        public static readonly Font Button = new Font(FontFamily, 10f, FontStyle.Regular);
        public static readonly Font Caption = new Font(FontFamily, 9f);
        public static readonly Font Label = new Font(FontFamily, 10f, FontStyle.Bold);
    }
    
    // Espacements (uniquement les espacements utilisés)
    public static class Spacing
    {
        public const int Small = 8;
        public const int Medium = 16;
        public const int Large = 24;
        public const int XLarge = 32;
    }
    
    // Tailles standardisées pour les contrôles
    public static class Sizes
    {
        // Boutons standards
        public const int ButtonWidth = 120;
        public const int ButtonHeight = 40;
        public const int ButtonHeightLarge = 45;
        public const int ButtonHeightXLarge = 70;
        public const int ButtonWidthLarge = 300;
        
        // Champs de formulaire (TextBox, ComboBox)
        public const int InputHeight = 36;
        
        // Boutons de recherche
        public const int SearchButtonSize = 32;
        
        // Colonnes de ListView
        public const int ColumnWidthSmall = 200;
        public const int ColumnWidthMedium = 250;
        public const int ColumnWidthLarge = 800;
    }
    
    /// <summary>
    /// Style de bouton principal (bleu foncé) avec effet au survol
    /// Le paramètre setHeight permet de garder la hauteur actuelle si besoin
    /// </summary>
    public static Button StylePrimaryButton(Button button, string? text = null, bool setHeight = true)
    {
        if (text != null) button.Text = text;
        button.BackColor = Colors.Primary;
        button.ForeColor = Colors.TextOnPrimary;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.Cursor = Cursors.Hand;
        button.TextAlign = ContentAlignment.MiddleCenter;
        button.UseCompatibleTextRendering = false;
        button.AutoSize = false;
        
        if (setHeight)
        {
            button.Height = 40;
            button.Font = Fonts.Button;
            button.Padding = new Padding(0);
        }
        else
        {
            button.Padding = new Padding(0);
        }
        
        // Effets hover
        button.MouseEnter += (s, e) => {
            button.BackColor = Colors.PrimaryDark;
        };
        button.MouseLeave += (s, e) => {
            button.BackColor = Colors.Primary;
        };
        
        return button;
    }
    
    /// <summary>
    /// Style bouton secondaire (gris/bleu) - moins important que le primaire
    /// </summary>
    public static Button StyleSecondaryButton(Button button, string? text = null, bool setHeight = true)
    {
        if (text != null) button.Text = text;
        button.BackColor = Colors.Secondary;
        button.ForeColor = Colors.TextOnPrimary;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.Cursor = Cursors.Hand;
        button.TextAlign = ContentAlignment.MiddleCenter;
        button.UseCompatibleTextRendering = false;
        button.AutoSize = false;
        
        if (setHeight)
        {
            button.Height = 40;
            button.Font = Fonts.Button;
            button.Padding = new Padding(0);
        }
        else
        {
            button.Padding = new Padding(0);
        }
        
        button.MouseEnter += (s, e) => {
            button.BackColor = Colors.SecondaryDark;
        };
        button.MouseLeave += (s, e) => {
            button.BackColor = Colors.Secondary;
        };
        
        return button;
    }
    
    /// <summary>
    /// Bouton avec juste le contour coloré (style "outline")
    /// Utile pour les actions secondaires
    /// </summary>
    public static Button StyleOutlineButton(Button button, string? text = null, bool setHeight = true)
    {
        if (text != null) button.Text = text;
        button.BackColor = Color.Transparent;
        button.ForeColor = Colors.Primary;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderColor = Colors.Primary;
        button.FlatAppearance.BorderSize = 2;
        button.Cursor = Cursors.Hand;
        button.TextAlign = ContentAlignment.MiddleCenter;
        button.UseCompatibleTextRendering = false;
        button.AutoSize = false;
        
        if (setHeight)
        {
            button.Height = 40;
            button.Font = Fonts.Button;
            button.Padding = new Padding(0);
        }
        else
        {
            button.Padding = new Padding(0);
        }
        
        button.MouseEnter += (s, e) => {
            button.BackColor = Colors.PrimaryLight;
            button.ForeColor = Colors.TextOnPrimary;
        };
        button.MouseLeave += (s, e) => {
            button.BackColor = Color.Transparent;
            button.ForeColor = Colors.Primary;
        };
        
        return button;
    }
    
    /// <summary>
    /// Bouton vert pour les actions positives (valider, sauvegarder, etc)
    /// </summary>
    public static Button StyleSuccessButton(Button button, string? text = null, bool setHeight = true)
    {
        if (text != null) button.Text = text;
        button.BackColor = Colors.Success;
        button.ForeColor = Colors.TextOnPrimary;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.Cursor = Cursors.Hand;
        button.TextAlign = ContentAlignment.MiddleCenter;
        button.UseCompatibleTextRendering = false;
        button.AutoSize = false;
        
        if (setHeight)
        {
            button.Height = 40;
            button.Font = Fonts.Button;
            button.Padding = new Padding(0);
        }
        else
        {
            button.Padding = new Padding(0);
        }
        
        button.MouseEnter += (s, e) => {
            button.BackColor = Color.FromArgb(67, 160, 71);
        };
        button.MouseLeave += (s, e) => {
            button.BackColor = Colors.Success;
        };
        
        return button;
    }
    
    /// <summary>
    /// Bouton rouge pour les actions risquées (supprimer, annuler)
    /// </summary>
    public static Button StyleDangerButton(Button button, string? text = null, bool setHeight = true)
    {
        if (text != null) button.Text = text;
        button.BackColor = Colors.Danger;
        button.ForeColor = Colors.TextOnPrimary;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.Cursor = Cursors.Hand;
        button.TextAlign = ContentAlignment.MiddleCenter;
        button.UseCompatibleTextRendering = false;
        button.AutoSize = false;
        
        if (setHeight)
        {
            button.Height = 40;
            button.Font = Fonts.Button;
            button.Padding = new Padding(0);
        }
        else
        {
            button.Padding = new Padding(0);
        }
        
        button.MouseEnter += (s, e) => {
            button.BackColor = Color.FromArgb(229, 57, 53);
        };
        button.MouseLeave += (s, e) => {
            button.BackColor = Colors.Danger;
        };
        
        return button;
    }
    
    /// <summary>
    /// Style pour les champs de texte - police et bordures
    /// </summary>
    public static TextBox StyleTextBox(TextBox textBox)
    {
        textBox.Font = Fonts.Body;
        textBox.BorderStyle = BorderStyle.FixedSingle;
        textBox.BackColor = Colors.Surface;
        textBox.ForeColor = Colors.TextPrimary;
        
        return textBox;
    }
    
    /// <summary>
    /// Style pour les listes déroulantes
    /// </summary>
    public static ComboBox StyleComboBox(ComboBox comboBox)
    {
        comboBox.Font = Fonts.Body;
        comboBox.FlatStyle = FlatStyle.Flat;
        comboBox.BackColor = Colors.Surface;
        comboBox.ForeColor = Colors.TextPrimary;
        
        return comboBox;
    }

    /// <summary>
    /// Améliore la lisibilité d'une ListView (zébrage + auto-dimensionnement des colonnes).
    /// Appeler typiquement après avoir (re)rempli Items.
    /// </summary>
    public static void ApplyListViewReadability(ListView listView, int maxColumnWidth = Sizes.ColumnWidthLarge)
    {
        ApplyListViewAlternatingRowColors(listView);
        AutoResizeListViewColumns(listView, maxColumnWidth);
    }

    /// <summary>
    /// Applique une alternance de couleur légère sur les lignes (blanc / gris très clair).
    /// </summary>
    public static void ApplyListViewAlternatingRowColors(ListView listView)
    {
        var even = Colors.Surface;
        var odd = Colors.SurfaceHover;

        for (int i = 0; i < listView.Items.Count; i++)
        {
            listView.Items[i].BackColor = (i % 2 == 0) ? even : odd;
        }
    }

    /// <summary>
    /// Auto-dimensionne les colonnes en gardant l'en-tête lisible.
    /// Ne modifie pas les colonnes volontairement cachées (Width = 0).
    /// </summary>
    public static void AutoResizeListViewColumns(ListView listView, int maxColumnWidth = Sizes.ColumnWidthLarge)
    {
        if (listView.Columns.Count == 0)
        {
            return;
        }

        void DoResize()
        {
            // Garantit que l'en-tête reste lisible tout en s'adaptant au contenu.
            var headerWidths = new int[listView.Columns.Count];
            for (int i = 0; i < listView.Columns.Count; i++)
            {
                if (listView.Columns[i].Width <= 0) continue;

                listView.AutoResizeColumn(i, ColumnHeaderAutoResizeStyle.HeaderSize);
                headerWidths[i] = listView.Columns[i].Width;
            }

            for (int i = 0; i < listView.Columns.Count; i++)
            {
                if (listView.Columns[i].Width <= 0) continue;

                listView.AutoResizeColumn(i, ColumnHeaderAutoResizeStyle.ColumnContent);
                var desired = Math.Max(listView.Columns[i].Width, headerWidths[i]);
                listView.Columns[i].Width = Math.Min(maxColumnWidth, desired);
            }
        }

        if (!listView.IsHandleCreated)
        {
            // Si appelé trop tôt, on reporte après création du handle.
            listView.HandleCreated += (_, __) => DoResize();
            return;
        }

        DoResize();
    }
}
