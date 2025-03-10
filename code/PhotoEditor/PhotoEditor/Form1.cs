using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PhotoEditor
{
    public partial class Form1 : Form
    {
        private bool drawing;
        private Pen pen;
        private bool drawMode;
        private Color selectedColor;
        Bitmap map;
        Graphics graphics;
        private Bitmap adjustedImage;
        private Image bufImage;
        int brightness = 0;
        private Point startPoint;
        private Point endPoint;
        private bool isDragging = false;
        private bool flag = false;
        private string path;
        private bool isOpened;
        Bitmap temp;
        Graphics gr_temp;
        public Form1() 
        {
            InitializeComponent();
            //Додавання функції, що виконується при натисканні миші на pictureBox
            pictureBox1.MouseDown += new MouseEventHandler(pictureBox1_MouseDown);
            //Додавання функції, що виконується при відтисканні миші від pictureBox
            pictureBox1.MouseUp += new MouseEventHandler(pictureBox1_MouseUp);
            //Додавання функції, що виконується під час руху миші на pictureBox
            pictureBox1.MouseMove += new MouseEventHandler(pictureBox1_MouseMove);
            //Додавання функції, що виконується при кліку миші по pictureBox
            pictureBox1.MouseClick += new MouseEventHandler(pictureBox1_Click);

            drawing = false; //Прапорець, що вказує на поточний процес малювання (Під час малювання або стирання drawing = true)
            drawMode = false; //Прапорець режиму малювання (drawing = true означає, що можна малювати)
            textBox1.Text = "false"; //Встановлення тексту для поля, що вказує режим малювання
            selectedColor = Color.Black; //Встановлення кольору за замовчанням
            textBox2.Text = selectedColor.Name; //Встановлення тексту з назвою кольору
            pen = new Pen(Color.Black, 2); //Ініціалізація пензля
            trackBar1.Value = 2; //Встановлення ширини пензля за замовчанням
            textBox3.Text = "2"; //Встановлення тексту для поля, що показує ширину пензля
            
            //======Ініціалізація нового пустого зображення=======
            pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            setSize();
            pictureBox1.Image = map;
            //====================================================

            radioButton3.Checked = true; //Встановлення режиму малювання None за замовчанням
            //=====Режими малювання=============
            //Draw - звичайне малювання
            //Erase - стирання
            //Fill - заливка
            //None - жоден з режимів не обраний
            //Transparent fill - прозора заливка
            //==================================
            
            bufImage = pictureBox1.Image; //Буферне зображення (не відрізняється від оригіналу, використовується в деяких функціях)
            textBox4.Text = "0"; //Встановлення тексту для поля, що вказує на яскравість

            pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
            pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
        }

        private class arrayPoints //Клас для роботи з масивом точок (використовується при малюванні та стиранні)
        {
            private int index = 0; //Індекс, що вказує на останню точку
            private Point[] points; //Масив точок

            public arrayPoints(int size) //Конструктор класу
            {
                if (size <= 0)
                {
                    size = 2; //Встановлення розміру у значення 2, якщо початковий розмір менше нуля
                }

                points = new Point[size]; //Ініціалізація масиву точок
            }

            public void setPoint(int x, int y) //Функція додавання точки до масиву
            {
                if (index >= points.Length)
                {
                    index = 0; //Обнулення індексу, якщо його значення дорівнює розміру масиву, або перевищує його
                }

                points[index] = new Point(x, y); //Додавання точки до масиву
                index++; //Збільшення значення індексу
            }

            public void resetPoints() //Функція для обнулення індексу (використовується при завершенні малювання)
            {
                index = 0;
            }

            public int getCountPoints() //Функція, що повертає поточне значення індексу
            {
                return index;
            }

            public Point[] getPoints() //Функція, що повертає масив точок
            {
                return points;
            }
        }

        private arrayPoints arrPoints = new arrayPoints(2); //Створення та ініціалізація нового об'єкту класу arrPoints

        //Функція, що ініціалізує новий об'єкт класу Bitmap з пустим зображенням та пов'язує його з об'єктом класу Graphics
        private void setSize()
        {
            Rectangle rectangle = Screen.PrimaryScreen.Bounds; //Визначення робочої області розміром з pictureBox
            map = new Bitmap(rectangle.Width, rectangle.Height);
            graphics = Graphics.FromImage(map);
        }

        //Функція, що відкриває зображення з файлу
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Створення нового об'єкту класу OpenFileDialog, що буде використовуватися
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            //Фільтр з переліком можливих для відкриття форматів файлів
            openFileDialog1.Filter = "Image Files (*.png;*.jpg;*.jpeg;*.gif;*.bmp)|*.png;*.jpg;*.jpeg;*.gif;*.bmp|All files (*.*)|*.*";

            //Дії, якщо файл було успішно обрано
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Зміна режиму pictureBox для розтягування обраного зображення до розмірів робочої області
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                //Визначення розмірів робочої області
                Rectangle rectangle = Screen.PrimaryScreen.Bounds;
                //Створення об'єкту класу Bitmap з обраним файлом
                Bitmap original = new Bitmap(openFileDialog1.FileName);
                //Перенесення створеного Bitmap до основного Bitmap з іншим форматом
                map = new Bitmap(original.Width, original.Height, PixelFormat.Format32bppArgb);
                //Зв'язування graphics з map
                graphics = Graphics.FromImage(map);
                //Малювання обраного зображення на graphics
                graphics.DrawImage(original, 0, 0);
                //Перенесення зображення на pictureBox
                pictureBox1.Image = map;
                //Перенесення зображення в буфер (використовується в деяких функціях)
                bufImage = pictureBox1.Image;
                //Обнулення яскравості
                brightness = 0;
                //Встановлення прапорця, що показує чи відкрито файл
                isOpened = true;
            }
        }

        //Функція, що закриває поточне зображення
        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                pictureBox1.Image.Dispose(); //Очищення ресурсів pictureBox
            }

            pictureBox1.Image = null; //Видалення зображення
            bufImage = null; //Видалення буферного зображення
            brightness = 0; //Обнулення яскравості
            isOpened = false; //Обнулення прапорця, що показує чи відкрито файл
        }

        //Функція, що зберігає поточне зображення у файл
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.Filter = "PNG Image|*.png|JPEG Image|*.jpg;*.jpeg|BMP Image|*.bmp|GIF Image|*.gif";

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    string filePath = saveFileDialog1.FileName;

                    System.Drawing.Imaging.ImageFormat format = System.Drawing.Imaging.ImageFormat.Png;

                    string extension = System.IO.Path.GetExtension(filePath);
                    switch (extension.ToLower())
                    {
                        case ".jpg":
                            format = System.Drawing.Imaging.ImageFormat.Jpeg;
                            break;
                        case ".jpeg":
                            format = System.Drawing.Imaging.ImageFormat.Jpeg;
                            break;
                        case ".bmp":
                            format = System.Drawing.Imaging.ImageFormat.Bmp;
                            break;
                        case ".gif":
                            format = System.Drawing.Imaging.ImageFormat.Gif;
                            break;
                    }

                    pictureBox1.Image.Save(filePath, format);
                }
            }
        }

        //Функція, що створює нове пусте зображення
        private void createToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                pictureBox1.Image.Dispose();
            }

            pictureBox1.SizeMode = PictureBoxSizeMode.Normal;
            setSize();
            pictureBox1.Image = map;
            bufImage = pictureBox1.Image;
            brightness = 0;
            isOpened = false;
        }

        //Функція, що виконується при натисканні миші на pictureBox
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (!flag)
            {
                if (drawMode)
                {
                    drawing = true;
                }
            }
            else
            {
                isDragging = true;
                startPoint = e.Location;
            }
        }
        
        //Функція, що виконується при відтисканні миші від pictureBox
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (!isDragging)
            {
                drawing = false;
                arrPoints.resetPoints();
            }
            else
            {
                isDragging = false;
                flag = false;
                InsertImageInArea();
            }

            if (pictureBox1.Image != null)
            {
                if (!isOpened)
                {
                    ReplaceColor(map, Color.FromArgb(255, 255, 255, 254), Color.FromArgb(0, 0, 0, 0));
                }
                else
                {
                    ReplaceColor(map, Color.FromArgb(255, 255, 255, 254), Color.FromArgb(0, 0, 0, 0));
                }

                pictureBox1.Image = map;
                bufImage = pictureBox1.Image;
                pictureBox1.Image = bufImage;
                adjustedImage = new Bitmap(pictureBox1.Image.Width, pictureBox1.Image.Height);
                AdjustBrightness(adjustedImage, brightness / 10.0f, true);
                pictureBox1.Image = adjustedImage;
            }
        }

        //Функція, що виконується під час руху миші по pictureBox
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                if (e.X < map.Width && e.X > 0 && e.Y < map.Height && e.Y > 0)
                {
                    if (!isDragging)
                    {
                        if (!drawing)
                        {
                            return;
                        }

                        if (drawing && drawMode && pictureBox1.Image != null)
                        {
                            Point p;
                            if (pictureBox1.SizeMode != PictureBoxSizeMode.StretchImage)
                            {
                                p = new Point(e.X, e.Y);
                            }
                            else
                            {
                                p = PointToImageCoordinates(new Point(e.X, e.Y));
                            }
                            arrPoints.setPoint(p.X, p.Y);
                            if (arrPoints.getCountPoints() >= 2)
                            {
                                graphics.DrawLines(pen, arrPoints.getPoints());
                                pictureBox1.Image = map;
                                bufImage = pictureBox1.Image;
                                arrPoints.setPoint(p.X, p.Y);
                            }
                        }
                    }
                    else
                    {
                        endPoint = e.Location;
                        //====================
                        Rectangle rectangle = Screen.PrimaryScreen.Bounds;
                        temp = new Bitmap(map);
                        gr_temp = Graphics.FromImage(temp);
                        Bitmap newImage = new Bitmap(path);

                        Point pt = new Point(Math.Min(startPoint.X, endPoint.X), Math.Min(startPoint.Y, endPoint.Y));
                        if (isOpened)
                        {
                            pt = PointToImageCoordinates(pt);
                        }

                        int x = pt.X;
                        int y = pt.Y;
                        int width = Math.Abs(startPoint.X - endPoint.X);
                        int height = Math.Abs(startPoint.Y - endPoint.Y);
                        Rectangle insertArea = new Rectangle(x, y, width, height);
                        gr_temp.DrawImage(newImage, insertArea);
                        pictureBox1.Image = temp;
                        //====================
                        pictureBox1.Invalidate();
                    }
                }
            }
        }

        //Функція, що виконується при натисканні кнопки вибору кольору
        private void button2_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.Color = selectedColor;

            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                selectedColor = colorDialog.Color;
                textBox2.Text = selectedColor.Name;
                pen.Color = selectedColor;
            }
        }

        private Point PointToImageCoordinates(Point controlCoordinates) //Функція розрахунку координат для малювання
        {
            float imgPercentX = (float)pictureBox1.Image.Width / 100;
            float imgPercentY = (float)pictureBox1.Image.Height / 100;
            float picPercentX = (float)pictureBox1.Width / 100;
            float picPercentY = (float)pictureBox1.Height / 100;
            float controlPercentsX = (float)controlCoordinates.X / picPercentX;
            float controlPercentsY = (float)controlCoordinates.Y / picPercentY;
            float imageX = imgPercentX * controlPercentsX;
            float imageY = imgPercentY * controlPercentsY;
            return new Point((int)imageX, (int)imageY);
        }

        private void trackBar1_Scroll(object sender, EventArgs e) //Функція зміни розміру пензля
        {
            textBox3.Text = (trackBar1.Value).ToString();
            pen.Width = trackBar1.Value;
        }

        //Функція вибору режиму малювання
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            drawMode = true;
            textBox1.Text = "Draw";
            selectedColor = Color.Black;
            pen.Color = selectedColor;
        }

        //Функція вибору режиму стирання
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            drawMode = true;
            textBox1.Text = "Erase";
            selectedColor = Color.FromArgb(255, 255, 255, 254);
            pen.Color = selectedColor;
        }

        //Функція скасування вибору режиму
        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            drawMode = false;
            textBox1.Text = "None";
        }

        //Функція для зміни яскравості зображення
        private void AdjustBrightness(Bitmap image, float value, bool flag)
        {
            using (Graphics g = Graphics.FromImage(image))
            {
                //Матриця кольорів для зміни значень RGB
                ColorMatrix cm = new ColorMatrix(new float[][]
                {
                    new float[] { 1 + value, 0, 0, 0, 0 },
                    new float[] {0, 1 + value, 0, 0, 0},
                    new float[] {0, 0, 1 + value, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] { 0, 0, 0, 0, 1}
                });

                ImageAttributes attributes = new ImageAttributes();
                attributes.SetColorMatrix(cm);

                g.DrawImage(pictureBox1.Image, new Rectangle(0, 0, pictureBox1.Image.Width, pictureBox1.Image.Height),
                    0, 0, pictureBox1.Image.Width, pictureBox1.Image.Height, GraphicsUnit.Pixel, attributes);
            }
        }

        //Функція, що виконується при зменшенні яскравості
        private void button1_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                pictureBox1.Image = bufImage;
                adjustedImage = new Bitmap(pictureBox1.Image.Width, pictureBox1.Image.Height);
                brightness -= 1;
                textBox4.Text = brightness.ToString();
                AdjustBrightness(adjustedImage, brightness / 10.0f, false);
                pictureBox1.Image = adjustedImage;
            }
        }

        //Функція, що виконується при збільшенні яскравості
        private void button3_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                pictureBox1.Image = bufImage;
                adjustedImage = new Bitmap(pictureBox1.Image.Width, pictureBox1.Image.Height);
                brightness += 1;
                textBox4.Text = brightness.ToString();
                AdjustBrightness(adjustedImage, brightness / 10.0f, true);
                pictureBox1.Image = adjustedImage;
            }
        }

        //Функція, що виконується під час заливки
        private void pictureBox1_Click(object sender, MouseEventArgs e)
        {
            if (textBox1.Text == "Fill" && pictureBox1.Image != null)
            {
                Point pt;
                if (isOpened)
                {
                    pt = PointToImageCoordinates(e.Location);
                }
                else
                {
                    pt = e.Location;
                }  
                
                Color targetColor = map.GetPixel(pt.X, pt.Y);
                Color fillColor = selectedColor;
                FloodFill(map, pt, targetColor, fillColor);
                pictureBox1.Image = map;
                bufImage = pictureBox1.Image;
            }
        }

        //Функція, заливки певної області
        private void FloodFill(Bitmap bmp, Point pt, Color targetColor, Color fillColor)
        {
            if (pictureBox1.Image != null)
            {
                Queue<Point> q = new Queue<Point>();
                q.Enqueue(pt);

                while (q.Count > 0)
                {
                    Point n = q.Dequeue();
                    if (bmp.GetPixel(n.X, n.Y) != targetColor)
                    {
                        continue;
                    }

                    Point w = n, e = new Point(n.X + 1, n.Y);
                    while ((w.X > 0) && (bmp.GetPixel(w.X, w.Y) == targetColor))
                    {
                        bmp.SetPixel(w.X, w.Y, fillColor);
                        if ((w.Y > 0) && (bmp.GetPixel(w.X, w.Y - 1) == targetColor))
                        {
                            q.Enqueue(new Point(w.X, w.Y - 1));
                        }

                        if ((w.Y < bmp.Height - 1) && (bmp.GetPixel(w.X, w.Y + 1) == targetColor))
                        {
                            q.Enqueue(new Point(w.X, w.Y + 1));
                        }

                        w.X--;
                    }
                    while ((e.X < bmp.Width) && (bmp.GetPixel(e.X, e.Y) == targetColor))
                    {
                        bmp.SetPixel(e.X, e.Y, fillColor);
                        if ((e.Y > 0) && (bmp.GetPixel(e.X, e.Y - 1) == targetColor))
                        {
                            q.Enqueue(new Point(e.X, e.Y - 1));
                        }

                        if ((e.Y < bmp.Height - 1) && (bmp.GetPixel(e.X, e.Y + 1) == targetColor))
                        {
                            q.Enqueue(new Point(e.X, e.Y + 1));
                        }

                        e.X++;
                    }
                }
            }
        }

        //Функція вибору режиму заливки
        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            drawMode = false;
            textBox1.Text = "Fill";
            selectedColor = Color.Black;
            textBox2.Text = "Black";
            pen.Color = selectedColor;
        }

        //Функція вставки зображення
        private void InsertImageInArea()
        {
            Bitmap newImage = new Bitmap(path);

            Point pt = new Point(Math.Min(startPoint.X, endPoint.X), Math.Min(startPoint.Y, endPoint.Y));
            if (isOpened)
            {
                pt = PointToImageCoordinates(pt);
            }

            int x = pt.X;
            int y = pt.Y;
            int width = Math.Abs(startPoint.X - endPoint.X);
            int height = Math.Abs(startPoint.Y - endPoint.Y);
            Rectangle insertArea = new Rectangle(x, y, width, height);

            using (Graphics g = Graphics.FromImage(map))
            {
                g.DrawImage(newImage, insertArea);
            }

            pictureBox1.Image = map;
            bufImage = pictureBox1.Image;
        }
        
        //Функція вибору зображення для вставки
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            radioButton3.Checked = true;
            flag = true;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.Filter = "Image Files (*.png;*.jpg;*.jpeg;*.gif;*.bmp)|*.png;*.jpg;*.jpeg;*.gif;*.bmp|All files (*.*)|*.*";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                path = openFileDialog1.FileName;
            }
        }
        
        //Функція заміни кольору
        private void ReplaceColor(Bitmap bmp, Color targetColor, Color replacementColor)
        {
            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    if (bmp.GetPixel(x, y) == targetColor)
                    {
                        bmp.SetPixel(x, y, replacementColor);
                    }
                }
            }
        }

        //Функція вибору режиму прозорої заливки
        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            drawMode = false;
            textBox1.Text = "Fill";
            pen.Color = selectedColor = Color.FromArgb(255, 255, 255, 254);
        }
    }
}
