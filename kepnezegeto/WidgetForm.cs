using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace kepnezegeto
{
    class WidgetForm
    {
        Form1 mainForm;

        Timer hiderTimer = new Timer();
        PictureBox close = new PictureBox();
        PictureBox resizer = new PictureBox();
        PictureBox maximize = new PictureBox();
        PictureBox settings = new PictureBox();
        PictureBox minimize = new PictureBox();

        WidgetFormSettings Settings;

        Panel settingsPanel = new Panel() { Visible = false };
        public CheckBox checkbox_rememberMainformPosition = new CheckBox();
        public CheckBox checkbox_rememberMainformSize = new CheckBox();
        public CheckBox checkbox_alwaysOnTop = new CheckBox();

        bool mouseClicked = false;
        bool dragEnter = false;
        bool maximized = false;
        Point defaultFirstSettingsPropertyLocation = new Point(5, 25);
        Point mousePos;
        Size maxSize, minSize;
        Size buttonSize;
        FormWindowState previousState = FormWindowState.Normal;

        public WidgetForm(Form1 mainForm)
        {
            this.mainForm = mainForm;
            Settings = new WidgetFormSettings(this, mainForm);

            mainForm.TransparencyKey = Color.Fuchsia;
            mainForm.BackColor = Color.Fuchsia;

            mainForm.MouseMove += MainForm_MouseMove;

            minSize = new Size(400, 300);
            maxSize = new Size(Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height);

            buttonSize = new Size(20, 18);

            hiderTimer.Tick += hiderTimer_Tick;
            hiderTimer.Interval = 2000;
            hiderTimer.Enabled = true;

            close.Size = buttonSize;
            close.Location = new Point(mainForm.ClientSize.Width - close.Size.Width - 20, 2);
            close.MouseClick += Close_MouseClick;
            mainForm.Controls.Add(close);
            close.BringToFront();

            resizer.Size = buttonSize;
            resizer.Location = new Point(mainForm.ClientSize.Width - resizer.Size.Width - 20, mainForm.ClientSize.Height - resizer.Size.Height - 2);
            resizer.MouseDown += Resizer_MouseDown;
            resizer.MouseUp += Resizer_MouseUp;
            resizer.MouseMove += Resizer_MouseMove;
            mainForm.Controls.Add(resizer);
            resizer.BringToFront();

            maximize.Size = buttonSize;
            maximize.Location = new Point(close.Location.X - buttonSize.Width - 7, 2);
            maximize.MouseDown += Maximize_MouseDown;
            mainForm.Controls.Add(maximize);
            maximize.BringToFront();

            minimize.Size = buttonSize;
            minimize.Location = new Point(close.Location.X - buttonSize.Width - buttonSize.Width - 14, 2);
            minimize.MouseDown += Minimize_MouseDown;
            mainForm.Controls.Add(minimize);
            minimize.BringToFront();

            settings.Size = buttonSize;
            settings.Location = new Point(20, 2);
            settings.MouseDown += Settings_MouseDown;
            mainForm.Controls.Add(settings);
            settings.BringToFront();


            settingsPanel.Size = new Size(150, 150);
            settingsPanel.Location = new Point(settings.Location.X, settings.Location.Y + settings.Height);
            settingsPanel.BackColor = Color.Black;

            checkbox_rememberMainformPosition.Location = defaultFirstSettingsPropertyLocation;
            checkbox_rememberMainformPosition.Text = "remember position";
            checkbox_rememberMainformPosition.BackColor = settingsPanel.BackColor;
            checkbox_rememberMainformPosition.ForeColor = Color.Gray;
            checkbox_rememberMainformPosition.AutoSize = true;
            checkbox_rememberMainformPosition.CheckedChanged += Checkbox_rememberMainformPosition_CheckedChanged;

            checkbox_rememberMainformSize.Location = new Point(defaultFirstSettingsPropertyLocation.X, defaultFirstSettingsPropertyLocation.Y + checkbox_rememberMainformPosition.Height);
            checkbox_rememberMainformSize.Text = "remember size";
            checkbox_rememberMainformSize.BackColor = settingsPanel.BackColor;
            checkbox_rememberMainformSize.ForeColor = Color.Gray;
            checkbox_rememberMainformSize.AutoSize = true;
            checkbox_rememberMainformSize.CheckedChanged += Checkbox_rememberMainformSize_CheckedChanged;

            checkbox_alwaysOnTop.Location = new Point(defaultFirstSettingsPropertyLocation.X, defaultFirstSettingsPropertyLocation.Y + checkbox_rememberMainformPosition.Height + checkbox_rememberMainformPosition.Height);
            checkbox_alwaysOnTop.Text = "always on top";
            checkbox_alwaysOnTop.BackColor = settingsPanel.BackColor;
            checkbox_alwaysOnTop.ForeColor = Color.Gray;
            checkbox_alwaysOnTop.AutoSize = true;
            checkbox_alwaysOnTop.CheckedChanged += Checkbox_alwaysOnTop_CheckedChanged;

            settingsPanel.Controls.Add(checkbox_rememberMainformPosition);
            settingsPanel.Controls.Add(checkbox_rememberMainformSize);
            settingsPanel.Controls.Add(checkbox_alwaysOnTop);
            mainForm.Controls.Add(settingsPanel);
            settingsPanel.BringToFront();
            //settingsPanel.Visible = false;


            drawButtons();

            relocator();
        }

        #region Events

        private void Sensor_Tick(object sender, EventArgs e)
        {
            // DragDrop event segéd Timer-e!!!
            /*if (Cursor.Position.X >= Location.X && Cursor.Position.X < Location.X + Size.Width && Cursor.Position.Y >= (Location.Y + 25) && Cursor.Position.Y < (Location.Y + 25) + (Size.Height - 50))
            {
                if (dragEnter) chrome.Enabled = false;
                else chrome.Enabled = true;
            }
            else
            {
                chrome.Enabled = false;
            }*/
        }

        private void Checkbox_alwaysOnTop_CheckedChanged(object sender, EventArgs e)
        {
            Settings.AlwaysOnTop = checkbox_alwaysOnTop.Checked;
        }

        private void Checkbox_rememberMainformSize_CheckedChanged(object sender, EventArgs e)
        {
            Settings.RememberMainformSize = checkbox_rememberMainformSize.Checked;
            Settings.MainformSize = mainForm.ClientSize;
        }

        private void Checkbox_rememberMainformPosition_CheckedChanged(object sender, EventArgs e)
        {
            Settings.RememberMainformPosition = checkbox_rememberMainformPosition.Checked;
            Settings.MainformPosition = mainForm.Location;
        }

        private void Settings_MouseDown(object sender, MouseEventArgs e)
        {
            settingsPanel.Visible = !settingsPanel.Visible;
        }

        private void Minimize_MouseDown(object sender, MouseEventArgs e)
        {
            if (mainForm.WindowState == FormWindowState.Normal || mainForm.WindowState == FormWindowState.Maximized)
            {
                previousState = mainForm.WindowState;
                mainForm.WindowState = FormWindowState.Minimized;
            }
            else
            {
                mainForm.WindowState = previousState;
            }
        }

        private void Maximize_MouseDown(object sender, MouseEventArgs e)
        {
            if (maximized)
            {
                mainForm.WindowState = FormWindowState.Normal;
                Settings.MainformMaximized = false;
                if (Settings.RememberMainformSize) Settings.MainformSize = mainForm.ClientSize;
                if (Settings.RememberMainformPosition) Settings.MainformPosition = mainForm.Location;
                relocator();
                maximized = false;
            }
            else
            {
                mainForm.WindowState = FormWindowState.Maximized;
                Settings.MainformMaximized = true;
                relocator();
                maximized = true;
            }
            mainForm.ReloadImage();
        }

        private void Resizer_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseClicked && mainForm.WindowState == FormWindowState.Normal)
            {
                Size newSize = new Size((resizer.Location.X + mousePos.X) - ((resizer.Location.X + mousePos.X) - (resizer.Location.X + e.X)), (resizer.Location.Y + mousePos.Y) - ((resizer.Location.Y + mousePos.Y) - (resizer.Location.Y + e.Y)));
                if (newSize.Width < maxSize.Width && newSize.Width > minSize.Width && newSize.Height < maxSize.Height && newSize.Height > minSize.Height)
                {
                    mainForm.ClientSize = newSize;
                    mousePos = e.Location;

                    relocator();
                }
            }
        }

        private void Resizer_MouseUp(object sender, MouseEventArgs e)
        {
            mouseClicked = false;
            if (Settings.RememberMainformSize && mainForm.WindowState != FormWindowState.Maximized) Settings.MainformSize = mainForm.ClientSize;
            mainForm.ReloadImage();
        }

        private void Resizer_MouseDown(object sender, MouseEventArgs e)
        {
            mousePos = new Point(e.X, e.Y);
            mouseClicked = true;
        }

        private void Close_MouseClick(object sender, MouseEventArgs e)
        {
            Application.Exit();
        }

        private void hiderTimer_Tick(object sender, EventArgs e)
        {
            hiderTimer.Enabled = false;
            close.Visible = false;
            resizer.Visible = false;
            maximize.Visible = false;
            minimize.Visible = false;
            settings.Visible = false;
            hiderTimer.Stop();
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            dragEnter = true;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            // LOADIMAGE!!!!
            
            //linkApply(e.Data.GetData(DataFormats.Text, false).ToString());
            dragEnter = false;
        }

        private void Form1_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.UnicodeText)) e.Effect = DragDropEffects.Copy;
            else e.Effect = DragDropEffects.None;
        }

        private void Form1_DragLeave(object sender, EventArgs e)
        {
            dragEnter = false;
        }

        private void Form1_MouseWheel(object sender, MouseEventArgs e)
        {
            // ZOOM!!!!
            /*if (e.Delta > 0)
            {
                volume += 10;
            }
            else if (e.Delta < 0)
            {
                volume -= 10;
            }
            if (volume > 100) volume = 100;
            if (volume < 0) volume = 0;
            string javascriptVolume = "SetVolume(" + volume + ");";
            displayVolume();

            chrome.ExecuteScriptAsync(javascriptVolume);*/
        }

        private void MainForm_MouseMove(object sender, MouseEventArgs e)
        {
            close.Visible = true;
            resizer.Visible = true;
            maximize.Visible = true;
            minimize.Visible = true;
            settings.Visible = true;
            hiderTimer.Start();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Settings.RememberMainformPosition) Settings.MainformPosition = mainForm.Location;
            if (Settings.RememberMainformSize)
            {
                switch (mainForm.WindowState)
                {
                    case FormWindowState.Maximized:
                        Settings.MainformMaximized = true;
                        break;
                    case FormWindowState.Normal:
                        Settings.MainformSize = mainForm.ClientSize;
                        break;
                }
            }
            Settings.SaveSettings();
        }

        #endregion


        void drawButtons()
        {
            Pen defaultPen = new Pen(Color.FromArgb(255, 58, 59, 59), 2f);
            SolidBrush defaultBrush = new SolidBrush(Color.FromArgb(255, 58, 59, 59));

            Bitmap tmp = new Bitmap(buttonSize.Width, buttonSize.Height);
            Graphics g = Graphics.FromImage(tmp);
            g.FillRectangle(Brushes.Black, new Rectangle(0, 0, buttonSize.Width, buttonSize.Height));
            g.DrawLine(defaultPen, new Point(0, 0), new Point(buttonSize.Width, buttonSize.Height));
            g.DrawLine(defaultPen, new Point(0, buttonSize.Height), new Point(buttonSize.Width, 0));
            close.Image = tmp;

            // A nyíl aljával nemstimmel valami!
            tmp = new Bitmap(buttonSize.Width, buttonSize.Height);
            g = Graphics.FromImage(tmp);
            g.FillRectangle(Brushes.Black, new Rectangle(0, 0, buttonSize.Width, buttonSize.Height));
            g.DrawLine(defaultPen, 0, 0, 18, 18);
            g.DrawLine(defaultPen, 16, 5, 18, 18);
            g.DrawLine(defaultPen, 5, 16, 18, 18);
            resizer.Image = tmp;

            tmp = new Bitmap(buttonSize.Width, buttonSize.Height);
            g = Graphics.FromImage(tmp);
            g.FillRectangle(Brushes.Black, new Rectangle(0, 0, buttonSize.Width, buttonSize.Height));
            g.DrawLine(defaultPen, 2, 4, 2, 16);
            g.DrawLine(defaultPen, 2, 16, 18, 16);
            g.DrawLine(defaultPen, 18, 16, 18, 4);
            g.DrawLine(defaultPen, 18, 4, 2, 4);
            maximize.Image = tmp;

            tmp = new Bitmap(buttonSize.Width, buttonSize.Height);
            g = Graphics.FromImage(tmp);
            g.FillRectangle(Brushes.Black, new Rectangle(0, 0, buttonSize.Width, buttonSize.Height));
            g.DrawLine(defaultPen, 2, buttonSize.Height / 2, buttonSize.Width - 2, buttonSize.Height / 2);
            minimize.Image = tmp;

            tmp = new Bitmap(buttonSize.Width, buttonSize.Height);
            g = Graphics.FromImage(tmp);
            g.FillRectangle(Brushes.Black, new Rectangle(0, 0, buttonSize.Width, buttonSize.Height));
            g.DrawImage(Properties.Resources.gear_512, 0, 0, buttonSize.Width, buttonSize.Height);
            settings.Image = tmp;

            tmp = new Bitmap(settingsPanel.Width, settingsPanel.Height);
            g = Graphics.FromImage(tmp);
            //g.FillRectangle(Brushes.DarkGray, new Rectangle(0, 0, settingsPanel.Width, settingsPanel.Height));
            FontFamily fontFamily = new FontFamily("Arial");
            Font font = new Font(
               fontFamily,
               12,
               FontStyle.Regular,
               GraphicsUnit.Point);
            g.DrawString("Settings", font, Brushes.Gray, new Point(0, 0));
            settingsPanel.BackgroundImage = tmp;

            GC.Collect();
        }

        void drawRoundedFormWindow()
        {
            Bitmap bmp = new Bitmap(mainForm.ClientSize.Width, mainForm.ClientSize.Height);
            Graphics gfx = Graphics.FromImage(bmp);

            Rectangle Bounds = new Rectangle(new Point(0, 0), mainForm.ClientSize);
            int CornerRadius = 50;
            Pen DrawPen = Pens.Black;
            Color FillColor = Color.Black;

            int strokeOffset = Convert.ToInt32(Math.Ceiling(DrawPen.Width));
            Bounds = Rectangle.Inflate(Bounds, -strokeOffset, -strokeOffset);

            GraphicsPath gfxPath = new GraphicsPath();
            gfxPath.AddArc(Bounds.X, Bounds.Y, CornerRadius, CornerRadius, 180, 90);
            gfxPath.AddArc(Bounds.X + Bounds.Width - CornerRadius, Bounds.Y, CornerRadius, CornerRadius, 270, 90);
            gfxPath.AddArc(Bounds.X + Bounds.Width - CornerRadius, Bounds.Y + Bounds.Height - CornerRadius, CornerRadius, CornerRadius, 0, 90);
            gfxPath.AddArc(Bounds.X, Bounds.Y + Bounds.Height - CornerRadius, CornerRadius, CornerRadius, 90, 90);
            gfxPath.CloseAllFigures();

            gfx.FillPath(new SolidBrush(FillColor), gfxPath);
            gfx.DrawPath(DrawPen, gfxPath);

            mainForm.BackgroundImage = bmp;

            GC.Collect();
        }

        void relocator()
        {
            if (mainForm.WindowState == FormWindowState.Maximized)
            {
                close.Location = new Point(mainForm.ClientSize.Width - close.Size.Width - 2, 2);
                resizer.Location = new Point(mainForm.ClientSize.Width - resizer.Size.Width - 2, mainForm.ClientSize.Height - resizer.Size.Height - 2);
                maximize.Location = new Point(close.Location.X - buttonSize.Width - 7, 2);
                minimize.Location = new Point(close.Location.X - buttonSize.Width - buttonSize.Width - 14, 2);
                settings.Location = new Point(2, 2);

                mainForm.BackColor = Color.Black;
                mainForm.BackgroundImage = null;
            }
            else
            {
                drawRoundedFormWindow();

                close.Location = new Point(mainForm.ClientSize.Width - close.Size.Width - 20, 2);
                resizer.Location = new Point(mainForm.ClientSize.Width - resizer.Size.Width - 20, mainForm.ClientSize.Height - resizer.Size.Height - 2);
                maximize.Location = new Point(close.Location.X - buttonSize.Width - 7, 2);
                minimize.Location = new Point(close.Location.X - buttonSize.Width - buttonSize.Width - 14, 2);
                settings.Location = new Point(20, 2);

                mainForm.BackColor = mainForm.TransparencyKey;
                
            }
            mainForm.picture.Size = new Size(mainForm.ClientSize.Width, mainForm.ClientSize.Height - 50);
            //if (mainForm.picture.Image != null) mainForm.picture.Image = mainForm.resize(mainForm.picture.Image);
            //ReloadImage();
            
        }

        

    }
}
