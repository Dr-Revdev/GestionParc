using System;
using System.Drawing;
using System.Windows.Forms;
using GestiParc.Ui.Data;
using GestiParc.Ui.Services;
using GestiParc.Infrastructure.Data.Repositories;

namespace GestiParc.Ui.Views.Auth
{
    /// <summary>
    /// Formulaire de connexion avec authentification utilisateur
    /// </summary>
    public class LoginView : Form
    {
        private readonly UtilisateurMySqlRepository _utilisateurRepo;
        private TextBox _txtUsername = null!;
        private TextBox _txtPassword = null!;
        private Button _btnLogin = null!;
        private Label _lblTitle = null!;
        private Label _lblUsername = null!;
        private Label _lblPassword = null!;
        private Label _lblError = null!;

        public LoginView()
        {
            _utilisateurRepo = new UtilisateurMySqlRepository();
            InitializeComponents();
            ApplyTheme();
        }

        private void InitializeComponents()
        {
            // Configuration du formulaire
            Text = "GestiParc - Connexion";
            Size = new Size(400, 350);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            
            // Icône de l'application
            try
            {
                Icon = new Icon("GestionParc.ico");
            }
            catch { /* Icône non trouvée, utiliser l'icône par défaut */ }

            // Titre
            _lblTitle = new Label
            {
                Text = "CONNEXION",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                Location = new Point(100, 30),
                Size = new Size(200, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Label Username
            _lblUsername = new Label
            {
                Text = "Nom d'utilisateur",
                Location = new Point(50, 100),
                Size = new Size(130, 20),
                Font = new Font("Segoe UI", 10)
            };

            // TextBox Username
            _txtUsername = new TextBox
            {
                Location = new Point(50, 125),
                Size = new Size(300, 30),
                Font = new Font("Segoe UI", 11)
            };

            // Label Password
            _lblPassword = new Label
            {
                Text = "Mot de passe",
                Location = new Point(50, 165),
                Size = new Size(130, 20),
                Font = new Font("Segoe UI", 10)
            };

            // TextBox Password
            _txtPassword = new TextBox
            {
                Location = new Point(50, 190),
                Size = new Size(300, 30),
                Font = new Font("Segoe UI", 11),
                PasswordChar = '●',
                UseSystemPasswordChar = false
            };

            // Label Erreur
            _lblError = new Label
            {
                Location = new Point(50, 230),
                Size = new Size(300, 20),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Red,
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false
            };

            // Bouton Login
            _btnLogin = new Button
            {
                Text = "SE CONNECTER",
                Location = new Point(100, 260),
                Size = new Size(200, 40)
            };

            // Événements
            _btnLogin.Click += BtnLogin_Click;
            _txtPassword.KeyPress += TxtPassword_KeyPress;
            _txtUsername.KeyPress += TxtPassword_KeyPress;

            // Ajout des contrôles
            Controls.Add(_lblTitle);
            Controls.Add(_lblUsername);
            Controls.Add(_txtUsername);
            Controls.Add(_lblPassword);
            Controls.Add(_txtPassword);
            Controls.Add(_lblError);
            Controls.Add(_btnLogin);
        }

        private void ApplyTheme()
        {
            // Couleurs de base
            BackColor = Theme.Colors.Background;
            _lblTitle.ForeColor = Theme.Colors.Primary;
            _lblUsername.ForeColor = Theme.Colors.TextPrimary;
            _lblPassword.ForeColor = Theme.Colors.TextPrimary;
            
            // Application des styles Theme
            Theme.StyleTextBox(_txtUsername);
            Theme.StyleTextBox(_txtPassword);
            Theme.StylePrimaryButton(_btnLogin);
        }

        private void TxtPassword_KeyPress(object? sender, KeyPressEventArgs e)
        {
            // Connexion avec la touche Enter
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                BtnLogin_Click(sender, EventArgs.Empty);
            }
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            // Masquer le message d'erreur précédent
            _lblError.Visible = false;

            // Validation des champs
            if (string.IsNullOrWhiteSpace(_txtUsername.Text))
            {
                ShowError("Veuillez saisir votre nom d'utilisateur");
                _txtUsername.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(_txtPassword.Text))
            {
                ShowError("Veuillez saisir votre mot de passe");
                _txtPassword.Focus();
                return;
            }

            // Tentative d'authentification
            try
            {
                _btnLogin.Enabled = false;
                _btnLogin.Text = "CONNEXION...";
                Cursor = Cursors.WaitCursor;

                var utilisateur = _utilisateurRepo.Authentifier(_txtUsername.Text.Trim(), _txtPassword.Text);

                if (utilisateur != null)
                {
                    // Connexion réussie
                    SessionManager.UtilisateurCourant = utilisateur;
                    
                    // Ouvrir la page d'accueil
                    var welcomePage = new WelcomePage();
                    welcomePage.FormClosed += (s, args) => Close();
                    Hide();
                    welcomePage.Show();
                }
                else
                {
                    // Échec de l'authentification
                    ShowError("Nom d'utilisateur ou mot de passe incorrect");
                    _txtPassword.Clear();
                    _txtPassword.Focus();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Erreur lors de la connexion : {ex.Message}");
            }
            finally
            {
                _btnLogin.Enabled = true;
                _btnLogin.Text = "SE CONNECTER";
                Cursor = Cursors.Default;
            }
        }

        private void ShowError(string message)
        {
            _lblError.Text = message;
            _lblError.Visible = true;
        }
    }
}
