using System;
using System.Linq;
using System.Windows.Forms;

namespace KaynakMakinesi.UI.Utils
{
    public static class UiHelpers
    {
        // MDI singleton açýcý: ayný tipten bir child varsa öne getirir; yoksa yaratýr.
        public static TForm ShowMdiSingleton<TForm>(Form mdiParent, Func<TForm> factory, string text = null, bool maximize = true)
            where TForm : Form
        {
            if (mdiParent == null) throw new ArgumentNullException(nameof(mdiParent));
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            var existing = mdiParent.MdiChildren.OfType<TForm>().FirstOrDefault();
            if (existing != null)
            {
                if (!string.IsNullOrWhiteSpace(text)) existing.Text = text;
                try
                {
                    existing.BringToFront();
                    if (maximize) existing.WindowState = FormWindowState.Maximized;
                    existing.Activate();
                }
                catch { }
                return existing;
            }

            var form = factory();
            if (!string.IsNullOrWhiteSpace(text)) form.Text = text;
            form.MdiParent = mdiParent;
            form.Show();
            if (maximize) form.WindowState = FormWindowState.Maximized;
            return form;
        }

        // Dokunmatik sayýsal giriþ kolaylaþtýrýcý
        public static bool TryTouchNumeric(IWin32Window owner, string title, decimal? initial, out decimal value)
        {
            return Controls.FrmTouchCalculator.TryPrompt(owner, title, initial, out value);
        }
    }
}
