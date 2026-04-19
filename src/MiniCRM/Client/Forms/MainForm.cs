using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Windows.Forms;

using MiniCRM.Client.Services;
using MiniCRM.Core.Contracts;

using CRMClient = MiniCRM.Core.Models.Client;

namespace MiniCRM.Client.Forms
{
    public partial class MainForm : Form
    {
        private List<CRMClient> _clients;

        public MainForm()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            SetupGrid();
            LoadClients();
        }

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

        private void LoadClients(string query = null)
        {
            try
            {
                using (var svc = new CrmServiceClient())
                {
                    _clients = string.IsNullOrWhiteSpace(query)
                        ? svc.GetAllClients()
                        : svc.SearchClients(query);
                }
                dgvClients.DataSource = null;
                dgvClients.DataSource = _clients;
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

        private void TxtSearch_Click(object sender, EventArgs e)
        {

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

        #region Helper Functions

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
