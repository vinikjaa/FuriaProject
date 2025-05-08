using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;

namespace InterfaceFuria
{
    public partial class Form1 : Form
    {
        //import para movimentar a janela
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;

        //dwm - sombra externa da janela
        [DllImport("dwmapi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [StructLayout(LayoutKind.Sequential)]
        public struct MARGINS
        {
            public int leftWidth;
            public int rightWidth;
            public int topHeight;
            public int bottomHeight;
        }

        ChromiumWebBrowser browser;

        public Form1()
        {
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(40, 40, 40);
            this.Size = new Size(500, 670);

            this.MouseDown += Form_MouseDown; //chamando a função de movimentar a janela com o mouse

            Panel titleBar = new Panel //barra de titulo
            {
                Size = new Size(this.Width, 30),
                Location = new Point(0, 0),
                BackColor = Color.FromArgb(30, 30, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            titleBar.MouseDown += Form_MouseDown;
            this.Controls.Add(titleBar);

            Panel endBar = new Panel //barra decoratva para o label do github
            {
                Size = new Size(this.Width, 30),
                Location = new Point(0, 640),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            endBar.MouseDown += Form_MouseDown;
            this.Controls.Add(endBar);

            Button btnClose = new Button //botão de fechar
            {
                Text = "X",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(30, 30),
                Location = new Point(titleBar.Width - 30, 0),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btnClose.FlatAppearance.MouseDownBackColor = Color.Transparent;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();
            titleBar.Controls.Add(btnClose);

            Button btnMinimize = new Button //botão de minimizar
            {
                Text = "—",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(30, 30),
                Location = new Point(titleBar.Width - 60, 0),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Cursor = Cursors.Hand
            };
            btnMinimize.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btnMinimize.FlatAppearance.MouseDownBackColor = Color.Transparent;
            btnMinimize.FlatAppearance.BorderSize = 0;
            btnMinimize.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
            titleBar.Controls.Add(btnMinimize);

            Label WinTitle = new Label //titulo da janela
            {
                Text = "Experiência conversacional",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(10, 0),
                Size = new Size(200, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            titleBar.Controls.Add(WinTitle);
            WinTitle.MouseDown += Form_MouseDown;

            Label Version = new Label //Versão do projeto (apenas estético)
            {
                Text = "v1.0",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.Gray,
                BackColor = Color.Transparent,
                AutoSize = false,
                TextAlign = ContentAlignment.TopCenter,
                Location = new Point(460, 4),
                Size = new Size(40, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            endBar.Controls.Add(Version);
            Version.MouseDown += Form_MouseDown;

            Label Github = new Label //Github direcionado
            {
                Text = "Github.com/vinikjaa",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = false,
                TextAlign = ContentAlignment.TopCenter,
                Location = new Point(175, 4),
                Size = new Size(151, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            endBar.Controls.Add(Github);
            Github.Click += (s, e) =>
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://github.com/vinikjaa",
                    UseShellExecute = true
                });
            };

            this.Resize += (s, e) => //atualiza a localização dos botões se a janela for arrastada
            {
                titleBar.Width = this.Width;
                //btnClose.Location = new Point(this.Width - 25, 0); //sem titlebar
                //btnMinimize.Location = new Point(this.Width - 50, 0); //sem titlebar
            };

            CefSettings settings = new CefSettings();
            Cef.Initialize(settings);
            string localHtmlPath = Path.Combine(Application.StartupPath, "Resources", "Desafio1.html");
            string localHtmlUrl = "file:///" + localHtmlPath.Replace("\\", "/");
            browser = new ChromiumWebBrowser(localHtmlUrl);
            browser.Dock = DockStyle.Fill;
            this.Controls.Add(browser);

            this.Load += Form1_Load; //chamando a função de sombra da janela
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            int val = 2;
            DwmSetWindowAttribute(this.Handle, 2, ref val, 4);

            var margins = new MARGINS()
            {
                bottomHeight = 1,
                leftWidth = 1,
                rightWidth = 1,
                topHeight = 1
            };
            DwmExtendFrameIntoClientArea(this.Handle, ref margins);
        }

        private void Form_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0); //mover a janela arrastando com o mouse
            }
        }
    }
}