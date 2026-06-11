using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.Win32;

namespace OopsCaps
{
    static class Strings
    {
        public static string TraySettings(string lang) { return lang == "lv" ? "Iestatījumi..." : (lang == "ru" ? "Настройки..." : "Settings..."); }
        public static string TrayExit(string lang) { return lang == "lv" ? "Iziet (OopsCaps)" : (lang == "ru" ? "Выход (OopsCaps)" : "Exit (OopsCaps)"); }
        public static string SettingsTitle(string lang) { return lang == "lv" ? "Iestatījumi" : (lang == "ru" ? "Настройки" : "Settings"); }
        public static string GrpHotkeys(string lang) { return lang == "lv" ? "Karstie taustiņi (kopīgie modifikatori)" : (lang == "ru" ? "Горячие клавиши (общие модификаторы)" : "Hotkeys (shared modifiers)"); }
        public static string GrpOptions(string lang) { return lang == "lv" ? "Iespējas" : (lang == "ru" ? "Опции" : "Options"); }
        public static string LblInv(string lang) { return lang == "lv" ? "Invertēt:" : (lang == "ru" ? "Инверт:" : "Invert:"); }
        public static string LblUpr(string lang) { return lang == "lv" ? "LIELIE:" : (lang == "ru" ? "КРУПНЫЕ:" : "UPPER:"); }
        public static string LblLwr(string lang) { return lang == "lv" ? "mazie:" : (lang == "ru" ? "мелкие:" : "lower:"); }
        public static string LblTtl(string lang) { return lang == "lv" ? "Titulburti:" : (lang == "ru" ? "Заглавные:" : "Title Case:"); }
        public static string ChkAuto(string lang) { return lang == "lv" ? "Startēt ar Windows" : (lang == "ru" ? "Запуск с Windows" : "Start with Windows"); }
        public static string ChkSnd(string lang) { return lang == "lv" ? "Skaņas signāls" : (lang == "ru" ? "Звуковой сигнал" : "Sound effect"); }
        public static string ChkToggleCaps(string lang) { return lang == "lv" ? "Pārslēgt Caps Lock\n(pēc Invert)" : (lang == "ru" ? "Переключить Caps Lock\n(после Invert)" : "Toggle Caps Lock\n(after Invert)"); }
        public static string LblLang(string lang) { return lang == "lv" ? "Izvēlies valodu:" : (lang == "ru" ? "Выберите язык:" : "Language:"); }
        public static string LblSupport(string lang) { return lang == "lv" ? "Atbalsti projektu:" : (lang == "ru" ? "Поддержать проект:" : "Support the project:"); }
        public static string BtnSave(string lang) { return lang == "lv" ? "Saglabāt" : (lang == "ru" ? "Сохранить" : "Save"); }
        public static string VerAuth(string lang) { return lang == "lv" ? "Versija 1.1\n© 2026 did.this.lv" : (lang == "ru" ? "Версия 1.1\n© 2026 did.this.lv" : "Version 1.1\n© 2026 did.this.lv"); }
    }

    static class Program
    {
        [DllImport("user32.dll")] private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll")] private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        [DllImport("user32.dll")] private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
        private const uint KEYEVENTF_KEYUP = 0x0002;
        private const int HK_INV = 9001, HK_UPR = 9002, HK_LWR = 9003, HK_TTL = 9004;

        internal static Bitmap GenerateOoImage(int size)
        {
            Bitmap bmp = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent); g.SmoothingMode = SmoothingMode.AntiAlias; g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                Color purpleColor = Color.FromArgb(180, 50, 255); int penWidth = Math.Max(2, size / 14);
                
                // Pievienota 2 pikseļu drošības atkāpe, lai Anti-Aliasing un malas nenogrieztos
                float pad = 2.0f;
                float offset = (penWidth / 2.0f) + pad;
                float drawSize = size - penWidth - (pad * 2);

                using (Brush backBrush = new SolidBrush(Color.Black)) { g.FillEllipse(backBrush, offset, offset, drawSize, drawSize); }
                using (Pen borderPen = new Pen(purpleColor, penWidth)) { g.DrawEllipse(borderPen, offset, offset, drawSize, drawSize); }
                using (Font font = new Font("Arial", size * 0.42f, FontStyle.Bold)) using (Brush textBrush = new SolidBrush(purpleColor)) { StringFormat sf = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center }; g.DrawString("Oo", font, textBrush, new RectangleF(0, size * 0.03f, size, size), sf); }
            }
            return bmp;
        }

        class BackgroundAppForm : Form
        {
            private NotifyIcon trayIcon; private ContextMenuStrip trayMenu;
            public uint Mods = 0x0002 | 0x0004; // Ctrl + Shift
            public uint VkInv = 0x49, VkUpr = 0x55, VkLwr = 0x4C, VkTtl = 0x54; // I, U, L, T
            public bool PlaySound = true; public string Lang = "en";
            public bool ToggleCaps = false; 

            protected override void SetVisibleCore(bool value) { if (!this.IsHandleCreated) CreateHandle(); base.SetVisibleCore(false); }

            public BackgroundAppForm()
            {
                LoadSettings();
                trayMenu = new ContextMenuStrip();
                trayMenu.Items.Add(Strings.TraySettings("en"), null, OnSettings); trayMenu.Items.Add("-"); trayMenu.Items.Add(Strings.TrayExit("en"), null, OnExit);
                trayIcon = new NotifyIcon { Text = "OopsCaps", ContextMenuStrip = trayMenu };
                trayIcon.MouseUp += (s, e) => { if (e.Button == MouseButtons.Left) { MethodInfo mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.NonPublic | BindingFlags.Instance); if (mi != null) mi.Invoke(trayIcon, null); } };
                
                try { using (Bitmap bmp = GenerateOoImage(128)) trayIcon.Icon = Icon.FromHandle(bmp.GetHicon()); } catch {}
                
                trayIcon.Visible = true;
                RegKeys();
            }

            private void RegKeys() { UnregisterHotKey(this.Handle, HK_INV); UnregisterHotKey(this.Handle, HK_UPR); UnregisterHotKey(this.Handle, HK_LWR); UnregisterHotKey(this.Handle, HK_TTL); RegisterHotKey(this.Handle, HK_INV, Mods, VkInv); RegisterHotKey(this.Handle, HK_UPR, Mods, VkUpr); RegisterHotKey(this.Handle, HK_LWR, Mods, VkLwr); RegisterHotKey(this.Handle, HK_TTL, Mods, VkTtl); }

            private void LoadSettings() 
            { 
                string cfg = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OopsCaps.cfg"); 
                if (File.Exists(cfg)) 
                { 
                    try 
                    { 
                        var lines = File.ReadAllLines(cfg); 
                        if (lines.Length >= 7) 
                        { 
                            uint.TryParse(lines[0], out Mods); uint.TryParse(lines[1], out VkInv); uint.TryParse(lines[2], out VkUpr); 
                            uint.TryParse(lines[3], out VkLwr); uint.TryParse(lines[4], out VkTtl); Lang = lines[5].Trim().ToLower(); 
                            bool.TryParse(lines[6], out PlaySound); 
                        } 
                        if (lines.Length >= 8) 
                        {
                            bool.TryParse(lines[7], out ToggleCaps);
                        }
                    } catch {} 
                } 
            }
            
            public void SaveSettings() 
            { 
                try 
                { 
                    File.WriteAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OopsCaps.cfg"), new string[] { Mods.ToString(), VkInv.ToString(), VkUpr.ToString(), VkLwr.ToString(), VkTtl.ToString(), Lang, PlaySound.ToString(), ToggleCaps.ToString() }); 
                    RegKeys(); 
                } catch {} 
            }

            protected override void WndProc(ref Message m) { if (m.Msg == 0x0312) { int id = m.WParam.ToInt32(); if (id == HK_INV) TransformText(0, VkInv); else if (id == HK_UPR) TransformText(1, VkUpr); else if (id == HK_LWR) TransformText(2, VkLwr); else if (id == HK_TTL) TransformText(3, VkTtl); } base.WndProc(ref m); }

            private void TransformText(int mode, uint vk)
            {
                keybd_event(0x10, 0, KEYEVENTF_KEYUP, UIntPtr.Zero); keybd_event(0x11, 0, KEYEVENTF_KEYUP, UIntPtr.Zero); keybd_event(0x12, 0, KEYEVENTF_KEYUP, UIntPtr.Zero); keybd_event((byte)vk, 0, KEYEVENTF_KEYUP, UIntPtr.Zero); System.Threading.Thread.Sleep(50);
                string oldText = Clipboard.ContainsText() ? Clipboard.GetText() : null; Clipboard.Clear();
                keybd_event(0x11, 0, 0, UIntPtr.Zero); keybd_event(0x43, 0, 0, UIntPtr.Zero); System.Threading.Thread.Sleep(20); keybd_event(0x43, 0, KEYEVENTF_KEYUP, UIntPtr.Zero); keybd_event(0x11, 0, KEYEVENTF_KEYUP, UIntPtr.Zero); System.Threading.Thread.Sleep(150);
                
                if (Clipboard.ContainsText())
                {
                    string text = Clipboard.GetText(); string fixedText = text;
                    if (mode == 0) { char[] chars = text.ToCharArray(); for (int i = 0; i < chars.Length; i++) { if (char.IsUpper(chars[i])) chars[i] = char.ToLower(chars[i]); else if (char.IsLower(chars[i])) chars[i] = char.ToUpper(chars[i]); } fixedText = new string(chars); }
                    else if (mode == 1) fixedText = text.ToUpper(); else if (mode == 2) fixedText = text.ToLower(); else if (mode == 3) fixedText = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(text.ToLower());
                    
                    Clipboard.SetText(fixedText); System.Threading.Thread.Sleep(50);
                    keybd_event(0x11, 0, 0, UIntPtr.Zero); keybd_event(0x56, 0, 0, UIntPtr.Zero); System.Threading.Thread.Sleep(20); keybd_event(0x56, 0, KEYEVENTF_KEYUP, UIntPtr.Zero); keybd_event(0x11, 0, KEYEVENTF_KEYUP, UIntPtr.Zero); System.Threading.Thread.Sleep(150);
                    
                    if (mode == 0 && ToggleCaps)
                    {
                        keybd_event(0x14, 0x3A, 0, UIntPtr.Zero); 
                        keybd_event(0x14, 0x3A, KEYEVENTF_KEYUP, UIntPtr.Zero); 
                    }

                    if (PlaySound) 
                    {
                        try 
                        {
                            string wavPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Media", "Speech On.wav");
                            if (File.Exists(wavPath)) { using (System.Media.SoundPlayer sp = new System.Media.SoundPlayer(wavPath)) sp.Play(); }
                            else System.Console.Beep(2200, 40);
                        } 
                        catch {}
                    }
                }
                if (oldText != null) Clipboard.SetText(oldText); else Clipboard.Clear();
            }

            private void OnSettings(object sender, EventArgs e) { using (var frm = new SettingsForm(this)) { if (frm.ShowDialog() == DialogResult.OK) { SaveSettings(); } } }
            private void OnExit(object sender, EventArgs e) { Application.Exit(); }
            protected override void OnFormClosing(FormClosingEventArgs e) { trayIcon.Dispose(); base.OnFormClosing(e); }
        }

        class SettingsForm : Form
        {
            BackgroundAppForm app; GroupBox grpHotkeys, grpOptions; Button btnSave, btnCoffee; LinkLabel lnkGit; PictureBox picLogo;
            CheckBox chkCtrl, chkShift, chkAlt, chkToggleCaps, chkAuto, chkSound; ComboBox cmbInv, cmbUpr, cmbLwr, cmbTtl, cmbLang;
            Label lblInv, lblUpr, lblLwr, lblTtl, lblLang, lblSupport, lblVerAuth;

            public SettingsForm(BackgroundAppForm mainApp)
            {
                app = mainApp; 
                this.Size = new Size(420, 530); 
                this.FormBorderStyle = FormBorderStyle.FixedDialog; 
                this.StartPosition = FormStartPosition.CenterScreen;

                grpHotkeys = new GroupBox() { Location = new Point(15, 15), Size = new Size(375, 170) }; 
                chkCtrl = new CheckBox() { Text = "Ctrl", Location = new Point(15, 30), Checked = (app.Mods & 0x0002) != 0, Width = 60 };
                chkShift = new CheckBox() { Text = "Shift", Location = new Point(15, 60), Checked = (app.Mods & 0x0004) != 0, Width = 60 };
                chkAlt = new CheckBox() { Text = "Alt", Location = new Point(15, 90), Checked = (app.Mods & 0x0001) != 0, Width = 60 };
                
                chkToggleCaps = new CheckBox() { Location = new Point(15, 120), AutoSize = true, Checked = app.ToggleCaps };
                
                lblInv = new Label() { Location = new Point(170, 32), AutoSize = true }; cmbInv = CreateCmb(app.VkInv, 270, 30);
                lblUpr = new Label() { Location = new Point(170, 62), AutoSize = true }; cmbUpr = CreateCmb(app.VkUpr, 270, 60);
                lblLwr = new Label() { Location = new Point(170, 92), AutoSize = true }; cmbLwr = CreateCmb(app.VkLwr, 270, 90);
                lblTtl = new Label() { Location = new Point(170, 122), AutoSize = true }; cmbTtl = CreateCmb(app.VkTtl, 270, 120);
                grpHotkeys.Controls.AddRange(new Control[] { chkCtrl, chkShift, chkAlt, chkToggleCaps, lblInv, cmbInv, lblUpr, cmbUpr, lblLwr, cmbLwr, lblTtl, cmbTtl }); this.Controls.Add(grpHotkeys);

                grpOptions = new GroupBox() { Location = new Point(15, 195), Size = new Size(375, 95) }; 
                lblLang = new Label() { Location = new Point(15, 25), AutoSize = true };
                cmbLang = new ComboBox() { Location = new Point(15, 45), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
                cmbLang.Items.AddRange(new object[] { "English", "Latviešu", "Русский" });
                cmbLang.SelectedIndex = app.Lang == "ru" ? 2 : (app.Lang == "lv" ? 1 : 0);
                cmbLang.SelectedIndexChanged += (s, e) => { app.Lang = cmbLang.SelectedIndex == 2 ? "ru" : (cmbLang.SelectedIndex == 1 ? "lv" : "en"); UpdateTexts(); };
                
                RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", false);
                bool isAuto = false;
                if (key != null) { isAuto = key.GetValue("OopsCaps") != null; key.Close(); }
                
                chkAuto = new CheckBox() { Location = new Point(200, 25), AutoSize = true, Checked = isAuto };
                chkSound = new CheckBox() { Location = new Point(200, 55), AutoSize = true, Checked = app.PlaySound };
                grpOptions.Controls.AddRange(new Control[] { lblLang, cmbLang, chkAuto, chkSound }); this.Controls.Add(grpOptions);

                picLogo = new PictureBox() { Size = new Size(80, 80), Location = new Point(15, 305), Image = GenerateOoImage(80), SizeMode = PictureBoxSizeMode.Zoom };
                lblVerAuth = new Label() { Location = new Point(105, 320), AutoSize = true, Font = new Font(this.Font, FontStyle.Italic) };
                lnkGit = new LinkLabel() { Text = "github.com/didthislv", Location = new Point(105, 355), AutoSize = true };
                lnkGit.LinkClicked += (s, ev) => Process.Start("https://github.com/didthislv");

                lblSupport = new Label() { Location = new Point(250, 313), AutoSize = true, Font = new Font(this.Font, FontStyle.Bold) };
                btnCoffee = new Button() { Text = "☕ Buy me a coffee", Location = new Point(250, 338), Size = new Size(140, 32), BackColor = Color.FromArgb(255, 221, 0), ForeColor = Color.Black, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };
                btnCoffee.Click += (s, ev) => Process.Start("https://www.buymeacoffee.com/didthislv");

                btnSave = new Button() { Location = new Point(165, 440), Width = 90, Height = 35 };
                btnSave.Click += BtnSave_Click;

                this.Controls.AddRange(new Control[] { picLogo, lblVerAuth, lnkGit, lblSupport, btnCoffee, btnSave });
                UpdateTexts();
            }

            private ComboBox CreateCmb(uint vk, int x, int y) { ComboBox cmb = new ComboBox() { Location = new Point(x, y), Width = 60, DropDownStyle = ComboBoxStyle.DropDownList }; for (char c = 'A'; c <= 'Z'; c++) cmb.Items.Add(c.ToString()); string ck = ((char)vk).ToString(); if (cmb.Items.Contains(ck)) cmb.SelectedItem = ck; else cmb.SelectedIndex = 0; return cmb; }

            private void UpdateTexts() 
            { 
                this.Text = Strings.SettingsTitle(app.Lang); 
                grpHotkeys.Text = Strings.GrpHotkeys(app.Lang); 
                grpOptions.Text = Strings.GrpOptions(app.Lang); 
                lblInv.Text = Strings.LblInv(app.Lang); 
                lblUpr.Text = Strings.LblUpr(app.Lang); 
                lblLwr.Text = Strings.LblLwr(app.Lang); 
                lblTtl.Text = Strings.LblTtl(app.Lang); 
                chkToggleCaps.Text = Strings.ChkToggleCaps(app.Lang); 
                chkAuto.Text = Strings.ChkAuto(app.Lang); 
                chkSound.Text = Strings.ChkSnd(app.Lang); 
                lblLang.Text = Strings.LblLang(app.Lang); 
                lblSupport.Text = Strings.LblSupport(app.Lang); 
                lblVerAuth.Text = Strings.VerAuth(app.Lang); 
                btnSave.Text = Strings.BtnSave(app.Lang); 
            }

            private void BtnSave_Click(object sender, EventArgs e)
            {
                app.Mods = 0; if (chkAlt.Checked) app.Mods |= 0x0001; if (chkCtrl.Checked) app.Mods |= 0x0002; if (chkShift.Checked) app.Mods |= 0x0004;
                if (cmbInv.SelectedItem != null) app.VkInv = (uint)cmbInv.SelectedItem.ToString()[0];
                if (cmbUpr.SelectedItem != null) app.VkUpr = (uint)cmbUpr.SelectedItem.ToString()[0];
                if (cmbLwr.SelectedItem != null) app.VkLwr = (uint)cmbLwr.SelectedItem.ToString()[0];
                if (cmbTtl.SelectedItem != null) app.VkTtl = (uint)cmbTtl.SelectedItem.ToString()[0];
                app.PlaySound = chkSound.Checked;
                app.ToggleCaps = chkToggleCaps.Checked;
                
                try { RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true); if (chkAuto.Checked) { key.SetValue("OopsCaps", Application.ExecutablePath); } else { key.DeleteValue("OopsCaps", false); } key.Close(); } catch {}
                this.DialogResult = DialogResult.OK; this.Close();
            }
        }

        [STAThread] static void Main() { Application.EnableVisualStyles(); Application.SetCompatibleTextRenderingDefault(false); Application.Run(new BackgroundAppForm()); }
    }
}