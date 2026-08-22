using System;
using System.Data;
using System.Windows.Forms;

namespace EmployeeDetails
{
    public partial class frmEmployee : Form
    {
        private Employee employee = new Employee();

        public frmEmployee()
        {
            InitializeComponent();
        }

        private void frmEmployee_Load(object sender, EventArgs e)
        {
            LoadEmployeeData();
            if (!string.IsNullOrEmpty(Session.Username))
            {
                lblLoggedInUser.Text = string.Format("Current User: {0}", Session.Username);
            }
        }

        private void LoadEmployeeData()
        {
            try
            {
                DataTable dt = employee.GetEmployees();
                dgvEmployeeDetails.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading employee data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtEmpId.Text) || string.IsNullOrWhiteSpace(txtEmpName.Text))
            {
                MessageBox.Show("Employee ID and Name are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                employee.EmpId = txtEmpId.Text.Trim();
                employee.EmpName = txtEmpName.Text.Trim();
                employee.Age = txtAge.Text.Trim();
                employee.ContactNo = txtContactNo.Text.Trim();
                employee.Gender = (cboGender.SelectedItem != null) ? cboGender.SelectedItem.ToString() : cboGender.Text;
                
                // Stamp the logged in user's ID
                employee.CreatedBy = (Session.UserID > 0) ? (int?)Session.UserID : null;

                bool success = employee.InsertEmployee(employee);
                LoadEmployeeData();
                ClearControls();

                if (success)
                {
                    MessageBox.Show("Employee has been added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Failed to add employee. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtEmpId.Text))
            {
                MessageBox.Show("Please select an employee to update.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                employee.EmpId = txtEmpId.Text.Trim();
                employee.EmpName = txtEmpName.Text.Trim();
                employee.Age = txtAge.Text.Trim();
                employee.ContactNo = txtContactNo.Text.Trim();
                employee.Gender = (cboGender.SelectedItem != null) ? cboGender.SelectedItem.ToString() : cboGender.Text;

                bool success = employee.UpdateEmployee(employee);
                LoadEmployeeData();
                ClearControls();

                if (success)
                {
                    MessageBox.Show("Employee has been updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Failed to update employee. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtEmpId.Text))
            {
                MessageBox.Show("Please select an employee to delete.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Bonus Feature: Confirm dialog before delete
            DialogResult confirm = MessageBox.Show(string.Format("Are you sure you want to delete employee '{0}' ({1})?", txtEmpName.Text, txtEmpId.Text), "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes)
            {
                return;
            }

            try
            {
                employee.EmpId = txtEmpId.Text.Trim();
                bool success = employee.DeleteEmployee(employee);
                LoadEmployeeData();
                ClearControls();

                if (success)
                {
                    MessageBox.Show("Employee has been deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Failed to delete employee. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearControls();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                string term = txtSearch.Text.Trim();
                if (string.IsNullOrEmpty(term))
                {
                    LoadEmployeeData();
                }
                else
                {
                    DataTable dt = employee.SearchEmployees(term);
                    dgvEmployeeDetails.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Search Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            LoadEmployeeData();
        }

        private void ClearControls()
        {
            txtEmpId.Text = "";
            txtEmpName.Text = "";
            txtAge.Text = "";
            txtContactNo.Text = "";
            cboGender.SelectedIndex = -1;
            cboGender.Text = "";
        }

        private void dgvEmployeeDetails_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            PopulateFromGrid(e.RowIndex);
        }

        private void dgvEmployeeDetails_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                PopulateFromGrid(e.RowIndex);
            }
        }

        private void PopulateFromGrid(int rowIndex)
        {
            if (rowIndex >= 0 && rowIndex < dgvEmployeeDetails.Rows.Count)
            {
                DataGridViewRow row = dgvEmployeeDetails.Rows[rowIndex];
                if (row.Cells["EmpId"].Value != null)
                    txtEmpId.Text = row.Cells["EmpId"].Value.ToString();
                if (row.Cells["EmpName"].Value != null)
                    txtEmpName.Text = row.Cells["EmpName"].Value.ToString();
                if (row.Cells["EmpAge"].Value != null)
                    txtAge.Text = row.Cells["EmpAge"].Value.ToString();
                if (row.Cells["EmpContact"].Value != null)
                    txtContactNo.Text = row.Cells["EmpContact"].Value.ToString();
                if (row.Cells["EmpGender"].Value != null)
                    cboGender.Text = row.Cells["EmpGender"].Value.ToString();
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
