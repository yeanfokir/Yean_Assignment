using System;
using System.Windows.Forms;

namespace EmployeeDetails
{
    public partial class frmDashboard : Form
    {
        public frmDashboard()
        {
            InitializeComponent();
        }

        private void frmDashboard_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Session.Username))
            {
                lblUser.Text = string.Format("Logged in as: {0} ({1})", Session.Username, Session.Role ?? "User");
            }
            else
            {
                lblUser.Text = "Logged in as: Guest";
            }
        }

        private void btnManageEmployees_Click(object sender, EventArgs e)
        {
            frmEmployee empForm = new frmEmployee();
            empForm.ShowDialog();
        }

        private void visitWeb_Click(object sender, EventArgs e)
        {
            bmBrowser.Navigate("https://bloggingmetrics.com/");
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to log out?", "Confirm Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                // Stamp logout in LoginHistory
                User.LogActivity(Session.UserID, Session.Username, "LOGOUT");

                // Clear session
                Session.Clear();

                // Show fresh login form
                frmLogin login = new frmLogin();
                login.Show();

                // Close the dashboard cleanly
                this.Close();
            }
        }
    }
}
