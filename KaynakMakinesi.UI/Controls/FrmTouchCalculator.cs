using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace KaynakMakinesi.UI.Controls
{
    // Dokunmatik uyumlu sayýsal giriþ penceresi
    // Basit numerik keypad + gösterge
    public sealed class FrmTouchCalculator : XtraForm
    {
        private readonly TextEdit _display;
        private readonly SimpleButton _btnOk;
        private readonly SimpleButton _btnCancel;
        private readonly TableLayoutPanel _grid;

        public decimal? Value { get; private set; }

        public FrmTouchCalculator(decimal? initial = null, string title = null)
        {
            Text = string.IsNullOrWhiteSpace(title) ? "Sayý Giriþi" : title;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            MaximizeBox = false;
            MinimizeBox = false;
            TopMost = false;
            KeyPreview = true;
            Width = 420;
            Height = 520;

            _display = new TextEdit
            {
                Dock = DockStyle.Fill,
                Properties = { ReadOnly = false }
            };
            _display.Properties.Appearance.Font = new Font("Segoe UI", 20f, FontStyle.Regular);
            _display.Properties.AutoHeight = false;
            _display.Height = 60;

            if (initial.HasValue)
                _display.EditValue = initial.Value.ToString(CultureInfo.CurrentCulture);

            _grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 6,
                Padding = new Padding(10),
            };

            for (int i = 0; i < 4; i++) _grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            _grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 70)); // display
            for (int i = 1; i <= 4; i++) _grid.RowStyles.Add(new RowStyle(SizeType.Percent, 17.5f));
            _grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 70)); // buttons row

            Controls.Add(_grid);

            // row 0: display spans 4
            _grid.Controls.Add(_display, 0, 0);
            _grid.SetColumnSpan(_display, 4);

            // helper to create big buttons
            SimpleButton Btn(string text)
            {
                var b = new SimpleButton
                {
                    Text = text,
                    Dock = DockStyle.Fill,
                };
                b.Appearance.Font = new Font("Segoe UI", 16f, FontStyle.Bold);
                b.Click += KeyButton_Click;
                return b;
            }

            // rows with numbers
            _grid.Controls.Add(Btn("7"), 0, 1);
            _grid.Controls.Add(Btn("8"), 1, 1);
            _grid.Controls.Add(Btn("9"), 2, 1);
            _grid.Controls.Add(Btn("Sil"), 3, 1); // backspace

            _grid.Controls.Add(Btn("4"), 0, 2);
            _grid.Controls.Add(Btn("5"), 1, 2);
            _grid.Controls.Add(Btn("6"), 2, 2);
            _grid.Controls.Add(Btn("C"), 3, 2);  // clear

            _grid.Controls.Add(Btn("1"), 0, 3);
            _grid.Controls.Add(Btn("2"), 1, 3);
            _grid.Controls.Add(Btn("3"), 2, 3);
            _grid.Controls.Add(Btn("±"), 3, 3);

            var decSep = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            _grid.Controls.Add(Btn(decSep), 0, 4);
            _grid.Controls.Add(Btn("0"), 1, 4);
            _grid.Controls.Add(Btn("00"), 2, 4);
            _grid.Controls.Add(Btn("-"), 3, 4); // quick minus

            _btnOk = new SimpleButton { Text = "Tamam", Dock = DockStyle.Fill, DialogResult = DialogResult.OK };
            _btnOk.Appearance.Font = new Font("Segoe UI Semibold", 14f);
            _btnOk.Click += (s, e) =>
            {
                if (TryParse(_display.Text, out var d)) Value = d; else Value = null;
            };
            _grid.Controls.Add(_btnOk, 0, 5);
            _grid.SetColumnSpan(_btnOk, 2);

            _btnCancel = new SimpleButton { Text = "Vazgeç", Dock = DockStyle.Fill, DialogResult = DialogResult.Cancel };
            _btnCancel.Appearance.Font = new Font("Segoe UI", 14f);
            _grid.Controls.Add(_btnCancel, 2, 5);
            _grid.SetColumnSpan(_btnCancel, 2);

            AcceptButton = _btnOk;
            CancelButton = _btnCancel;
        }

        private void KeyButton_Click(object sender, EventArgs e)
        {
            if (!(sender is SimpleButton b)) return;
            var t = b.Text;

            if (t == "C")
            {
                _display.Text = string.Empty; return;
            }
            if (t == "Sil")
            {
                var s = _display.Text ?? string.Empty;
                if (s.Length > 0) _display.Text = s.Substring(0, s.Length - 1);
                return;
            }
            if (t == "±")
            {
                var s = _display.Text ?? string.Empty;
                if (s.StartsWith("-")) _display.Text = s.TrimStart('-');
                else _display.Text = "-" + s;
                return;
            }
            if (t == "-")
            {
                var s = _display.Text ?? string.Empty;
                if (!s.StartsWith("-")) _display.Text = "-" + s;
                return;
            }

            // append digit or separator
            _display.Text = (_display.Text ?? string.Empty) + t;
        }

        private static bool TryParse(string text, out decimal value)
        {
            text = (text ?? string.Empty).Trim();
            if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out value)) return true;
            if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out value)) return true;
            value = 0m; return false;
        }

        public static bool TryPrompt(IWin32Window owner, string title, decimal? initial, out decimal value)
        {
            using (var f = new FrmTouchCalculator(initial, title))
            {
                var dr = f.ShowDialog(owner);
                if (dr == DialogResult.OK && f.Value.HasValue)
                {
                    value = f.Value.Value; return true;
                }
                value = 0m; return false;
            }
        }

        public static decimal? Prompt(IWin32Window owner, string title, decimal? initial = null)
        {
            return TryPrompt(owner, title, initial, out var v) ? (decimal?)v : null;
        }
    }
}
