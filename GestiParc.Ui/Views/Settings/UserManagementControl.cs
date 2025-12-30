using GestiParc.Core.DTOs;
using GestiParc.Ui.Data;
using GestiParc.Ui.Services.Api;

namespace GestiParc.Ui.Views.Settings;

internal sealed class UserManagementControl : UserControl
{
    private readonly UtilisateurApiClient _utilisateurApiClient = new UtilisateurApiClient();

    private ListView _listView = null!;
    private ListViewColumnSorter _sorter = null!;

    private TextBox _txtUsername = null!;
    private TextBox _txtNom = null!;
    private TextBox _txtPrenom = null!;
    private ComboBox _cmbRole = null!;
    private CheckBox _chkActif = null!;
    private TextBox _txtPassword = null!;
    private TextBox _txtConfirmPassword = null!;
    private Button _btnCreate = null!;
    private Button _btnRefresh = null!;

    private Label _lblSelected = null!;
    private ComboBox _cmbSelectedRole = null!;
    private CheckBox _chkSelectedActif = null!;
    private Button _btnApplySelected = null!;
    private Button _btnResetPassword = null!;
    private Button _btnDeleteSelected = null!;

    private int? _selectedUserId;

    public UserManagementControl()
    {
        Dock = DockStyle.Fill;
        BackColor = Theme.Colors.Background;

        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Theme.Colors.Background,
            Padding = new Padding(Theme.Spacing.Medium)
        };
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

        mainLayout.Controls.Add(CreateListPanel(), 0, 0);
        mainLayout.Controls.Add(CreateRightPanel(), 1, 0);

        Controls.Add(mainLayout);

        Load += async (s, e) => await RefreshAsync();
    }

    private Control CreateListPanel()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 0, Theme.Spacing.Medium, 0),
            BackColor = Theme.Colors.Background
        };

        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 48,
            ColumnCount = 2,
            BackColor = Theme.Colors.Background
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));

        var title = new Label
        {
            Text = "Utilisateurs",
            Font = Theme.Fonts.H5,
            ForeColor = Theme.Colors.TextPrimary,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };

        _btnRefresh = new Button
        {
            Text = "Actualiser",
            Dock = DockStyle.Fill,
            Height = Theme.Sizes.ButtonHeight
        };
        Theme.StyleOutlineButton(_btnRefresh);
        _btnRefresh.Click += async (_, __) => await RefreshAsync();

        header.Controls.Add(title, 0, 0);
        header.Controls.Add(_btnRefresh, 1, 0);

        _listView = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            Font = Theme.Fonts.Body,
            BackColor = Theme.Colors.Surface,
            BorderStyle = BorderStyle.FixedSingle
        };

        _listView.Columns.Add("ID", 0);
        _listView.Columns.Add("Username", 140);
        _listView.Columns.Add("Nom", 150);
        _listView.Columns.Add("Prénom", 150);
        _listView.Columns.Add("Rôle", 90);
        _listView.Columns.Add("Actif", 70);
        _listView.Columns.Add("Dernière connexion", 160);

        _sorter = new ListViewColumnSorter();
        _listView.ListViewItemSorter = _sorter;
        _listView.ColumnClick += (s, e) => { _sorter.SetSortColumn(e.Column); _listView.Sort(); Theme.ApplyListViewAlternatingRowColors(_listView); };
        _listView.SelectedIndexChanged += OnSelectionChanged;

        panel.Controls.Add(_listView);
        panel.Controls.Add(header);

        return panel;
    }

    private Control CreateRightPanel()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Theme.Colors.Background,
            AutoScroll = true
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Theme.Colors.Background
        };

        layout.Controls.Add(CreateCreateGroup(), 0, 0);
        layout.Controls.Add(CreateSelectedGroup(), 0, 1);

        panel.Controls.Add(layout);
        return panel;
    }

    private GroupBox CreateCreateGroup()
    {
        var group = new GroupBox
        {
            Text = "Créer un utilisateur",
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Font = Theme.Fonts.Body,
            BackColor = Theme.Colors.Background,
            Padding = new Padding(Theme.Spacing.Medium)
        };

        var form = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            RowCount = 8,
            BackColor = Theme.Colors.Background
        };
        form.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        // Lignes en AutoSize pour éviter le clipping (DPI / redimensionnement)
        for (var i = 0; i < 7; i++)
            form.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        // Bouton: hauteur contrôlée
        form.RowStyles.Add(new RowStyle(SizeType.Absolute, Theme.Sizes.ButtonHeightLarge + Theme.Spacing.Small));

        form.RowStyles[4].SizeType = SizeType.Absolute;
        form.RowStyles[4].Height = Theme.Sizes.InputHeight;

        form.Controls.Add(CreateLabel("Username"), 0, 0);
        _txtUsername = CreateTextBox();
        form.Controls.Add(_txtUsername, 1, 0);

        form.Controls.Add(CreateLabel("Nom"), 0, 1);
        _txtNom = CreateTextBox();
        form.Controls.Add(_txtNom, 1, 1);

        form.Controls.Add(CreateLabel("Prénom"), 0, 2);
        _txtPrenom = CreateTextBox();
        form.Controls.Add(_txtPrenom, 1, 2);

        form.Controls.Add(CreateLabel("Rôle"), 0, 3);
        _cmbRole = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = Theme.Fonts.Body, IntegralHeight = false };
        _cmbRole.Items.AddRange(new object[] { "USER", "ADMIN" });
        _cmbRole.SelectedIndex = 0;
        form.Controls.Add(_cmbRole, 1, 3);

        form.Controls.Add(CreateLabel("Actif"), 0, 4);
        _chkActif = new CheckBox
        {
            Checked = true,
            AutoSize = false,
            Width = 18,
            Height = Theme.Sizes.InputHeight,
            Dock = DockStyle.Left,
            Text = "",
            BackColor = Theme.Colors.Background
        };
        form.Controls.Add(_chkActif, 1, 4);

        form.Controls.Add(CreateLabel("Password"), 0, 5);
        _txtPassword = CreatePasswordBox();
        form.Controls.Add(_txtPassword, 1, 5);

        form.Controls.Add(CreateLabel("Confirmer"), 0, 6);
        _txtConfirmPassword = CreatePasswordBox();
        form.Controls.Add(_txtConfirmPassword, 1, 6);

        _btnCreate = new Button
        {
            Text = "Créer",
            Dock = DockStyle.Fill,
            Height = Theme.Sizes.ButtonHeightLarge,
            Margin = new Padding(0, Theme.Spacing.Small, 0, 0)
        };
        Theme.StyleSuccessButton(_btnCreate);
        _btnCreate.Click += async (_, __) => await CreateAsync();

        form.SetColumnSpan(_btnCreate, 2);
        form.Controls.Add(_btnCreate, 0, 7);

        group.Controls.Add(form);

        return group;
    }

    private GroupBox CreateSelectedGroup()
    {
        var group = new GroupBox
        {
            Text = "Utilisateur sélectionné",
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Font = Theme.Fonts.Body,
            BackColor = Theme.Colors.Background,
            Padding = new Padding(Theme.Spacing.Medium)
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            RowCount = 6,
            BackColor = Theme.Colors.Background
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, Theme.Sizes.ButtonHeight + Theme.Spacing.Small));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, Theme.Sizes.ButtonHeight + Theme.Spacing.Small));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, Theme.Sizes.ButtonHeight + Theme.Spacing.Small));

        layout.RowStyles[2].SizeType = SizeType.Absolute;
        layout.RowStyles[2].Height = Theme.Sizes.InputHeight;

        _lblSelected = new Label
        {
            Text = "Aucun",
            Dock = DockStyle.Fill,
            Font = Theme.Fonts.Body,
            ForeColor = Theme.Colors.TextSecondary,
            TextAlign = ContentAlignment.MiddleLeft
        };
        layout.SetColumnSpan(_lblSelected, 2);
        layout.Controls.Add(_lblSelected, 0, 0);

        layout.Controls.Add(CreateLabel("Rôle"), 0, 1);
        _cmbSelectedRole = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = Theme.Fonts.Body, IntegralHeight = false };
        _cmbSelectedRole.Items.AddRange(new object[] { "USER", "ADMIN" });
        _cmbSelectedRole.SelectedIndex = 0;
        layout.Controls.Add(_cmbSelectedRole, 1, 1);

        layout.Controls.Add(CreateLabel("Actif"), 0, 2);
        _chkSelectedActif = new CheckBox
        {
            AutoSize = false,
            Width = 18,
            Height = Theme.Sizes.InputHeight,
            Dock = DockStyle.Left,
            BackColor = Theme.Colors.Background
        };
        layout.Controls.Add(_chkSelectedActif, 1, 2);

        _btnApplySelected = new Button { Text = "Appliquer", Dock = DockStyle.Fill, Height = Theme.Sizes.ButtonHeight, Margin = new Padding(0, Theme.Spacing.Small, 0, 0) };
        Theme.StylePrimaryButton(_btnApplySelected);
        _btnApplySelected.Click += async (_, __) => await ApplySelectedAsync();
        layout.Controls.Add(_btnApplySelected, 1, 3);

        _btnResetPassword = new Button { Text = "Reset mdp", Dock = DockStyle.Fill, Height = Theme.Sizes.ButtonHeight, Margin = new Padding(0, Theme.Spacing.Small, 0, 0) };
        Theme.StyleSecondaryButton(_btnResetPassword);
        _btnResetPassword.Click += async (_, __) => await ResetPasswordAsync();
        layout.Controls.Add(_btnResetPassword, 1, 4);

        _btnDeleteSelected = new Button { Text = "Supprimer", Dock = DockStyle.Fill, Height = Theme.Sizes.ButtonHeight, Margin = new Padding(0, Theme.Spacing.Small, 0, 0) };
        Theme.StyleDangerButton(_btnDeleteSelected);
        _btnDeleteSelected.Click += async (_, __) => await DeleteSelectedAsync();
        layout.Controls.Add(_btnDeleteSelected, 1, 5);

        group.Controls.Add(layout);
        SetSelectedEnabled(false);

        return group;
    }

    private static Label CreateLabel(string text)
    {
        return new Label
        {
            Text = text,
            Font = Theme.Fonts.Body,
            ForeColor = Theme.Colors.TextPrimary,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(0, 8, 0, 0)
        };
    }

    private static TextBox CreateTextBox()
    {
        var tb = new TextBox
        {
            Dock = DockStyle.Top,
            Height = Theme.Sizes.InputHeight,
            Font = Theme.Fonts.Body
        };
        Theme.StyleTextBox(tb);
        return tb;
    }

    private static TextBox CreatePasswordBox()
    {
        var tb = new TextBox
        {
            Dock = DockStyle.Top,
            Height = Theme.Sizes.InputHeight,
            Font = Theme.Fonts.Body,
            PasswordChar = '●',
            UseSystemPasswordChar = false
        };
        Theme.StyleTextBox(tb);
        return tb;
    }

    private void OnSelectionChanged(object? sender, EventArgs e)
    {
        if (_listView.SelectedItems.Count == 0)
        {
            _selectedUserId = null;
            _lblSelected.Text = "Aucun";
            SetSelectedEnabled(false);
            return;
        }

        var item = _listView.SelectedItems[0];
        if (!int.TryParse(item.SubItems[0].Text, out var id))
        {
            _selectedUserId = null;
            _lblSelected.Text = "Aucun";
            SetSelectedEnabled(false);
            return;
        }
 
        _selectedUserId = id;
        _lblSelected.Text = $"Sélection : {item.SubItems[1].Text}";

        var role = item.SubItems[4].Text;
        var actif = item.SubItems[5].Text == "Oui";

        _cmbSelectedRole.SelectedIndex = role == "ADMIN" ? 1 : 0;
        _chkSelectedActif.Checked = actif;

        SetSelectedEnabled(true);

        // Refus simple: pas de suppression du compte connecté
        var currentUserId = SessionManager.UtilisateurCourant?.Id;
        if (currentUserId != null && currentUserId.Value == id)
        {
            _btnDeleteSelected.Enabled = false;
            return;
        }

        // Empêche aussi la suppression du dernier ADMIN (même contrôle que côté API)
        if (role == "ADMIN")
        {
            var adminCount = _listView.Items
                .Cast<ListViewItem>()
                .Count(i => i.SubItems.Count > 4 && i.SubItems[4].Text == "ADMIN");

            if (adminCount <= 1)
                _btnDeleteSelected.Enabled = false;
        }
    }

     private void SetSelectedEnabled(bool enabled)
     {
         _cmbSelectedRole.Enabled = enabled;
         _chkSelectedActif.Enabled = enabled;
         _btnApplySelected.Enabled = enabled;
         _btnResetPassword.Enabled = enabled;
         _btnDeleteSelected.Enabled = enabled;
     }

     private async Task DeleteSelectedAsync()
     {
         if (_selectedUserId == null)
             return;

         var confirm = MessageBox.Show(
             "Supprimer cet utilisateur ?\n\nCette action est définitive.",
             "Confirmation",
             MessageBoxButtons.OKCancel,
             MessageBoxIcon.Warning);

         if (confirm != DialogResult.OK)
             return;

         try
         {
             _btnDeleteSelected.Enabled = false;
             await _utilisateurApiClient.DeleteAsync(_selectedUserId.Value);
             await RefreshAsync();
             MessageBox.Show("Utilisateur supprimé.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
         }
         catch (Exception ex)
         {
             MessageBox.Show(ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
         }
         finally
         {
             _btnDeleteSelected.Enabled = true;
         }
     }

     private async Task RefreshAsync()
     {
         try
         {
             _btnRefresh.Enabled = false;
             _listView.Items.Clear();

             var users = await _utilisateurApiClient.GetAllAsync();
             foreach (var u in users)
             {
                 var item = new ListViewItem(u.Id.ToString());
                 item.SubItems.Add(u.Username);
                 item.SubItems.Add(u.Nom);
                 item.SubItems.Add(u.Prenom);
                 item.SubItems.Add(u.Role);
                 item.SubItems.Add(u.Actif ? "Oui" : "Non");
                 item.SubItems.Add(u.DerniereConnexion?.ToString("dd/MM/yyyy HH:mm") ?? "-");

                 _listView.Items.Add(item);
             }

             Theme.ApplyListViewReadability(_listView);

             _selectedUserId = null;
             _lblSelected.Text = "Aucun";
             SetSelectedEnabled(false);
         }
         catch (Exception ex)
         {
             MessageBox.Show(ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
         }
         finally
         {
             _btnRefresh.Enabled = true;
         }
     }

     private async Task CreateAsync()
     {
         if (string.IsNullOrWhiteSpace(_txtUsername.Text))
         {
             MessageBox.Show("Username requis.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
             return;
         }

         if (string.IsNullOrWhiteSpace(_txtPassword.Text) || string.IsNullOrWhiteSpace(_txtConfirmPassword.Text))
         {
             MessageBox.Show("Mot de passe requis.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
             return;
         }

         if (_txtPassword.Text != _txtConfirmPassword.Text)
         {
             MessageBox.Show("Le mot de passe et sa confirmation ne correspondent pas.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
             return;
         }

         try
         {
             _btnCreate.Enabled = false;

             var request = new CreateUtilisateurRequestDto
             {
                 Username = _txtUsername.Text.Trim(),
                 Nom = _txtNom.Text.Trim(),
                 Prenom = _txtPrenom.Text.Trim(),
                 Role = _cmbRole.SelectedItem?.ToString() ?? "USER",
                 Actif = _chkActif.Checked,
                 Password = _txtPassword.Text,
                 ConfirmPassword = _txtConfirmPassword.Text
             };

             await _utilisateurApiClient.CreateAsync(request);

             _txtUsername.Clear();
             _txtNom.Clear();
             _txtPrenom.Clear();
             _cmbRole.SelectedIndex = 0;
             _chkActif.Checked = true;
             _txtPassword.Clear();
             _txtConfirmPassword.Clear();

             await RefreshAsync();
             MessageBox.Show("Utilisateur créé.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
         }
         catch (Exception ex)
         {
             MessageBox.Show(ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
         }
         finally
         {
             _btnCreate.Enabled = true;
         }
     }

     private async Task ApplySelectedAsync()
     {
         if (_selectedUserId == null)
             return;

         try
         {
             _btnApplySelected.Enabled = false;

             var role = _cmbSelectedRole.SelectedItem?.ToString() ?? "USER";
             await _utilisateurApiClient.SetRoleAsync(_selectedUserId.Value, role);
             await _utilisateurApiClient.SetActifAsync(_selectedUserId.Value, _chkSelectedActif.Checked);

             await RefreshAsync();
             MessageBox.Show("Modifications appliquées.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
         }
         catch (Exception ex)
         {
             MessageBox.Show(ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
         }
         finally
         {
             _btnApplySelected.Enabled = true;
         }
     }

     private async Task ResetPasswordAsync()
     {
         if (_selectedUserId == null)
             return;

         var confirm = MessageBox.Show(
             "Générer un mot de passe temporaire ?\n\nIl sera affiché une seule fois.",
             "Confirmation",
             MessageBoxButtons.OKCancel,
             MessageBoxIcon.Warning);

         if (confirm != DialogResult.OK)
             return;

         try
         {
             _btnResetPassword.Enabled = false;
             var result = await _utilisateurApiClient.ResetPasswordAsync(_selectedUserId.Value);

             try
             {
                 Clipboard.SetText(result.TemporaryPassword);
             }
             catch
             {
                 // ignore clipboard errors
             }

             ShowTemporaryPasswordDialog(result.TemporaryPassword);
         }
         catch (Exception ex)
         {
             MessageBox.Show(ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
         }
         finally
         {
             _btnResetPassword.Enabled = true;
         }
     }

     private static void ShowTemporaryPasswordDialog(string temporaryPassword)
     {
         var dialog = new Form
         {
             Text = "Mot de passe temporaire",
             StartPosition = FormStartPosition.CenterParent,
             FormBorderStyle = FormBorderStyle.FixedDialog,
             MaximizeBox = false,
             MinimizeBox = false,
             ShowInTaskbar = false,
             BackColor = Theme.Colors.Background,
             ClientSize = new Size(520, 180)
         };

         var layout = new TableLayoutPanel
         {
             Dock = DockStyle.Fill,
             ColumnCount = 2,
             RowCount = 4,
             Padding = new Padding(Theme.Spacing.Medium),
             BackColor = Theme.Colors.Background
         };
         layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
         layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
         layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
         layout.RowStyles.Add(new RowStyle(SizeType.Absolute, Theme.Sizes.InputHeight + Theme.Spacing.Small));
         layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
         layout.RowStyles.Add(new RowStyle(SizeType.Absolute, Theme.Sizes.ButtonHeight + Theme.Spacing.Small));

         var info = new Label
         {
             Text = "Mot de passe temporaire (copié automatiquement).\nTu peux aussi sélectionner le texte et faire Ctrl+C.",
             Dock = DockStyle.Fill,
             Font = Theme.Fonts.Body,
             ForeColor = Theme.Colors.TextPrimary
         };
         layout.SetColumnSpan(info, 2);
         layout.Controls.Add(info, 0, 0);

         var txt = new TextBox
         {
             Dock = DockStyle.Fill,
             Font = Theme.Fonts.Body,
             ReadOnly = true,
             ShortcutsEnabled = true,
             Text = temporaryPassword
         };
         Theme.StyleTextBox(txt);
         layout.SetColumnSpan(txt, 2);
         layout.Controls.Add(txt, 0, 1);

         var hint = new Label
         {
             Text = "Ce mot de passe est affiché une seule fois.",
             Dock = DockStyle.Fill,
             Font = Theme.Fonts.Body,
             ForeColor = Theme.Colors.TextSecondary
         };
         layout.SetColumnSpan(hint, 2);
         layout.Controls.Add(hint, 0, 2);

         var btnCopy = new Button { Text = "Copier", Dock = DockStyle.Fill, Height = Theme.Sizes.ButtonHeight };
         Theme.StyleSecondaryButton(btnCopy);
         btnCopy.Click += (_, __) =>
         {
             try { Clipboard.SetText(temporaryPassword); } catch { /* ignore */ }
         };
         layout.Controls.Add(btnCopy, 1, 3);

         var btnOk = new Button { Text = "OK", Dock = DockStyle.Fill, Height = Theme.Sizes.ButtonHeight };
         Theme.StylePrimaryButton(btnOk);
         btnOk.Click += (_, __) => dialog.Close();
         layout.Controls.Add(btnOk, 0, 3);

         dialog.Controls.Add(layout);

         dialog.Shown += (_, __) =>
         {
             try
             {
                 txt.Focus();
                 txt.SelectAll();
             }
             catch
             {
                 // ignore
             }
         };

         dialog.ShowDialog();
     }
}
