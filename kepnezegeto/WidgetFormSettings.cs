using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

/* settings.cfg format:

    rememberMainformPosition=0
    rememberMainformSize=0
    mainformMaximized=0
    alwaysOnTop=0
    mainformSize=400x300
    mainformPosition=0x0

*/

namespace kepnezegeto
{
    class WidgetFormSettings
    {
        Form1 mainForm;
        WidgetForm widgetForm;
        string settingsFilePath = "WidgetFormSettings.cfg";
        List<string> rawSettings = new List<string>();
        bool rememberMainformPosition = false;
        bool rememberMainformSize = false;
        bool mainformMaximized = false;
        bool alwaysOnTop = true;
        Size mainformSize;
        Point mainformPosition;

        #region Properties
        public bool RememberMainformPosition
        {
            get { return rememberMainformPosition; }
            set
            {
                rememberMainformPosition = value;
                int tmpIndex = rawSettings.FindIndex(a => a.Contains("rememberMainformPosition="));
                if (tmpIndex >= 0 && tmpIndex < rawSettings.Count)
                {
                    if (value) rawSettings[tmpIndex] = "rememberMainformPosition=1";
                    else rawSettings[tmpIndex] = "rememberMainformPosition=0";
                }
                else
                {
                    if (value) rawSettings.Add("rememberMainformPosition=1");
                    else rawSettings.Add("rememberMainformPosition=0");
                }
                SaveSettings();
            }
        }

        public bool RememberMainformSize
        {
            get { return rememberMainformSize; }
            set
            {
                rememberMainformSize = value;
                int tmpIndex = rawSettings.FindIndex(a => a.Contains("rememberMainformSize="));
                if (tmpIndex >= 0 && tmpIndex < rawSettings.Count)
                {
                    if (value) rawSettings[tmpIndex] = "rememberMainformSize=1";
                    else rawSettings[tmpIndex] = "rememberMainformSize=0";
                }
                else
                {
                    if (value) rawSettings.Add("rememberMainformSize=1");
                    else rawSettings.Add("rememberMainformSize=0");
                }
                SaveSettings();
            }
        }

        public bool MainformMaximized
        {
            get { return mainformMaximized; }
            set
            {
                mainformMaximized = value;
                int tmpIndex = rawSettings.FindIndex(a => a.Contains("mainformMaximized="));
                if (tmpIndex >= 0 && tmpIndex < rawSettings.Count)
                {
                    if (value) rawSettings[tmpIndex] = "mainformMaximized=1";
                    else rawSettings[tmpIndex] = "mainformMaximized=0";
                }
                else
                {
                    if (value) rawSettings.Add("mainformMaximized=1");
                    else rawSettings.Add("mainformMaximized=0");
                }
                SaveSettings();
            }
        }

        public bool AlwaysOnTop
        {
            get { return alwaysOnTop; }
            set
            {
                alwaysOnTop = value;
                mainForm.TopMost = value;
                int tmpIndex = rawSettings.FindIndex(a => a.Contains("alwaysOnTop="));
                if (tmpIndex >= 0 && tmpIndex < rawSettings.Count)
                {
                    if (value) rawSettings[tmpIndex] = "alwaysOnTop=1";
                    else rawSettings[tmpIndex] = "alwaysOnTop=0";
                }
                else
                {
                    if (value) rawSettings.Add("alwaysOnTop=1");
                    else rawSettings.Add("alwaysOnTop=0");
                }
                SaveSettings();
            }
        }

        public Size MainformSize
        {
            get { return mainformSize; }
            set
            {
                mainformSize = value;
                int tmpIndex = rawSettings.FindIndex(a => a.Contains("mainformSize="));
                if (tmpIndex >= 0 && tmpIndex < rawSettings.Count)
                {
                    rawSettings[tmpIndex] = "mainformSize=" + mainformSize.Width + "x" + mainformSize.Height;
                }
                else
                {
                    rawSettings.Add("mainformSize=" + mainformSize.Width + "x" + mainformSize.Height);
                }
                SaveSettings();
            }
        }

        public Point MainformPosition
        {
            get { return mainformPosition; }
            set
            {
                mainformPosition = value;
                int tmpIndex = rawSettings.FindIndex(a => a.Contains("mainformPosition="));
                if (tmpIndex >= 0 && tmpIndex < rawSettings.Count)
                {
                    rawSettings[tmpIndex] = "mainformPosition=" + mainformPosition.X + "x" + mainformPosition.Y;
                }
                else
                {
                    rawSettings.Add("mainformPosition=" + mainformPosition.X + "x" + mainformPosition.Y);
                }
                SaveSettings();
            }
        }
        #endregion

        public WidgetFormSettings(WidgetForm widgetForm, Form1 mainForm)
        {
            this.widgetForm = widgetForm;
            this.mainForm = mainForm;
            if (!File.Exists(settingsFilePath))
            {
                ResetSettings();
            }
            else
            {
                LoadSettings();
            }
        }

        public void LoadSettings()
        {
            rawSettings = File.ReadLines(settingsFilePath).ToList();

            if (rawSettings.Contains("rememberMainformPosition=1")) rememberMainformPosition = true;
            if (rawSettings.Contains("rememberMainformSize=1")) rememberMainformSize = true;
            if (rawSettings.Contains("mainformMaximized=1")) mainformMaximized = true;
            if (rawSettings.Contains("alwaysOnTop=0")) alwaysOnTop = false;

            string[] rawSize;
            string[] rawPosition;

            int tmpSizeIndex = rawSettings.FindIndex(a => a.Contains("mainformSize="));
            if (tmpSizeIndex >= 0 && tmpSizeIndex < rawSettings.Count)
            {
                rawSize = rawSettings[tmpSizeIndex].Substring(13).Split('x');
                mainformSize = new Size(Convert.ToInt32(rawSize[0]), Convert.ToInt32(rawSize[1]));
            }
            else mainformSize = mainForm.ClientSize;

            int tmpPositionIndex = rawSettings.FindIndex(a => a.Contains("mainformPosition="));
            if (tmpPositionIndex >= 0 && tmpPositionIndex < rawSettings.Count)
            {
                rawPosition = rawSettings[tmpPositionIndex].Substring(17).Split('x');
                mainformPosition = new Point(Convert.ToInt32(rawPosition[0]), Convert.ToInt32(rawPosition[1]));
            }
            else mainformPosition = mainForm.Location;

            // Apply loaded settings

            if (rememberMainformSize)
            {
                widgetForm.checkbox_rememberMainformSize.Checked = true;
                if (mainformMaximized) mainForm.WindowState = FormWindowState.Maximized;
                else mainForm.WindowState = FormWindowState.Normal;
                mainForm.ClientSize = mainformSize;
            }
            else
            {
                widgetForm.checkbox_rememberMainformSize.Checked = false;
            }

            if (rememberMainformPosition)
            {
                widgetForm.checkbox_rememberMainformPosition.Checked = true;
                mainForm.Location = mainformPosition;
            }
            else
            {
                widgetForm.checkbox_rememberMainformPosition.Checked = false;
                mainForm.Location = new Point(30, Screen.PrimaryScreen.WorkingArea.Height - mainForm.ClientSize.Height - 30);
            }

            if (alwaysOnTop)
            {
                widgetForm.checkbox_alwaysOnTop.Checked = true;
                mainForm.TopMost = true;
            }
            else
            {
                widgetForm.checkbox_alwaysOnTop.Checked = false;
                mainForm.TopMost = false;
            }

        }

        public void SaveSettings()
        {
            File.WriteAllLines(settingsFilePath, rawSettings);
        }

        public void ResetSettings()
        {
            if (File.Exists(settingsFilePath)) File.Delete(settingsFilePath);
            File.Create(settingsFilePath).Close();
            rawSettings = new List<string>();
            rawSettings.Add("rememberMainformPosition=0");
            rawSettings.Add("rememberMainformSize=0");
            rawSettings.Add("mainformMaximized=0");
            rawSettings.Add("alwaysOnTop=1");
            rawSettings.Add("mainformSize=" + mainForm.ClientSize.Width + "x" + mainForm.ClientSize.Height);
            rawSettings.Add("mainformPosition=" + mainForm.Location.X + "x" + mainForm.Location.Y);
            rememberMainformPosition = false;
            rememberMainformSize = false;
            mainformMaximized = false;
            alwaysOnTop = true;
            mainformSize = mainForm.ClientSize;
            mainformPosition = mainForm.Location;
            SaveSettings();

            widgetForm.checkbox_alwaysOnTop.Checked = alwaysOnTop;

            mainForm.TopMost = alwaysOnTop;
        }

    }
}
