using DevExpress.XtraBars;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraEditors;
using KaynakMakinesi.Core.Logging;
using KaynakMakinesi.Core.Model;
using KaynakMakinesi.Core.Plc.Service;
using KaynakMakinesi.Core.Tags;
using KaynakMakinesi.Infrastructure.Tags;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KaynakMakinesi.UI
{
    public partial class FrmTagManager : RibbonForm
    {
        private BindingList<TagRow> _rows = new BindingList<TagRow>();
        private readonly SqliteTagRepository _tagRepo;
        private readonly IModbusService _modbusService;
        private readonly IAppLogger _log;

        // snapshot for Undo
        private List<TagRow> _snapshot = new List<TagRow>();

        // monitor
        private CancellationTokenSource _monitorCts;
        private Task _monitorTask;

        public FrmTagManager(SqliteTagRepository tagRepo, IModbusService modbusService = null, IAppLogger log = null)
        {
            InitializeComponent();
            _tagRepo = tagRepo;
            _modbusService = modbusService;
            _log = log;

            // wire buttons
            btnImportExcel.ItemClick += BtnImportExcel_ItemClick;
            btnExportExcel.ItemClick += BtnExportExcel_ItemClick;
            btnReload.ItemClick += BtnReload_ItemClick;
            btnNewTag.ItemClick += BtnNewTag_ItemClick;
            btnReadSelected.ItemClick += BtnReadSelected_ItemClick;
            btnWriteSelected.ItemClick += BtnWriteSelected_ItemClick;
            btnStartMonitor.ItemClick += BtnStartMonitor_ItemClick;
            btnStopMonitor.ItemClick += BtnStopMonitor_ItemClick;
            btnUndo.ItemClick += BtnUndo_ItemClick;

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
                    Type = t.TypeOverride,
                    Group = t.GroupName,
                    Description = t.Description,
                    PollMs = t.PollMs,
                    ReadOnly = t.ReadOnly
                });
            }

            // snapshot for undo
            SaveSnapshot();
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

            var defs = _rows.Select(r =>
            {
                // Address1Based hesaplama
                int address1Based = 0;
                
                // Eğer Address bir sayıysa direkt kullan
                if (int.TryParse(r.Address.Trim(), out var numericAddress))
                {
                    address1Based = numericAddress;
                }
                else
                {
                    // Operand ise (MW0, MI1, vs) - profile'dan çöz
                    // Not: Burası opsiyonel, Address string olarak saklanıyor
                    address1Based = 0; // Default
                }
                
                return new TagDefinition
                {
                    Name = r.Name.Trim(),
                    Address = r.Address.Trim(),
                    TypeOverride = r.Type.Trim(),
                    GroupName = r.Group ?? "",
                    Description = r.Description ?? "",
                    PollMs = r.PollMs,
                    ReadOnly = r.ReadOnly,
                    Address1Based = address1Based
                };
            }).ToList();

            _tagRepo.UpsertMany(defs);

            // update snapshot after save
            SaveSnapshot();
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

        private void SaveSnapshot()
        {
            _snapshot = _rows.Select(r => new TagRow
            {
                Id = r.Id,
                Name = r.Name,
                Address = r.Address,
                Type = r.Type,
                Group = r.Group,
                Description = r.Description,
                PollMs = r.PollMs,
                ReadOnly = r.ReadOnly,
                LastValue = r.LastValue,
                Status = r.Status,
                UpdatedAt = r.UpdatedAt
            }).ToList();
        }

        private void RestoreSnapshot()
        {
            _rows.Clear();
            foreach (var r in _snapshot)
                _rows.Add(r);
        }

        // --- Ribbon handlers ---
        private void BtnImportExcel_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                using (var dlg = new OpenFileDialog())
                {
                    dlg.Filter = "CSV dosyası (*.csv)|*.csv|Tüm dosyalar (*.*)|*.*";
                    if (dlg.ShowDialog(this) != DialogResult.OK) return;

                    var lines = File.ReadAllLines(dlg.FileName);
                    if (lines.Length < 1) return;

                    var header = lines[0].Split(new[] { ',', ';', '\t' });
                    var idxName = Array.FindIndex(header, h => h.Trim().Equals("Name", StringComparison.OrdinalIgnoreCase) || h.Trim().Equals("Operanlar", StringComparison.OrdinalIgnoreCase));
                    var idxAddress = Array.FindIndex(header, h => h.Trim().Equals("Address", StringComparison.OrdinalIgnoreCase) || h.Trim().Equals("Modbus adresi", StringComparison.OrdinalIgnoreCase));
                    var idxType = Array.FindIndex(header, h => h.Trim().Equals("Type", StringComparison.OrdinalIgnoreCase) || h.Trim().Equals("Veri tipi", StringComparison.OrdinalIgnoreCase));
                    var idxGroup = Array.FindIndex(header, h => h.Trim().Equals("Group", StringComparison.OrdinalIgnoreCase) || h.Trim().Equals("Sistem adı", StringComparison.OrdinalIgnoreCase));
                    var idxDesc = Array.FindIndex(header, h => h.Trim().Equals("Description", StringComparison.OrdinalIgnoreCase) || h.Trim().Equals("Açıklama", StringComparison.OrdinalIgnoreCase));
                    var idxPoll = Array.FindIndex(header, h => h.Trim().Equals("PollMs", StringComparison.OrdinalIgnoreCase) || h.Trim().Equals("Poll (ms)", StringComparison.OrdinalIgnoreCase));
                    var idxRo = Array.FindIndex(header, h => h.Trim().Equals("ReadOnly", StringComparison.OrdinalIgnoreCase));

                    var imported = new List<TagRow>();
                    for (int i = 1; i < lines.Length; i++)
                    {
                        var parts = lines[i].Split(new[] { ',', ';', '\t' });
                        if (parts.Length == 0) continue;

                        var name = idxName >= 0 && idxName < parts.Length ? parts[idxName].Trim() : null;
                        var addr = idxAddress >= 0 && idxAddress < parts.Length ? parts[idxAddress].Trim() : null;
                        var type = idxType >= 0 && idxType < parts.Length ? parts[idxType].Trim() : "UShort";
                        // dış kaynaktaki tipleri iç sistem tiplerine çevir
                        if (!string.IsNullOrWhiteSpace(type))
                        {
                            var tLower = type.Trim().ToLowerInvariant();
                            switch (tLower)
                            {
                                case "real":
                                case "float":
                                    type = "Float"; // 32-bit float
                                    break;
                                case "integer":
                                case "int":
                                case "dint":
                                    type = "Int32"; // 32-bit signed
                                    break;
                                case "bit":
                                case "bool":
                                    type = "Bool";
                                    break;
                                case "word":
                                case "ushort":
                                    type = "UShort";
                                    break;
                            }
                        }
                        var group = idxGroup >= 0 && idxGroup < parts.Length ? parts[idxGroup].Trim() : null;
                        var desc = idxDesc >= 0 && idxDesc < parts.Length ? parts[idxDesc].Trim() : null;
                        int poll = 250;
                        if (idxPoll >= 0 && idxPoll < parts.Length) int.TryParse(parts[idxPoll], out poll);
                        bool ro = false;
                        if (idxRo >= 0 && idxRo < parts.Length) bool.TryParse(parts[idxRo], out ro);

                        if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(addr)) continue;

                        imported.Add(new TagRow
                        {
                            Name = name ?? addr,
                            Address = addr ?? "",
                            Type = string.IsNullOrWhiteSpace(type) ? "UShort" : type,
                            Group = group,
                            Description = desc,
                            PollMs = poll <= 0 ? 250 : poll,
                            ReadOnly = ro
                        });
                    }

                    if (imported.Count == 0)
                    {
                        XtraMessageBox.Show("İçe aktarılan veri bulunamadı.", "Bilgi");
                        return;
                    }

                    _rows.Clear();
                    foreach (var r in imported) _rows.Add(r);

                    SaveSnapshot();

                    XtraMessageBox.Show("İçe aktarıldı. Değişiklikleri kalıcı hale getirmek için 'Kaydet' butonuna basın.", "Tamam");
                }
            }
            catch (Exception ex)
            {
                try { _log?.Error(nameof(FrmTagManager), "Import hatası", ex); } catch { }
                XtraMessageBox.Show("İçe aktarma sırasında hata: " + ex.Message, "Hata");
            }
        }

        private void BtnExportExcel_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                using (var dlg = new SaveFileDialog())
                {
                    dlg.Filter = "CSV dosyası (*.csv)|*.csv|Tüm dosyalar (*.*)|*.*";
                    dlg.FileName = "tags_export.csv";
                    if (dlg.ShowDialog(this) != DialogResult.OK) return;

                    using (var sw = new StreamWriter(dlg.FileName, false))
                    {
                        sw.WriteLine("Name,Address,Type,Group,Description,PollMs,ReadOnly");
                        foreach (var r in _rows)
                        {
                            sw.WriteLine($"\"{Escape(r.Name)}\",\"{Escape(r.Address)}\",\"{Escape(r.Type)}\",\"{Escape(r.Group)}\",\"{Escape(r.Description)}\",{r.PollMs},{(r.ReadOnly?1:0)}");
                        }
                    }

                    XtraMessageBox.Show("Dışa aktarma tamamlandı.", "Tamam");
                }
            }
            catch (Exception ex)
            {
                try { _log?.Error(nameof(FrmTagManager), "Export hatası", ex); } catch { }
                XtraMessageBox.Show("Dışa aktarma sırasında hata: " + ex.Message, "Hata");
            }
        }

        private static string Escape(string s)
        {
            if (s == null) return string.Empty;
            return s.Replace("\"", "\"\"");
        }

        private void BtnReload_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                LoadFromDb();
                XtraMessageBox.Show("Yenilendi.", "Tamam");
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Yenileme hatası: " + ex.Message, "Hata");
            }
        }

        private void BtnNewTag_ItemClick(object sender, ItemClickEventArgs e)
        {
            var t = new TagRow { Name = "YeniTag", Address = "", Type = "UShort", PollMs = 250, ReadOnly = false };
            _rows.Insert(0, t);
            gvTags.FocusedRowHandle = 0;
            gvTags.ShowEditor();
        }

        private async void BtnReadSelected_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (_modbusService == null)
            {
                XtraMessageBox.Show("Modbus servisi bağlı değil.", "Uyarı");
                return;
            }

            var sels = gvTags.GetSelectedRows();
            foreach (var r in sels)
            {
                var row = gvTags.GetRow(r) as TagRow;
                if (row == null) continue;

                try
                {
                    var res = await _modbusService.ReadAutoAsync(row.Address, CancellationToken.None).ConfigureAwait(false);
                    if (res.Success)
                    {
                        BeginInvoke((Action)(() =>
                        {
                            row.LastValue = res.Value?.ToString();
                            row.Status = "Başarılı";
                            row.UpdatedAt = DateTime.Now;
                            gvTags.RefreshRow(gvTags.GetRowHandle(r));
                        }));
                    }
                    else
                    {
                        BeginInvoke((Action)(() =>
                        {
                            row.Status = "Hata: " + res.Error;
                            row.UpdatedAt = DateTime.Now;
                            gvTags.RefreshRow(gvTags.GetRowHandle(r));
                        }));
                    }
                }
                catch (Exception ex)
                {
                    try { _log?.Error(nameof(FrmTagManager), "Seçilen hatayı oku", ex); } catch { }
                    BeginInvoke((Action)(() =>
                    {
                        row.Status = "Hata";
                        row.UpdatedAt = DateTime.Now;
                        gvTags.RefreshRow(gvTags.GetRowHandle(r));
                    }));
                }
            }
        }

        private async void BtnWriteSelected_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (_modbusService == null)
                {
                    XtraMessageBox.Show("Modbus servisi bağlı değil.", "Uyarı");
                    return;
                }

                var sels = gvTags.GetSelectedRows();
                if (sels == null || sels.Length == 0)
                {
                    XtraMessageBox.Show("Lütfen tek bir satır seçin.", "Uyarı");
                    return;
                }
                var row = gvTags.GetRow(sels[0]) as TagRow;
                if (row == null) return;

                // Dokunmatik hesap makinesi ile değer al
                decimal value;
                var initial = 0m;
                decimal.TryParse(row.LastValue, out initial);
                if (!KaynakMakinesi.UI.Controls.FrmTouchCalculator.TryPrompt(this, $"{row.Name} değer yaz", initial, out value))
                    return; // vazgeçildi

                var text = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                var ok = await _modbusService.WriteTextAsync(row.Address, text, CancellationToken.None).ConfigureAwait(false);
                BeginInvoke((Action)(() =>
                {
                    row.Status = ok ? "Yazma OK" : "Yazma başarısız";
                    row.UpdatedAt = DateTime.Now;
                    if (ok) row.LastValue = text;
                    gvTags.RefreshData();
                }));
            }
            catch (Exception ex)
            {
                try { _log?.Error(nameof(FrmTagManager), "WriteSelected hata", ex); } catch { }
                XtraMessageBox.Show("Yazma sırasında hata: " + ex.Message, "Hata");
            }
        }

        private void BtnStartMonitor_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (_modbusService == null)
            {
                XtraMessageBox.Show("Modbus servisi bağlı değil.", "Uyarı");
                return;
            }

            if (_monitorCts != null) return; // zaten çalışıyor

            _monitorCts = new CancellationTokenSource();
            var ct = _monitorCts.Token;

            _monitorTask = Task.Run(async () =>
            {
                try
                {
                    while (!ct.IsCancellationRequested)
                    {
                        var rows = _rows.ToArray();
                        foreach (var row in rows)
                        {
                            if (ct.IsCancellationRequested) break;
                            try
                            {
                                var res = await _modbusService.ReadAutoAsync(row.Address, ct).ConfigureAwait(false);
                                if (res.Success)
                                {
                                    BeginInvoke((Action)(() =>
                                    {
                                        row.LastValue = res.Value?.ToString();
                                        row.Status = "Başarılı";
                                        row.UpdatedAt = DateTime.Now;
                                        gvTags.RefreshData();
                                    }));
                                }
                                else
                                {
                                    BeginInvoke((Action)(() =>
                                    {
                                        row.Status = "Hata";
                                        row.UpdatedAt = DateTime.Now;
                                        gvTags.RefreshData();
                                    }));
                                }
                            }
                            catch (OperationCanceledException) { break; }
                            catch (Exception ex)
                            {
                                try { _log?.Error(nameof(FrmTagManager), "Monitor okuma hatası", ex); } catch { }
                                BeginInvoke((Action)(() =>
                                {
                                    row.Status = "Hata";
                                    row.UpdatedAt = DateTime.Now;
                                    gvTags.RefreshData();
                                }));
                            }

                            // wait row-specific poll or small delay
                            await Task.Delay(Math.Max(100, row.PollMs), ct).ConfigureAwait(false);
                        }

                        // kısa aralık
                        await Task.Delay(100, ct).ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    try { _log?.Error(nameof(FrmTagManager), "Monitor döngüsü hata verdi", ex); } catch { }
                }
            }, ct).ContinueWith(t =>
            {
                if (t.IsFaulted) try { _log?.Error(nameof(FrmTagManager), "Monitor background task faulted", t.Exception); } catch { }
            }, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);

            XtraMessageBox.Show("İzleme başlatıldı.", "Bilgi");
        }

        private void BtnStopMonitor_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                _monitorCts?.Cancel();
                try { _monitorTask?.Wait(500); } catch { }
                _monitorCts = null;
                _monitorTask = null;
                XtraMessageBox.Show("İzleme durduruldu.", "Bilgi");
            }
            catch (Exception ex)
            {
                try { _log?.Error(nameof(FrmTagManager), "Monitor stop hatası", ex); } catch { }
                XtraMessageBox.Show("İzleme durdurulurken hata: " + ex.Message, "Hata");
            }
        }

        private void BtnUndo_ItemClick(object sender, ItemClickEventArgs e)
        {
            RestoreSnapshot();
            XtraMessageBox.Show("Geri alındı.", "Bilgi");
        }

        private void btnKaydet_Click(object sender, EventArgs e)
        {
            try
            {
                SaveToDb();
                XtraMessageBox.Show("Kaydedildi.", "Tamam");
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Kaydetme hatası: " + ex.Message, "Hata");
            }
        }

        private void btnSil_Click(object sender, EventArgs e)
        {
            var row = gvTags.GetFocusedRow() as TagRow;
            if (row == null) return;

            if (XtraMessageBox.Show($"{row.Name} silinsin mi?", "Onay", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;

            _tagRepo.DeleteByName(row.Name);
            _rows.Remove(row);
        }

        private void btnSaveChanges_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                SaveToDb();
                XtraMessageBox.Show("Kaydedildi.", "Tamam");
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
