using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;

enum Directions { previous, next }

namespace kepnezegeto
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            FormBorderStyle = FormBorderStyle.None;
            MouseWheel += new MouseEventHandler(Form1_MouseWheel);

            DoubleBuffered = true;
        }

        #region Moving Form
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        #endregion

        WidgetForm widgetForm;

        public PictureBox picture = new PictureBox();
        Image previousPicture;
        Image nextPicture;

        Thread loadCurrentPictureThread;
        Thread loadPreviousPictureThread;
        Thread loadNextPictureThread;

        Label debug = new Label();

        List<FileInfo> fileNames = new List<FileInfo>();

        string supportedExtensions;
        string defaultPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        int pictureCount = 0;
        int currentPictureIndex = 0;
        int previousLoadedPictureIndex = 0, nextLoadedPictureIndex = 0;
        bool previousPictureLoaded = false;
        bool nextPictureLoaded = false;

        private void Form1_Load(object sender, EventArgs e)
        {
            ClientSize = new Size(Screen.PrimaryScreen.WorkingArea.Width / 2, Screen.PrimaryScreen.WorkingArea.Height / 2);

            widgetForm = new WidgetForm(this);

            fileNames = Program.mainArgs;

            

            picture.MouseDown += Kep_MouseDown;
            picture.Size = new Size(ClientSize.Width, ClientSize.Height - 50);
            picture.Location = new Point(0, 25);
            Controls.Add(picture);
            //picture.BringToFront();

            supportedExtensions = "*.jpg,*.gif,*.png,*.bmp,*.jpe,*.jpeg,*.wmf,*.emf,*.xbm,*.ico,*.eps,*.tif,*.tiff,*.g01,*.g02,*.g03,*.g04,*.g05,*.g06,*.g07,*.g08";
            loadFileNames();

            ///*  DEBUG
            debug.AutoSize = true;
            debug.Size = new Size(50, 30);
            //Controls.Add(debug);
            debug.BringToFront();
            //*/

            if (fileNames.Count() != 0) debug.Text = currentPictureIndex + " - " + fileNames[0];

            // Make user interface (similar what the Youtube Watcher widget has)
            // Képnézegető (az összes kép megjelenik kicsiben) [több oldalas]
            // ZOOM with MouseWheel
            // SORT by filedate, filename
            // (Get path from parameter, better skin)
            // OPTIMALIZE -> filenames in dynamic list, better filenames loading algorithm;
            // Hibakezelés!!! (kattintás közben eltűnik a kép)

            if (fileNames.Count() != 0) picture.Image = resize(Image.FromFile(fileNames[currentPictureIndex].FullName));
            
            GC.Collect();
        }

        private void Form1_MouseHover(object sender, EventArgs e)
        {
            // Ha az egér sokáig nem mozdul, akkor eltűnik minden információ.
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            //widgetForm.WidgetForm_MouseMove();
            // Ha az egér megmozdul, akkor:
            //   + visszatér minden képinformáció
            //   + ha a kurzor közelvan a kilépésgombhoz akkor az megjelenik!
        }

        private void Form1_MouseWheel(object sender, MouseEventArgs e)
        {
            // ZOOM az egérgörgővel!
            //   zoomolásnál ha a kép nagyobb a képernyőnél akkor lehessen mozgatni a képet + [a kurzor megváltozik]
        }

        private void Kep_MouseDown(object sender, MouseEventArgs e)
        {
            switch(e.Button)
            {
                case MouseButtons.XButton1:
                    {
                        pictureIndexMover(Directions.previous);
                        loadPicture();
                    }
                    break;
                case MouseButtons.XButton2:
                    {
                        pictureIndexMover(Directions.next);
                        loadPicture();
                    }
                    break;
                default: return;
            }
            debug.Text += fileNames[currentPictureIndex];
            GC.Collect();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                    {
                        pictureIndexMover(Directions.previous);
                        loadPicture();
                    }
                    break;
                case Keys.Right:
                    {
                        pictureIndexMover(Directions.next);
                        loadPicture();
                    }
                    break;

                case Keys.Escape: Application.Exit();
                    break;
            }
        }

        // pictureCount is an unnecessary variable (use fileNames.Count instead)

        void pictureIndexMover(Directions direction)
        {
            switch (direction)
            {
                case Directions.previous:
                    {
                        if (currentPictureIndex <= 0) currentPictureIndex = pictureCount;
                        --currentPictureIndex;

                    }
                    break;
                case Directions.next:
                    {
                        if (currentPictureIndex >= pictureCount - 1) currentPictureIndex = -1;
                        ++currentPictureIndex;

                    }
                    break;
            }
            
        }
        /*
            
            Don't load all picture between picture 0 and 100, when the key is pressed 100 times!!!!

        */
        public void loadPicture()
        {
            bool preloadedImageExist = false;


            // Loading preloaded images...


            if (nextPictureLoaded && currentPictureIndex == nextLoadedPictureIndex)
            {
                previousPicture = picture.Image;
                int pictureIndexTMP = currentPictureIndex;
                if (pictureIndexTMP <= 0) pictureIndexTMP = pictureCount;
                previousLoadedPictureIndex = --pictureIndexTMP;
                previousPictureLoaded = true;

                if (loadNextPictureThread.ThreadState == ThreadState.Stopped)
                {
                    picture.Image = nextPicture;
                    preloadedImageExist = true;
                }
                else
                {
                    loadNextPictureThread.Join();
                    picture.Image = nextPicture;
                    //preloadedImageExist = false;
                    preloadedImageExist = true;
                }
                
                nextPictureLoaded = false;
            }

            if (previousPictureLoaded && currentPictureIndex == previousLoadedPictureIndex && !preloadedImageExist)
            {
                nextPicture = picture.Image;
                int pictureIndexTMP = currentPictureIndex;
                if (pictureIndexTMP >= pictureCount - 1) pictureIndexTMP = -1;
                nextLoadedPictureIndex = ++pictureIndexTMP;
                nextPictureLoaded = true;

                if (loadPreviousPictureThread.ThreadState == ThreadState.Stopped)
                {
                    picture.Image = previousPicture;
                    preloadedImageExist = true;
                }
                else
                {
                    loadPreviousPictureThread.Join();
                    picture.Image = previousPicture;
                    //preloadedImageExist = false;
                    preloadedImageExist = true;
                }
                
                previousPictureLoaded = false;
            }


            // Preload images...


            if (!preloadedImageExist)
            {
                loadCurrentPictureThread = new Thread(() =>
                {
                    //picture.ImageLocation = fileNames[currentPictureIndex].FullName;
                    picture.Image = resize(Image.FromFile(fileNames[currentPictureIndex].FullName));
                    
                });
                loadCurrentPictureThread.Priority = ThreadPriority.Highest;
                loadCurrentPictureThread.Start();
            }

            if (!previousPictureLoaded)
            {
                //loadPreviousPictureThread.Suspend();

                loadPreviousPictureThread = new Thread(() =>
                {
                    int pictureIndexTMP = currentPictureIndex;
                    if (pictureIndexTMP <= 0) pictureIndexTMP = pictureCount;
                    previousPicture = resize(Image.FromFile(fileNames[--pictureIndexTMP].FullName));
                    previousPictureLoaded = true;
                    previousLoadedPictureIndex = pictureIndexTMP;
                });
                loadPreviousPictureThread.Priority = ThreadPriority.AboveNormal;
                loadPreviousPictureThread.Start();
            }

            if (!nextPictureLoaded)
            {
                //loadNextPictureThread.Suspend();

                loadNextPictureThread = new Thread(() =>
                {
                    int pictureIndexTMP = currentPictureIndex;
                    if (pictureIndexTMP >= pictureCount - 1) pictureIndexTMP = -1;
                    nextPicture = resize(Image.FromFile(fileNames[++pictureIndexTMP].FullName));
                    nextPictureLoaded = true;
                    nextLoadedPictureIndex = pictureIndexTMP;
                });
                loadNextPictureThread.Priority = ThreadPriority.BelowNormal;
                loadNextPictureThread.Start();
            }

            //////////////////////////////////////////////////
                        /*if (currentPictureIndex >= pictureCount - 1) currentPictureIndex = -1;
                        picture.Image = resize(Image.FromFile(fileNames[++currentPictureIndex].FullName));*/

            GC.Collect();
            debug.Text = "" + currentPictureIndex + " - ";
        }

        public void ReloadImage()
        {
            if (picture.Image != null) loadPicture();
        }

        public Bitmap resize(Image originalPicture, int width = 0, int height = 0)
        {
            if (width<=0 || height<=0)
            {
                width = ClientSize.Width;
                height = ClientSize.Height-50;
            }
            Brush brush = new SolidBrush(Color.Black);
            double scale = Math.Min((double)width / originalPicture.Width, (double)height / originalPicture.Height);
            int scaleWidth = (int)(originalPicture.Width * scale);
            int scaleHeight = (int)(originalPicture.Height * scale);

            Bitmap returnPixels = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(returnPixels);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            g.FillRectangle(brush, new RectangleF(0, 0, width, height));

            g.DrawImage(originalPicture, new Rectangle((width - scaleWidth) / 2, (height - scaleHeight) / 2, scaleWidth, scaleHeight));

            return returnPixels;
        }

        Bitmap pictureBrowser()
        {

            return null;
        }

        string pathFinder(string szov)
        {
            //for(int i=szov.Length;i>=0;i--) 
            int j = szov.Length;
            string returnString = "";
            while (--j >= 0 && szov[j] != '\\') ;
            for (int i = 0; i <= j; i++) returnString += szov[i];
            debug.Text += "\n" + returnString+"\n";
            return returnString;
        }

        void loadFileNames()
        {
            string path = fileNames.Count != 0 ? pathFinder(fileNames[0].FullName) : defaultPath;
            foreach (string imageFile in Directory.GetFiles(path, "*.*").Where(s => supportedExtensions.Contains(Path.GetExtension(s).ToLower())))
            {
                if (/*!fileNames.Contains(new FileInfo(imageFile)) ||*/ !fileNames.Exists(x => x.FullName == imageFile))
                {
                    fileNames.Add(new FileInfo(imageFile));
                }
            }
            pictureCount = fileNames.Count;
            sortPictures();
        }

        void deleteFileNames(int i = -1)
        {
            //Predicate<string> pred = new Predicate<string>(x => true);
            if (i <= -1)
            {
                //fileNames.RemoveAll(pred);
                fileNames.Clear();
            }
            else
            {
                fileNames.RemoveAt(i);
            }
            
        }

        void sortPictures()
        {
            // Többfajta rendezés!
            fileNames.Sort((x, y) => string.Compare(y.CreationTime.ToString(), x.CreationTime.ToString()));
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    {
                        ReleaseCapture();
                        SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                    }
                    break;
            }
        }
    }

}
