﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;

namespace Clicker
{
    public partial class KeyBinder : Form
    {
        Form1 mainForm;
        bool isStartButton;

        public KeyBinder(Form1 main,bool isStart = true)
        {
            InitializeComponent();
            mainForm = main;
            isStartButton = isStart;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(Keyboard.IsKeyDown(Key.Escape))
            {
                Close();
                return;
            }
            Key[] pressedKeys = Util.GetPressedKey();
            Console.WriteLine(Util.BuildKeyString(pressedKeys));
            if (pressedKeys.Length > 3)
            {
                MessageBox.Show("4つ以上のショートカットキーを設定することはできません");
                return;
            }
            if(pressedKeys.Length == 0)
            {
                MessageBox.Show("ショートカットキーが押されていません。");
                return;
            }
            Console.WriteLine(Util.BuildKeyString(pressedKeys));

            mainForm.SetShortcutKey(pressedKeys, isStartButton);
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            mainForm.SetShortcutKey(null, isStartButton);
            Close();
        }
    }

    public class Util
    {
        public static Key[] GetPressedKey()
        {
            List<Key> list = new List<Key>();
            for(int i = 1;i < 155;i++)
            {
                if(Keyboard.IsKeyDown((Key)i))
                {
                    list.Add((Key)i);
                }
            }
            return list.ToArray();
        }

        public static String BuildKeyString(params Key[] keys)
        {
            if(keys == null || keys.Length <= 0)
            {
                return "<NONE>";
            }
            StringBuilder sb = new StringBuilder();
            for(int i = 0;i < keys.Length;i++)
            {
                if(i == keys.Length - 1)
                {
                    sb.Append(ReplaceLR(keys[i].ToString()));
                }
                else
                {
                    sb.Append(ReplaceLR(keys[i].ToString()) + " + ");
                }
            }
            return sb.ToString();
        }

        public static bool IsAllKeyDown(params Key[] keys)
        {
            if(keys == null || keys.Length <= 0)
            {
                return false;
            }
            foreach (Key key in keys)
            {
                if (Keyboard.IsKeyUp(key))
                {
                    return false;
                }
            }
            return true;
        }

        private static string ReplaceLR(string str)
        {
            if (str.Contains("Left") && str.Trim() != "Left")
            {
                return str.Replace("Left", "L");
            }
            if (str.Contains("Right") && str.Trim() != "Right")
            {
                return str.Replace("Right", "R");
            }
            return str;
        }
    }
}
