using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace KaynakMakinesi.UI.Controls
{
    /// <summary>
    /// Motor kalibrasyon formlarý için özel CalcEdit kontrolü
    /// TouchCalculator ile entegre çalýþýr
    /// </summary>
    public class MotorCalcEdit : CalcEdit
    {
        private string _promptTitle = "Deðer Girin";
        private decimal _minValue = decimal.MinValue;
        private decimal _maxValue = decimal.MaxValue;
        private bool _enableValidation = false;

        public MotorCalcEdit()
        {
            InitializeControl();
        }

        /// <summary>
        /// Prompt penceresi baþlýðý
        /// </summary>
        public string PromptTitle
        {
            get => _promptTitle;
            set => _promptTitle = value ?? "Deðer Girin";
        }

        /// <summary>
        /// Minimum deðer (validasyon için)
        /// </summary>
        public decimal MinValue
        {
            get => _minValue;
            set
            {
                _minValue = value;
                UpdateValidationSettings();
            }
        }

        /// <summary>
        /// Maximum deðer (validasyon için)
        /// </summary>
        public decimal MaxValue
        {
            get => _maxValue;
            set
            {
                _maxValue = value;
                UpdateValidationSettings();
            }
        }

        /// <summary>
        /// Validasyon aktif mi?
        /// </summary>
        public bool EnableValidation
        {
            get => _enableValidation;
            set
            {
                _enableValidation = value;
                UpdateValidationSettings();
            }
        }

        /// <summary>
        /// Ondalýk basamak sayýsý
        /// </summary>
        public int DecimalPlaces
        {
            get => Properties.DisplayFormat.FormatString.Contains("N0") ? 0 : 
                   Properties.DisplayFormat.FormatString.Contains("N1") ? 1 :
                   Properties.DisplayFormat.FormatString.Contains("N2") ? 2 : 2;
            set
            {
                Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                Properties.DisplayFormat.FormatString = $"N{value}";
                Properties.EditFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                Properties.EditFormat.FormatString = $"N{value}";
            }
        }

        private void InitializeControl()
        {
            // Temel ayarlar
            Properties.Buttons.Clear();
            Properties.ButtonsStyle = DevExpress.XtraEditors.Controls.BorderStyles.Style3D;
            Properties.Appearance.Font = new Font("Segoe UI", 11f, FontStyle.Regular);
            Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            
            // Touch Calculator butonu ekle
            var btn = new DevExpress.XtraEditors.Controls.EditorButton(
                DevExpress.XtraEditors.Controls.ButtonPredefines.Glyph);
            btn.Image = GetCalculatorIcon();
            btn.ToolTip = "Dokunmatik Klavye";
            Properties.Buttons.Add(btn);

            // Click event
            Properties.ButtonClick += OnButtonClick;

            // Default format
            DecimalPlaces = 2;

            // ReadOnly rengini ayarla
            Properties.ReadOnly = false;
            Properties.Appearance.BackColor = Color.White;
        }

        private void OnButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            if (Properties.ReadOnly)
                return;

            // Mevcut deðeri al
            decimal currentValue = 0m;
            if (EditValue != null && EditValue != DBNull.Value)
            {
                if (decimal.TryParse(EditValue.ToString(), NumberStyles.Any, 
                    CultureInfo.CurrentCulture, out var val))
                {
                    currentValue = val;
                }
            }

            // TouchCalculator göster
            if (FrmTouchCalculator.TryPrompt(FindForm(), _promptTitle, currentValue, out var newValue))
            {
                // Validasyon
                if (_enableValidation)
                {
                    if (newValue < _minValue)
                    {
                        XtraMessageBox.Show(
                            $"Deðer minimum limitin altýnda!\nMinimum: {_minValue}", 
                            "Uyarý", 
                            MessageBoxButtons.OK, 
                            MessageBoxIcon.Warning);
                        return;
                    }

                    if (newValue > _maxValue)
                    {
                        XtraMessageBox.Show(
                            $"Deðer maximum limitin üstünde!\nMaximum: {_maxValue}", 
                            "Uyarý", 
                            MessageBoxButtons.OK, 
                            MessageBoxIcon.Warning);
                        return;
                    }
                }

                EditValue = newValue;
            }
        }

        private void UpdateValidationSettings()
        {
            if (_enableValidation)
            {
                Properties.Appearance.BackColor = Color.FromArgb(255, 255, 200); // Hafif sarý
            }
            else
            {
                Properties.Appearance.BackColor = Properties.ReadOnly ? 
                    Color.FromArgb(240, 240, 240) : Color.White;
            }
        }

        /// <summary>
        /// Deðeri decimal olarak döner
        /// </summary>
        public decimal GetDecimalValue()
        {
            if (EditValue == null || EditValue == DBNull.Value)
                return 0m;

            if (decimal.TryParse(EditValue.ToString(), NumberStyles.Any, 
                CultureInfo.CurrentCulture, out var val))
            {
                return val;
            }

            return 0m;
        }

        /// <summary>
        /// Deðeri float olarak döner
        /// </summary>
        public float GetFloatValue()
        {
            return (float)GetDecimalValue();
        }

        /// <summary>
        /// Deðeri int olarak döner
        /// </summary>
        public int GetIntValue()
        {
            return (int)Math.Round(GetDecimalValue());
        }

        /// <summary>
        /// Deðeri set eder
        /// </summary>
        public void SetValue(decimal value)
        {
            EditValue = value;
        }

        /// <summary>
        /// Deðeri set eder
        /// </summary>
        public void SetValue(float value)
        {
            EditValue = (decimal)value;
        }

        /// <summary>
        /// Deðeri set eder
        /// </summary>
        public void SetValue(int value)
        {
            EditValue = (decimal)value;
        }

        /// <summary>
        /// ReadOnly modunu ayarla ve görünümü güncelle
        /// </summary>
        public new bool ReadOnly
        {
            get => Properties.ReadOnly;
            set
            {
                Properties.ReadOnly = value;
                Properties.Appearance.BackColor = value ? 
                    Color.FromArgb(240, 240, 240) : Color.White;
                Properties.Buttons[0].Enabled = !value;
            }
        }

        private Image GetCalculatorIcon()
        {
            // Basit bir hesap makinesi ikonu oluþtur
            var bmp = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.FillRectangle(Brushes.DodgerBlue, 2, 2, 12, 12);
                g.DrawRectangle(Pens.White, 2, 2, 12, 12);
                g.DrawLine(Pens.White, 5, 7, 11, 7);
                g.DrawLine(Pens.White, 8, 4, 8, 10);
            }
            return bmp;
        }
    }
}
