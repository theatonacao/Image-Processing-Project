using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CS162_Project_1___Tonacao
{
    public partial class Form1 : Form
    {

        //global variables
        string path, compressedpath; // contain directory path of the uploaded PCX file
        Bitmap image, orig_image;
        Bitmap image_noise;

        public Form1()
        {
            InitializeComponent();

        }

        private void clearDisplay()
        {
            //clear display
            chart1.Series.Clear();
            pictureBox3.Image = null;
            pictureBox4.Image = null;
            pictureBox5.Image = null;
            pictureBox6.Image = null;
            chart1.ChartAreas[0].RecalculateAxesScale();
            chart1.BackColor = Color.Transparent;
            textBox3.Text = " ";
            textBox4.Text = " ";
            textBox5.Text = " ";
            textBox6.Text = " ";
            textBox7.Text = " ";
            textBox8.Text = " ";
        }
        //this function will load the pcx file into the program and display PCX header info
        private void button1_Click(object sender, EventArgs e)
        {
            clearDisplay();

            //set path the location of the pcx file
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "pcx files (*.pcx)|*.pcx| All files (*.*)|*.*";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                path = openFileDialog1.FileName;
                PCXinfo();
               
            }
            else//if file unsuccesfully loaded
            {
                MessageBox.Show("Error opening file");
            }
        }
        //this function will display PCX info and palette
        private void PCXinfo()
        {
                int readByte = 0;
                Stream input = File.OpenRead(path);


                //display header information while reading bytes of pcx file   
                textBox1.Multiline = true;
                textBox1.ReadOnly = true;
                textBox1.Text = "PCX Header Information\r\n";
                //Reading byte by byte in the PCX file
                readByte = input.ReadByte();
                textBox1.Text += "\r\nManufacturer: Zsoft.pcx(" + readByte + ")";
                readByte = input.ReadByte();
                textBox1.Text += "\r\nVersion: " + readByte;
                readByte = input.ReadByte();
                textBox1.Text += "\r\nEncoding: " + readByte;
                readByte = input.ReadByte();
                textBox1.Text += "\r\nBits Per Pixel: " + readByte;
                
                textBox1.Text += "\r\nImage Dimensions:  ";

                for (int i = 0; i < 4; i++)
                {
                    readByte = input.ReadByte() + input.ReadByte();
                    if (i <= 2)
                    {
                    textBox1.Text += readByte +" ";
                    }
                    else
                    {
                    textBox1.Text += readByte + " ";
                    }
                }
                readByte = input.ReadByte() + input.ReadByte();
                textBox1.Text += "\r\nHDPI: " + readByte;
                readByte = input.ReadByte() + input.ReadByte();
                textBox1.Text += "\r\nVDPI: " + readByte;
                for (int i = 16; i < 64; i++)
                {
                    //Color Palette Settings
                    readByte = input.ReadByte();
                }
                //reserved skip
                readByte = input.ReadByte();

                //Continue
                readByte = input.ReadByte();
                textBox1.Text += "\r\nNumber of Color Planes: " + readByte;
                readByte = input.ReadByte();
                readByte = input.ReadByte() << 8 | readByte;
                textBox1.Text += "\r\nBytes per Line: " + readByte;
                readByte = input.ReadByte() + input.ReadByte();
                textBox1.Text += "\r\nPalette Information: " + readByte;
                readByte = input.ReadByte() + input.ReadByte();
                textBox1.Text += "\r\nHorizontal Screen Size :" + readByte;
                readByte = input.ReadByte() + input.ReadByte();
                textBox1.Text += "\r\nVertical Screen Size:" + readByte;

                //Store all bytes of PCX file to access color palette at the end
                byte[] PCXBytes = File.ReadAllBytes(path);
                List<Color> ColorPalette = new List<Color>();
                bool ColorPaletteEmpty = true;
                if (PCXBytes.Length > 768)
                {
                   
                        ColorPaletteEmpty = false;
                        for (int i = PCXBytes.Length - 768; i < PCXBytes.Length; i += 3)
                        {
                            ColorPalette.Add(Color.FromArgb(PCXBytes[i], PCXBytes[i + 1], PCXBytes[i + 2]));
                        }
                    
                }

                 //display image of pcx file

                Color[] savepalette = new Color[256 * 256];
                List<byte> palettevalue = new List<byte>();
                int pos = 128;
                byte runcount = 0;
                byte runvalue = 0;
                do
                {
                    byte Byte = PCXBytes[pos++];
                    //if 2 Byte code
                    if ((Byte & 0xC0) == 0xC0 && pos < PCXBytes.Length)
                    {
                        //Get runcount and pixel value
                        runcount = (byte)(Byte & 0x3F);
                        runvalue = PCXBytes[pos++];
                    }
                    //if 1 Byte code
                    else
                    {
                        //Run count is one and get pixel value
                        runcount = 1;
                        runvalue = Byte;
                    }
                    for (int j = 0; j < runcount; j++)
                    {
                        //Image byte value for the image
                        palettevalue.Add(runvalue);
                    }
                } while (pos < PCXBytes.Length);

                //Creating the image using the information retrieved
                Bitmap printpalette = new Bitmap(256, 256);
                if (!ColorPaletteEmpty)
                {
                    for (int i = 0; i < 256 * 256; i++)
                    {
                        savepalette[i] = ColorPalette[palettevalue[i]];
                        int y = i / 256;
                        int x = i - (256 * y);
                        printpalette.SetPixel(x, y, savepalette[i]);
                    }
                }
                textBox2.Text = "Original Image ";
                 textBox3.Text = "Original Image ";
                  image = new Bitmap(printpalette);
                  orig_image = image;
                pictureBox1.Image = image;
                pictureBox3.Image = image;

            //Creating the color palette map from the set of colors
            Bitmap ColorMap = new Bitmap(128, 128);
                int z = 0;
                for (int i = 0; i < 80; i = i + 5)
                {
                    for (int j = 0; j < 80; j = j + 5)
                    {
                        using (Graphics gfx = Graphics.FromImage(ColorMap))
                        using (SolidBrush brush = new SolidBrush(ColorPalette[z]))
                        {
                            gfx.FillRectangle(brush, i, j, 5, 5);
                        }
                        z++;
                    }

                }
                pictureBox2.Image = new Bitmap(ColorMap);
            }

        //trigger button for red channel
        private void button2_Click(object sender, EventArgs e)
        {
            int color = 1;//red
            RGBchannels(color);
        }

        //trigger button for green channel
        private void button3_Click(object sender, EventArgs e)
        {
            int color = 2;// green
            RGBchannels(color);
        }
        //trigger button for blue channel
        private void button4_Click(object sender, EventArgs e)
        {
            int color = 3;//blue
            RGBchannels(color);
        }
        double[,] red, green, blue;

       
        private void RGBchannels(int color)
        {
            clearDisplay();

            textBox8.Text = "RGB Channels with Histogram";
            Bitmap bm = new Bitmap(image);
           
           

            chart1.Series.Clear();
            chart1.Series.Add("Pixels");
            chart1.ChartAreas[0].RecalculateAxesScale();


           int w = bm.Width;
           int h = bm.Height;

            Bitmap bpmImgR = new Bitmap(256, 256);
            Bitmap bpmImgG = new Bitmap(256, 256);
            Bitmap bpmImgB = new Bitmap(256, 256);

            red = new double[w, h];
            green = new double[w, h];
            blue = new double[w, h];

            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < 256; j++)
                {
                    red[i, j] = bm.GetPixel(i, j).R;
                    green[i, j] = bm.GetPixel(i, j).G;
                    blue[i, j] = bm.GetPixel(i, j).B;
                }
            }
            bpmImgR = new Bitmap(w, h);
            bpmImgG = new Bitmap(w, h);
            bpmImgB = new Bitmap(w, h);

            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    bpmImgR.SetPixel(i, j, Color.FromArgb((int)red[i, j], 0, 0));
                    bpmImgG.SetPixel(i, j, Color.FromArgb(0, (int)green[i, j], 0));
                    bpmImgB.SetPixel(i, j, Color.FromArgb(0, 0, (int)blue[i, j]));
                }
            }

            int[] his_R = new int[h];
            int[] his_G = new int[h];
            int[] his_B = new int[h];

            Histogram(w, h, red, ref his_R);
            Histogram(w, h, green, ref his_G);
            Histogram(w, h, blue, ref his_B);


            if (color == 1)
            {
                textBox3.Text = "Red Channel";
                textBox4.Text = "Red Channel Histogram";
                pictureBox3.Image = bpmImgR;

                for (int i = 0; i < h; i++)
                {
                    chart1.Series["Pixels"].Points.AddXY(i + 1, his_R[i]);
                }

            }
            else if (color == 2)
            {
                textBox3.Text = "Green Channel";
                textBox4.Text = "Green Channel Histogram";
                pictureBox3.Image = bpmImgG;

                for (int i = 0; i < h; i++)
                {
                    chart1.Series["Pixels"].Points.AddXY(i + 1, his_G[i]);
                }

            }
            else if (color == 3)
            {
                textBox3.Text = "Blue Channel";
                textBox4.Text = "Blue Channel Histogram";
                pictureBox3.Image = bpmImgB;

                for (int i = 0; i < h; i++)
                {
                    chart1.Series["Pixels"].Points.AddXY(i + 1, his_B[i]);
                }
            }
            chart1.Visible = true;
        }


        public void Histogram(int width, int height, double[,] I, ref int[] His)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (I[i, j] > 255) I[i, j] = 255;
                    if (I[i, j] < 0) I[i, j] = 0;

                    His[(int)(I[i, j])]++;
                }
            }
        }


        //displays the black and white threshold
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            clearDisplay();

            Bitmap bmp = new Bitmap(image);
            int w = bmp.Width;
            int h = bmp.Height;

            //Set min, max, and center k
            int min = 0;
            int max = 255;
            float k = trackBar1.Value;
            label6.Text = k.ToString();

            //Black and White
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    //get pixel value
                    Color p = bmp.GetPixel(x, y);

                    //extract ARGB value
                    int a = p.A;
                    int r = p.R;
                    int g = p.G;
                    int b = p.B;

                    //grayscale function
                    int gray = (r + g + b) / 3;

                    //black and white
                    if (gray > k)
                        gray = max;
                    else
                        gray = min;

                    //set pixel value
                    bmp.SetPixel(x, y, Color.FromArgb(a, gray, gray, gray));
                }
            }

            //display image
            textBox3.Text = "Black And White Threshold";

            pictureBox3.Image = bmp;
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {

            clearDisplay();

            Bitmap bmp = new Bitmap(image);
            int w = bmp.Width;
            int h = bmp.Height;

            //Set min, max, and gamma k
            int max = 255;
            int k = trackBar2.Value;
            label7.Text = k.ToString();

            int s = 0;

            //Set constant c value
            int c = 1;

            //Gamma
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    //get pixel value
                    Color img = bmp.GetPixel(x, y);

                    //extract ARGB value
                    int a = img.A;
                    int r = img.R;
                    int g = img.G;
                    int b = img.B;

                    //grayscale function
                    int gray = (r + g + b) / 3;

                    //gamma
                    s = c * (gray ^ k);

                    if (s > max)
                        s = max;

                    //set pixel value
                    bmp.SetPixel(x, y, Color.FromArgb(a, s, s, s));
                }
            }

            //display image
            textBox3.Text = "Power Law Gamma+" ;

            pictureBox3.Image = bmp;
        }


        //grayscale transformation of the input image
        private void button5_Click(object sender, EventArgs e)
        {
            grayscale();

        }
        private void grayscale()
        {
            Bitmap bmp = new Bitmap(image);
            clearDisplay();
            int w = bmp.Width;
            int h = bmp.Height;

            //color pixel
            Color p;

            for (int x = 0; x < h; x++)
            {
                for (int y = 0; y < w; y++)
                {
                    //get pixel value
                    p = bmp.GetPixel(x, y);

                    //extract pixel component ARGB
                    int a = p.A;
                    int r = p.R;
                    int g = p.G;
                    int b = p.B;

                    //find average (transformation function)
                    int avg = (r + g + b) / 3;

                    //set new pixel value
                    bmp.SetPixel(x, y, Color.FromArgb(a, avg, avg, avg));
                }
            }

            //display image
            textBox3.Text = "Grayscale Transformation";
           
            pictureBox3.Image = bmp;
        }

        //negative transformation of the input image
        private void button6_Click(object sender, EventArgs e)
        {

            Bitmap bmp = new Bitmap(image);
            clearDisplay();
            int w = bmp.Width;
            int h = bmp.Height;
          
            //color pixel
            Color p;

            for (int x = 0; x < h; x++)
            {
                for (int y = 0; y < w; y++)
                {
                    //get pixel value
                    p = bmp.GetPixel(x, y);

                    //extract pixel component ARGB
                    int a = p.A;
                    int r = p.R;
                    int g = p.G;
                    int b = p.B;

                    //negatives of RGB (transformation function)
                    r = 255 - r;
                    g = 255 - g;
                    b = 255 - b;

                    //set new pixel value
                    bmp.SetPixel(x, y, Color.FromArgb(a, r, g, b));
                }
            }

            //display image
            textBox3.Text = "Negative of Image";

            pictureBox3.Image = bmp;
        }

       // lowpass filter
        private void button7_Click(object sender, EventArgs e)
        {
            clearDisplay();
            grayscale();
            avefilter();
            medianfilter();
            textBox8.Text = "Low Pass Filtering";

        }
        // highpass filter
        private void button8_Click(object sender, EventArgs e)
        {
            clearDisplay();
            grayscale();
            unsharp();
            highboost();
            laplacian();
            textBox8.Text = "High Pass Filtering";
        }



        //avergaing filter
        private void avefilter()
        {
            Bitmap bmp = new Bitmap(image_noise);

            int w = bmp.Width;
            int h = bmp.Height;

            Bitmap temp = new Bitmap(256, 256);
            Color c;
            float sum = 0;
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    c = bmp.GetPixel(i, j);
                    byte gray = (byte)(.333 * c.R + .333 * c.G + .333 * c.B);
                    bmp.SetPixel(i, j, Color.FromArgb(gray, gray, gray));
                }
            }

            temp = bmp;

            for (int i = 0; i <= w - 3; i++)
                for (int j = 0; j <= h - 3; j++)
                {
                    for (int x = i; x <= i + 2; x++)
                        for (int y = j; y <= j + 2; y++)
                        {
                            c = bmp.GetPixel(x, y);
                            sum = sum + c.R;
                        }
                    int color = (int)Math.Round(sum / 9, 10);
                    temp.SetPixel(i + 1, j + 1, Color.FromArgb(color, color, color));
                    sum = 0;
                }
            bmp = temp;
            //display image
            textBox5.Text = "Average Filter";

            pictureBox4.Image = bmp;
        }

        //median filter
        private void medianfilter()
        {
            Bitmap median = new Bitmap(image_noise);

            List<byte> termsList = new List<byte>();

            byte[,] img = new byte[median.Width, median.Height];

            //Get value
            for (int a = 0; a < median.Width; a++)
            {
                for (int b = 0; b < median.Height; b++)
                {
                    var c = median.GetPixel(a, b);
                    byte g = (byte)(c.R);
                    img[a, b] = g;
                }
            }

            //applying Median Filtering 
            for (int i = 0; i <= median.Width - 3; i++)
                for (int j = 0; j <= median.Height - 3; j++)
                {
                    for (int x = i; x <= i + 2; x++)
                        for (int y = j; y <= j + 2; y++)
                        {
                            termsList.Add(img[x, y]);
                        }
                    byte[] terms = termsList.ToArray();
                    termsList.Clear();
                    Array.Sort<byte>(terms);
                    Array.Reverse(terms);
                    byte color = terms[4];
                    median.SetPixel(i + 1, j + 1, Color.FromArgb(color, color, color));
                }
          
            //display image
            textBox6.Text = "Median Square Filter";

            pictureBox5.Image = median;
        }

        //unsharp filter
        private void unsharp()
        {
            Bitmap unsharp = new Bitmap(image_noise);

            int w = image.Width - 1;
            int h = image.Height - 1;

            for (int i = 1; i < w; i++)
            {
                for (int j = 1; j < h; j++)
                {
                    int originalPixelVal = 0;
                    Color c1, c2, c3, c4, c5, c6, c7, c8, c9;

                    c1 = image_noise.GetPixel(i - 1, j - 1);
                    c2 = image_noise.GetPixel(i, j - 1);
                    c3 = image_noise.GetPixel(i + 1, j - 1);
                    c4 = image_noise.GetPixel(i - 1, j);
                    c5 = image_noise.GetPixel(i, j);
                    c6 = image_noise.GetPixel(i + 1, j);
                    c7 = image_noise.GetPixel(i - 1, j + 1);
                    c8 = image_noise.GetPixel(i, j + 1);
                    c9 = image_noise.GetPixel(i + 1, j + 1);


                    int cross = (c4.R + c2.R + c8.R + c6.R) * (-1);
                    int edge = (c1.R + c3.R + c7.R + c9.R) * (-1);
                    int middle = (9) * c5.R;

                    int unsharpVal = originalPixelVal + edge + middle + cross;
                    if (unsharpVal > 255) unsharpVal = 255;
                    if (unsharpVal < 0) unsharpVal = 0;

                    Color grayC = Color.FromArgb(unsharpVal, unsharpVal, unsharpVal);
                    unsharp.SetPixel(i, j, grayC);
                }
            }

            //display image
            textBox5.Text = "Unsharp Masking";

            pictureBox4.Image = unsharp;
        }

        //highboost filter
        private void highboost()
        {
            int boostValue = 2;
            Bitmap highboost = new Bitmap(image_noise);

            int x = image_noise.Width - 1;
            int y = image_noise.Height - 1;

            for (int a = 1; a < x; a++)
            {
                for (int b = 1; b < y; b++)
                {
                    Color c1, c2, c3, c4, c5, c6, c7, c8, c9;

                    c1 = image_noise.GetPixel(a - 1, b - 1);
                    c2 = image_noise.GetPixel(a, b - 1);
                    c3 = image_noise.GetPixel(a + 1, b - 1);
                    c4 = image_noise.GetPixel(a - 1, b);
                    c5 = image_noise.GetPixel(a, b);
                    c6 = image_noise.GetPixel(a + 1, b);
                    c7 = image_noise.GetPixel(a - 1, b + 1);
                    c8 = image_noise.GetPixel(a, b + 1);
                    c9 = image_noise.GetPixel(a + 1, b + 1);

                    int originalPixelVal = 0;
                    int cross = (c4.R + c2.R + c8.R + c6.R) * (-1);
                    int edge = (c1.R + c3.R + c7.R + c9.R) * (-1);
                    int middle = (boostValue + 8) * c5.R;
                    int boost = originalPixelVal + edge + middle + cross;

                    if (boost > 255) boost = 255;
                    if (boost < 0) boost = 0;

                    Color grayC = Color.FromArgb(boost, boost, boost);
                    highboost.SetPixel(a, b, grayC);
                }
            }

            //display image
            textBox6.Text = "Highboost filtering K = 2";

            pictureBox5.Image = highboost;
        }

        //laplacian filter
        private void laplacian()
        {
           
            Bitmap laplacian = new Bitmap(image_noise);

            int width = image_noise.Width - 1;
            int height = image_noise.Height - 1;

            for (int i = 1; i < width; i++)
            {
                for (int j = 1; j < height; j++)
                {
                    Color color2, color4, color5, color6, color8;
                    color2 = image_noise.GetPixel(i, j - 1);
                    color4 = image_noise.GetPixel(i - 1, j);
                    color5 = image_noise.GetPixel(i, j);
                    color6 = image_noise.GetPixel(i + 1, j);
                    color8 = image_noise.GetPixel(i, j + 1);

                    int colorR = color2.R + color4.R + color5.R * (-4) + color6.R + color8.R;
                    int colorG = color2.G + color4.G + color5.G * (-4) + color6.G + color8.G;
                    int colorB = color2.B + color4.B + color5.B * (-4) + color6.B + color8.B;

                    int avr = ((colorR + colorG + colorB) / 3);
                    if (avr > 255) avr = 255;
                    if (avr < 0) avr = 0;

                    laplacian.SetPixel(i, j, Color.FromArgb(avr, avr, avr));

                }
            }

            //display image
            textBox7.Text = "Highpass with Laplacian Operator";

            pictureBox6.Image = laplacian;
        }

        //button for gradient filter 
        private void button9_Click(object sender, EventArgs e)
        {
            clearDisplay();
            grayscale();
            sobel();
        }


        //sobel gradient filter
        private void sobel()
        {

            Bitmap sobelX = new Bitmap(image_noise);
            Bitmap sobelY = new Bitmap(image_noise);
            Bitmap sobelXY = new Bitmap(image_noise);

            for (int a = 1; a < image_noise.Width - 1; a++)
            {
                for (int b = 1; b < image_noise.Height - 1; b++)
                {
                    Color c1, c2, c3, c4, c5, c6, c7, c8, c9;

                    c1 = image_noise.GetPixel(a - 1, b - 1);
                    c2 = image_noise.GetPixel(a, b - 1);
                    c3 = image_noise.GetPixel(a + 1, b - 1);
                    c4 = image_noise.GetPixel(a - 1, b);
                    c5 = image_noise.GetPixel(a, b);
                    c6 = image_noise.GetPixel(a + 1, b);
                    c7 = image_noise.GetPixel(a - 1, b + 1);
                    c8 = image_noise.GetPixel(a, b + 1);
                    c9 = image_noise.GetPixel(a + 1, b + 1);

                    int x_c1 = (c1.R * -1) + (c2.R * -2) + (c3.R * -1);
                    int x_c2 = (c4.R * 0) + (c5.R * 0) + (c6.R * 0);
                    int x_c3 = (c7.R * 1) + (c8.R * 2) + (c9.R * 1);
                    int x = x_c1 + x_c2 + x_c3;

                    int y_r1 = (c1.R * -1) + (c4.R * -2) + (c7.R * -1);
                    int y_r2 = (c2.R * 0) + (c5.R * 0) + (c8.R * 0);
                    int y_r3 = (c3.R * 1) + (c6.R * 2) + (c9.R * 1);
                    int y = y_r1 + y_r2 + y_r3;

                    double xxyy = Math.Sqrt((x * x) + (y * y));
                    int xy = (int)xxyy;

                    if (x < 0)  {
                        x = 0;
                    }else if (x > 255)   {
                        x = 255;
                    }

                    if (y < 0) {
                        y = 0;
                    }else if (y > 255)  {
                        y = 255;
                    }

                    if (xy < 0)  {
                        xy = 0;
                    }else if (xy > 255){
                        xy = 255;
                    }

                    Color grayX = Color.FromArgb(x, x, x);
                    sobelX.SetPixel(a, b, grayX);

                    Color grayY = Color.FromArgb(y, y, y);
                    sobelY.SetPixel(a, b, grayY);

                    Color grayXY = Color.FromArgb(xy, xy, xy);
                    sobelXY.SetPixel(a, b, grayXY);
                }
            }

            pictureBox4.Image = sobelXY;
            pictureBox5.Image = sobelX;
            pictureBox6.Image = sobelY;

            textBox5.Text = "Gradient: Sobel - XY";
            textBox6.Text = "Gradient: Sobel - X";
            textBox7.Text = "Gradient: Sobel - Y";
            textBox8.Text = "Gradient: Sobel";
        }

   
        //gaussian noise button
        private void button10_Click(object sender, EventArgs e)
        {
            clearDisplay();
         
            gaussian();
        }

        //original image restore button
        private void button12_Click(object sender, EventArgs e)
        {
            clearDisplay();
            textBox3.Text = "Original Image ";
            image = orig_image;
            pictureBox1.Image = image;
            pictureBox3.Image = image;


        }

        private void button13_Click(object sender, EventArgs e)
        {
            clearDisplay();
            exponential();

        }

        //salt and pepper button
        private void button11_Click(object sender, EventArgs e)
        {

            clearDisplay();
           
            saltandpepper();
        }

        //gaussian noise
        private void gaussian()
        {
            image = orig_image;
            image_noise = image;

            int w = image_noise.Width;
            int h = image_noise.Height;

            BitmapData image_data = image_noise.LockBits(
                new Rectangle(0, 0, w, h),
                ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);

            int bytes = image_data.Stride * image_data.Height;

            byte[] buffer = new byte[bytes];
            byte[] result = new byte[bytes];
            Marshal.Copy(image_data.Scan0, buffer, 0, bytes);
            image_noise.UnlockBits(image_data);

            byte[] noise = new byte[bytes];
            double[] gaussian = new double[256];
            int std = 20;
            Random rnd = new Random();
            double sum = 0;
            for (int i = 0; i < 256; i++)
            {
                gaussian[i] = (double)((1 / (Math.Sqrt(2 * Math.PI) * std)) * Math.Exp(-Math.Pow(i, 2) / (2 * Math.Pow(std, 2))));
                sum += gaussian[i];
            }

            for (int i = 0; i < 256; i++)
            {
                gaussian[i] /= sum;
                gaussian[i] *= bytes;
                gaussian[i] = (int)Math.Floor(gaussian[i]);
            }

            int count = 0;
            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < (int)gaussian[i]; j++)
                {
                    noise[j + count] = (byte)i;
                }
                count += (int)gaussian[i];
            }

            for (int i = 0; i < bytes - count; i++)
            {
                noise[count + i] = 0;
            }

            noise = noise.OrderBy(x => rnd.Next()).ToArray();

            for (int i = 0; i < bytes; i++)
            {
                result[i] = (byte)(buffer[i] + noise[i]);
            }

            Bitmap result_image = new Bitmap(w, h);
            BitmapData result_data = result_image.LockBits(
                new Rectangle(0, 0, w, h),
                ImageLockMode.WriteOnly,
                PixelFormat.Format24bppRgb);
            Marshal.Copy(result, 0, result_data.Scan0, bytes);
            result_image.UnlockBits(result_data);

            image_noise = result_image;
            image = result_image;
            grayscale();
            textBox3.Text = "Gaussian Noise";

        }
        //salt and pepper function
        private void saltandpepper()
        {
            image = orig_image;
            image_noise = image;

            int w = image_noise.Width;
            int h = image_noise.Height;

            BitmapData image_data = image_noise.LockBits(
                new Rectangle(0, 0, w, h),
                ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);
            int bytes = image_data.Stride * image_data.Height;
            byte[] buffer = new byte[bytes];
            byte[] result = new byte[bytes];
            Marshal.Copy(image_data.Scan0, buffer, 0, bytes);
            image_noise.UnlockBits(image_data);

            Random rnd = new Random();
            int noise_chance = 10;
            for (int i = 0; i < bytes; i += 3)
            {
                int max = (int)(1000 / noise_chance);
                int tmp = rnd.Next(max + 1);
                for (int j = 0; j < 3; j++)
                {
                    if (tmp == 0 || tmp == max)
                    {
                        int sorp = tmp / max;
                        result[i + j] = (byte)(sorp * 255);
                    }
                    else
                    {
                        result[i + j] = buffer[i + j];
                    }
                }
            }

            Bitmap result_image = new Bitmap(w, h);
            BitmapData result_data = result_image.LockBits(
                new Rectangle(0, 0, w, h),
                ImageLockMode.WriteOnly,
                PixelFormat.Format24bppRgb);
            Marshal.Copy(result, 0, result_data.Scan0, bytes);
            result_image.UnlockBits(result_data);

            image_noise = result_image;
            image = result_image;
            grayscale();
            textBox3.Text = "Salt & Pepper Noise";

        }

        //exponential noise
        private void exponential()
        {
            image = orig_image;
            int w = image.Width;
            int h = image.Height;

            BitmapData image_data = image.LockBits(
                new Rectangle(0, 0, w, h),
                ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);
            int bytes = image_data.Stride * image_data.Height;
            byte[] buffer = new byte[bytes];
            byte[] result = new byte[bytes];
            Marshal.Copy(image_data.Scan0, buffer, 0, bytes);
            image.UnlockBits(image_data);

            byte[] noise = new byte[bytes];
            double[] erlang = new double[256];
            double a = 5;
            Random rnd = new Random();
            double sum = 0;

            for (int i = 0; i < 256; i++)
            {
                double step = (double)i * 0.01;
                if (step >= 0)
                {
                    erlang[i] = (double)(a * Math.Exp(-a * step));
                }
                else
                {
                    erlang[i] = 0;
                }
                sum += erlang[i];
            }

            for (int i = 0; i < 256; i++)
            {
                erlang[i] /= sum;
                erlang[i] *= bytes;
                erlang[i] = (int)Math.Floor(erlang[i]);
            }

            int count = 0;
            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < (int)erlang[i]; j++)
                {
                    noise[j + count] = (byte)i;
                }
                count += (int)erlang[i];
            }

            for (int i = 0; i < bytes - count; i++)
            {
                noise[count + i] = 0;
            }

            noise = noise.OrderBy(x => rnd.Next()).ToArray();

            for (int i = 0; i < bytes; i++)
            {
                result[i] = (byte)(buffer[i] + noise[i]);
            }

            Bitmap result_image = new Bitmap(w, h);
            BitmapData result_data = result_image.LockBits(
                new Rectangle(0, 0, w, h),
                ImageLockMode.WriteOnly,
                PixelFormat.Format24bppRgb);
            Marshal.Copy(result, 0, result_data.Scan0, bytes);
            result_image.UnlockBits(result_data);

          
            image = result_image;
            grayscale();
            textBox3.Text = "Exponential Noise";

           
        }

        

        //Image restoration button
        private void button16_Click(object sender, EventArgs e)
        {
            arithmeticmean();
            midpoint();
        }
        //arithemetic mean function
         private void arithmeticmean()
        {
            Bitmap img = image;
            int w = img.Width;
            int h = img.Height;
           
            //converying to grayscale
            //color pixel
            Color p;

            for (int x = 0; x < h; x++)
            {
                for (int y = 0; y < w; y++)
                {
                    //get pixel value
                    p = img.GetPixel(x, y);

                    //extract pixel component ARGB
                    int a = p.A;
                    int red = p.R;
                    int g = p.G;
                    int b = p.B;

                    //find average (transformation function)
                    int avg = (red + g + b) / 3;

                    //set new pixel value
                    img.SetPixel(x, y, Color.FromArgb(a, avg, avg, avg));
                }
            }

            BitmapData image_data = img.LockBits(
                new Rectangle(0, 0, w, h),
                ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);
            int bytes = image_data.Stride * image_data.Height;
            byte[] buffer = new byte[bytes];
            Marshal.Copy(image_data.Scan0, buffer, 0, bytes);
            img.UnlockBits(image_data);

            int r = 1;
            int wres = w - 2 * r;
            int hres = h - 2 * r;
            Bitmap result_image = new Bitmap(wres, hres);
            BitmapData result_data = result_image.LockBits(
                new Rectangle(0, 0, wres, hres),
                ImageLockMode.WriteOnly,
                PixelFormat.Format24bppRgb);
            int res_bytes = result_data.Stride * result_data.Height;
            byte[] result = new byte[res_bytes];

            for (int x = r; x < w - r; x++)
            {
                for (int y = r; y < h - r; y++)
                {
                    int pixel_location = x * 3 + y * image_data.Stride;
                    int res_pixel_loc = (x - r) * 3 + (y - r) * result_data.Stride;
                    double[] mean = new double[3];

                    for (int kx = -r; kx <= r; kx++)
                    {
                        for (int ky = -r; ky <= r; ky++)
                        {
                            int kernel_pixel = pixel_location + kx * 3 + ky * image_data.Stride;

                            for (int c = 0; c < 3; c++)
                            {
                                mean[c] += buffer[kernel_pixel + c] / Math.Pow(2 * r + 1, 2);
                            }
                        }
                    }

                    for (int c = 0; c < 3; c++)
                    {
                        result[res_pixel_loc + c] = (byte)mean[c];
                    }
                }
            }

            Marshal.Copy(result, 0, result_data.Scan0, res_bytes);
            result_image.UnlockBits(result_data);

            pictureBox4.Image = result_image;
            textBox5.Text = "Arithmetic Mean Filter";
        }

        private void midpoint()
        {
            Bitmap img = image;
            int w = img.Width;
            int h = img.Height;

            //color pixel
            Color p;

            for (int x = 0; x < h; x++)
            {
                for (int y = 0; y < w; y++)
                {
                    //get pixel value
                    p = img.GetPixel(x, y);

                    //extract pixel component ARGB
                    int a = p.A;
                    int red = p.R;
                    int g = p.G;
                    int b = p.B;

                    //find average (transformation function)
                    int avg = (red + g + b) / 3;

                    //set new pixel value
                    img.SetPixel(x, y, Color.FromArgb(a, avg, avg, avg));
                }
            }
          

            BitmapData image_data = img.LockBits(
                new Rectangle(0, 0, w, h),
                ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);
            int bytes = image_data.Stride * image_data.Height;
            byte[] buffer = new byte[bytes];
            Marshal.Copy(image_data.Scan0, buffer, 0, bytes);
            img.UnlockBits(image_data);

            int r = 1;
            int wres = w - 2 * r;
            int hres = h - 2 * r;

            Bitmap result_image = new Bitmap(wres, hres);
            BitmapData result_data = result_image.LockBits(
                new Rectangle(0, 0, wres, hres),
                ImageLockMode.WriteOnly,
                PixelFormat.Format24bppRgb);
            int res_bytes = result_data.Stride * result_data.Height;
            byte[] result = new byte[res_bytes];
            for (int x = r; x < w - r; x++)
            {
                for (int y = r; y < h - r; y++)
                {
                    int pixel_location = x * 3 + y * image_data.Stride;
                    int res_pixel_loc = (x - r) * 3 + (y - r) * result_data.Stride;
                    double[] median = new double[3];
                    byte[][] neighborhood = new byte[3][];

                    for (int c = 0; c < 3; c++)
                    {
                        neighborhood[c] = new byte[(int)Math.Pow(2 * r + 1, 2)];
                        int added = 0;
                        for (int kx = -r; kx <= r; kx++)
                        {
                            for (int ky = -r; ky <= r; ky++)
                            {
                                int kernel_pixel = pixel_location + kx * 3 + ky * image_data.Stride;
                                neighborhood[c][added] = buffer[kernel_pixel + c];
                                added++;
                            }
                        }
                    }

                    for (int c = 0; c < 3; c++)
                    {
                        result[res_pixel_loc + c] = (byte)((neighborhood[c].Min() + neighborhood[c].Max()) / 2);
                    }
                }
            }

            Marshal.Copy(result, 0, result_data.Scan0, res_bytes);
            result_image.UnlockBits(result_data);

            pictureBox5.Image = result_image;
            textBox6.Text = "MidPoint Filter";

        }


        //Image Compression button
        private void button14_Click(object sender, EventArgs e)
        {
            //display original image
            clearDisplay();
            image = orig_image;
            pictureBox3.Image = image;
            //get original file size
            var fileLength = new FileInfo(path).Length;
            textBox3.Text = "Original Image (" + fileLength + " bytes)";

            //Compression 
            
           // int n = image.Length;
            //for (int i = 0; i < n; i++)
            //{

            //    // Count occurrences of current character
            //    int count = 1;
            //    while (i < n - 1 && str[i] == str[i + 1])
            //    {
            //        count++;
            //        i++;
            //    }
            //}

              pictureBox4.Image = image;
            textBox5.Text = "Compressed Image (" + fileLength + " bytes)";


        }
    }//end of form1


}
