using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cybele;
using Cybele.Thinfinity;
using System.Drawing.Printing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace MyOwnApp
{
    public partial class frmMyOwnApp : Form
    {
        private Cybele.Thinfinity.VirtualUI vui;
        private Cybele.Thinfinity.JSObject ro;
        private Font printFont;
        private frmPopup fPopup;
        private delegate void TIMEOUT();
        private StreamReader streamToPrint;
        static string filePath;
        private Boolean initVUI = true;
        private System.Drawing.Color NormalGroupBck;
        private System.Drawing.Color AcceptDropBck = System.Drawing.Color.Lime;
        private System.Drawing.Color DeniedDropBck = System.Drawing.Color.Red;
        private Boolean isUploadOpened = false;
        //private ClipboardAssist.ClipboardMonitor ClipMonitor = new ClipboardAssist.ClipboardMonitor();
        private Int16 suspendTime = 5;
        private Int16 cSuspendTime = 5;
        private Int32 nfsIdx = 1;
        private CancellationTokenSource tokenCountdown = new CancellationTokenSource();
        CancellationToken tokenCancel;
        public frmMyOwnApp()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize() {
            try {
                if (initVUI)
                {
                    groupBox3.AllowDrop = true;
                    txtSuspend.Text = suspendTime.ToString();
                    vui = new Cybele.Thinfinity.VirtualUI();
                    vui.ClientSettings.CursorVisible = true;
                    vui.ClientSettings.MouseMoveGestureStyle = MouseMoveGestureStyle.MM_STYLE_RELATIVE;
                    vui.OnClose += vui_OnClose;
                    vui.Start();
                    vui.StdDialogs = false;
                    fPopup = new frmPopup();
                    NormalGroupBck = groupBox3.BackColor;
                    // -- The given name, is how the model shown this object in the model reference.
                    ro = new Cybele.Thinfinity.JSObject("ro");
                    ro.Events.Add("button_click").AddArgument("data", IJSDataType.JSDT_JSON);
                    ro.Events.Add("JsROCopy");//.AddArgument("data", IJSDataType.JSDT_JSON);
                    ro.Events.Add("JsROPaste");//.AddArgument("data", IJSDataType.JSDT_JSON);
                    if (vui.Active)
                    {
                        // -- Adding properties, methods and events.

                        ro.Properties.Add("int").AsInt = 3;
                        ro.Properties.Add("float").AsFloat = 2.5f;
                        ro.Properties.Add("boolean").AsBool = true;
                        ro.Properties.Add("string").AsString = "Hello world";
                        //ro.Properties.Add("JSON").AsJSON = "{'x':42}";

                        // Cybele.Thinfinity.JSObject sub_ro = new Cybele.Thinfinity.JSObject("sub_ro");
                        Cybele.Thinfinity.IJSObject data = ro.Objects.Add("data");
                        data.Properties.Add("height").AsInt = 1;
                        data.Properties.Add("scale").AsFloat = 44.1235f;
                        data.Properties.Add("isReady").AsBool = false;
                        data.Properties.Add("phrase").AsString = "bye world";
                        data.Properties.Add("band").AsString = "Marillion";
                        //data.Properties.R

                        //sub_ro.Properties.Add("JSON").AsJSON = "{'x':455}";

                        ro.Methods.Add("GetValue")
                            .OnCall(new Cybele.Thinfinity.JSCallback(delegate(IJSObject parent, IJSMethod Method)
                        {

                        })).ReturnValue.AsString = "ss";


                        ro.Properties.Add("backgroundColor").OnSet(new JSBinding(delegate(IJSObject parent, IJSProperty prop)
                        {
                            string value = prop.AsString;
                        })).AsString = "";
                        ro.Properties.Add("copy")
                            //.OnGet(new JSBinding(
                            //                     // This anonymous procedure do the actual get
                            //   delegate(IJSObject Parent, IJSProperty Prop)
                            //   {
                            //       Prop.AsString = "#"
                            //           + this.BackColor.R.ToString("X2")
                            //           + this.BackColor.G.ToString("X2")
                            //           + this.BackColor.B.ToString("X2");
                            //   }))
                           .OnSet(new JSBinding(
                            // This anonymous procedure do the actual set
                              delegate(IJSObject Parent, IJSProperty Prop)
                              {
                                  string value = Prop.AsString;
                                  lblJsRO.Text = "Callback copy: " + value;
                              })).AsString = "";

                        ro.Properties.Add("paste")
                            //.OnGet(new JSBinding(
                            //                     // This anonymous procedure do the actual get
                            //   delegate(IJSObject Parent, IJSProperty Prop)
                            //   {
                            //       Prop.AsString = "#"
                            //           + this.BackColor.R.ToString("X2")
                            //           + this.BackColor.G.ToString("X2")
                            //           + this.BackColor.B.ToString("X2");
                            //   }))
                           .OnSet(new JSBinding(
                            // This anonymous procedure do the actual set
                              delegate(IJSObject Parent, IJSProperty Prop)
                              {
                                  string value = Prop.AsString;
                                  lblJsRO.Text = "Callback paste: " + value;
                              })).AsString = "";

                        //ro.Events.Add("dragEnter");//.AddArgument("type", IJSDataType.JSDT_STRING);
                        //ro.Properties.Add("dragRaw").OnSet(new JSBinding(delegate(IJSObject parent, IJSProperty prop)
                        //{
                        //    string s = prop.AsString;
                        //}));

                        vui.OnDragFile += vui_OnDragFile;
                        ro.ApplyModel();
                    }
                    else
                    {
                        Console.WriteLine("VirtualUI is not ready");
                    }
                }
                //this.ActiveControl = textBox1;

                //BackgroundWorker t = new BackgroundWorker();
                //t.DoWork += runWork;
                //t.RunWorkerCompleted += taskCompleted;
                //t.WorkerReportsProgress = true;
                //System.Windows.Forms.Cursor.Current = Cursors.WaitCursor;
                //Cursor.Hide();
                //t.RunWorkerAsync();
                vui.OnUploadEnd += vui_OnUploadEnd;
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }


            //if (Clipboard.ContainsImage())
            //{
            //    //System.Drawing.Image returnImage = null;
            //    pictureBox1.Image = Clipboard.GetImage();
            //    //Windows.ApplicationModel.DataTransfer.Clipboard.GetContent();
            //}
        }

        void vui_OnClose(object sender, CloseArgs e)
        {
            throw new NotImplementedException();
        }

        private void runWork(object sender, DoWorkEventArgs e)
        {
            System.Threading.Thread.Sleep(2000);
        }

        private void taskCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Reset mouse cursor
            Cursor.Show();
            Cursor.Current = Cursors.WaitCursor;
            Cursor.Current = Cursors.Default;
        }


        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            logShortcut("CTRL + S");
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            logShortcut("CTRL + O");
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            logShortcut("CTRL + Q");
        }

        private void alternateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            logShortcut("CTRL + A");
        }

        private void windowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            logShortcut("ALT + W");
        }

        private void logShortcut(String k)
        {
            lblShortcut.Text = k + " - " + DateTime.Now.ToString();
        }

        private void frmMyOwnApp_KeyDown(object sender, KeyEventArgs e)
        {
            logShortcut("down + W");
            String cmdKey = (e.Alt)?" ,Alt":"";
            cmdKey += (e.Shift) ? " ,Shift" : "";
            cmdKey += (e.Control) ? "Ctrl" : "";
            lblKeyDown.Text = "code: " + e.KeyCode.ToString() + ", value: " + e.KeyValue +  ((cmdKey!="")?cmdKey:"");
        }

        private void frmMyOwnApp_KeyPress(object sender, KeyPressEventArgs e)
        {
            lblKeyPress.Text = "code: " + e.KeyChar.ToString();
        }

        private void frmMyOwnApp_KeyUp(object sender, KeyEventArgs e)
        {
            String cmdKey = (e.Alt) ? " ,Alt" : "";
            cmdKey += (e.Shift) ? " ,Shift" : "";
            cmdKey += (e.Control) ? "Ctrl" : "";
            // -- EDIT
            if (e.Control)
            {
                if ((e.KeyCode.ToString() == "x") || (e.KeyCode.ToString() == "X")) {
                    // - X
                    logShortcut("Cut Detected");
                }else if ((e.KeyCode.ToString() == "v") || (e.KeyCode.ToString() == "V")) {
                    // - V
                    pasteImage();
                    logShortcut("Paste Detected");
                }else if ((e.KeyCode.ToString() == "c") || (e.KeyCode.ToString() == "C")) {
                    // - C
                    logShortcut("Copy Detected");
                }
            }
            // -- EDIT
            lblKeyUp.Text = "code: " + e.KeyCode.ToString() + ", value: " + e.KeyValue + ((cmdKey != "") ? cmdKey : "");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ro.Events["button_click"].ArgumentAsJSON("data","{\"object\":\"button1\"}").Fire();
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //BC:\Windows\WinSxS\amd64_server-help-chm.mmc.resources_31bf3856ad364e35_6.3.9600.16384_en-us_ad9c3904a1753a7b
            Help.ShowHelp(this, "file://c:\\Windows\\WinSxS\\amd64_server-help-chm.mmc.resources_31bf3856ad364e35_6.3.9600.16384_en-us_ad9c3904a1753a7b\\mmc.CHM");
            //Help.ShowHelp(this, "file:help.chm"); //c:\\helpfiles\\
            //frmHelp temp = new frmHelp();
            //temp.ShowDialog();
        }

        private void commandToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lblShortcut.Text = "Ctrl + Alt + A";
        }

        private void pd_PrintPage(object sender, PrintPageEventArgs ev)
        {
            float linesPerPage = 0;
            float yPos = 0;
            int count = 0;
            float leftMargin = ev.MarginBounds.Left;
            float topMargin = ev.MarginBounds.Top;
            String line = null;

            // Calculate the number of lines per page.
            linesPerPage = ev.MarginBounds.Height /
               printFont.GetHeight(ev.Graphics);

            // Iterate over the file, printing each line.
            while (count < linesPerPage &&
               ((line = streamToPrint.ReadLine()) != null))
            {
                yPos = topMargin + (count * printFont.GetHeight(ev.Graphics));
                ev.Graphics.DrawString(line, printFont, Brushes.Black,
                   leftMargin, yPos, new StringFormat());
                count++;
            }

            // If more lines exist, print another page.
            if (line != null)
                ev.HasMorePages = true;
            else
                ev.HasMorePages = false;
        }

        public void PrintFile(String filename)
        {
            try
            {
                filePath = System.IO.Directory.GetCurrentDirectory() + "\\resources\\" + filename;
                //filePath = System.IO.Path.GetTempFileName();
                streamToPrint = new StreamReader(filePath);
                try
                {
                    printFont = new Font("Arial", 10);
                    PrintDocument pd = new PrintDocument();
                    pd.PrintPage += new PrintPageEventHandler(pd_PrintPage);
                    // Print the document.
                    pd.Print();
                }
                finally
                {
                    streamToPrint.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            PrintFile("Photoshop CS4 Read Me.pdf");
            PrintFile("pdf_sample.pdf");
        }

        private void btnOpenLink_Click(object sender, EventArgs e)
        {
            String surl = "http://www.cybelesoft.com";
            if (textBox3.Text != "") surl = textBox3.Text;
            vui.OpenLinkDlg(surl, null);
            //if (textBox2.Text != "") surl = textBox2.Text;
            //vui.OpenLinkDlg(surl, null);
        }

        private void btnSaveFile_Click(object sender, EventArgs e)
        {
            DialogResult dr = saveFileDialog1.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {

            }
            //sdf
            //vui.DownloadFile("BrowserRules.ini");
            //vui.Sa

                /*
                 SaveDialog1.DefaultExt:= 'pdf';
    SaveDialog1.Filter:= Base.Traduce('Documentos del visor')+' (HCG)|*.hcg;*.HCG|'+
                         Base.Traduce('Formato')+' PDF|*.pdf;*.PDF';
    SaveDialog1.FileName:= copy(Titulo, 1, 250);
    if SaveDialog1.Execute then begin
                 */
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            vui.UploadFile();
        }

        private void btnSeverMessage_Click(object sender, EventArgs e)
        {
            vui.SendMessage("Server message testing");
        }

        private void btnPopupError_Click(object sender, EventArgs e)
        {
            vui.SendMessage("Server message testing");
        }

        private void btnErrorReport_Click(object sender, EventArgs e)
        {

        }

        private void btnOTURL_Click(object sender, EventArgs e)
        {
            vui.OpenLinkDlg(txtSuspend.Text, null);
        }

        private void btnJsROCopy_Click(object sender, EventArgs e)
        {
            //ro.Events["button_click"].ArgumentAsJSON("data", "{\"object\":\"button1\"}").Fire();
            ro.Events["JsROCopy"].Fire();
        }

        private void btnJsROPaste_Click(object sender, EventArgs e)
        {
            ro.Events["JsROPaste"].Fire();
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            System.Windows.Forms.Cursor.Current = Cursors.WaitCursor;
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void startCountdown()
        {

            tokenCancel = tokenCountdown.Token;
            var dueTime = TimeSpan.FromSeconds(0);
            var interval = TimeSpan.FromMilliseconds(1000);
            RunPeriodicAsync(updateSuspendTime, dueTime, interval, tokenCancel);
        }

        private void updateSuspendTime() {
            if (cSuspendTime < suspendTime && cSuspendTime >= 0)
            {
                lblSuspend.Text = cSuspendTime.ToString();
            }
            cSuspendTime--;
            if (cSuspendTime < 0)
            {
                tokenCountdown.Cancel();
                lblSuspend.Visible = false;
            }
        }

        private async Task RunPeriodicAsync(Action onTick, TimeSpan dueTime, TimeSpan interval, CancellationToken token)
        {
            // Initial wait time before we begin the periodic loop.
            if (dueTime > TimeSpan.Zero)
            {
                await Task.Delay(dueTime, token);
            }

            // Repeat this loop until cancelled.
            while (!token.IsCancellationRequested)
            {
                // Call our onTick function.
                onTick.Invoke();
                // Wait to repeat again.
                if (interval > TimeSpan.Zero)
                {
                    await Task.Delay(interval, token);
                }
            }
        }

        private void btnSuspend_Click(object sender, EventArgs e)
        {
            cSuspendTime = suspendTime;
            lblSuspend.Text = cSuspendTime.ToString();
            lblSuspend.Visible = true;
            tokenCountdown = new CancellationTokenSource();
            startCountdown();

            vui.Suspend();
            System.Threading.Thread.Sleep(suspendTime * 1000);
            vui.Resume();
        }

        void completeDragInformation(String action, String X, String Y, String Filenames)
        {
            try
            {
                txtAction.Text = action;
                txtX.Text = X;
                txtY.Text = Y;
                if (Filenames != null) txtFilenames.Text = Filenames;
                else txtFilenames.Text = "";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        void vui_OnDragFile(object sender, DragFileArgs e)
        {

            if (e.Action == Cybele.Thinfinity.DragAction.Start)
            {
                groupBox3.BackColor = NormalGroupBck;
                completeDragInformation(Cybele.Thinfinity.DragAction.Start.ToString(), e.X.ToString(), e.Y.ToString(), e.Filenames);
            }
            else if (e.Action == Cybele.Thinfinity.DragAction.Over)
            {
                completeDragInformation(Cybele.Thinfinity.DragAction.Over.ToString(), e.X.ToString(), e.Y.ToString(), e.Filenames);
                if (e.X >= groupBox3.Location.X && e.X <= (groupBox3.Size.Width +groupBox3.Location.X) && e.Y >= groupBox3.Location.Y && e.Y <= (groupBox3.Size.Height +groupBox3.Location.Y))
                {
                    // -- Check Filters
                    groupBox3.BackColor = AcceptDropBck;
                }
                else
                {
                    groupBox3.BackColor = NormalGroupBck;
                }
            }
            else if (e.Action == Cybele.Thinfinity.DragAction.Drop)
            {
                completeDragInformation(Cybele.Thinfinity.DragAction.Drop.ToString(), e.X.ToString(), e.Y.ToString(), e.Filenames);
                if (!isUploadOpened)
                {
                    isUploadOpened = true;

                    BackgroundWorker t = new BackgroundWorker();
                    t.DoWork += runUploadFileTask;
                    t.RunWorkerCompleted += runUploadFileTaskCompleted;
                    t.RunWorkerAsync();

                }
            }
            else if (e.Action == Cybele.Thinfinity.DragAction.Cancel)
            {
                completeDragInformation(Cybele.Thinfinity.DragAction.Cancel.ToString(), e.X.ToString(), e.Y.ToString(), e.Filenames);
                groupBox3.BackColor = NormalGroupBck;
            }
            else if (e.Action == Cybele.Thinfinity.DragAction.Error)
            {
                completeDragInformation(Cybele.Thinfinity.DragAction.Error.ToString(), e.X.ToString(), e.Y.ToString(), e.Filenames);
                groupBox3.BackColor = NormalGroupBck;
            }
        }

        private void runUploadFileTask(object sender, DoWorkEventArgs e)
        {
            vui.UploadFile();
        }

        private void runUploadFileTaskCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            groupBox3.BackColor = NormalGroupBck;
        }

        private void groupBox3_DragEnter(object sender, DragEventArgs e)
        {
            //ro.Events["dragEnter"].ArgumentAsString("type","file").Fire();
        }

        private void btnDownloadFile_Click(object sender, EventArgs e)
        {
            String s = @"C:\temp\test.pdf";
            vui.OnDownloadEnd += vui_OnDownloadEnd;
            vui.DownloadFile(s);
        }

        void vui_OnDownloadEnd(object sender, DownloadEndArgs e)
        {

        }

        void vui_OnUploadEnd(object sender, UploadEndArgs e)
        {
            txtAction.Text = "Download End.";
            isUploadOpened = false;
        }

        private void groupBox3_DragDrop(object sender, DragEventArgs e)
        {
            //vui.UploadFile();
        }

        private void rbRelative_CheckedChanged(object sender, EventArgs e)
        {
            vui.ClientSettings.MouseMoveGestureStyle = MouseMoveGestureStyle.MM_STYLE_RELATIVE;
        }

        private void rbAbsolute_CheckedChanged(object sender, EventArgs e)
        {
            vui.ClientSettings.MouseMoveGestureStyle = MouseMoveGestureStyle.MM_STYLE_ABSOLUTE;
        }

        private void rbMove_CheckedChanged(object sender, EventArgs e)
        {
            vui.ClientSettings.MouseMoveGestureAction = MouseMoveGestureAction.MM_ACTION_MOVE;
        }

        private void rbWheel_CheckedChanged(object sender, EventArgs e)
        {
            vui.ClientSettings.MouseMoveGestureAction = MouseMoveGestureAction.MM_ACTION_WHEEL;
        }

        private void chkCursorVisible_CheckedChanged(object sender, EventArgs e)
        {
            vui.ClientSettings.CursorVisible = chkCursorVisible.Checked;
        }

        private Boolean pasteImage()
        {
            if (Clipboard.ContainsImage())
            {
                pictureBox1.Image = Clipboard.GetImage();

                return true;
            }
            else
            {
                return false;
            }
        }

        private void copyImage()
        {
            if (pictureBox2.Image != null)
            {
                Clipboard.SetImage(pictureBox2.Image);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            nfsIdx++;
            if (nfsIdx > 6) { nfsIdx = 1; }
            switch (nfsIdx)
            {
                case 1:
                    pictureBox2.Image = MyOwnApp.Properties.Resources.nfs1;
                    break;
                case 2:
                    pictureBox2.Image = MyOwnApp.Properties.Resources.nfs2;
                    break;
                case 3:
                    pictureBox2.Image = MyOwnApp.Properties.Resources.nfs3;
                    break;
                case 4:
                    pictureBox2.Image = MyOwnApp.Properties.Resources.nfs4;
                    break;
                case 5:
                    pictureBox2.Image = MyOwnApp.Properties.Resources.nfs5;
                    break;
                case 6:
                    pictureBox2.Image = MyOwnApp.Properties.Resources.nfs6;
                    break;
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            copyImage();
        }


        private void textBox8_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter){
                 lblCopy.Text = textBox8.Text;
            }
        }

        private void txtSuspend_TextChanged(object sender, EventArgs e)
        {
            String t = txtSuspend.Text.Trim();
            Int16 st = 5;
            if (Int16.TryParse(t, out st))
            {
                btnSuspend.Text = "Suspend (for " + st.ToString() + "s)";
                suspendTime = st;
                cSuspendTime = st;
            }
        }

    }
}

namespace ClipboardAssist
{

    // Must inherit Control, not Component, in order to have Handle
    [DefaultEvent("ClipboardChanged")]
    public partial class ClipboardMonitor : Control
    {
        IntPtr nextClipboardViewer;

        public ClipboardMonitor()
        {
            this.BackColor = Color.Red;
            this.Visible = false;

            nextClipboardViewer = (IntPtr)SetClipboardViewer((int)this.Handle);
        }

        /// <summary>
        /// Clipboard contents changed.
        /// </summary>
        public event EventHandler<ClipboardChangedEventArgs> ClipboardChanged;

        protected override void Dispose(bool disposing)
        {
            ChangeClipboardChain(this.Handle, nextClipboardViewer);
        }

        [DllImport("User32.dll")]
        protected static extern int SetClipboardViewer(int hWndNewViewer);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            // defined in winuser.h
            const int WM_DRAWCLIPBOARD = 0x308;
            const int WM_CHANGECBCHAIN = 0x030D;

            switch (m.Msg)
            {
                case WM_DRAWCLIPBOARD:
                    OnClipboardChanged();
                    SendMessage(nextClipboardViewer, m.Msg, m.WParam, m.LParam);
                    break;

                case WM_CHANGECBCHAIN:
                    if (m.WParam == nextClipboardViewer)
                        nextClipboardViewer = m.LParam;
                    else
                        SendMessage(nextClipboardViewer, m.Msg, m.WParam, m.LParam);
                    break;

                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        void OnClipboardChanged()
        {
            try
            {
                IDataObject iData = Clipboard.GetDataObject();
                if (ClipboardChanged != null)
                {
                    ClipboardChanged(this, new ClipboardChangedEventArgs(iData));
                }

            }
            catch (Exception e)
            {
                // Swallow or pop-up, not sure
                // Trace.Write(e.ToString());
                MessageBox.Show(e.ToString());
            }
        }

        public class ClipboardChangedEventArgs : EventArgs
        {
            public readonly IDataObject DataObject;

            public ClipboardChangedEventArgs(IDataObject dataObject)
            {
                DataObject = dataObject;
            }
        }
    }
}


