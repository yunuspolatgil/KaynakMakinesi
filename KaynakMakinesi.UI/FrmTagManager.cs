using DevExpress.XtraBars;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraEditors;
using KaynakMakinesi.Core.Logging;
using KaynakMakinesi.Core.Model;
using KaynakMakinesi.Core.Plc.Service;
using KaynakMakinesi.Core.Entities;
using KaynakMakinesi.Core.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KaynakMakinesi.UI
{
    public partial class FrmTagManager : RibbonForm
    {
        private BindingList<TagEntityRow> _rows = new BindingList<TagEntityRow>();
        private readonly ITagEntityRepository _tagRepo;
        private readonly IModbusService _modbusService;
        private readonly IAppLogger _log;

        // snapshot for Undo
        private List<TagEntityRow> _snapshot = new List<TagEntityRow>();

        // monitor
        private CancellationTokenSource _monitorCts;
        private Task _monitorTask;

        public FrmTagManager(ITagEntityRepository tagRepo, IModbusService modbusService = null, IAppLogger log = null)
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
            
            // Yeni butonlar Designer'da event handler'larý tanýmlý
            // btnDeleteSelected.ItemClick ve btnDeleteAll.ItemClick Designer.cs'de ayarlandý

            SetupGrid();
            LoadFromDb();
        }

        private void LoadFromDb()
        {
            try
            {
                var entities = _tagRepo.GetAll();

                _rows.Clear();
                foreach (var entity in entities)
                {
                    _rows.Add(MapEntityToRow(entity));
                }

                SaveSnapshot();
                _log?.Info(nameof(FrmTagManager), $"{_rows.Count} adet tag yüklendi");
            }
            catch (Exception ex)
            {
                _log?.Error(nameof(FrmTagManager), "Tag yükleme hatasý", ex);
                XtraMessageBox.Show($"Tag yükleme hatasý:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    throw new Exception("Tag Adý boþ olamaz.");
                if (string.IsNullOrWhiteSpace(r.Address))
                    throw new Exception($"{r.Name} için Adres boþ olamaz.");
                if (string.IsNullOrWhiteSpace(r.DataType))
                    r.DataType = "UShort"; // default
                if (r.PollMs <= 0) r.PollMs = 250;
            }

            var entities = _rows.Select(MapRowToEntity).ToList();

            _tagRepo.UpsertMany(entities);
            SaveSnapshot();
            
            _log?.Info(nameof(FrmTagManager), $"{entities.Count} adet tag kaydedildi");
        }

        private void SetupGrid()
        {
            gcTags.DataSource = _rows;

            gvTags.OptionsBehavior.Editable = true;
            gvTags.OptionsView.NewItemRowPosition = DevExpress.XtraGrid.Views.Grid.NewItemRowPosition.Top;
            gvTags.OptionsFind.AlwaysVisible = true;
            gvTags.OptionsView.ShowAutoFilterRow = true;
            gvTags.OptionsView.RowAutoHeight = true;
            gvTags.RowHeight = 34;

            // ÇOK SEÇME AKTÝF
            gvTags.OptionsSelection.MultiSelect = true;
            gvTags.OptionsSelection.MultiSelectMode = DevExpress.XtraGrid.Views.Grid.GridMultiSelectMode.CheckBoxRowSelect;
            
            // Checkbox column geniþliði
            gvTags.OptionsSelection.CheckBoxSelectorColumnWidth = 40;

            gvTags.PopulateColumns();

            // Column captions - TagEntityRow property'lerine göre
            gvTags.Columns[nameof(TagEntityRow.Name)].Caption = "Tag Adý";
            gvTags.Columns[nameof(TagEntityRow.Address)].Caption = "Adres";
            gvTags.Columns[nameof(TagEntityRow.DataType)].Caption = "Veri Tipi";
            gvTags.Columns[nameof(TagEntityRow.GroupName)].Caption = "Grup";
            gvTags.Columns[nameof(TagEntityRow.Description)].Caption = "Açýklama";
            gvTags.Columns[nameof(TagEntityRow.PollMs)].Caption = "Poll (ms)";
            gvTags.Columns[nameof(TagEntityRow.ReadOnly)].Caption = "RO";
            gvTags.Columns[nameof(TagEntityRow.Scale)].Caption = "Scale";
            gvTags.Columns[nameof(TagEntityRow.Offset)].Caption = "Offset";
            gvTags.Columns[nameof(TagEntityRow.Unit)].Caption = "Birim";
            gvTags.Columns[nameof(TagEntityRow.LastValue)].Caption = "Son Deðer";
            gvTags.Columns[nameof(TagEntityRow.Status)].Caption = "Durum";
            gvTags.Columns[nameof(TagEntityRow.UpdatedAt)].Caption = "Güncellendi";

            // Hide internal fields
            gvTags.Columns[nameof(TagEntityRow.Id)].Visible = false;
            gvTags.Columns[nameof(TagEntityRow.MetadataJson)].Visible = false;

            // ReadOnly columns (monitor için)
            gvTags.Columns[nameof(TagEntityRow.LastValue)].OptionsColumn.AllowEdit = false;
            gvTags.Columns[nameof(TagEntityRow.Status)].OptionsColumn.AllowEdit = false;
            gvTags.Columns[nameof(TagEntityRow.UpdatedAt)].OptionsColumn.AllowEdit = false;
            
            // ?? DataType sütununa dropdown (ComboBox) ekle
            var dataTypeColumn = gvTags.Columns[nameof(TagEntityRow.DataType)];
            var comboBoxEdit = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox();
            comboBoxEdit.Items.AddRange(new object[] { "Bool", "UShort", "Int32", "Float" });
            comboBoxEdit.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor; // Sadece dropdown'dan seçim
            gcTags.RepositoryItems.Add(comboBoxEdit);
            dataTypeColumn.ColumnEdit = comboBoxEdit;
        }

        private void SaveSnapshot()
        {
            _snapshot = _rows.Select(r => new TagEntityRow
            {
                Id = r.Id,
                Name = r.Name,
                Address = r.Address,
                DataType = r.DataType,
                GroupName = r.GroupName,
                Description = r.Description,
                PollMs = r.PollMs,
                ReadOnly = r.ReadOnly,
                Scale = r.Scale,
                Offset = r.Offset,
                Unit = r.Unit,
                LastValue = r.LastValue,
                Status = r.Status,
                UpdatedAt = r.UpdatedAt,
                MetadataJson = r.MetadataJson
            }).ToList();
        }

        private void RestoreSnapshot()
        {
            _rows.Clear();
            foreach (var r in _snapshot)
                _rows.Add(r);
        }
        
        #region Entity <-> Row Mapping
        
        /// <summary>
        /// TagEntity -> TagEntityRow (UI row)
        /// </summary>
        private TagEntityRow MapEntityToRow(TagEntity entity)
        {
            return new TagEntityRow
            {
                Id = entity.Id,
                Name = entity.Name,
                Address = entity.Address,
                DataType = entity.DataType,
                GroupName = entity.GroupName,
                Description = entity.Description,
                PollMs = entity.PollMs,
                ReadOnly = entity.ReadOnly,
                Scale = entity.Scale,
                Offset = entity.Offset,
                Unit = entity.Unit,
                MetadataJson = entity.MetadataJson
            };
        }
        
        /// <summary>
        /// TagEntityRow (UI row) -> TagEntity
        /// </summary>
        private TagEntity MapRowToEntity(TagEntityRow row)
        {
            var entity = new TagEntity
            {
                Id = row.Id,
                Name = row.Name?.Trim(),
                Address = row.Address?.Trim(),
                DataType = row.DataType?.Trim() ?? "UShort",
                GroupName = row.GroupName?.Trim() ?? "",
                MetadataJson = row.MetadataJson
            };
            
            // Convenience property'leri metadata'ya yaz
            entity.Description = row.Description ?? "";
            entity.PollMs = row.PollMs <= 0 ? 250 : row.PollMs;
            entity.ReadOnly = row.ReadOnly;
            entity.Scale = row.Scale;
            entity.Offset = row.Offset;
            entity.Unit = row.Unit ?? "";
            
            return entity;
        }
        
        #endregion

        // --- Ribbon handlers ---
        private void BtnImportExcel_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                using (var dlg = new OpenFileDialog())
                {
                    dlg.Filter = "CSV/GPF dosyasý (*.csv;*.gpf)|*.csv;*.gpf|Tüm dosyalar (*.*)|*.*";
                    if (dlg.ShowDialog(this) != DialogResult.OK) return;

                    // GMT PLC dosyalarý MUTLAKA UTF-16 LE kullanýr
                    string[] lines;
                    try
                    {
                        // Direkt UTF-16 ile baþla
                        lines = File.ReadAllLines(dlg.FileName, Encoding.Unicode);
                    }
                    catch
                    {
                        // Fallback UTF-8
                        try
                        {
                            lines = File.ReadAllLines(dlg.FileName, Encoding.UTF8);
                        }
                        catch
                        {
                            // Son çare - Default encoding
                            lines = File.ReadAllLines(dlg.FileName);
                        }
                    }

                    _log?.Debug(nameof(FrmTagManager), $"Toplam {lines.Length} satýr okundu");
                    if (lines.Length > 0)
                    {
                        var preview = lines[0].Length > 100 ? lines[0].Substring(0, 100) : lines[0];
                        _log?.Debug(nameof(FrmTagManager), $"Ýlk satýr ({lines[0].Length} karakter): {preview}");
                    }

                    if (lines.Length < 2)
                    {
                        XtraMessageBox.Show("Dosya boþ veya geçersiz.", "Hata");
                        return;
                    }

                    // Ýlk satýrý atla (metadata varsa)
                    int headerIndex = 0;
                    if (lines[0].Contains("AddrTagLib") || lines[0].Contains("Version") || lines[0].Contains("V106"))
                        headerIndex = 1;

                    if (headerIndex >= lines.Length)
                    {
                        XtraMessageBox.Show("Header bulunamadý.", "Hata");
                        return;
                    }

                    var headerLine = lines[headerIndex];
                    
                    // Delimiter detection - TAB her zaman öncelikli
                    char delimiter = '\t';
                    if (!headerLine.Contains('\t'))
                    {
                        if (headerLine.Contains(','))
                            delimiter = ',';
                        else if (headerLine.Contains(';'))
                            delimiter = ';';
                    }
                    
                    _log?.Debug(nameof(FrmTagManager), $"Delimiter: '{(delimiter == '\t' ? "TAB" : delimiter.ToString())}', Header Index: {headerIndex}");
                    _log?.Debug(nameof(FrmTagManager), $"Header line ({headerLine.Length} karakter)");

                    // Basit split ile test et
                    var headerTest = headerLine.Split(delimiter);
                    _log?.Debug(nameof(FrmTagManager), $"Basit split sonucu: {headerTest.Length} kolon");
                    
                    var header = ParseCsvLine(headerLine, delimiter);
                    
                    _log?.Debug(nameof(FrmTagManager), $"Header kolon sayýsý: {header.Length}");
                    for (int i = 0; i < Math.Min(15, header.Length); i++)
                    {
                        var colPreview = header[i];
                        if (colPreview != null && colPreview.Length > 50)
                            colPreview = colPreview.Substring(0, 50) + "...";
                        _log?.Debug(nameof(FrmTagManager), $"  Kolon[{i}]: '{colPreview}' (uzunluk: {header[i]?.Length ?? 0})");
                    }
                    
                    var idxName = FindColumnIndex(header, "AddrTagName", "Name", "Operanlar", "TagName");
                    var idxAddr = FindColumnIndex(header, "Addr", "Address");
                    var idxModbusAddr = FindColumnIndex(header, "Modbus adresi", "ModbusAddress", "AddrPLCID");
                    var idxDataType = FindColumnIndex(header, "AddrTagDataType", "DataType", "Type", "Veri tipi");
                    var idxAddrType = FindColumnIndex(header, "AddrType");
                    
                    _log?.Debug(nameof(FrmTagManager), $"Index - Name:{idxName}, Addr:{idxAddr}, ModbusAddr:{idxModbusAddr}, DataType:{idxDataType}, AddrType:{idxAddrType}");
                    
                    // Eðer hala -1 ise, RAW header'ý göster
                    if (idxName == -1)
                    {
                        var debugMsg = "HEADER KOLONLARI:\n";
                        for (int i = 0; i < Math.Min(10, header.Length); i++)
                        {
                            debugMsg += $"[{i}] = '{header[i]}'\n";
                        }
                        _log?.Warn(nameof(FrmTagManager), debugMsg);
                    }
                    
                    var idxGroup = FindColumnIndex(header, "Group", "Sistem adý", "GroupName");
                    var idxDesc = FindColumnIndex(header, "Description", "Açýklama");
                    var idxPoll = FindColumnIndex(header, "PollMs", "Poll (ms)");
                    var idxRo = FindColumnIndex(header, "ReadOnly");

                    var imported = new List<TagEntityRow>();
                    int skippedCount = 0;
                    
                    for (int i = headerIndex + 1; i < lines.Length; i++)
                    {
                        if (string.IsNullOrWhiteSpace(lines[i]))
                        {
                            skippedCount++;
                            continue;
                        }

                        var parts = ParseCsvLine(lines[i], delimiter);
                        if (parts.Length < idxName + 1)
                        {
                            skippedCount++;
                            continue;
                        }

                        var name = GetValue(parts, idxName)?.Trim('"', ' ');
                        if (string.IsNullOrWhiteSpace(name))
                        {
                            if (i - headerIndex <= 3)
                                _log?.Debug(nameof(FrmTagManager), $"Satýr {i}: Name boþ (index={idxName}, parts.Length={parts.Length})");
                            skippedCount++;
                            continue;
                        }

                        // ADRES: Ýlk önce "Modbus adresi" kolonuna bak, yoksa AddrType+Addr'dan hesapla
                        string modbusAddress = null;

                        // 1. Öncelik: "Modbus adresi" (AddrPLCID) kolonu varsa ve boþ deðilse direkt kullan
                        if (idxModbusAddr >= 0)
                        {
                            modbusAddress = GetValue(parts, idxModbusAddr)?.Trim('"', ' ');
                            // "0" deðerini de geçersiz say (GMT PLC default deðeri)
                            if (modbusAddress == "0")
                            {
                                modbusAddress = null;
                            }
                            
                            if (!string.IsNullOrWhiteSpace(modbusAddress))
                            {
                                _log?.Debug(nameof(FrmTagManager), $"Satýr {i}: 'AddrPLCID' kolonundan alýndý: {modbusAddress}");
                            }
                        }
                        
                        // 2. Alternatif: AddrType ve Addr'den Modbus adresini hesapla
                        // DÝKKAT: GMT PLC'de Addr kolonu direkt Modbus offset deðerini içerir
                        // Örnek: AddrType=4, Addr=2001 -> 42001 (40000 + 2001)
                        //        AddrType=4, Addr=1    -> 40001 (40000 + 1)
                        //        AddrType=1, Addr=1    -> 10001 (10000 + 1)
                        //        AddrType=0, Addr=1    -> 1 (0 + 1)
                        if (string.IsNullOrWhiteSpace(modbusAddress))
                        {
                            var addr = GetValue(parts, idxAddr)?.Trim('"', ' ');
                            var addrType = GetValue(parts, idxAddrType)?.Trim('"', ' ');
                            
                            if (!string.IsNullOrWhiteSpace(addrType) && !string.IsNullOrWhiteSpace(addr))
                            {
                                if (int.TryParse(addrType, out var typeCode) && int.TryParse(addr, out var address))
                                {
                                    // GMT PLC AddrType -> Modbus base address mapping
                                    int baseAddress = 0;
                                    switch (typeCode)
                                    {
                                        case 0: // Coil (MB) - Coil adresleri 1-based
                                            // Addr=1 -> address1Based=1, Addr=2 -> address1Based=2
                                            baseAddress = 0;  // Base 0 çünkü Addr zaten 1'den baþlýyor
                                            break;
                                        case 1: // Discrete Input (IP) - base = 10000
                                            baseAddress = 10000;
                                            break;
                                        case 2: // Input Register (IW) - base = 30000
                                            baseAddress = 30000;
                                            break;
                                        case 4: // Holding Register (MW, MI, MF) - base = 40000
                                            baseAddress = 40000;
                                            break;
                                        default:
                                            _log?.Warn(nameof(FrmTagManager), $"Satýr {i}: Bilinmeyen AddrType={typeCode}");
                                            break;
                                    }
                                    
                                    // Coil için Addr direkt adrestir (1-based)
                                    // Diðerleri için base + offset
                                    int modbusAddr;
                                    if (typeCode == 0)
                                    {
                                        // Coil: Addr direkt 1-based adrestir
                                        modbusAddr = address;
                                    }
                                    else
                                    {
                                        // Diðerleri: Base + Offset
                                        modbusAddr = baseAddress + address;
                                    }
                                    
                                    modbusAddress = modbusAddr.ToString();
                                    
                                    _log?.Debug(nameof(FrmTagManager), $"Satýr {i}: AddrType={typeCode}, Addr={address}, Base={baseAddress} -> Modbus={modbusAddress}");
                                }
                            }
                        }
                        
                        // 3. Son çare: Addr kolonunu direkt kullan
                        if (string.IsNullOrWhiteSpace(modbusAddress) && idxAddr >= 0)
                        {
                            modbusAddress = GetValue(parts, idxAddr)?.Trim('"', ' ');
                            _log?.Debug(nameof(FrmTagManager), $"Satýr {i}: Addr kolonundan direkt kullanýldý: {modbusAddress}");
                        }
                        
                        _log?.Debug(nameof(FrmTagManager), $"Satýr {i}: Name='{name}', Final Address='{modbusAddress}'");

                        if (string.IsNullOrWhiteSpace(modbusAddress))
                        {
                            _log?.Debug(nameof(FrmTagManager), $"Satýr {i}: Adres boþ, atlandý");
                            skippedCount++;
                            continue;
                        }

                        var dataTypeStr = GetValue(parts, idxDataType)?.Trim('"', ' ');
                        var type = ConvertGmtDataTypeToInternal(dataTypeStr);

                        var group = GetValue(parts, idxGroup)?.Trim('"', ' ');
                        var desc = GetValue(parts, idxDesc)?.Trim('"', ' ');
                        
                        // ?? YENÝ: Eðer grup boþsa, tag isminden otomatik belirle
                        if (string.IsNullOrWhiteSpace(group))
                        {
                            group = AutoDetectGroupFromTagName(name);
                            
                            if (!string.IsNullOrWhiteSpace(group))
                            {
                                _log?.Debug(nameof(FrmTagManager), $"Satýr {i}: Grup otomatik atandý: '{name}' -> Grup: '{group}'");
                            }
                        }
                        
                        int poll = 250;

                        var pollStr = GetValue(parts, idxPoll);
                        if (!string.IsNullOrWhiteSpace(pollStr))
                            int.TryParse(pollStr, out poll);

                        bool ro = false;
                        var roStr = GetValue(parts, idxRo);
                        if (!string.IsNullOrWhiteSpace(roStr))
                        {
                            ro = roStr.Trim() == "1" || roStr.Trim().Equals("true", StringComparison.OrdinalIgnoreCase);
                        }

                        imported.Add(new TagEntityRow
                        {
                            Name = name,
                            Address = modbusAddress,
                            DataType = type,
                            GroupName = group ?? "",
                            Description = desc ?? "",
                            PollMs = poll <= 0 ? 250 : poll,
                            ReadOnly = ro
                        });

                        if (imported.Count <= 3)
                            _log?.Debug(nameof(FrmTagManager), $"  Tag eklendi: {name} -> {modbusAddress} ({type})");
                    }

                    _log?.Info(nameof(FrmTagManager), $"Ýçe aktarma tamamlandý: {imported.Count} adet tag eklendi, {skippedCount} satýr atlandý");

                    if (imported.Count == 0)
                    {
                        var msg = $"Ýçe aktarýlan veri bulunamadý.\n\n" +
                                  $"Toplam satýr: {lines.Length}\n" +
                                  $"Header index: {headerIndex}\n" +
                                  $"Atlanan satýr: {skippedCount}\n\n" +
                                  $"Kolon indexleri:\n" +
                                  $"  Name: {idxName}\n" +
                                  $"  Addr: {idxAddr}\n" +
                                  $"  ModbusAddr: {idxModbusAddr}\n" +
                                  $"  AddrType: {idxAddrType}\n" +
                                  $"  DataType: {idxDataType}\n\n" +
                                  $"Delimiter: {(delimiter == '\t' ? "TAB" : delimiter.ToString())}\n" +
                                  $"Header kolon sayýsý: {header.Length}\n\n" +
                                  $"Ýlk 5 header kolonu:\n";
                        
                        for (int i = 0; i < Math.Min(5, header.Length); i++)
                        {
                            msg += $"  [{i}] = '{header[i]}'\n";
                        }
                        
                        msg += "\nLoglara bakýn veya dosyayý kontrol edin.";
                        XtraMessageBox.Show(msg, "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    _rows.Clear();
                    foreach (var r in imported) _rows.Add(r);

                    SaveSnapshot();

                    XtraMessageBox.Show($"{imported.Count} adet tag içe aktarýldý.\nDeðiþiklikleri kalýcý hale getirmek için 'Kaydet' butonuna basýn.", "Tamam");
                }
            }
            catch (Exception ex)
            {
                try { _log?.Error(nameof(FrmTagManager), "Import hatasý", ex); } catch { }
                XtraMessageBox.Show("Ýçe aktarma sýrasýnda hata:\n" + ex.Message + "\n\n" + ex.StackTrace, "Hata");
            }
        }

        private void BtnExportExcel_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                using (var dlg = new SaveFileDialog())
                {
                    dlg.Filter = "CSV Dosyasý (*.csv)|*.csv|Tüm Dosyalar (*.*)|*.*";
                    dlg.DefaultExt = "csv";
                    dlg.AddExtension = true;
                    if (dlg.ShowDialog(this) != DialogResult.OK) return;

                    var sb = new StringBuilder();

                    // Header yaz
                    for (int i = 0; i < gvTags.Columns.Count; i++)
                    {
                        var col = gvTags.Columns[i];
                        if (!col.Visible) continue;

                        if (sb.Length > 0) sb.Append(',');
                        sb.Append('"' + col.Caption + '"');
                    }

                    sb.AppendLine();

                    // Data yaz
                    foreach (var row in _rows)
                    {
                        for (int i = 0; i < gvTags.Columns.Count; i++)
                        {
                            var col = gvTags.Columns[i];
                            if (!col.Visible) continue;

                            if (i > 0) sb.Append(',');

                            var value = row.GetType().GetProperty(col.FieldName).GetValue(row, null);
                            if (value != null)
                            {
                                sb.Append('"' + value.ToString().Replace("\"", "\"\"") + '"');
                            }
                        }

                        sb.AppendLine();
                    }

                    File.WriteAllText(dlg.FileName, sb.ToString(), Encoding.UTF8);

                    XtraMessageBox.Show("Veriler baþarýyla dýþa aktarýldý.", "Tamam", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Dýþa aktarma sýrasýnda hata:\n" + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnReload_ItemClick(object sender, ItemClickEventArgs e)
        {
            LoadFromDb();
        }

        private void BtnNewTag_ItemClick(object sender, ItemClickEventArgs e)
        {
            var newItem = new TagEntityRow 
            { 
                Name = "<< Yeni Tag >>", 
                Address = "0", 
                DataType = "UShort", 
                PollMs = 250 
            };
            _rows.Insert(0, newItem);
            gvTags.FocusedRowHandle = 0;
            gvTags.FocusedColumn = gvTags.Columns[nameof(TagEntityRow.Name)];
            gvTags.ShowEditor();
        }

        private async void BtnReadSelected_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (_modbusService == null)
            {
                XtraMessageBox.Show("Modbus servisi baðlý deðil.", "Uyarý");
                return;
            }

            var sels = gvTags.GetSelectedRows();
            if (sels.Length == 0)
            {
                XtraMessageBox.Show("Lütfen okumak için en az bir tag seçin.", "Uyarý");
                return;
            }
            
            _log?.Info(nameof(FrmTagManager), $"Tag okuma baþlatýldý. Seçili tag sayýsý: {sels.Length}");
            
            int successCount = 0;
            int errorCount = 0;
            
            foreach (var r in sels)
            {
                var row = gvTags.GetRow(r) as TagEntityRow;
                if (row == null) continue;

                try
                {
                    _log?.Debug(nameof(FrmTagManager), $"Tag okunuyor: {row.Name} -> Address: {row.Address}, Type: {row.DataType}");
                    
                    var res = await _modbusService.ReadAutoAsync(row.Address, CancellationToken.None).ConfigureAwait(false);
                    if (res.Success)
                    {
                        successCount++;
                        _log?.Debug(nameof(FrmTagManager), $"? {row.Name} = {res.Value}");
                        
                        BeginInvoke((Action)(() =>
                        {
                            row.LastValue = res.Value?.ToString();
                            row.Status = "Baþarýlý";
                            row.UpdatedAt = DateTime.Now;
                            gvTags.RefreshRow(gvTags.GetRowHandle(r));
                        }));
                    }
                    else
                    {
                        errorCount++;
                        _log?.Error(nameof(FrmTagManager), $"? {row.Name} okuma hatasý: {res.Error}");
                        
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
                    errorCount++;
                    _log?.Error(nameof(FrmTagManager), $"? {row.Name} exception", ex);
                    
                    BeginInvoke((Action)(() =>
                    {
                        row.Status = "Exception: " + ex.Message;
                        row.UpdatedAt = DateTime.Now;
                        gvTags.RefreshRow(gvTags.GetRowHandle(r));
                    }));
                }
            }
            
            _log?.Info(nameof(FrmTagManager), $"Tag okuma tamamlandý. Baþarýlý: {successCount}, Hatalý: {errorCount}");
            
            BeginInvoke((Action)(() =>
            {
                XtraMessageBox.Show($"Okuma tamamlandý.\n\nBaþarýlý: {successCount}\nHatalý: {errorCount}", "Bilgi");
            }));
        }

        private async void BtnWriteSelected_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (_modbusService == null)
                {
                    XtraMessageBox.Show("Modbus servisi baðlý deðil.", "Uyarý");
                    return;
                }

                var sels = gvTags.GetSelectedRows();
                if (sels == null || sels.Length == 0)
                {
                    XtraMessageBox.Show("Lütfen yazmak için tek bir satýr seçin.", "Uyarý");
                    return;
                }
                
                if (sels.Length > 1)
                {
                    XtraMessageBox.Show("Tek seferde sadece bir tag'e yazabilirsiniz.", "Uyarý");
                    return;
                }
                
                var row = gvTags.GetRow(sels[0]) as TagEntityRow;
                if (row == null) return;

                if (row.ReadOnly)
                {
                    XtraMessageBox.Show($"{row.Name} tag'i ReadOnly!\nYazma iþlemi yapýlamaz.", "Uyarý");
                    return;
                }

                _log?.Info(nameof(FrmTagManager), $"Tag yazma baþlatýldý: {row.Name} -> Address: {row.Address}, Type: {row.DataType}");

                decimal value;
                var initial = 0m;
                decimal.TryParse(row.LastValue, out initial);
                if (!KaynakMakinesi.UI.Controls.FrmTouchCalculator.TryPrompt(this, $"{row.Name} deðer yaz", initial, out value))
                    return;

                _log?.Debug(nameof(FrmTagManager), $"Yazýlacak deðer: {value}");

                var text = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                var ok = await _modbusService.WriteTextAsync(row.Address, text, CancellationToken.None).ConfigureAwait(false);
                
                if (ok)
                {
                    _log?.Info(nameof(FrmTagManager), $"? {row.Name} = {text} yazýldý");
                }
                else
                {
                    _log?.Error(nameof(FrmTagManager), $"? {row.Name} yazma baþarýsýz!");
                }
                
                BeginInvoke((Action)(() =>
                {
                    row.Status = ok ? "Yazma OK" : "Yazma baþarýsýz";
                    row.UpdatedAt = DateTime.Now;
                    if (ok) row.LastValue = text;
                    gvTags.RefreshData();
                    
                    XtraMessageBox.Show(ok ? "Yazma baþarýlý!" : "Yazma BAÞARISIZ!\nLoglara bakýn.", 
                        ok ? "Bilgi" : "Hata",
                        MessageBoxButtons.OK,
                        ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);
                }));
            }
            catch (Exception ex)
            {
                _log?.Error(nameof(FrmTagManager), "WriteSelected exception", ex);
                XtraMessageBox.Show("Yazma sýrasýnda hata: " + ex.Message, "Hata");
            }
        }

        private void BtnStartMonitor_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (_modbusService == null)
            {
                XtraMessageBox.Show("Modbus servisi baðlý deðil.", "Uyarý");
                return;
            }

            if (_monitorCts != null) return;

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
                                        row.Status = "Baþarýlý";
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
                                try { _log?.Error(nameof(FrmTagManager), "Monitor okuma hatasý", ex); } catch { }
                                BeginInvoke((Action)(() =>
                                {
                                    row.Status = "Hata";
                                    row.UpdatedAt = DateTime.Now;
                                    gvTags.RefreshData();
                                }));
                            }

                            await Task.Delay(Math.Max(100, row.PollMs), ct).ConfigureAwait(false);
                        }

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

            XtraMessageBox.Show("Ýzleme baþlatýldý.", "Bilgi");
        }

        private void BtnStopMonitor_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                _monitorCts?.Cancel();
                try { _monitorTask?.Wait(500); } catch { }
                _monitorCts = null;
                _monitorTask = null;
                XtraMessageBox.Show("Ýzleme durduruldu.", "Bilgi");
            }
            catch (Exception ex)
            {
                try { _log?.Error(nameof(FrmTagManager), "Monitor stop hatasý", ex); } catch { }
                XtraMessageBox.Show("Ýzleme durdurulurken hata: " + ex.Message, "Hata");
            }
        }

        private void BtnUndo_ItemClick(object sender, ItemClickEventArgs e)
        {
            RestoreSnapshot();
            gvTags.RefreshData();
        }

        private void gvTags_RowUpdated(object sender, DevExpress.XtraGrid.Views.Base.RowObjectEventArgs e)
        {
            try
            {
                // ?? FÝX: Her satýr deðiþikliðinde tüm DB'yi kaydetme!
                // Sadece deðiþtirilen satýrý kaydet
                var row = e.Row as TagEntityRow;
                if (row == null) return;
                
                // Validasyon
                if (string.IsNullOrWhiteSpace(row.Name))
                {
                    XtraMessageBox.Show("Tag Adý boþ olamaz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                if (string.IsNullOrWhiteSpace(row.Address))
                {
                    XtraMessageBox.Show($"{row.Name} için Adres boþ olamaz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                if (string.IsNullOrWhiteSpace(row.DataType))
                    row.DataType = "UShort";
                
                if (row.PollMs <= 0) 
                    row.PollMs = 250;
                
                // Tek tag'i DB'ye kaydet (Upsert)
                var entity = MapRowToEntity(row);
                _tagRepo.UpsertMany(new[] { entity });
                
                _log?.Debug(nameof(FrmTagManager), $"Tag güncellendi: {row.Name}");
            }
            catch (Exception ex)
            {
                _log?.Error(nameof(FrmTagManager), "Satýr güncelleme hatasý", ex);
                XtraMessageBox.Show($"Kaydetme sýrasýnda hata:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // CSV satýrlarýný ayýrmak için yardýmcý metodlar
        private string[] ParseCsvLine(string line, char delimiter)
        {
            var parts = new List<string>();
            var sb = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == delimiter && !inQuotes)
                {
                    parts.Add(sb.ToString().Trim());
                    sb.Clear();
                }
                else
                {
                    sb.Append(c);
                }
            }

            parts.Add(sb.ToString().Trim());

            return parts.ToArray();
        }

        private int FindColumnIndex(string[] header, params string[] possibleNames)
        {
            for (int i = 0; i < header.Length; i++)
            {
                foreach (var name in possibleNames)
                {
                    if (header[i].Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        private string GetValue(string[] parts, int index)
        {
            if (index < 0 || index >= parts.Length) return null;
            return parts[index];
        }

        private string ConvertGmtDataTypeToInternal(string dataTypeStr)
        {
            if (string.IsNullOrWhiteSpace(dataTypeStr))
                return "UShort";

            dataTypeStr = dataTypeStr.Trim();

            if (int.TryParse(dataTypeStr, out var code))
            {
                switch (code)
                {
                    case 0: return "Bool";
                    case 1: return "UShort";
                    case 2: return "Int32";
                    case 4: return "Float";
                    default: return "UShort";
                }
            }

            var lower = dataTypeStr.ToLowerInvariant();
            switch (lower)
            {
                case "real":
                case "float":
                    return "Float";
                case "integer":
                case "int":
                case "dint":
                    return "Int32";
                case "bit":
                case "bool":
                case "boolean":
                    return "UShort";
                case "word":
                case "ushort":
                case "uint":
                    return "UShort";
                default:
                    return "UShort";
            }
        }
        
        /// <summary>
        /// ?? Tag isminden otomatik grup belirler
        /// Örnek: "K0_Home_Hiz" -> "Motor_K0"
        ///        "K1_Ileri_Hiz" -> "Motor_K1"
        ///        "Temp_Sensor" -> "" (tanýmsýz)
        /// </summary>
        private string AutoDetectGroupFromTagName(string tagName)
        {
            if (string.IsNullOrWhiteSpace(tagName))
                return "";
            
            // Tag adýný büyük harfe çevir (case-insensitive)
            var upperName = tagName.ToUpperInvariant();
            
            // Motor prefix'lerini kontrol et (K0_, K1_, K2_, vb.)
            var motorPrefixes = new[] { "K0_", "K1_", "K2_", "K3_", "K4_", "K5_", "K6_", "K7_", "K8_", "K9_" };
            
            foreach (var prefix in motorPrefixes)
            {
                if (upperName.StartsWith(prefix))
                {
                    // Prefix'ten motor numarasýný al (K0 -> Motor_K0)
                    var motorNum = prefix.TrimEnd('_'); // "K0_" -> "K0"
                    return "Motor_" + motorNum; // "Motor_K0"
                }
            }
            
            // Sistem tag'leri için özel gruplar
            if (upperName.StartsWith("SYS_") || upperName.StartsWith("SYSTEM_"))
                return "System";
            
            if (upperName.StartsWith("ALARM_") || upperName.StartsWith("WARN_"))
                return "Alarms";
            
            if (upperName.StartsWith("TEMP_") || upperName.StartsWith("SENSOR_"))
                return "Sensors";
            
            // Grup bulunamadý
            return "";
        }

        private void btnSaveChanges_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                // ?? FÝX: Tüm satýrlarý kaydet (elle deðiþtirilenler + yeniler)
                gvTags.CloseEditor();
                gvTags.UpdateCurrentRow();
                
                // Validasyon - tüm satýrlar için
                foreach (var r in _rows)
                {
                    if (string.IsNullOrWhiteSpace(r.Name))
                    {
                        XtraMessageBox.Show("Tag Adý boþ olamaz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(r.Address))
                    {
                        XtraMessageBox.Show($"{r.Name} için Adres boþ olamaz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(r.DataType))
                        r.DataType = "UShort";
                    if (r.PollMs <= 0) 
                        r.PollMs = 250;
                }
                
                // Tüm tag'leri DB'ye kaydet (Upsert - var olanlar güncelle, yeniler ekle)
                var entities = _rows.Select(MapRowToEntity).ToList();
                _tagRepo.UpsertMany(entities);
                
                SaveSnapshot(); // Undo için snapshot kaydet
                
                _log?.Info(nameof(FrmTagManager), $"{entities.Count} adet tag kaydedildi");

                XtraMessageBox.Show(
                    $"? {entities.Count} adet tag baþarýyla kaydedildi!\n\n" +
                    "?? ÖNEMLÝ: Tag deðiþikliklerinin etkin olmasý için uygulamayý yeniden baþlatýn.", 
                    "Kayýt Baþarýlý", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Information);
                
                LoadFromDb(); // DB'den tekrar yükle
            }
            catch (Exception ex)
            {
                _log?.Error(nameof(FrmTagManager), "Toplu kaydetme hatasý", ex);
                XtraMessageBox.Show($"Kaydetme sýrasýnda hata:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDeleteTag_ItemClick(object sender, ItemClickEventArgs e)
        {
            var row = gvTags.GetFocusedRow() as TagEntityRow;
            if (row == null) return;

            if (XtraMessageBox.Show($"{row.Name} silinsin mi?", "Onay", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;

            _tagRepo.RemoveByName(row.Name);
            _rows.Remove(row);
        }

        /// <summary>
        /// Seçili tüm tag'leri siler
        /// </summary>
        private void BtnDeleteSelected_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                var selectedRows = gvTags.GetSelectedRows();
                if (selectedRows == null || selectedRows.Length == 0)
                {
                    XtraMessageBox.Show("Lütfen silmek için en az bir tag seçin.", "Uyarý", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Seçili tag'leri listele
                var tagsToDelete = new List<TagEntityRow>();
                foreach (var rowHandle in selectedRows)
                {
                    var row = gvTags.GetRow(rowHandle) as TagEntityRow;
                    if (row != null)
                        tagsToDelete.Add(row);
                }

                if (tagsToDelete.Count == 0) return;

                // Onay mesajý
                var confirmMsg = tagsToDelete.Count == 1
                    ? $"'{tagsToDelete[0].Name}' tag'i silinsin mi?"
                    : $"{tagsToDelete.Count} adet tag silinsin mi?\n\n" +
                      $"Ýlk 5 tag:\n{string.Join("\n", tagsToDelete.Take(5).Select(t => $"- {t.Name}"))}";

                if (tagsToDelete.Count > 5)
                    confirmMsg += $"\n... ve {tagsToDelete.Count - 5} tane daha";

                var result = XtraMessageBox.Show(confirmMsg, "Toplu Silme Onayý", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result != DialogResult.Yes)
                    return;

                // Database'den sil
                int deletedCount = 0;
                var namesToDelete = tagsToDelete.Select(t => t.Name).ToList();
                _tagRepo.RemoveByNames(namesToDelete);
                
                foreach (var tag in tagsToDelete)
                {
                    _rows.Remove(tag);
                    deletedCount++;
                }

                gvTags.RefreshData();
                SaveSnapshot();

                XtraMessageBox.Show($"{deletedCount} adet tag baþarýyla silindi.", "Baþarýlý", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                _log?.Info(nameof(FrmTagManager), $"{deletedCount} adet tag toplu silindi");
            }
            catch (Exception ex)
            {
                _log?.Error(nameof(FrmTagManager), "Toplu silme hatasý", ex);
                XtraMessageBox.Show($"Toplu silme sýrasýnda hata:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Tüm tag'leri siler (Dikkatli kullanýlmalý!)
        /// </summary>
        private void BtnDeleteAll_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (_rows.Count == 0)
                {
                    XtraMessageBox.Show("Silinecek tag bulunamadý.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var confirmMsg = $"?? DÝKKAT! TÜM TAG'LER SÝLÝNECEK!\n\n" +
                                $"Toplam {_rows.Count} adet tag kalýcý olarak VERÝTABANINDAN silinecek.\n\n" +
                                $"Bu iþlem GERÝ ALINAMAZ!\n\n" +
                                $"Devam etmek istediðinize emin misiniz?";

                var result = XtraMessageBox.Show(confirmMsg, "TEHLÝKELÝ ÝÞLEM - Tümünü Sil", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result != DialogResult.Yes)
                    return;

                // Ýkinci onay (daha agresif)
                var doubleCheck = XtraMessageBox.Show(
                    $"?? SON ONAY: {_rows.Count} adet tag VERÝTABANINDAN SÝLINSÝN MÝ?\n\n" +
                    $"Bu iþlem geri alýnamaz!\n\n" +
                    $"Emin misiniz?", 
                    "Son Onay - VERÝTABANINDAN SÝL", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Stop);
                
                if (doubleCheck != DialogResult.Yes)
                {
                    XtraMessageBox.Show("Ýþlem iptal edildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // ?? FÝX: Tüm tag'leri DB'DEN SÝL (Soft delete deðil, permanent delete!)
                var allTags = _rows.ToList();
                int deletedCount = 0;
                
                // Database'den permanent silme (RemoveByNames soft delete yapýyor, o yüzden teker teker permanent delete)
                foreach (var tag in allTags)
                {
                    try
                    {
                        // Önce DB'den entity'i al
                        var entity = _tagRepo.GetByName(tag.Name);
                        if (entity != null)
                        {
                            // Permanent sil (soft delete deðil!)
                            _tagRepo.RemovePermanently(entity);
                            deletedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _log?.Error(nameof(FrmTagManager), $"Tag silme hatasý: {tag.Name}", ex);
                    }
                }

                // UI'dan temizle
                _rows.Clear();
                gvTags.RefreshData();
                SaveSnapshot();

                XtraMessageBox.Show(
                    $"? {deletedCount} adet tag VERÝTABANINDAN silindi!\n\n" +
                    $"Tag'ler kalýcý olarak kaldýrýldý.", 
                    "Silme Tamamlandý", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Information);
                
                _log?.Warn(nameof(FrmTagManager), $"?? TÜM TAG'LER VERÝTABANINDAN SÝLÝNDÝ! Toplam: {deletedCount}");
            }
            catch (Exception ex)
            {
                _log?.Error(nameof(FrmTagManager), "Tümünü silme hatasý", ex);
                XtraMessageBox.Show($"Silme sýrasýnda hata:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
