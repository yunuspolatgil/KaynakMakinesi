using DevExpress.XtraBars;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraEditors;
using KaynakMakinesi.Core.Logging;
using KaynakMakinesi.Core.Model;
using KaynakMakinesi.Core.Plc.Service;
using KaynakMakinesi.Infrastructure.Tags;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace KaynakMakinesi.UI
{
    public partial class FrmTagManager : RibbonForm
    {
        private BindingList<TagRow> _rows = new BindingList<TagRow>();
        private readonly SqliteTagRepository _tagRepo;
        public FrmTagManager(SqliteTagRepository tagRepo)
        {
            InitializeComponent();
            _tagRepo = tagRepo;
            
            SetupGrid();
            LoadFromDb(); // şimdilik DB yok, önce grid çalışsın
        }

        private void LoadFromDb()
        {
            var list = _tagRepo.ListAll();

            _rows.Clear();
            foreach (var t in list)
            {
                _rows.Add(new TagRow
                {
                    Id = t.Id,
                    Name = t.Name,
                    Address = t.Address,
                    Type = t.Type,
                    Group = t.GroupName,
                    Description = t.Description,
                    PollMs = t.PollMs,
                    ReadOnly = t.ReadOnly
                });
            }
        }

        private void SaveToDb()
        {
            gvTags.CloseEditor();
            gvTags.UpdateCurrentRow();

            // basit validasyon
            foreach (var r in _rows)
            {
                if (string.IsNullOrWhiteSpace(r.Name))
                    throw new Exception("Tag Adı boş olamaz.");
                if (string.IsNullOrWhiteSpace(r.Address))
                    throw new Exception($"{r.Name} için Adres boş olamaz.");
                if (string.IsNullOrWhiteSpace(r.Type))
                    r.Type = "UShort"; // default
                if (r.PollMs <= 0) r.PollMs = 250;
            }

            var defs = _rows.Select(r => new KaynakMakinesi.Core.Tags.TagDef
            {
                Name = r.Name.Trim(),
                Address = r.Address.Trim(),
                Type = r.Type.Trim(),
                GroupName = r.Group ?? "",
                Description = r.Description ?? "",
                PollMs = r.PollMs,
                ReadOnly = r.ReadOnly,
                Address1Based = 0 // gerekli ise uygun değeri ver
            }).ToList();

            _tagRepo.UpsertMany(defs);
        }

        private void SetupGrid()
        {
            // gcTags ve gvTags isimleri sende aynı olmalı
            gcTags.DataSource = _rows;

            gvTags.OptionsBehavior.Editable = true;
            gvTags.OptionsView.NewItemRowPosition = DevExpress.XtraGrid.Views.Grid.NewItemRowPosition.Top;

            // Arama (grid üzerinden)
            gvTags.OptionsFind.AlwaysVisible = true;
            gvTags.OptionsView.ShowAutoFilterRow = true;

            // Dokunmatik hissi
            gvTags.OptionsView.RowAutoHeight = true;
            gvTags.RowHeight = 34;

            // İlk kurulumda kolonları otomatik üretir
            gvTags.PopulateColumns();

            // İstersen kolon başlıklarını güzelleştir
            gvTags.Columns[nameof(TagRow.Name)].Caption = "Tag Adı";
            gvTags.Columns[nameof(TagRow.Address)].Caption = "Adres";
            gvTags.Columns[nameof(TagRow.Type)].Caption = "Tip";
            gvTags.Columns[nameof(TagRow.Group)].Caption = "Grup";
            gvTags.Columns[nameof(TagRow.Description)].Caption = "Açıklama";
            gvTags.Columns[nameof(TagRow.PollMs)].Caption = "Poll (ms)";
            gvTags.Columns[nameof(TagRow.ReadOnly)].Caption = "RO";
            gvTags.Columns[nameof(TagRow.LastValue)].Caption = "Son Değer";
            gvTags.Columns[nameof(TagRow.Status)].Caption = "Durum";
            gvTags.Columns[nameof(TagRow.UpdatedAt)].Caption = "Güncellendi";

            // DB'ye yazılmayacak kolonları sağa al / istersen readonly yap
            gvTags.Columns[nameof(TagRow.LastValue)].OptionsColumn.AllowEdit = false;
            gvTags.Columns[nameof(TagRow.Status)].OptionsColumn.AllowEdit = false;
            gvTags.Columns[nameof(TagRow.UpdatedAt)].OptionsColumn.AllowEdit = false;
        }

        private void btnKaydet_Click(object sender, EventArgs e)
        {
            
        }

        private void btnSil_Click(object sender, EventArgs e)
        {
            
        }

        private void btnSaveChanges_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                SaveToDb();
                XtraMessageBox.Show("Kaydedildi.", "OK");
                LoadFromDb(); // id’ler/normalize için yeniden yüklemek iyi
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Hata");
            }
        }

        private void btnDeleteTag_ItemClick(object sender, ItemClickEventArgs e)
        {
            var row = gvTags.GetFocusedRow() as TagRow;
            if (row == null) return;

            if (XtraMessageBox.Show($"{row.Name} silinsin mi?", "Onay",
                    MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;

            _tagRepo.DeleteByName(row.Name);
            _rows.Remove(row);
        }
    }
}
