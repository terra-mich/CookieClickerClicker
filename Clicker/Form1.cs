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

        //定数
        private const int MOUSEEVENTF_LEFTDOWN = 0x2;
        private const int MOUSEEVENTF_LEFTUP = 0x4;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x8;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;
        private const int MARGIN = 10;

        private bool isExecuting = false; //現在実行中かを保持しておく変数だね

        private int prevProcID = -1; //直前にクリックしたプロセスのIDを格納

        private Key[] startKeys = null;
        private Key[] stopKeys = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Configuration.LoadValue(ref startKeys, ref stopKeys);
            label5.Text = Util.BuildKeyString(startKeys);
            label7.Text = Util.BuildKeyString(stopKeys);
        }

        private static void ClickMouse(bool isLeft = true)
        {
            int dwFlagDown = isLeft ? MOUSEEVENTF_LEFTDOWN : MOUSEEVENTF_RIGHTDOWN, dwFlagUp = isLeft ? MOUSEEVENTF_LEFTUP : MOUSEEVENTF_RIGHTUP;

            WinAPI.mouse_event(dwFlagDown, 0, 0, 0, 0); //押してー
            WinAPI.mouse_event(dwFlagUp, 0, 0, 0, 0); //離す！
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
            if(startKeys == null || stopKeys == null)
            {
                return;
            }
            if(isExecuting)
            {
                if(Util.IsAllKeyDown(stopKeys))
                {
                    prevProcID = -1;
                    isExecuting = false;
                    return;
                }

                Process p = GetActiveProcess();
                if(prevProcID == -1)
                {
                    prevProcID = p.Id;
                }
                if(p.Id != prevProcID)
                {
                    //誤爆した！
                    prevProcID = -1;
                    isExecuting = false;
                    return;
                }

                if(p.MainWindowHandle != IntPtr.Zero) //ウィンドウハンドラ持ってるかどうか判定する(ウィンドウから取得したプロセスだから持ってるとは思うけど一応)
                {
                    Point mousePos = MousePosition;                 //マウスの座標を格納
                    WinAPI.RECT windowRect = new WinAPI.RECT();     //ウィンドウサイズを格納
                    WinAPI.RECT clientSize = new WinAPI.RECT();     //クライアント領域のサイズを格納

                    WinAPI.GetWindowRect(p.MainWindowHandle, out windowRect);   //アクティブウィンドウの座標を取得
                    WinAPI.GetClientRect(p.MainWindowHandle, out clientSize);   //クライアント領域のサイズを取得

                    WinAPI.POINT clientRightBottomPoint = new WinAPI.POINT();   //クライアント領域の右下の座標を格納する
                    clientRightBottomPoint.x = clientSize.right;
                    clientRightBottomPoint.y = clientSize.bottom;

                    int code = WinAPI.ClientToScreen(p.MainWindowHandle, ref clientRightBottomPoint);   //右下の座標を画面座標に変換

                    if(code == 0)
                    {
                        //変換失敗したから戻るよ
                        return;
                    }

                    WinAPI.POINT clientLeftTopPoint = new WinAPI.POINT();                   //クライアント領域の左上の座標を格納する
                    clientLeftTopPoint.x = clientRightBottomPoint.x - clientSize.right;     //X座標を計算
                    clientLeftTopPoint.y = clientRightBottomPoint.y - clientSize.bottom;    //Y座標を計算

                    int mouseX = mousePos.X, mouseY = mousePos.Y;

                    bool flag = false;

                    //これ以降はマウスの動きを制限するコードだよ
                    if(mouseX < clientLeftTopPoint.x + MARGIN)
                    {
                        flag = true;
                        mouseX = clientLeftTopPoint.x + MARGIN;
                    }
                    if(mouseX > clientRightBottomPoint.x - MARGIN)
                    {
                        flag = true;
                        mouseX = clientRightBottomPoint.x - MARGIN;
                    }
                    if(mouseY < clientLeftTopPoint.y + MARGIN)
                    {
                        flag = true;
                        mouseY = clientLeftTopPoint.y + MARGIN;
                    }
                    if(mouseY > clientRightBottomPoint.y - MARGIN)
                    {
                        flag = true;
                        mouseY = clientRightBottomPoint.y - MARGIN;
                    }

                    if (flag)
                    {
                        WinAPI.SetCursorPos(mouseX, mouseY);    //マウスを移動
                    }
                }
            }
            else
            {
                if (stopKeys != null && stopKeys.Length > 0 &&  Util.IsAllKeyDown(startKeys))
                {
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
            //TopMost = true;
            if (isExecuting) //実行してるかな？
            {
                if (radioButton1.Checked || radioButton3.Checked)
                {
                    ClickMouse(); //左クリック！
                }
                if (radioButton2.Checked || radioButton3.Checked)
                {
                    ClickMouse(false); //右クリック！
                } 
            }
        }

        public void SetShortcutKey(Key[] keys, bool isStart = true)
        {
            if(isStart)
            {
                startKeys = keys;
                label5.Text = Util.BuildKeyString(keys);
            }
            else
            {
                stopKeys = keys;
                label7.Text = Util.BuildKeyString(keys);
            }
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            KeyBinder kbStart = new KeyBinder(this);
            AddOwnedForm(kbStart);
            kbStart.ShowDialog();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            KeyBinder kbStop = new KeyBinder(this, false);
            AddOwnedForm(kbStop);
            kbStop.ShowDialog();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Configuration.SaveValue(startKeys, stopKeys);
        }
    }
    public class WinAPI
    {
        /*
         * WIN32APIを使うための宣言をまとめたクラス。
         */
        [StructLayout(LayoutKind.Sequential, Pack = 4)] //WIN32APIでいうところのRECT構造体にあたる構造体
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 4)] //WIN32APIでいうところのPOINT構造体にあたる構造体
        public struct POINT
        {
            public int x;
            public int y;
        }
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow(); //現在アクティブなウィンドウハンドラを返すよ
        [DllImport("user32.dll")]
        public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId); //ウィンドウハンドラからプロセスIDを割り出すよ
        [DllImport("User32.Dll")]
        public static extern int GetWindowRect(IntPtr hWnd, out RECT lpRect); //ウィンドウハンドラを渡すとウィンドウの座標を返してくれるAPIだよ
        [DllImport("User32.Dll")]
        public static extern int GetClientRect(IntPtr hWnd, out RECT lpRect); //ウィンドウハンドラを渡すとクライアント領域のサイズを返してくれるAPIだよ
        [DllImport("User32.Dll")]
        public static extern int ClientToScreen(IntPtr hWnd, ref POINT lpPoint); //ウィンドウハンドラを渡すとクライアント領域の座標を画面座標に変換するよ
        [DllImport("User32.dll")]
        public static extern int SetCursorPos(int x, int y); //カーソル位置を設定する感じかも
        [DllImport("USER32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo); //マウスのイベントを起こす 色々イベントはあるらしいけどUPとDOWNしか使わないね

    }
}
