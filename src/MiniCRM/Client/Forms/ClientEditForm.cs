using System;
using System.Linq;
using System.Windows.Forms;

using MiniCRM.Core.Models;

namespace MiniCRM.Client.Forms
{
    public partial class ClientEditForm : Form
    {
        public CRMClient Result { get; private set; }
        private readonly CRMClient _existing;

        public ClientEditForm() : this(null) { }

        public ClientEditForm(CRMClient existing)
        {
            InitializeComponent();
            _existing = existing;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            cmbStatus.DataSource = Enum.GetValues(typeof(CRMClientStatus))
                .Cast<CRMClientStatus>()
                .Select(x => new
                {
                    Value = x,
                    Text = x.GetDescription()
                })
                .ToList();

            cmbStatus.DisplayMember = "Text";
            cmbStatus.ValueMember = "Value";

            cmbStatus.SelectedIndex = 0;

            if (_existing != null)
            {
                Text = "Редактировать клиента";
                txtFullName.Text = _existing.FullName;
                txtPhone.Text = _existing.Phone;
                txtEmail.Text = _existing.Email;
                txtCompany.Text = _existing.Company;
                cmbStatus.SelectedItem = _existing.Status;
            }
            else
            {
                Text = "Новый клиент";
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("ФИО обязательно", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Result = new CRMClient
            {
                Id = _existing?.Id ?? 0,
                FullName = txtFullName.Text.Trim(),
                Phone = txtPhone.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                Company = txtCompany.Text.Trim(),
                Status = (CRMClientStatus)cmbStatus.SelectedValue,
                CreatedAt = _existing?.CreatedAt ?? DateTime.Now
            };

            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
