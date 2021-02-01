using System;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System.Windows.Forms;
using System.Linq;
using System.Resources;
using System.Drawing;

namespace chomo
{
    public partial class MainForm : Form
    {
        public string startUrl = "https://www.baidu.com/";
        private readonly string _mainTitle;
        Image CloseImage = (Image)new ResourceManager("chomo.MainForm", System.Reflection.Assembly.GetExecutingAssembly()).GetObject("close");
        Image RefreshImage = (Image)new ResourceManager("chomo.MainForm", System.Reflection.Assembly.GetExecutingAssembly()).GetObject("refresh");
        private int size;
        public MainForm()
        {
            InitializeComponent();
            NewTab(startUrl);
            _mainTitle = "Chomo";
            size = 18;
        }

        private async void NewTab(string startUrl)
        {
            TabPage page = new TabPage("新标签页") { Padding = new Padding(0, 0, 0, 0) };
            WebView2 browser = new WebView2
            {
                Source = new Uri(startUrl),
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom,
                Dock = DockStyle.Fill
            };
            CoreWebView2Environment ev = await CoreWebView2Environment.CreateAsync(null, AppContext.BaseDirectory);
            await browser.EnsureCoreWebView2Async(ev);
            CoreWebView2 coreWebView2 = browser.CoreWebView2;

            //Event Start

            coreWebView2.SourceChanged += (s, e) =>
            {
                BeginInvoke(new Action(() =>
                {
                    addressTextBox.Text = coreWebView2.Source;
                }));
            };
            coreWebView2.DocumentTitleChanged += (s, e) =>
            {
                BeginInvoke(new Action(() =>
                {
                    var title = coreWebView2.DocumentTitle;
                    if (title != null)
                    {
                        if (tabControl.SelectedTab == page)
                        {
                            Text = coreWebView2.DocumentTitle + " - " + _mainTitle;
                        }
                        page.ToolTipText = title;
                        if (title.Length > size)
                        {
                            title = title.Substring(0, size) + "...";
                        }
                        page.Text = title;
                    }

                }));
            };
            refreshToolStripButton.Image = CloseImage;
            refreshToolStripButton.Click += stopRefreshToolStripButton_Click;
            coreWebView2.NavigationCompleted += (s, e) =>
            {
                refreshToolStripButton.Image = RefreshImage;
                refreshToolStripButton.Click += refreshToolStripButton_Click;
            };
            coreWebView2.NewWindowRequested += OnNewWindowRequested;

            page.Controls.Add(browser);
            tabControl.TabPages.Add(page);
            tabControl.SelectedTab = page;
        }

        private void OnNewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            e.Handled = true;
            NewTab(e.Uri);
        }

        private void GoAddress(string url)
        {
            var ctl = GetAliveBrowser();
            if (ctl != null)
            {
                try
                {
                    ctl.Source = new Uri(url);
                }
                catch (UriFormatException)
                {
                    if (url.PadLeft(8, '0').ToLower() != "https://" || url.PadLeft(7, '0').ToLower() != "http://")
                    {
                        ctl.Source = new Uri("https://" + url);
                    }
                    else
                    {
                        MessageBox.Show(url + "不是一个正确的url。");
                    }

                }
            }
            else
            {
                NewTab(addressTextBox.Text);
            }
        }

        private WebView2 GetAliveBrowser()
        {
            if (tabControl.TabCount > 0)
            {
                TabPage page = tabControl.TabPages[tabControl.SelectedIndex];
                return page.Controls.OfType<WebView2>().FirstOrDefault();
            }
            return null;
        }

        private void goButton_Click(object sender, EventArgs e)
        {
            GoAddress(addressTextBox.Text);
        }

        private void newToolButton_Click(object sender, EventArgs e)
        {
            NewTab(startUrl);
        }

        private void addressTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                GoAddress(addressTextBox.Text);
            }
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            addressTextBox.Width = toolStrip.Width - 30 * 5;
            size = (int)(this.Width / (1.5 * tabControl.Font.Size) / tabControl.TabCount);
            if (tabControl.TabCount > 0)
            {
                var page = tabControl.TabPages[tabControl.SelectedIndex];
                foreach (var crl in page.Controls)
                {
                    WebView2 browser = crl as WebView2;
                    CoreWebView2 coreWebView2 = browser.CoreWebView2;
                    var title = coreWebView2.DocumentTitle;
                    if (title != null)
                    {
                        page.ToolTipText = title;
                        if (title.Length > size)
                        {
                            title = title.Substring(0, size) + "...";
                        }
                        page.Text = title;
                    }
                }
            }
        }

        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl.TabCount > 0)
            {
                var page = tabControl.TabPages[tabControl.SelectedIndex];
                foreach (var crl in page.Controls)
                {
                    WebView2 browser = crl as WebView2;
                    Text = browser.CoreWebView2.DocumentTitle + " - " + _mainTitle;
                    addressTextBox.Text = browser.CoreWebView2.Source;
                }
            }
            else
            {
                Text = _mainTitle;
            }
        }

        private void backToolStripButton_Click(object sender, EventArgs e)
        {
            WebView2 browser = GetAliveBrowser();
            if (browser.CoreWebView2 != null)
            {
                browser.CoreWebView2.GoBack();
            }
        }

        private void forwardToolStripButton_Click(object sender, EventArgs e)
        {
            WebView2 browser = GetAliveBrowser();
            if (browser.CoreWebView2 != null)
            {
                browser.CoreWebView2.GoForward();
            }
        }

        private void refreshToolStripButton_Click(object sender, EventArgs e)
        {
            refreshToolStripButton.Image = CloseImage;
            refreshToolStripButton.Click += stopRefreshToolStripButton_Click;
            WebView2 browser = GetAliveBrowser();
            if (browser.CoreWebView2 != null)
            {
                browser.CoreWebView2.Reload();
                browser.CoreWebView2.NavigationCompleted += (s, d) =>
                {
                    BeginInvoke(new Action(() =>
                    {
                        refreshToolStripButton.Image = RefreshImage;
                        refreshToolStripButton.Click += refreshToolStripButton_Click;
                    }));
                };
            }
        }
        private void stopRefreshToolStripButton_Click(object sender, EventArgs e)
        {
            WebView2 browser = GetAliveBrowser();
            if (browser.CoreWebView2 != null)
            {
                browser.CoreWebView2.Stop();
            }
            refreshToolStripButton.Image = RefreshImage;
            refreshToolStripButton.Click += refreshToolStripButton_Click;
        }

        private void closeTabContextMenuItem_Click(object sender, EventArgs e)
        {
                ToolStripMenuItem s = (ToolStripMenuItem)sender;
                TabPage page = s.Tag as TabPage;
                if (page != null)
                {
                    page.Dispose();
                }
        }

        private void tabControl_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                for (int i = 0; i < tabControl.TabCount; i++)
                {
                    Rectangle r = tabControl.GetTabRect(i);
                    if (r.Contains(e.Location))
                    {
                        closeTabContextMenuItem.Tag = tabControl.TabPages[i];
                        reloadStripMenuItem.Tag = tabControl.TabPages[i];
                        tabContextMenu.LostFocus += (s, ev) => { tabContextMenu.Hide(); };
                        tabContextMenu.ChangeUICues += (s, ev) => { tabContextMenu.Hide(); };
                        tabContextMenu.Show(tabControl, e.Location);
                    }
                }
            }
            else if (e.Button == MouseButtons.Middle)
            {
                for (int i = 0; i < tabControl.TabCount; i++)
                {
                    Rectangle r = tabControl.GetTabRect(i);
                    if (r.Contains(e.Location))
                    {
                        TabPage page = tabControl.TabPages[i];
                        if (page != null)
                        {
                            page.Dispose();
                        }
                    }
                }
            }
        }

        private void mainToolStripButton_Click(object sender, EventArgs e)
        {
            WebView2 browser = GetAliveBrowser();
            if (browser.CoreWebView2 != null)
            {
                browser.Source = new Uri(startUrl);
            }
        }
    }
}
    
