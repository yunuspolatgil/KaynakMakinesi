namespace KaynakMakinesi.UI
{
    partial class FrmAnaForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmAnaForm));
            this.ribbon = new DevExpress.XtraBars.Ribbon.RibbonControl();
            this.btnSagTorcKalibrasyon = new DevExpress.XtraBars.BarButtonItem();
            this.btnTagYonetim = new DevExpress.XtraBars.BarButtonItem();
            this.barButtonItem1 = new DevExpress.XtraBars.BarButtonItem();
            this.btnSolTorcKalibrasyon = new DevExpress.XtraBars.BarButtonItem();
            this.btnSikistirmaKlaibrasyon = new DevExpress.XtraBars.BarButtonItem();
            this.btnSikistirmaRecete = new DevExpress.XtraBars.BarButtonItem();
            this.barStaticItem1 = new DevExpress.XtraBars.BarStaticItem();
            this.lblConnState = new DevExpress.XtraBars.BarStaticItem();
            this.barStaticItem2 = new DevExpress.XtraBars.BarStaticItem();
            this.lblConnReason = new DevExpress.XtraBars.BarStaticItem();
            this.barStaticItem3 = new DevExpress.XtraBars.BarStaticItem();
            this.lblPlcTarget = new DevExpress.XtraBars.BarStaticItem();
            this.barStaticItem4 = new DevExpress.XtraBars.BarStaticItem();
            this.lblLastOk = new DevExpress.XtraBars.BarStaticItem();
            this.ribbonPage1 = new DevExpress.XtraBars.Ribbon.RibbonPage();
            this.ribbonPageGroup1 = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.ribbonPageGroup2 = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.ribbonPageGroup3 = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.ribbonStatusBar = new DevExpress.XtraBars.Ribbon.RibbonStatusBar();
            this.xtraTabbedMdiManager1 = new DevExpress.XtraTabbedMdi.XtraTabbedMdiManager(this.components);
            this.listLogs = new DevExpress.XtraEditors.ListBoxControl();
            ((System.ComponentModel.ISupportInitialize)(this.ribbon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.xtraTabbedMdiManager1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.listLogs)).BeginInit();
            this.SuspendLayout();
            // 
            // ribbon
            // 
            this.ribbon.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ribbon.ExpandCollapseItem.Id = 0;
            this.ribbon.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.ribbon.ExpandCollapseItem,
            this.ribbon.SearchEditItem,
            this.btnSagTorcKalibrasyon,
            this.btnTagYonetim,
            this.barButtonItem1,
            this.btnSolTorcKalibrasyon,
            this.btnSikistirmaKlaibrasyon,
            this.btnSikistirmaRecete,
            this.barStaticItem1,
            this.lblConnState,
            this.barStaticItem2,
            this.lblConnReason,
            this.barStaticItem3,
            this.lblPlcTarget,
            this.barStaticItem4,
            this.lblLastOk});
            this.ribbon.Location = new System.Drawing.Point(0, 0);
            this.ribbon.MaxItemId = 15;
            this.ribbon.Name = "ribbon";
            this.ribbon.Pages.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPage[] {
            this.ribbonPage1});
            this.ribbon.Size = new System.Drawing.Size(1301, 146);
            this.ribbon.StatusBar = this.ribbonStatusBar;
            // 
            // btnSagTorcKalibrasyon
            // 
            this.btnSagTorcKalibrasyon.Caption = "Sağ Torç Kalibrasyon";
            this.btnSagTorcKalibrasyon.Id = 1;
            this.btnSagTorcKalibrasyon.ImageOptions.Image = global::KaynakMakinesi.UI.Properties.Resources.torc_sol_ayar__130_x_130_piksel_;
            this.btnSagTorcKalibrasyon.ItemAppearance.Normal.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.btnSagTorcKalibrasyon.ItemAppearance.Normal.Options.UseFont = true;
            this.btnSagTorcKalibrasyon.LargeWidth = 90;
            this.btnSagTorcKalibrasyon.Name = "btnSagTorcKalibrasyon";
            this.btnSagTorcKalibrasyon.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            // 
            // btnTagYonetim
            // 
            this.btnTagYonetim.Caption = "Tag Yönetim";
            this.btnTagYonetim.Id = 2;
            this.btnTagYonetim.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnTagYonetim.ImageOptions.SvgImage")));
            this.btnTagYonetim.ItemAppearance.Normal.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.btnTagYonetim.ItemAppearance.Normal.Options.UseFont = true;
            this.btnTagYonetim.LargeWidth = 90;
            this.btnTagYonetim.Name = "btnTagYonetim";
            this.btnTagYonetim.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnTagYonetim_ItemClick);
            // 
            // barButtonItem1
            // 
            this.barButtonItem1.Caption = "PLC Ayar";
            this.barButtonItem1.Id = 3;
            this.barButtonItem1.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("barButtonItem1.ImageOptions.SvgImage")));
            this.barButtonItem1.ItemAppearance.Normal.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.barButtonItem1.ItemAppearance.Normal.Options.UseFont = true;
            this.barButtonItem1.LargeWidth = 90;
            this.barButtonItem1.Name = "barButtonItem1";
            this.barButtonItem1.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            this.barButtonItem1.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonItem1_ItemClick);
            // 
            // btnSolTorcKalibrasyon
            // 
            this.btnSolTorcKalibrasyon.Caption = "Sol Torç Kalibrasyon";
            this.btnSolTorcKalibrasyon.Id = 4;
            this.btnSolTorcKalibrasyon.ImageOptions.Image = global::KaynakMakinesi.UI.Properties.Resources.torc_sag_ayar__130_x_130_piksel_;
            this.btnSolTorcKalibrasyon.ItemAppearance.Normal.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.btnSolTorcKalibrasyon.ItemAppearance.Normal.Options.UseFont = true;
            this.btnSolTorcKalibrasyon.LargeWidth = 90;
            this.btnSolTorcKalibrasyon.Name = "btnSolTorcKalibrasyon";
            this.btnSolTorcKalibrasyon.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            // 
            // btnSikistirmaKlaibrasyon
            // 
            this.btnSikistirmaKlaibrasyon.Caption = "Sıkıştırma Kalibrasyon";
            this.btnSikistirmaKlaibrasyon.Id = 5;
            this.btnSikistirmaKlaibrasyon.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnSikistirmaKlaibrasyon.ImageOptions.SvgImage")));
            this.btnSikistirmaKlaibrasyon.ItemAppearance.Normal.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.btnSikistirmaKlaibrasyon.ItemAppearance.Normal.Options.UseFont = true;
            this.btnSikistirmaKlaibrasyon.LargeWidth = 90;
            this.btnSikistirmaKlaibrasyon.Name = "btnSikistirmaKlaibrasyon";
            // 
            // btnSikistirmaRecete
            // 
            this.btnSikistirmaRecete.Caption = "Sıkıştırma Reçete";
            this.btnSikistirmaRecete.Id = 6;
            this.btnSikistirmaRecete.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnSikistirmaRecete.ImageOptions.SvgImage")));
            this.btnSikistirmaRecete.ItemAppearance.Normal.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.btnSikistirmaRecete.ItemAppearance.Normal.Options.UseFont = true;
            this.btnSikistirmaRecete.LargeWidth = 90;
            this.btnSikistirmaRecete.Name = "btnSikistirmaRecete";
            // 
            // barStaticItem1
            // 
            this.barStaticItem1.Alignment = DevExpress.XtraBars.BarItemLinkAlignment.Right;
            this.barStaticItem1.Caption = "Bağlantı Durumu :";
            this.barStaticItem1.Id = 7;
            this.barStaticItem1.ItemAppearance.Normal.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.barStaticItem1.ItemAppearance.Normal.Options.UseFont = true;
            this.barStaticItem1.Name = "barStaticItem1";
            // 
            // lblConnState
            // 
            this.lblConnState.Alignment = DevExpress.XtraBars.BarItemLinkAlignment.Right;
            this.lblConnState.Caption = "...";
            this.lblConnState.Id = 8;
            this.lblConnState.ItemAppearance.Normal.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.lblConnState.ItemAppearance.Normal.Options.UseFont = true;
            this.lblConnState.Name = "lblConnState";
            // 
            // barStaticItem2
            // 
            this.barStaticItem2.Alignment = DevExpress.XtraBars.BarItemLinkAlignment.Right;
            this.barStaticItem2.Caption = "Tekrar :";
            this.barStaticItem2.Id = 9;
            this.barStaticItem2.ItemAppearance.Normal.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.barStaticItem2.ItemAppearance.Normal.Options.UseFont = true;
            this.barStaticItem2.Name = "barStaticItem2";
            // 
            // lblConnReason
            // 
            this.lblConnReason.Alignment = DevExpress.XtraBars.BarItemLinkAlignment.Right;
            this.lblConnReason.Caption = "...";
            this.lblConnReason.Id = 10;
            this.lblConnReason.ItemAppearance.Normal.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.lblConnReason.ItemAppearance.Normal.Options.UseFont = true;
            this.lblConnReason.Name = "lblConnReason";
            // 
            // barStaticItem3
            // 
            this.barStaticItem3.Alignment = DevExpress.XtraBars.BarItemLinkAlignment.Right;
            this.barStaticItem3.Caption = "PLC Bilgi :";
            this.barStaticItem3.Id = 11;
            this.barStaticItem3.ItemAppearance.Normal.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.barStaticItem3.ItemAppearance.Normal.Options.UseFont = true;
            this.barStaticItem3.Name = "barStaticItem3";
            // 
            // lblPlcTarget
            // 
            this.lblPlcTarget.Alignment = DevExpress.XtraBars.BarItemLinkAlignment.Right;
            this.lblPlcTarget.Caption = "...";
            this.lblPlcTarget.Id = 12;
            this.lblPlcTarget.ItemAppearance.Normal.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.lblPlcTarget.ItemAppearance.Normal.Options.UseFont = true;
            this.lblPlcTarget.Name = "lblPlcTarget";
            // 
            // barStaticItem4
            // 
            this.barStaticItem4.Alignment = DevExpress.XtraBars.BarItemLinkAlignment.Right;
            this.barStaticItem4.Caption = "Son Bağlantı : ";
            this.barStaticItem4.Id = 13;
            this.barStaticItem4.ItemAppearance.Normal.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.barStaticItem4.ItemAppearance.Normal.Options.UseFont = true;
            this.barStaticItem4.Name = "barStaticItem4";
            // 
            // lblLastOk
            // 
            this.lblLastOk.Alignment = DevExpress.XtraBars.BarItemLinkAlignment.Right;
            this.lblLastOk.Caption = "...";
            this.lblLastOk.Id = 14;
            this.lblLastOk.ItemAppearance.Normal.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.lblLastOk.ItemAppearance.Normal.Options.UseFont = true;
            this.lblLastOk.Name = "lblLastOk";
            // 
            // ribbonPage1
            // 
            this.ribbonPage1.Groups.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPageGroup[] {
            this.ribbonPageGroup1,
            this.ribbonPageGroup2,
            this.ribbonPageGroup3});
            this.ribbonPage1.Name = "ribbonPage1";
            this.ribbonPage1.Text = "Kaynak Makinesi";
            // 
            // ribbonPageGroup1
            // 
            this.ribbonPageGroup1.ItemLinks.Add(this.btnSagTorcKalibrasyon);
            this.ribbonPageGroup1.ItemLinks.Add(this.btnSolTorcKalibrasyon);
            this.ribbonPageGroup1.ItemLinks.Add(this.btnSikistirmaKlaibrasyon);
            this.ribbonPageGroup1.Name = "ribbonPageGroup1";
            this.ribbonPageGroup1.Text = "Kalibrasyonlar";
            // 
            // ribbonPageGroup2
            // 
            this.ribbonPageGroup2.Alignment = DevExpress.XtraBars.Ribbon.RibbonPageGroupAlignment.Far;
            this.ribbonPageGroup2.ItemLinks.Add(this.btnTagYonetim);
            this.ribbonPageGroup2.ItemLinks.Add(this.barButtonItem1);
            this.ribbonPageGroup2.Name = "ribbonPageGroup2";
            this.ribbonPageGroup2.Text = "Sistem Ayarları";
            // 
            // ribbonPageGroup3
            // 
            this.ribbonPageGroup3.ItemLinks.Add(this.btnSikistirmaRecete);
            this.ribbonPageGroup3.Name = "ribbonPageGroup3";
            this.ribbonPageGroup3.Text = "Reçeteler";
            // 
            // ribbonStatusBar
            // 
            this.ribbonStatusBar.ItemLinks.Add(this.barStaticItem1);
            this.ribbonStatusBar.ItemLinks.Add(this.lblConnState);
            this.ribbonStatusBar.ItemLinks.Add(this.barStaticItem2, true);
            this.ribbonStatusBar.ItemLinks.Add(this.lblConnReason);
            this.ribbonStatusBar.ItemLinks.Add(this.barStaticItem3, true);
            this.ribbonStatusBar.ItemLinks.Add(this.lblPlcTarget);
            this.ribbonStatusBar.ItemLinks.Add(this.barStaticItem4, true);
            this.ribbonStatusBar.ItemLinks.Add(this.lblLastOk);
            this.ribbonStatusBar.Location = new System.Drawing.Point(0, 668);
            this.ribbonStatusBar.Name = "ribbonStatusBar";
            this.ribbonStatusBar.Ribbon = this.ribbon;
            this.ribbonStatusBar.Size = new System.Drawing.Size(1301, 23);
            // 
            // xtraTabbedMdiManager1
            // 
            this.xtraTabbedMdiManager1.ClosePageButtonShowMode = DevExpress.XtraTab.ClosePageButtonShowMode.InAllTabPageHeaders;
            this.xtraTabbedMdiManager1.MdiParent = this;
            // 
            // listLogs
            // 
            this.listLogs.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.listLogs.Location = new System.Drawing.Point(0, 691);
            this.listLogs.Name = "listLogs";
            this.listLogs.Size = new System.Drawing.Size(1301, 103);
            this.listLogs.TabIndex = 3;
            // 
            // FrmAnaForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1301, 794);
            this.Controls.Add(this.ribbonStatusBar);
            this.Controls.Add(this.listLogs);
            this.Controls.Add(this.ribbon);
            this.IsMdiContainer = true;
            this.Name = "FrmAnaForm";
            this.Ribbon = this.ribbon;
            this.StatusBar = this.ribbonStatusBar;
            this.Text = "FrmAnaForm";
            ((System.ComponentModel.ISupportInitialize)(this.ribbon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.xtraTabbedMdiManager1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.listLogs)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraBars.Ribbon.RibbonControl ribbon;
        private DevExpress.XtraBars.Ribbon.RibbonPage ribbonPage1;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup ribbonPageGroup1;
        private DevExpress.XtraBars.Ribbon.RibbonStatusBar ribbonStatusBar;
        private DevExpress.XtraBars.BarButtonItem btnSagTorcKalibrasyon;
        private DevExpress.XtraBars.BarButtonItem btnTagYonetim;
        private DevExpress.XtraBars.BarButtonItem barButtonItem1;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup ribbonPageGroup2;
        private DevExpress.XtraBars.BarButtonItem btnSolTorcKalibrasyon;
        private DevExpress.XtraBars.BarButtonItem btnSikistirmaKlaibrasyon;
        private DevExpress.XtraBars.BarButtonItem btnSikistirmaRecete;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup ribbonPageGroup3;
        private DevExpress.XtraBars.BarStaticItem barStaticItem1;
        private DevExpress.XtraBars.BarStaticItem lblConnState;
        private DevExpress.XtraBars.BarStaticItem barStaticItem2;
        private DevExpress.XtraBars.BarStaticItem lblConnReason;
        private DevExpress.XtraBars.BarStaticItem barStaticItem3;
        private DevExpress.XtraBars.BarStaticItem lblPlcTarget;
        private DevExpress.XtraBars.BarStaticItem barStaticItem4;
        private DevExpress.XtraBars.BarStaticItem lblLastOk;
        private DevExpress.XtraTabbedMdi.XtraTabbedMdiManager xtraTabbedMdiManager1;
        private DevExpress.XtraEditors.ListBoxControl listLogs;
    }
}