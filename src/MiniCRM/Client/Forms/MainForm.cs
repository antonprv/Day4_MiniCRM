using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Windows.Forms;

using MiniCRM.Client.Interop;
using MiniCRM.Client.Services;
using MiniCRM.Core.Contracts;
using MiniCRM.Core.Models;

namespace MiniCRM.Client.Forms
{
    public partial class MainForm : Form
    {
        private string _defaultSearchText;
        private List<CRMClient> _allClients;

        public MainForm()
        {
            InitializeComponent();

            _defaultSearchText = txtSearch.Text;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            SetupGrid();
            LoadClients();
        }

        #region Forms Events

        private void TxtSearch_Click(object sender, EventArgs e)
        {
            txtSearch.Text = string.Empty;
        }

        private void TxtSearch_Leave(object sender, EventArgs e)
        {
            if (txtSearch.Text == string.Empty)
                txtSearch.Text = _defaultSearchText;
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            LoadClients(txtSearch.Text);
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (var form = new ClientEditForm())
            {
                if (form.ShowDialog() != DialogResult.OK) return;
                try
                {
                    using (var svc = new CrmServiceClient())
                        svc.AddClient(form.Result);
                    LoadClients();
                }
                catch (FaultException<ClientFault> ex)
                {
                    MessageBox.Show(ex.Detail.Message, "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            var client = GetSelectedClient();
            if (client == null) return;

            using (var form = new ClientEditForm(client))
            {
                if (form.ShowDialog() != DialogResult.OK) return;
                try
                {
                    using (var svc = new CrmServiceClient())
                        svc.UpdateClient(form.Result);
                    LoadClients();
                }
                catch (FaultException<ClientFault> ex)
                {
                    MessageBox.Show(ex.Detail.Message, "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            var client = GetSelectedClient();
            if (client == null) return;

            var confirm = MessageBox.Show(
                $"Удалить клиента «{client.FullName}»?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            try
            {
                using (var svc = new CrmServiceClient())
                    svc.DeleteClient(client.Id);
                LoadClients();
            }
            catch (FaultException<ClientFault> ex)
            {
                MessageBox.Show(ex.Detail.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Clients Loading

        private void LoadClients(string query = null)
        {
            try
            {
                using (var svc = new CrmServiceClient())
                    _allClients = svc.GetAllClients();

                // Если есть запрос — фильтруем через C++ DLL
                // Если нет — показываем всё
                var toShow = string.IsNullOrWhiteSpace(query)
                    ? _allClients
                    : ClientFilterInterop.Filter(_allClients, query);

                dgvClients.DataSource = null;
                dgvClients.DataSource = toShow;
            }
            catch (FaultException<ClientFault> ex)
            {
                MessageBox.Show(ex.Detail.Message, "Ошибка сервиса",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Нет связи с сервисом:\n{ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Visual Helper Functions

        private void SetupGrid()
        {
            dgvClients.AutoGenerateColumns = false;
            dgvClients.ReadOnly = true;
            dgvClients.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvClients.MultiSelect = false;

            dgvClients.Columns.Add(new DataGridViewTextBoxColumn
            { DataPropertyName = "FullName", HeaderText = "ФИО", FillWeight = 40 });
            dgvClients.Columns.Add(new DataGridViewTextBoxColumn
            { DataPropertyName = "Phone", HeaderText = "Телефон", FillWeight = 40 });
            dgvClients.Columns.Add(new DataGridViewTextBoxColumn
            { DataPropertyName = "Email", HeaderText = "Email", FillWeight = 20 });
            dgvClients.Columns.Add(new DataGridViewTextBoxColumn
            { DataPropertyName = "Company", HeaderText = "Компания", FillWeight = 20 });
            dgvClients.Columns.Add(new DataGridViewTextBoxColumn
            { DataPropertyName = "Status", HeaderText = "Статус", FillWeight = 20 });
        }

        private CRMClient GetSelectedClient()
        {
            if (dgvClients.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите клиента в списке", "Подсказка",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }
            return dgvClients.SelectedRows[0].DataBoundItem as CRMClient;
        }

        #endregion
    }
}
