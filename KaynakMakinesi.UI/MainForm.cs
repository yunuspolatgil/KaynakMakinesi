using System;

namespace KaynakMakinesi.UI
{
    public partial class MainForm : DevExpress.XtraEditors.XtraForm
    {
        public MainForm()
        {
            InitializeComponent();
        }
        private  void MainForm_Load(object sender, EventArgs e)
        {
            //var r = await _modbusService.ReadAutoAsync("42013", CancellationToken.None);
            //lblConnState.Text = r.Success ? r.Value?.ToString() : r.Error;
        }

 
    }
}