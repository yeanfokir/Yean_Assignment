using System;
using System.Drawing;
using System.Windows.Forms;

namespace EmployeeDetails
{
    public partial class frmRegister : Form
    {
        public frmRegister()
        {
            InitializeComponent();
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;
            string conPassword = txtConPassword.Text;

            // Fix the bug: use || instead of &&
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(conPassword))
            {
                MessageBox.Show("Username and Password fields are required.", "Registration Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (password != conPassword)
            {
                MessageBox.Show("Passwords do not match. Please re-enter.", "Registration Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPassword.Text = "";
                txtConPassword.Text = "";
                txtPassword.Focus();
                return;
            }

            if (User.UsernameExists(username))
            {
                MessageBox.Show("Username already exists. Please choose a different username.", "Registration Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return;
            }

            int newUserId = User.RegisterUser(username, password, "User");
            if (newUserId > 0)
            {
                MessageBox.Show("Your account has been successfully created!", "Registration Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtUsername.Text = "";
                txtPassword.Text = "";
                txtConPassword.Text = "";

                frmLogin login = new frmLogin();
                login.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("An error occurred during registration. Please try again.", "Registration Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void checkbxShowPas_CheckedChanged(object sender, EventArgs e)
        {
            if (checkbxShowPas.Checked)
            {
                txtPassword.PasswordChar = '\0';
                txtConPassword.PasswordChar = '\0';
            }
            else
            {
                txtPassword.PasswordChar = '•';
                txtConPassword.PasswordChar = '•';
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtUsername.Text = "";
            txtPassword.Text = "";
            txtConPassword.Text = "";
            txtUsername.Focus();
        }

        private void clickLogin_Click(object sender, EventArgs e)
        {
            frmLogin login = new frmLogin();
            login.Show();
            this.Hide();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void txtUsername_TextChanged(object sender, EventArgs e)
        {

        }

        private void frmRegister_Load(object sender, EventArgs e)
        {
            txtPassword.PasswordChar = '•';
            txtConPassword.PasswordChar = '•';
        }
    }
}
