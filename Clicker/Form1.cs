using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Input;

namespace Clicker
{
    /*
     * TODO: キーコンフィグ出来るようになるといいな
     * WIN32APIを結構使っちゃってるからPureじゃない気がするけどしかたないね
     */
    public partial class Form1 : Form
    {
        [StructLayout(LayoutKind.Sequential, Pack = 4)] //WIN32APIでいうところのLPRECT構造体にあたる構造体
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }
        [DllImport("User32.Dll")]
        static extern int GetWindowRect(IntPtr hWnd, out RECT lpRect); //ウィンドウハンドラを渡すとウィンドウの座標を返してくれるAPIだよ
        [DllImport("User32.dll")]
        static extern int SetCursorPos(int x, int y); //カーソル位置を設定する感じかも
        [DllImport("USER32.dll")]
        static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo); //マウスのイベントを起こす 色々イベントはあるらしいけどUPとDOWNしか使わないね

        //定数
        private const int MOUSEEVENTF_LEFTDOWN = 0x2;
        private const int MOUSEEVENTF_LEFTUP = 0x4;
        private const int MARGIN = 6;

        private bool isExecuting = false; //現在実行中かを保持しておく変数だね

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private static void LeftClick()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0); //押してー
            Thread.Sleep(1);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0); //離す！
        }

        public static Process GetActiveProcess()
        {
            // アクティブなウィンドウハンドルの取得
            IntPtr hWnd = WinAPI.GetForegroundWindow();
            int id;
            // ウィンドウハンドルからプロセスIDを取得
            WinAPI.GetWindowThreadProcessId(hWnd, out id);
            Process process = Process.GetProcessById(id);
            return process;
        }

        private void keyboardChecker_Tick(object sender, EventArgs e)
        {
            label2.Text = isExecuting ? "有効" : "無効";
            if(isExecuting)
            {
                if(Keyboard.IsKeyDown(Key.F8) && Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    isExecuting = false;
                    return;
                }
                Process p = GetActiveProcess();
                if(p.MainWindowHandle != IntPtr.Zero) //ウィンドウハンドラ持ってるかどうか判定する(ウィンドウから取得したプロセスだから持ってるとは思うけど一応)
                {
                    Point mousePos = MousePosition;
                    RECT windowRect = new RECT();

                    GetWindowRect(p.MainWindowHandle, out windowRect); //アクティブウィンドウの座標を取得

                    int mouseX = mousePos.X, mouseY = mousePos.Y;

                    bool flag = false; //カーソルを移動するか判定する変数だよ(これを入れないとガクガクになるよ)


                    //これ以降はマウスの動きを制限するコードだよ
                    if(mouseX < windowRect.left + MARGIN)
                    {
                        flag = true;
                        mouseX = windowRect.left + MARGIN;
                    }
                    if(mouseX > windowRect.right - MARGIN)
                    {
                        flag = true;
                        mouseX = windowRect.right - MARGIN;
                    }
                    if(mouseY < windowRect.top + 4)
                    {
                        flag = true;
                        mouseY = windowRect.top + MARGIN;
                    }
                    if(mouseY > windowRect.bottom - MARGIN)
                    {
                        flag = true;
                        mouseY = windowRect.bottom - MARGIN;
                    }
                    //Console.WriteLine("mouseX':{0}, mouseY':{1}", mouseX, mouseY);
                    if (flag)
                    {
                        SetCursorPos(mouseX, mouseY);
                    }
                }
            }
            else
            {
                if (Keyboard.IsKeyDown(Key.F7) && Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    Console.WriteLine("Enabled");
                    isExecuting = true;
                    return;
                }
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            clicker.Interval = (int)numericUpDown1.Value;
        }

        private void clicker_Tick(object sender, EventArgs e)
        {
            if(isExecuting) //実行してるかな？
            {
                LeftClick(); //クリック！
            }
        }
    }
    public class WinAPI
    {
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow(); //現在アクティブなウィンドウハンドラを返すよ

        [DllImport("user32.dll")]
        public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId); //ウィンドウハンドラからプロセスIDを割り出すよ
    }
}
