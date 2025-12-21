namespace KaynakMakinesi.UI
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.btnAyarlar = new DevExpress.XtraEditors.SimpleButton();
            this.panelHeader = new DevExpress.XtraEditors.PanelControl();
            this.panelFooter = new DevExpress.XtraEditors.PanelControl();
            this.panelControl2 = new DevExpress.XtraEditors.PanelControl();
            this.listLogs = new DevExpress.XtraEditors.ListBoxControl();
            this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
            this.lblLastOk = new DevExpress.XtraEditors.LabelControl();
            this.lblPlcTarget = new DevExpress.XtraEditors.LabelControl();
            this.lblConnReason = new DevExpress.XtraEditors.LabelControl();
            this.lblConnState = new DevExpress.XtraEditors.LabelControl();
            this.svgConn = new DevExpress.XtraEditors.SvgImageBox();
            this.panelMain = new DevExpress.XtraEditors.PanelControl();
            this.simpleButton1 = new DevExpress.XtraEditors.SimpleButton();
            this.btnSagTorc = new DevExpress.XtraEditors.SimpleButton();
            this.btnSolTorc = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this.panelHeader)).BeginInit();
            this.panelHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelFooter)).BeginInit();
            this.panelFooter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl2)).BeginInit();
            this.panelControl2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.listLogs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.panelControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.svgConn)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelMain)).BeginInit();
            this.SuspendLayout();
            // 
            // btnAyarlar
            // 
            this.btnAyarlar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAyarlar.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.TopCenter;
            this.btnAyarlar.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnAyarlar.ImageOptions.SvgImage")));
            this.btnAyarlar.Location = new System.Drawing.Point(991, 12);
            this.btnAyarlar.Name = "btnAyarlar";
            this.btnAyarlar.Size = new System.Drawing.Size(102, 77);
            this.btnAyarlar.TabIndex = 0;
            this.btnAyarlar.Text = "Ayarlar";
            this.btnAyarlar.Click += new System.EventHandler(this.btnAyarlar_Click);
            // 
            // panelHeader
            // 
            this.panelHeader.Controls.Add(this.simpleButton1);
            this.panelHeader.Controls.Add(this.btnSagTorc);
            this.panelHeader.Controls.Add(this.btnSolTorc);
            this.panelHeader.Controls.Add(this.btnAyarlar);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Location = new System.Drawing.Point(0, 0);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Size = new System.Drawing.Size(1128, 187);
            this.panelHeader.TabIndex = 1;
            // 
            // panelFooter
            // 
            this.panelFooter.Controls.Add(this.panelControl2);
            this.panelFooter.Controls.Add(this.panelControl1);
            this.panelFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelFooter.Location = new System.Drawing.Point(0, 664);
            this.panelFooter.Name = "panelFooter";
            this.panelFooter.Size = new System.Drawing.Size(1128, 125);
            this.panelFooter.TabIndex = 2;
            // 
            // panelControl2
            // 
            this.panelControl2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelControl2.Controls.Add(this.listLogs);
            this.panelControl2.Location = new System.Drawing.Point(12, 13);
            this.panelControl2.Name = "panelControl2";
            this.panelControl2.Size = new System.Drawing.Size(726, 100);
            this.panelControl2.TabIndex = 1;
            // 
            // listLogs
            // 
            this.listLogs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listLogs.Location = new System.Drawing.Point(2, 2);
            this.listLogs.Name = "listLogs";
            this.listLogs.Size = new System.Drawing.Size(722, 96);
            this.listLogs.TabIndex = 1;
            // 
            // panelControl1
            // 
            this.panelControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.panelControl1.Controls.Add(this.lblLastOk);
            this.panelControl1.Controls.Add(this.lblPlcTarget);
            this.panelControl1.Controls.Add(this.lblConnReason);
            this.panelControl1.Controls.Add(this.lblConnState);
            this.panelControl1.Controls.Add(this.svgConn);
            this.panelControl1.Location = new System.Drawing.Point(744, 13);
            this.panelControl1.Name = "panelControl1";
            this.panelControl1.Size = new System.Drawing.Size(372, 100);
            this.panelControl1.TabIndex = 0;
            // 
            // lblLastOk
            // 
            this.lblLastOk.Location = new System.Drawing.Point(112, 64);
            this.lblLastOk.Name = "lblLastOk";
            this.lblLastOk.Size = new System.Drawing.Size(63, 13);
            this.lblLastOk.TabIndex = 4;
            this.lblLastOk.Text = "labelControl1";
            // 
            // lblPlcTarget
            // 
            this.lblPlcTarget.Location = new System.Drawing.Point(112, 47);
            this.lblPlcTarget.Name = "lblPlcTarget";
            this.lblPlcTarget.Size = new System.Drawing.Size(63, 13);
            this.lblPlcTarget.TabIndex = 3;
            this.lblPlcTarget.Text = "labelControl1";
            // 
            // lblConnReason
            // 
            this.lblConnReason.Location = new System.Drawing.Point(112, 28);
            this.lblConnReason.Name = "lblConnReason";
            this.lblConnReason.Size = new System.Drawing.Size(63, 13);
            this.lblConnReason.TabIndex = 2;
            this.lblConnReason.Text = "labelControl1";
            // 
            // lblConnState
            // 
            this.lblConnState.Location = new System.Drawing.Point(112, 9);
            this.lblConnState.Name = "lblConnState";
            this.lblConnState.Size = new System.Drawing.Size(63, 13);
            this.lblConnState.TabIndex = 1;
            this.lblConnState.Text = "labelControl1";
            // 
            // svgConn
            // 
            this.svgConn.Location = new System.Drawing.Point(5, 5);
            this.svgConn.Name = "svgConn";
            this.svgConn.Size = new System.Drawing.Size(92, 74);
            this.svgConn.TabIndex = 0;
            this.svgConn.Text = "svgImageBox1";
            // 
            // panelMain
            // 
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(0, 187);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(1128, 477);
            this.panelMain.TabIndex = 3;
            // 
            // simpleButton1
            // 
            this.simpleButton1.Appearance.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.simpleButton1.Appearance.Options.UseFont = true;
            this.simpleButton1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.simpleButton1.ImageOptions.AllowGlyphSkinning = DevExpress.Utils.DefaultBoolean.False;
            this.simpleButton1.ImageOptions.Image = global::KaynakMakinesi.UI.Properties.Resources.kazan_sıkıstırma;
            this.simpleButton1.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.TopCenter;
            this.simpleButton1.Location = new System.Drawing.Point(346, 12);
            this.simpleButton1.Name = "simpleButton1";
            this.simpleButton1.Size = new System.Drawing.Size(160, 160);
            this.simpleButton1.TabIndex = 3;
            this.simpleButton1.Text = "Sol Torç";
            // 
            // btnSagTorc
            // 
            this.btnSagTorc.Appearance.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.btnSagTorc.Appearance.Options.UseFont = true;
            this.btnSagTorc.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSagTorc.ImageOptions.AllowGlyphSkinning = DevExpress.Utils.DefaultBoolean.False;
            this.btnSagTorc.ImageOptions.Image = global::KaynakMakinesi.UI.Properties.Resources.torc_sag_ayar__130_x_130_piksel_;
            this.btnSagTorc.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.TopCenter;
            this.btnSagTorc.Location = new System.Drawing.Point(180, 12);
            this.btnSagTorc.Name = "btnSagTorc";
            this.btnSagTorc.Size = new System.Drawing.Size(160, 160);
            this.btnSagTorc.TabIndex = 2;
            this.btnSagTorc.Text = "Sol Torç";
            // 
            // btnSolTorc
            // 
            this.btnSolTorc.Appearance.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.btnSolTorc.Appearance.Options.UseFont = true;
            this.btnSolTorc.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSolTorc.ImageOptions.AllowGlyphSkinning = DevExpress.Utils.DefaultBoolean.False;
            this.btnSolTorc.ImageOptions.Image = global::KaynakMakinesi.UI.Properties.Resources.torc_sol_ayar__130_x_130_piksel_;
            this.btnSolTorc.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.TopCenter;
            this.btnSolTorc.Location = new System.Drawing.Point(12, 12);
            this.btnSolTorc.Name = "btnSolTorc";
            this.btnSolTorc.Size = new System.Drawing.Size(160, 160);
            this.btnSolTorc.TabIndex = 1;
            this.btnSolTorc.Text = "Sağ Torç";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1128, 789);
            this.Controls.Add(this.panelMain);
            this.Controls.Add(this.panelFooter);
            this.Controls.Add(this.panelHeader);
            this.Name = "MainForm";
            this.Text = "Kaynak Makinesi V1.0.5";
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.panelHeader)).EndInit();
            this.panelHeader.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.panelFooter)).EndInit();
            this.panelFooter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.panelControl2)).EndInit();
            this.panelControl2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.listLogs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            this.panelControl1.ResumeLayout(false);
            this.panelControl1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.svgConn)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelMain)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.SimpleButton btnAyarlar;
        private DevExpress.XtraEditors.PanelControl panelHeader;
        private DevExpress.XtraEditors.PanelControl panelFooter;
        private DevExpress.XtraEditors.PanelControl panelMain;
        private DevExpress.XtraEditors.PanelControl panelControl1;
        private DevExpress.XtraEditors.LabelControl lblLastOk;
        private DevExpress.XtraEditors.LabelControl lblPlcTarget;
        private DevExpress.XtraEditors.LabelControl lblConnReason;
        private DevExpress.XtraEditors.LabelControl lblConnState;
        private DevExpress.XtraEditors.SvgImageBox svgConn;
        private DevExpress.XtraEditors.PanelControl panelControl2;
        private DevExpress.XtraEditors.ListBoxControl listLogs;
        private DevExpress.XtraEditors.SimpleButton btnSolTorc;
        private DevExpress.XtraEditors.SimpleButton btnSagTorc;
        private DevExpress.XtraEditors.SimpleButton simpleButton1;
    }
}

