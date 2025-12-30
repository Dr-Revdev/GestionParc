using GestiParc.Core.DTOs;
using GestiParc.Ui.Services.Api;

namespace GestiParc.Ui.Views.Settings;

internal sealed class PasswordChangeControl : UserControl
{
    private readonly UtilisateurApiClient _utilisateurApiClient = new UtilisateurApiClient();

    private TextBox _txtOld = null!;
    private TextBox _txtNew = null!;
    private TextBox _txtConfirm = null!;
    private Button _btnChange = null!;

    public PasswordChangeControl()
    {
        Dock = DockStyle.Fill;
        BackColor = Theme.Colors.Background;
        Padding = new Padding(Theme.Spacing.Medium);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            RowCount = 6,
            BackColor = Theme.Colors.Background
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

        var title = new Label
        {
            Text = "Changer mon mot de passe",
            Font = Theme.Fonts.H5,
            ForeColor = Theme.Colors.TextPrimary,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };
        layout.SetColumnSpan(title, 2);
        layout.Controls.Add(title, 0, 0);

        layout.Controls.Add(CreateLabel("Ancien mot de passe"), 0, 1);
        _txtOld = CreatePasswordBox();
        layout.Controls.Add(_txtOld, 1, 1);

        layout.Controls.Add(CreateLabel("Nouveau mot de passe"), 0, 2);
        _txtNew = CreatePasswordBox();
        layout.Controls.Add(_txtNew, 1, 2);

        layout.Controls.Add(CreateLabel("Confirmer"), 0, 3);
        _txtConfirm = CreatePasswordBox();
        layout.Controls.Add(_txtConfirm, 1, 3);

        var help = new Label
        {
            Text = "Règles : 12 caractères min. + 1 majuscule, 1 minuscule, 1 chiffre, 1 spécial.",
            Font = Theme.Fonts.Caption,
            ForeColor = Theme.Colors.TextSecondary,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };
        layout.SetColumnSpan(help, 2);
        layout.Controls.Add(help, 0, 5);

        _btnChange = new Button
        {
            Text = "Modifier",
            Width = Theme.Sizes.ButtonWidth,
            Height = Theme.Sizes.ButtonHeight,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, Theme.Spacing.Small, 0, 0)
        };
        Theme.StyleSuccessButton(_btnChange);
        _btnChange.Click += async (_, __) => await ChangeAsync();
        layout.Controls.Add(_btnChange, 1, 4);

        Controls.Add(layout);
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

    private async Task ChangeAsync()
    {
        if (string.IsNullOrWhiteSpace(_txtOld.Text) || string.IsNullOrWhiteSpace(_txtNew.Text) || string.IsNullOrWhiteSpace(_txtConfirm.Text))
        {
            MessageBox.Show("Veuillez remplir tous les champs.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_txtNew.Text != _txtConfirm.Text)
        {
            MessageBox.Show("Le nouveau mot de passe et sa confirmation ne correspondent pas.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            _btnChange.Enabled = false;
            var dto = new ChangePasswordRequestDto
            {
                OldPassword = _txtOld.Text,
                NewPassword = _txtNew.Text,
                ConfirmNewPassword = _txtConfirm.Text
            };

            await _utilisateurApiClient.ChangePasswordAsync(dto);

            _txtOld.Clear();
            _txtNew.Clear();
            _txtConfirm.Clear();

            MessageBox.Show("Mot de passe modifié.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _btnChange.Enabled = true;
        }
    }
}
