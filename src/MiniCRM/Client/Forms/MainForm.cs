using System;
using System.Collections.Generic;
using System.Drawing;
using System.ServiceModel;
using System.Windows.Forms;
using MiniCRM.Client.Controllers;
using MiniCRM.Client.Infrastructure;
using MiniCRM.Core.Contracts;
using MiniCRM.Core.Models;

namespace MiniCRM.Client.Forms
{
    public partial class MainForm : Form
    {
        private readonly ClientsController _controller = new ClientsController();
        private readonly Debounce _debounce = new Debounce();

        private string _defaultSearchText;

        // search button color
        // ------------
        private readonly Color _cancelActiveColor = Color.DimGray;
        private readonly Color _cancelInctiveColor = Color.WhiteSmoke;
        // ------------

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

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            _controller.Dispose();
            _debounce.Dispose();
        }

        #region Events

        private void TxtSearch_Click(object sender, EventArgs e)
        {
            txtSearch.Text = string.Empty;
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;

            BtnSearch_Click(sender, e);
        }

        private void TxtSearch_Leave(object sender, EventArgs e)
        {
            if (txtSearch.Text == string.Empty)
                txtSearch.Text = _defaultSearchText;
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            var text = txtSearch.Text;

            if (text == _defaultSearchText || string.IsNullOrWhiteSpace(text))
            {
                LoadClients();
                DisableCancelButton();
                return;
            }

            EnableCancelButton();

            // Фильтруем по кэшу с debounce 300мс
            SetLoading(true);
            _debounce.Run(300, () =>
                _controller.Filter(
                    query: text,
                    onSuccess: result => UI(() =>
                    {
                        dgvClients.DataSource = result;
                        SetLoading(false);
                    }),
                    onError: ex => ShowError(ex, "Ошибка фильтрации")));
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (var form = new ClientEditForm())
            {
                if (form.ShowDialog() != DialogResult.OK) return;

                SetLoading(true);
                _controller.AddClient(
                    client: form.Result,
                    onSuccess: () => UI(LoadClients),    // после добавления — перезагружаем список
                    onError: ex => ShowError(ex, "Ошибка добавления"));
            }
        }

        private void DgvClients_CellMouseDoubleClick(
            object sender, 
            DataGridViewCellMouseEventArgs e)
        {
            BtnEdit_Click(sender, e);
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            var client = GetSelectedClient();
            if (client == null) return;

            using (var form = new ClientEditForm(client))
            {
                if (form.ShowDialog() != DialogResult.OK) return;

                SetLoading(true);
                _controller.UpdateClient(
                    client: form.Result,
                    onSuccess: () => UI(LoadClients),
                    onError: ex => ShowError(ex, "Ошибка обновления"));
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

            SetLoading(true);
            _controller.DeleteClient(
                clientId: client.Id,
                onSuccess: () => UI(LoadClients),
                onError: ex => ShowError(ex, "Ошибка удаления"));
        }

        private void BtnCancelSearch_Click(object sender, EventArgs e)
        {
            DisableCancelButton();
            LoadClients();
        }

        #endregion

        #region Loading

        private void LoadClients()
        {
            SetLoading(true);
            _controller.LoadClients(
                onSuccess: clients => UI(() =>
                {
                    dgvClients.DataSource = clients;
                    SetLoading(false);
                }),
                onError: ex => ShowError(ex, "Ошибка загрузки"));
        }

        private void SetLoading(bool isLoading)
        {
            // SetLoading может вызываться из фонового потока - защищаемся
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => SetLoading(isLoading)));
                return;
            }

            btnAdd.Enabled = !isLoading;
            btnEdit.Enabled = !isLoading;
            btnDelete.Enabled = !isLoading;
            btnSearch.Enabled = !isLoading;
        }

        #endregion

        #region Thread helpers

        // Безопасный переход на UI-поток
        private void UI(Action action)
        {
            if (IsDisposed) return;

            if (InvokeRequired)
                BeginInvoke(action);
            else
                action();
        }

        private void ShowError(Exception ex, string title)
        {
            var msg = ex is FaultException<ClientFault> fault
                ? fault.Detail.Message
                : ex.Message;

            UI(() =>
            {
                MessageBox
                .Show(
                    msg, 
                    title,
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error
                    );

                SetLoading(false);
            });
        }

        #endregion

        #region Visual helpers

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

        private void EnableCancelButton()
        {
            btnCancelSearch.Enabled = true;
            btnCancelSearch.BackColor = _cancelActiveColor;
        }

        private void DisableCancelButton()
        {
            btnCancelSearch.Enabled = false;
            btnCancelSearch.BackColor = _cancelInctiveColor;
        }

        #endregion
    }
}