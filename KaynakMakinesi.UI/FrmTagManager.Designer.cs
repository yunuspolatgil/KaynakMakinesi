namespace KaynakMakinesi.UI
{
    partial class FrmTagManager
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmTagManager));
            this.ribbonControl1 = new DevExpress.XtraBars.Ribbon.RibbonControl();
            this.btnImportExcel = new DevExpress.XtraBars.BarButtonItem();
            this.btnExportExcel = new DevExpress.XtraBars.BarButtonItem();
            this.btnReload = new DevExpress.XtraBars.BarButtonItem();
            this.btnNewTag = new DevExpress.XtraBars.BarButtonItem();
            this.btnSaveChanges = new DevExpress.XtraBars.BarButtonItem();
            this.btnDeleteTag = new DevExpress.XtraBars.BarButtonItem();
            this.btnUndo = new DevExpress.XtraBars.BarButtonItem();
            this.btnReadSelected = new DevExpress.XtraBars.BarButtonItem();
            this.btnWriteSelected = new DevExpress.XtraBars.BarButtonItem();
            this.btnStartMonitor = new DevExpress.XtraBars.BarButtonItem();
            this.btnStopMonitor = new DevExpress.XtraBars.BarButtonItem();
            this.btnDeleteSelected = new DevExpress.XtraBars.BarButtonItem();
            this.btnDeleteAll = new DevExpress.XtraBars.BarButtonItem();
            this.ribbonPage1 = new DevExpress.XtraBars.Ribbon.RibbonPage();
            this.grpFile = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.grpCrud = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.grpTest = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.ribbonPage2 = new DevExpress.XtraBars.Ribbon.RibbonPage();
            this.ribbonPageGroup1 = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.ribbonStatusBar = new DevExpress.XtraBars.Ribbon.RibbonStatusBar();
            this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
            this.gcTags = new DevExpress.XtraGrid.GridControl();
            this.gvTags = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.colId = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colName = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colAddress = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repoAddress = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
            this.colType = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repoType = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox();
            this.colReadOnly = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repoReadOnly = new DevExpress.XtraEditors.Repository.RepositoryItemToggleSwitch();
            this.colGroupName = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repoGroup = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox();
            this.colDescription = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colPollMs = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repoPollMs = new DevExpress.XtraEditors.Repository.RepositoryItemSpinEdit();
            this.colLastValue = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colQuality = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colUpdatedAt = new DevExpress.XtraGrid.Columns.GridColumn();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.panelControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gcTags)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvTags)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repoAddress)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repoType)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repoReadOnly)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repoGroup)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repoPollMs)).BeginInit();
            this.SuspendLayout();
            // 
            // ribbonControl1
            // 
            this.ribbonControl1.AllowKeyTips = false;
            this.ribbonControl1.AllowMdiChildButtons = false;
            this.ribbonControl1.AllowMinimizeRibbon = false;
            this.ribbonControl1.AllowTrimPageText = false;
            this.ribbonControl1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ribbonControl1.DrawGroupCaptions = DevExpress.Utils.DefaultBoolean.True;
            this.ribbonControl1.ExpandCollapseItem.Id = 0;
            this.ribbonControl1.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.ribbonControl1.ExpandCollapseItem,
            this.ribbonControl1.SearchEditItem,
            this.btnImportExcel,
            this.btnExportExcel,
            this.btnReload,
            this.btnNewTag,
            this.btnSaveChanges,
            this.btnDeleteTag,
            this.btnUndo,
            this.btnReadSelected,
            this.btnWriteSelected,
            this.btnStartMonitor,
            this.btnStopMonitor,
            this.btnDeleteSelected,
            this.btnDeleteAll});
            this.ribbonControl1.Location = new System.Drawing.Point(0, 0);
            this.ribbonControl1.MaxItemId = 15;
            this.ribbonControl1.Name = "ribbonControl1";
            this.ribbonControl1.Pages.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPage[] {
            this.ribbonPage1,
            this.ribbonPage2});
            this.ribbonControl1.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonControlStyle.Office2019;
            this.ribbonControl1.ShowApplicationButton = DevExpress.Utils.DefaultBoolean.False;
            this.ribbonControl1.ShowDisplayOptionsMenuButton = DevExpress.Utils.DefaultBoolean.False;
            this.ribbonControl1.ShowExpandCollapseButton = DevExpress.Utils.DefaultBoolean.False;
            this.ribbonControl1.ShowMoreCommandsButton = DevExpress.Utils.DefaultBoolean.False;
            this.ribbonControl1.ShowPageHeadersInFormCaption = DevExpress.Utils.DefaultBoolean.True;
            this.ribbonControl1.ShowPageHeadersMode = DevExpress.XtraBars.Ribbon.ShowPageHeadersMode.Show;
            this.ribbonControl1.ShowQatLocationSelector = false;
            this.ribbonControl1.ShowToolbarCustomizeItem = false;
            this.ribbonControl1.Size = new System.Drawing.Size(1558, 146);
            this.ribbonControl1.StatusBar = this.ribbonStatusBar;
            this.ribbonControl1.Toolbar.ShowCustomizeItem = false;
            // 
            // btnImportExcel
            // 
            this.btnImportExcel.Caption = "Excel Import";
            this.btnImportExcel.Id = 1;
            this.btnImportExcel.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnImportExcel.ImageOptions.SvgImage")));
            this.btnImportExcel.LargeWidth = 110;
            this.btnImportExcel.Name = "btnImportExcel";
            this.btnImportExcel.PaintStyle = DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph;
            this.btnImportExcel.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            // 
            // btnExportExcel
            // 
            this.btnExportExcel.Caption = "Excel Export";
            this.btnExportExcel.Id = 2;
            this.btnExportExcel.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnExportExcel.ImageOptions.SvgImage")));
            this.btnExportExcel.LargeWidth = 110;
            this.btnExportExcel.Name = "btnExportExcel";
            this.btnExportExcel.PaintStyle = DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph;
            this.btnExportExcel.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            // 
            // btnReload
            // 
            this.btnReload.Caption = "Yenile";
            this.btnReload.Id = 4;
            this.btnReload.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnReload.ImageOptions.SvgImage")));
            this.btnReload.LargeWidth = 95;
            this.btnReload.Name = "btnReload";
            this.btnReload.PaintStyle = DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph;
            this.btnReload.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            // 
            // btnNewTag
            // 
            this.btnNewTag.Caption = "Yeni Tag";
            this.btnNewTag.Id = 5;
            this.btnNewTag.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnNewTag.ImageOptions.SvgImage")));
            this.btnNewTag.LargeWidth = 95;
            this.btnNewTag.Name = "btnNewTag";
            this.btnNewTag.PaintStyle = DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph;
            this.btnNewTag.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            // 
            // btnSaveChanges
            // 
            this.btnSaveChanges.Caption = "Kaydet";
            this.btnSaveChanges.Id = 6;
            this.btnSaveChanges.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnSaveChanges.ImageOptions.SvgImage")));
            this.btnSaveChanges.LargeWidth = 95;
            this.btnSaveChanges.Name = "btnSaveChanges";
            this.btnSaveChanges.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            this.btnSaveChanges.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnSaveChanges_ItemClick);
            // 
            // btnDeleteTag
            // 
            this.btnDeleteTag.Caption = "Tag Sil";
            this.btnDeleteTag.Id = 7;
            this.btnDeleteTag.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnDeleteTag.ImageOptions.SvgImage")));
            this.btnDeleteTag.LargeWidth = 95;
            this.btnDeleteTag.Name = "btnDeleteTag";
            this.btnDeleteTag.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            this.btnDeleteTag.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnDeleteTag_ItemClick);
            // 
            // btnUndo
            // 
            this.btnUndo.Caption = "Geri Al";
            this.btnUndo.Id = 8;
            this.btnUndo.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnUndo.ImageOptions.SvgImage")));
            this.btnUndo.LargeWidth = 95;
            this.btnUndo.Name = "btnUndo";
            this.btnUndo.PaintStyle = DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph;
            this.btnUndo.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            // 
            // btnReadSelected
            // 
            this.btnReadSelected.Caption = "Oku";
            this.btnReadSelected.Id = 9;
            this.btnReadSelected.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnReadSelected.ImageOptions.SvgImage")));
            this.btnReadSelected.LargeWidth = 85;
            this.btnReadSelected.Name = "btnReadSelected";
            this.btnReadSelected.PaintStyle = DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph;
            this.btnReadSelected.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            // 
            // btnWriteSelected
            // 
            this.btnWriteSelected.Caption = "Yaz";
            this.btnWriteSelected.Id = 10;
            this.btnWriteSelected.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnWriteSelected.ImageOptions.SvgImage")));
            this.btnWriteSelected.LargeWidth = 85;
            this.btnWriteSelected.Name = "btnWriteSelected";
            this.btnWriteSelected.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            // 
            // btnStartMonitor
            // 
            this.btnStartMonitor.Caption = "İzleme Başlat";
            this.btnStartMonitor.Id = 11;
            this.btnStartMonitor.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnStartMonitor.ImageOptions.SvgImage")));
            this.btnStartMonitor.LargeWidth = 105;
            this.btnStartMonitor.Name = "btnStartMonitor";
            this.btnStartMonitor.PaintStyle = DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph;
            this.btnStartMonitor.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            // 
            // btnStopMonitor
            // 
            this.btnStopMonitor.Caption = "İzleme Durdur";
            this.btnStopMonitor.Id = 12;
            this.btnStopMonitor.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnStopMonitor.ImageOptions.SvgImage")));
            this.btnStopMonitor.LargeWidth = 105;
            this.btnStopMonitor.Name = "btnStopMonitor";
            this.btnStopMonitor.PaintStyle = DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph;
            this.btnStopMonitor.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            // 
            // btnDeleteSelected
            // 
            this.btnDeleteSelected.Caption = "Seçilenleri Sil";
            this.btnDeleteSelected.Id = 13;
            this.btnDeleteSelected.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnDeleteSelected.ImageOptions.SvgImage")));
            this.btnDeleteSelected.LargeWidth = 110;
            this.btnDeleteSelected.Name = "btnDeleteSelected";
            this.btnDeleteSelected.PaintStyle = DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph;
            this.btnDeleteSelected.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            this.btnDeleteSelected.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.BtnDeleteSelected_ItemClick);
            // 
            // btnDeleteAll
            // 
            this.btnDeleteAll.Caption = "Tümünü Sil";
            this.btnDeleteAll.Id = 14;
            this.btnDeleteAll.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnDeleteAll.ImageOptions.SvgImage")));
            this.btnDeleteAll.LargeWidth = 95;
            this.btnDeleteAll.Name = "btnDeleteAll";
            this.btnDeleteAll.PaintStyle = DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph;
            this.btnDeleteAll.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            this.btnDeleteAll.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.BtnDeleteAll_ItemClick);
            // 
            // ribbonPage1
            // 
            this.ribbonPage1.Groups.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPageGroup[] {
            this.grpFile,
            this.grpCrud,
            this.grpTest});
            this.ribbonPage1.Name = "ribbonPage1";
            this.ribbonPage1.Text = "Taglar";
            // 
            // grpFile
            // 
            this.grpFile.ItemLinks.Add(this.btnImportExcel);
            this.grpFile.ItemLinks.Add(this.btnExportExcel);
            this.grpFile.ItemLinks.Add(this.btnReload);
            this.grpFile.Name = "grpFile";
            this.grpFile.Text = "Dosya";
            // 
            // grpCrud
            // 
            this.grpCrud.ItemLinks.Add(this.btnNewTag);
            this.grpCrud.ItemLinks.Add(this.btnSaveChanges);
            this.grpCrud.ItemLinks.Add(this.btnDeleteTag);
            this.grpCrud.ItemLinks.Add(this.btnDeleteSelected);
            this.grpCrud.ItemLinks.Add(this.btnDeleteAll);
            this.grpCrud.ItemLinks.Add(this.btnUndo);
            this.grpCrud.Name = "grpCrud";
            this.grpCrud.Text = "Düzenleme";
            // 
            // grpTest
            // 
            this.grpTest.ItemLinks.Add(this.btnReadSelected);
            this.grpTest.ItemLinks.Add(this.btnWriteSelected);
            this.grpTest.ItemLinks.Add(this.btnStartMonitor);
            this.grpTest.ItemLinks.Add(this.btnStopMonitor);
            this.grpTest.Name = "grpTest";
            this.grpTest.Text = "Test / İzleme";
            // 
            // ribbonPage2
            // 
            this.ribbonPage2.Groups.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPageGroup[] {
            this.ribbonPageGroup1});
            this.ribbonPage2.Name = "ribbonPage2";
            this.ribbonPage2.Text = "Görünüm";
            // 
            // ribbonPageGroup1
            // 
            this.ribbonPageGroup1.Name = "ribbonPageGroup1";
            this.ribbonPageGroup1.Text = "ribbonPageGroup1";
            // 
            // ribbonStatusBar
            // 
            this.ribbonStatusBar.Location = new System.Drawing.Point(0, 796);
            this.ribbonStatusBar.Name = "ribbonStatusBar";
            this.ribbonStatusBar.Ribbon = this.ribbonControl1;
            this.ribbonStatusBar.Size = new System.Drawing.Size(1558, 23);
            // 
            // panelControl1
            // 
            this.panelControl1.Controls.Add(this.gcTags);
            this.panelControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelControl1.Location = new System.Drawing.Point(0, 146);
            this.panelControl1.Name = "panelControl1";
            this.panelControl1.Size = new System.Drawing.Size(1558, 650);
            this.panelControl1.TabIndex = 5;
            // 
            // gcTags
            // 
            this.gcTags.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gcTags.Location = new System.Drawing.Point(2, 2);
            this.gcTags.MainView = this.gvTags;
            this.gcTags.MenuManager = this.ribbonControl1;
            this.gcTags.Name = "gcTags";
            this.gcTags.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repoPollMs,
            this.repoType,
            this.repoReadOnly,
            this.repoAddress,
            this.repoGroup});
            this.gcTags.Size = new System.Drawing.Size(1554, 646);
            this.gcTags.TabIndex = 1;
            this.gcTags.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvTags});
            // 
            // gvTags
            // 
            this.gvTags.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colId,
            this.colName,
            this.colAddress,
            this.colType,
            this.colReadOnly,
            this.colGroupName,
            this.colDescription,
            this.colPollMs,
            this.colLastValue,
            this.colQuality,
            this.colUpdatedAt});
            this.gvTags.GridControl = this.gcTags;
            this.gvTags.Name = "gvTags";
            this.gvTags.OptionsNavigation.EnterMoveNextColumn = true;
            this.gvTags.OptionsSelection.MultiSelect = true;
            this.gvTags.OptionsView.ShowGroupPanel = false;
            // 
            // colId
            // 
            this.colId.Caption = "ID";
            this.colId.FieldName = "Id";
            this.colId.Name = "colId";
            // 
            // colName
            // 
            this.colName.Caption = "Tag Adı";
            this.colName.FieldName = "Name";
            this.colName.Name = "colName";
            this.colName.Visible = true;
            this.colName.VisibleIndex = 0;
            this.colName.Width = 281;
            // 
            // colAddress
            // 
            this.colAddress.Caption = "Adres";
            this.colAddress.ColumnEdit = this.repoAddress;
            this.colAddress.FieldName = "Address";
            this.colAddress.Name = "colAddress";
            this.colAddress.Visible = true;
            this.colAddress.VisibleIndex = 1;
            this.colAddress.Width = 133;
            // 
            // repoAddress
            // 
            this.repoAddress.AutoHeight = false;
            this.repoAddress.Name = "repoAddress";
            // 
            // colType
            // 
            this.colType.Caption = "Tip";
            this.colType.ColumnEdit = this.repoType;
            this.colType.FieldName = "Type";
            this.colType.Name = "colType";
            this.colType.Visible = true;
            this.colType.VisibleIndex = 2;
            this.colType.Width = 104;
            // 
            // repoType
            // 
            this.repoType.AutoHeight = false;
            this.repoType.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.repoType.Items.AddRange(new object[] {
            "Bool",
            "UShort",
            "Int32",
            "Float"});
            this.repoType.Name = "repoType";
            // 
            // colReadOnly
            // 
            this.colReadOnly.Caption = "RO";
            this.colReadOnly.ColumnEdit = this.repoReadOnly;
            this.colReadOnly.FieldName = "RO";
            this.colReadOnly.Name = "colReadOnly";
            this.colReadOnly.Visible = true;
            this.colReadOnly.VisibleIndex = 9;
            // 
            // repoReadOnly
            // 
            this.repoReadOnly.AutoHeight = false;
            this.repoReadOnly.Name = "repoReadOnly";
            this.repoReadOnly.OffText = "Off";
            this.repoReadOnly.OnText = "On";
            // 
            // colGroupName
            // 
            this.colGroupName.Caption = "Grup";
            this.colGroupName.ColumnEdit = this.repoGroup;
            this.colGroupName.FieldName = "GroupName";
            this.colGroupName.Name = "colGroupName";
            this.colGroupName.Visible = true;
            this.colGroupName.VisibleIndex = 3;
            // 
            // repoGroup
            // 
            this.repoGroup.AutoHeight = false;
            this.repoGroup.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.repoGroup.Name = "repoGroup";
            // 
            // colDescription
            // 
            this.colDescription.Caption = "Açıklama";
            this.colDescription.FieldName = "Description";
            this.colDescription.Name = "colDescription";
            this.colDescription.Visible = true;
            this.colDescription.VisibleIndex = 4;
            this.colDescription.Width = 199;
            // 
            // colPollMs
            // 
            this.colPollMs.Caption = "Poll (ms)";
            this.colPollMs.ColumnEdit = this.repoPollMs;
            this.colPollMs.FieldName = "PollMs";
            this.colPollMs.Name = "colPollMs";
            this.colPollMs.Visible = true;
            this.colPollMs.VisibleIndex = 5;
            // 
            // repoPollMs
            // 
            this.repoPollMs.AutoHeight = false;
            this.repoPollMs.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.repoPollMs.MaxValue = new decimal(new int[] {
            60000,
            0,
            0,
            0});
            this.repoPollMs.MinValue = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.repoPollMs.Name = "repoPollMs";
            // 
            // colLastValue
            // 
            this.colLastValue.Caption = "Son Değer";
            this.colLastValue.FieldName = "LastValue";
            this.colLastValue.Name = "colLastValue";
            this.colLastValue.Visible = true;
            this.colLastValue.VisibleIndex = 6;
            this.colLastValue.Width = 91;
            // 
            // colQuality
            // 
            this.colQuality.Caption = "Durum";
            this.colQuality.FieldName = "Quality";
            this.colQuality.Name = "colQuality";
            this.colQuality.Visible = true;
            this.colQuality.VisibleIndex = 7;
            // 
            // colUpdatedAt
            // 
            this.colUpdatedAt.Caption = "Güncellendi";
            this.colUpdatedAt.FieldName = "UpdatedAt";
            this.colUpdatedAt.Name = "colUpdatedAt";
            this.colUpdatedAt.Visible = true;
            this.colUpdatedAt.VisibleIndex = 8;
            this.colUpdatedAt.Width = 102;
            // 
            // FrmTagManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1558, 819);
            this.Controls.Add(this.panelControl1);
            this.Controls.Add(this.ribbonStatusBar);
            this.Controls.Add(this.ribbonControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FrmTagManager";
            this.Ribbon = this.ribbonControl1;
            this.StatusBar = this.ribbonStatusBar;
            this.Text = "Tag Yönetimi";
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            this.panelControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gcTags)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvTags)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repoAddress)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repoType)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repoReadOnly)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repoGroup)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repoPollMs)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraBars.Ribbon.RibbonControl ribbonControl1;
        private DevExpress.XtraBars.BarButtonItem btnImportExcel;
        private DevExpress.XtraBars.BarButtonItem btnExportExcel;
        private DevExpress.XtraBars.BarButtonItem btnReload;
        private DevExpress.XtraBars.BarButtonItem btnNewTag;
        private DevExpress.XtraBars.BarButtonItem btnSaveChanges;
        private DevExpress.XtraBars.BarButtonItem btnDeleteTag;
        private DevExpress.XtraBars.BarButtonItem btnUndo;
        private DevExpress.XtraBars.BarButtonItem btnReadSelected;
        private DevExpress.XtraBars.BarButtonItem btnWriteSelected;
        private DevExpress.XtraBars.BarButtonItem btnStartMonitor;
        private DevExpress.XtraBars.BarButtonItem btnStopMonitor;
        private DevExpress.XtraBars.BarButtonItem btnDeleteSelected;
        private DevExpress.XtraBars.BarButtonItem btnDeleteAll;
        private DevExpress.XtraBars.Ribbon.RibbonPage ribbonPage1;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup grpFile;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup grpCrud;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup grpTest;
        private DevExpress.XtraBars.Ribbon.RibbonStatusBar ribbonStatusBar;
        private DevExpress.XtraBars.Ribbon.RibbonPage ribbonPage2;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup ribbonPageGroup1;
        private DevExpress.XtraEditors.PanelControl panelControl1;
        private DevExpress.XtraGrid.GridControl gcTags;
        private DevExpress.XtraGrid.Views.Grid.GridView gvTags;
        private DevExpress.XtraGrid.Columns.GridColumn colName;
        private DevExpress.XtraGrid.Columns.GridColumn colAddress;
        private DevExpress.XtraEditors.Repository.RepositoryItemTextEdit repoAddress;
        private DevExpress.XtraGrid.Columns.GridColumn colType;
        private DevExpress.XtraEditors.Repository.RepositoryItemComboBox repoType;
        private DevExpress.XtraGrid.Columns.GridColumn colReadOnly;
        private DevExpress.XtraEditors.Repository.RepositoryItemToggleSwitch repoReadOnly;
        private DevExpress.XtraGrid.Columns.GridColumn colGroupName;
        private DevExpress.XtraEditors.Repository.RepositoryItemComboBox repoGroup;
        private DevExpress.XtraGrid.Columns.GridColumn colDescription;
        private DevExpress.XtraGrid.Columns.GridColumn colPollMs;
        private DevExpress.XtraEditors.Repository.RepositoryItemSpinEdit repoPollMs;
        private DevExpress.XtraGrid.Columns.GridColumn colLastValue;
        private DevExpress.XtraGrid.Columns.GridColumn colQuality;
        private DevExpress.XtraGrid.Columns.GridColumn colUpdatedAt;
        private DevExpress.XtraGrid.Columns.GridColumn colId;
    }
}
