namespace KaynakMakinesi.UI
{
    partial class SettingsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            this.dxErrorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.tabSettings = new DevExpress.XtraTab.XtraTabControl();
            this.tabPlc = new DevExpress.XtraTab.XtraTabPage();
            this.PLCPANEL = new DevExpress.XtraEditors.PanelControl();
            this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
            this.btnTestConnection = new DevExpress.XtraEditors.SimpleButton();
            this.txtPlcIp = new DevExpress.XtraEditors.TextEdit();
            this.labelControl6 = new DevExpress.XtraEditors.LabelControl();
            this.spnPlcPort = new DevExpress.XtraEditors.SpinEdit();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
            this.spnHeartbeatIntervalMs = new DevExpress.XtraEditors.SpinEdit();
            this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
            this.spnPlcUnitId = new DevExpress.XtraEditors.SpinEdit();
            this.spnHeartbeatAddress = new DevExpress.XtraEditors.SpinEdit();
            this.spnPlcTimeoutMs = new DevExpress.XtraEditors.SpinEdit();
            this.lblTestResult = new DevExpress.XtraEditors.LabelControl();
            this.tabDatabase = new DevExpress.XtraTab.XtraTabPage();
            this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
            this.labelControl7 = new DevExpress.XtraEditors.LabelControl();
            this.txtDbFullPath = new DevExpress.XtraEditors.TextEdit();
            this.lblDbName = new DevExpress.XtraEditors.LabelControl();
            this.txtDbFileName = new DevExpress.XtraEditors.ButtonEdit();
            this.tabLogging = new DevExpress.XtraTab.XtraTabPage();
            this.panelControl2 = new DevExpress.XtraEditors.PanelControl();
            this.spnLogKeepInMemory = new DevExpress.XtraEditors.SpinEdit();
            this.cmbLogMinLevel = new DevExpress.XtraEditors.ComboBoxEdit();
            this.labelControl8 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl9 = new DevExpress.XtraEditors.LabelControl();
            this.btnDefaults = new DevExpress.XtraEditors.SimpleButton();
            this.btnReload = new DevExpress.XtraEditors.SimpleButton();
            this.btnCancel = new DevExpress.XtraEditors.SimpleButton();
            this.btnApply = new DevExpress.XtraEditors.SimpleButton();
            this.btnSave = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this.dxErrorProvider1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tabSettings)).BeginInit();
            this.tabSettings.SuspendLayout();
            this.tabPlc.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PLCPANEL)).BeginInit();
            this.PLCPANEL.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtPlcIp.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spnPlcPort.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spnHeartbeatIntervalMs.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spnPlcUnitId.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spnHeartbeatAddress.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spnPlcTimeoutMs.Properties)).BeginInit();
            this.tabDatabase.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.panelControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtDbFullPath.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDbFileName.Properties)).BeginInit();
            this.tabLogging.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl2)).BeginInit();
            this.panelControl2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spnLogKeepInMemory.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbLogMinLevel.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // dxErrorProvider1
            // 
            this.dxErrorProvider1.ContainerControl = this;
            // 
            // tabSettings
            // 
            this.tabSettings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabSettings.Location = new System.Drawing.Point(0, 0);
            this.tabSettings.Name = "tabSettings";
            this.tabSettings.SelectedTabPage = this.tabPlc;
            this.tabSettings.Size = new System.Drawing.Size(665, 268);
            this.tabSettings.TabIndex = 0;
            this.tabSettings.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] {
            this.tabPlc,
            this.tabDatabase,
            this.tabLogging});
            // 
            // tabPlc
            // 
            this.tabPlc.Controls.Add(this.btnSave);
            this.tabPlc.Controls.Add(this.btnApply);
            this.tabPlc.Controls.Add(this.btnTestConnection);
            this.tabPlc.Controls.Add(this.btnReload);
            this.tabPlc.Controls.Add(this.btnDefaults);
            this.tabPlc.Controls.Add(this.PLCPANEL);
            this.tabPlc.Controls.Add(this.btnCancel);
            this.tabPlc.Name = "tabPlc";
            this.tabPlc.Size = new System.Drawing.Size(660, 242);
            this.tabPlc.Text = "PLC (Modbus TCP)";
            // 
            // PLCPANEL
            // 
            this.PLCPANEL.Controls.Add(this.labelControl5);
            this.PLCPANEL.Controls.Add(this.lblTestResult);
            this.PLCPANEL.Controls.Add(this.txtPlcIp);
            this.PLCPANEL.Controls.Add(this.labelControl6);
            this.PLCPANEL.Controls.Add(this.spnPlcPort);
            this.PLCPANEL.Controls.Add(this.labelControl1);
            this.PLCPANEL.Controls.Add(this.labelControl4);
            this.PLCPANEL.Controls.Add(this.spnHeartbeatIntervalMs);
            this.PLCPANEL.Controls.Add(this.labelControl2);
            this.PLCPANEL.Controls.Add(this.labelControl3);
            this.PLCPANEL.Controls.Add(this.spnPlcUnitId);
            this.PLCPANEL.Controls.Add(this.spnHeartbeatAddress);
            this.PLCPANEL.Controls.Add(this.spnPlcTimeoutMs);
            this.PLCPANEL.Location = new System.Drawing.Point(11, 18);
            this.PLCPANEL.Name = "PLCPANEL";
            this.PLCPANEL.Size = new System.Drawing.Size(406, 208);
            this.PLCPANEL.TabIndex = 15;
            // 
            // labelControl5
            // 
            this.labelControl5.Location = new System.Drawing.Point(21, 20);
            this.labelControl5.Name = "labelControl5";
            this.labelControl5.Size = new System.Drawing.Size(31, 13);
            this.labelControl5.TabIndex = 12;
            this.labelControl5.Text = "PLC IP";
            // 
            // btnTestConnection
            // 
            this.btnTestConnection.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTestConnection.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.TopCenter;
            this.btnTestConnection.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnTestConnection.ImageOptions.SvgImage")));
            this.btnTestConnection.Location = new System.Drawing.Point(423, 161);
            this.btnTestConnection.Name = "btnTestConnection";
            this.btnTestConnection.Size = new System.Drawing.Size(107, 65);
            this.btnTestConnection.TabIndex = 7;
            this.btnTestConnection.Text = "Bağlantı Test";
            // 
            // txtPlcIp
            // 
            this.txtPlcIp.Location = new System.Drawing.Point(148, 17);
            this.txtPlcIp.Name = "txtPlcIp";
            this.txtPlcIp.Size = new System.Drawing.Size(193, 20);
            this.txtPlcIp.TabIndex = 5;
            // 
            // labelControl6
            // 
            this.labelControl6.Location = new System.Drawing.Point(21, 150);
            this.labelControl6.Name = "labelControl6";
            this.labelControl6.Size = new System.Drawing.Size(90, 13);
            this.labelControl6.TabIndex = 13;
            this.labelControl6.Text = "Heartbeat Interval";
            // 
            // spnPlcPort
            // 
            this.spnPlcPort.EditValue = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.spnPlcPort.Location = new System.Drawing.Point(148, 43);
            this.spnPlcPort.Name = "spnPlcPort";
            this.spnPlcPort.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.spnPlcPort.Size = new System.Drawing.Size(100, 20);
            this.spnPlcPort.TabIndex = 0;
            // 
            // labelControl1
            // 
            this.labelControl1.Location = new System.Drawing.Point(21, 124);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(91, 13);
            this.labelControl1.TabIndex = 8;
            this.labelControl1.Text = "Heartbeat Address";
            // 
            // labelControl4
            // 
            this.labelControl4.Location = new System.Drawing.Point(21, 98);
            this.labelControl4.Name = "labelControl4";
            this.labelControl4.Size = new System.Drawing.Size(59, 13);
            this.labelControl4.TabIndex = 11;
            this.labelControl4.Text = "Zaman Aşımı";
            // 
            // spnHeartbeatIntervalMs
            // 
            this.spnHeartbeatIntervalMs.EditValue = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.spnHeartbeatIntervalMs.Location = new System.Drawing.Point(148, 147);
            this.spnHeartbeatIntervalMs.Name = "spnHeartbeatIntervalMs";
            this.spnHeartbeatIntervalMs.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.spnHeartbeatIntervalMs.Size = new System.Drawing.Size(100, 20);
            this.spnHeartbeatIntervalMs.TabIndex = 4;
            // 
            // labelControl2
            // 
            this.labelControl2.Location = new System.Drawing.Point(21, 46);
            this.labelControl2.Name = "labelControl2";
            this.labelControl2.Size = new System.Drawing.Size(20, 13);
            this.labelControl2.TabIndex = 9;
            this.labelControl2.Text = "Port";
            // 
            // labelControl3
            // 
            this.labelControl3.Location = new System.Drawing.Point(21, 72);
            this.labelControl3.Name = "labelControl3";
            this.labelControl3.Size = new System.Drawing.Size(56, 13);
            this.labelControl3.TabIndex = 10;
            this.labelControl3.Text = "Plc Slave ID";
            // 
            // spnPlcUnitId
            // 
            this.spnPlcUnitId.EditValue = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.spnPlcUnitId.Location = new System.Drawing.Point(148, 69);
            this.spnPlcUnitId.Name = "spnPlcUnitId";
            this.spnPlcUnitId.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.spnPlcUnitId.Size = new System.Drawing.Size(100, 20);
            this.spnPlcUnitId.TabIndex = 1;
            // 
            // spnHeartbeatAddress
            // 
            this.spnHeartbeatAddress.EditValue = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.spnHeartbeatAddress.Location = new System.Drawing.Point(148, 121);
            this.spnHeartbeatAddress.Name = "spnHeartbeatAddress";
            this.spnHeartbeatAddress.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.spnHeartbeatAddress.Size = new System.Drawing.Size(100, 20);
            this.spnHeartbeatAddress.TabIndex = 3;
            // 
            // spnPlcTimeoutMs
            // 
            this.spnPlcTimeoutMs.EditValue = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.spnPlcTimeoutMs.Location = new System.Drawing.Point(148, 95);
            this.spnPlcTimeoutMs.Name = "spnPlcTimeoutMs";
            this.spnPlcTimeoutMs.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.spnPlcTimeoutMs.Size = new System.Drawing.Size(100, 20);
            this.spnPlcTimeoutMs.TabIndex = 2;
            // 
            // lblTestResult
            // 
            this.lblTestResult.Appearance.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.lblTestResult.Appearance.Options.UseFont = true;
            this.lblTestResult.Location = new System.Drawing.Point(148, 173);
            this.lblTestResult.Name = "lblTestResult";
            this.lblTestResult.Size = new System.Drawing.Size(12, 16);
            this.lblTestResult.TabIndex = 14;
            this.lblTestResult.Text = "...";
            // 
            // tabDatabase
            // 
            this.tabDatabase.Controls.Add(this.panelControl1);
            this.tabDatabase.Name = "tabDatabase";
            this.tabDatabase.Size = new System.Drawing.Size(648, 291);
            this.tabDatabase.Text = "Veritabanı (SQLite)";
            // 
            // panelControl1
            // 
            this.panelControl1.Controls.Add(this.labelControl7);
            this.panelControl1.Controls.Add(this.txtDbFullPath);
            this.panelControl1.Controls.Add(this.lblDbName);
            this.panelControl1.Controls.Add(this.txtDbFileName);
            this.panelControl1.Location = new System.Drawing.Point(11, 14);
            this.panelControl1.Name = "panelControl1";
            this.panelControl1.Size = new System.Drawing.Size(629, 100);
            this.panelControl1.TabIndex = 1;
            // 
            // labelControl7
            // 
            this.labelControl7.Location = new System.Drawing.Point(14, 48);
            this.labelControl7.Name = "labelControl7";
            this.labelControl7.Size = new System.Drawing.Size(86, 13);
            this.labelControl7.TabIndex = 15;
            this.labelControl7.Text = "Veritabanı tam yol";
            // 
            // txtDbFullPath
            // 
            this.txtDbFullPath.Location = new System.Drawing.Point(141, 45);
            this.txtDbFullPath.Name = "txtDbFullPath";
            this.txtDbFullPath.Properties.ReadOnly = true;
            this.txtDbFullPath.Size = new System.Drawing.Size(193, 20);
            this.txtDbFullPath.TabIndex = 14;
            // 
            // lblDbName
            // 
            this.lblDbName.Location = new System.Drawing.Point(14, 22);
            this.lblDbName.Name = "lblDbName";
            this.lblDbName.Size = new System.Drawing.Size(65, 13);
            this.lblDbName.TabIndex = 13;
            this.lblDbName.Text = "Veritabanı adı";
            // 
            // txtDbFileName
            // 
            this.txtDbFileName.Location = new System.Drawing.Point(141, 19);
            this.txtDbFileName.Name = "txtDbFileName";
            this.txtDbFileName.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
            this.txtDbFileName.Size = new System.Drawing.Size(331, 20);
            this.txtDbFileName.TabIndex = 0;
            // 
            // tabLogging
            // 
            this.tabLogging.Controls.Add(this.panelControl2);
            this.tabLogging.Name = "tabLogging";
            this.tabLogging.Size = new System.Drawing.Size(648, 291);
            this.tabLogging.Text = "Log";
            // 
            // panelControl2
            // 
            this.panelControl2.Controls.Add(this.spnLogKeepInMemory);
            this.panelControl2.Controls.Add(this.cmbLogMinLevel);
            this.panelControl2.Controls.Add(this.labelControl8);
            this.panelControl2.Controls.Add(this.labelControl9);
            this.panelControl2.Location = new System.Drawing.Point(11, 14);
            this.panelControl2.Name = "panelControl2";
            this.panelControl2.Size = new System.Drawing.Size(629, 100);
            this.panelControl2.TabIndex = 2;
            // 
            // spnLogKeepInMemory
            // 
            this.spnLogKeepInMemory.EditValue = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.spnLogKeepInMemory.Location = new System.Drawing.Point(141, 45);
            this.spnLogKeepInMemory.Name = "spnLogKeepInMemory";
            this.spnLogKeepInMemory.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.spnLogKeepInMemory.Size = new System.Drawing.Size(128, 20);
            this.spnLogKeepInMemory.TabIndex = 17;
            // 
            // cmbLogMinLevel
            // 
            this.cmbLogMinLevel.Location = new System.Drawing.Point(141, 19);
            this.cmbLogMinLevel.Name = "cmbLogMinLevel";
            this.cmbLogMinLevel.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cmbLogMinLevel.Size = new System.Drawing.Size(249, 20);
            this.cmbLogMinLevel.TabIndex = 16;
            // 
            // labelControl8
            // 
            this.labelControl8.Location = new System.Drawing.Point(14, 48);
            this.labelControl8.Name = "labelControl8";
            this.labelControl8.Size = new System.Drawing.Size(86, 13);
            this.labelControl8.TabIndex = 15;
            this.labelControl8.Text = "Veritabanı tam yol";
            // 
            // labelControl9
            // 
            this.labelControl9.Location = new System.Drawing.Point(14, 22);
            this.labelControl9.Name = "labelControl9";
            this.labelControl9.Size = new System.Drawing.Size(65, 13);
            this.labelControl9.TabIndex = 13;
            this.labelControl9.Text = "Veritabanı adı";
            // 
            // btnDefaults
            // 
            this.btnDefaults.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDefaults.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.TopCenter;
            this.btnDefaults.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnDefaults.ImageOptions.SvgImage")));
            this.btnDefaults.Location = new System.Drawing.Point(423, 89);
            this.btnDefaults.Name = "btnDefaults";
            this.btnDefaults.Size = new System.Drawing.Size(107, 65);
            this.btnDefaults.TabIndex = 12;
            this.btnDefaults.Text = "Varsayılanlar";
            // 
            // btnReload
            // 
            this.btnReload.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnReload.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.TopCenter;
            this.btnReload.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnReload.ImageOptions.SvgImage")));
            this.btnReload.Location = new System.Drawing.Point(536, 90);
            this.btnReload.Name = "btnReload";
            this.btnReload.Size = new System.Drawing.Size(107, 65);
            this.btnReload.TabIndex = 11;
            this.btnReload.Text = "Yeniden Yükle";
            // 
            // btnCancel
            // 
            this.btnCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.TopCenter;
            this.btnCancel.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnCancel.ImageOptions.SvgImage")));
            this.btnCancel.Location = new System.Drawing.Point(536, 161);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(107, 65);
            this.btnCancel.TabIndex = 10;
            this.btnCancel.Text = "Vazgeç";
            // 
            // btnApply
            // 
            this.btnApply.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnApply.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.TopCenter;
            this.btnApply.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnApply.ImageOptions.SvgImage")));
            this.btnApply.Location = new System.Drawing.Point(423, 18);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(107, 65);
            this.btnApply.TabIndex = 9;
            this.btnApply.Text = "Uygula";
            // 
            // btnSave
            // 
            this.btnSave.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSave.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.TopCenter;
            this.btnSave.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnSave.ImageOptions.SvgImage")));
            this.btnSave.Location = new System.Drawing.Point(536, 19);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(107, 65);
            this.btnSave.TabIndex = 8;
            this.btnSave.Text = "Kaydet";
            // 
            // SettingsForm
            // 
            this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(665, 268);
            this.Controls.Add(this.tabSettings);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "SettingsForm";
            ((System.ComponentModel.ISupportInitialize)(this.dxErrorProvider1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tabSettings)).EndInit();
            this.tabSettings.ResumeLayout(false);
            this.tabPlc.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.PLCPANEL)).EndInit();
            this.PLCPANEL.ResumeLayout(false);
            this.PLCPANEL.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtPlcIp.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spnPlcPort.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spnHeartbeatIntervalMs.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spnPlcUnitId.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spnHeartbeatAddress.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spnPlcTimeoutMs.Properties)).EndInit();
            this.tabDatabase.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            this.panelControl1.ResumeLayout(false);
            this.panelControl1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtDbFullPath.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDbFileName.Properties)).EndInit();
            this.tabLogging.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.panelControl2)).EndInit();
            this.panelControl2.ResumeLayout(false);
            this.panelControl2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spnLogKeepInMemory.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbLogMinLevel.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ErrorProvider dxErrorProvider1;
        private DevExpress.XtraTab.XtraTabControl tabSettings;
        private DevExpress.XtraTab.XtraTabPage tabPlc;
        private DevExpress.XtraTab.XtraTabPage tabDatabase;
        private DevExpress.XtraEditors.TextEdit txtPlcIp;
        private DevExpress.XtraEditors.SpinEdit spnHeartbeatIntervalMs;
        private DevExpress.XtraEditors.SpinEdit spnHeartbeatAddress;
        private DevExpress.XtraEditors.SpinEdit spnPlcTimeoutMs;
        private DevExpress.XtraEditors.SpinEdit spnPlcUnitId;
        private DevExpress.XtraEditors.SpinEdit spnPlcPort;
        private DevExpress.XtraEditors.PanelControl PLCPANEL;
        private DevExpress.XtraEditors.LabelControl labelControl5;
        private DevExpress.XtraEditors.LabelControl lblTestResult;
        private DevExpress.XtraEditors.LabelControl labelControl6;
        private DevExpress.XtraEditors.LabelControl labelControl4;
        private DevExpress.XtraEditors.LabelControl labelControl3;
        private DevExpress.XtraEditors.LabelControl labelControl2;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.SimpleButton btnTestConnection;
        private DevExpress.XtraEditors.PanelControl panelControl1;
        private DevExpress.XtraEditors.LabelControl lblDbName;
        private DevExpress.XtraEditors.ButtonEdit txtDbFileName;
        private DevExpress.XtraEditors.LabelControl labelControl7;
        private DevExpress.XtraEditors.TextEdit txtDbFullPath;
        private DevExpress.XtraTab.XtraTabPage tabLogging;
        private DevExpress.XtraEditors.PanelControl panelControl2;
        private DevExpress.XtraEditors.SpinEdit spnLogKeepInMemory;
        private DevExpress.XtraEditors.ComboBoxEdit cmbLogMinLevel;
        private DevExpress.XtraEditors.LabelControl labelControl8;
        private DevExpress.XtraEditors.LabelControl labelControl9;
        private DevExpress.XtraEditors.SimpleButton btnSave;
        private DevExpress.XtraEditors.SimpleButton btnCancel;
        private DevExpress.XtraEditors.SimpleButton btnDefaults;
        private DevExpress.XtraEditors.SimpleButton btnReload;
        private DevExpress.XtraEditors.SimpleButton btnApply;
    }
}