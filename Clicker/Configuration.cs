using Newtonsoft.Json;
using System.Windows.Input;
using System.IO;
using System.Windows.Forms;

namespace Clicker
{
    public class SavedValue
    {
        public int[] startKeys;
        public int[] stopKeys;
    }
    public class Configuration
    {
        public static void SaveValue(Key[] startKeys, Key[] stopKeys)
        {
            SavedValue sv = new SavedValue();
            int[] startKeyIds, stopKeyIds;
            
            if(startKeys != null)
            {
                startKeyIds = new int[startKeys.Length];
            }
            else
            {
                startKeyIds = new int[0];
            }
            if(stopKeys != null)
            {
                stopKeyIds = new int[stopKeys.Length];
            }
            else
            {
                stopKeyIds = new int[0];
            }
            
            for(int i = 0; i < startKeyIds.Length; i++)
            {
                startKeyIds[i] = (int)startKeys[i];
            }
            for(int i = 0; i < stopKeyIds.Length; i++)
            {
                stopKeyIds[i] = (int)stopKeys[i];
            }

            sv.startKeys = startKeyIds;
            sv.stopKeys = stopKeyIds;

            string json = JsonConvert.SerializeObject(sv);
            File.WriteAllText("config.json", json);
        }

        public static void LoadValue(ref Key[] startKeys, ref Key[] stopKeys)
        {
            if(!File.Exists("config.json"))
            {
                return;
            }
            string json = File.ReadAllText("config.json");

            SavedValue sv   = JsonConvert.DeserializeObject<SavedValue>(json);
            startKeys       = new Key[sv.startKeys.Length];
            stopKeys        = new Key[sv.stopKeys.Length];

            for(int i = 0; i < startKeys.Length; i++)
            {
                startKeys[i] = (Key)sv.startKeys[i];
            }
            for(int i = 0;i < stopKeys.Length;i++)
            {
                stopKeys[i] = (Key)sv.stopKeys[i];
            }
            Form1 form = (Form1)Application.OpenForms[0];
            form.TopMost = true;
        }
    }
}
