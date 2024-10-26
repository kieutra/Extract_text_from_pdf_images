using System;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Windows.Forms;
using System.Xml.Linq;
using Ghostscript.NET;
using Ghostscript.NET.Processor;
using Ghostscript.NET.Rasterizer;
using Tesseract;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;
using System.Collections.Generic;

namespace ExtractText
{
    public partial class MainFrame : Form
    {
        public MainFrame()
        {
            InitializeComponent();
        }
        string file = "";
        string text;
        int page = 0;
        List<Bitmap> imageList = new List<Bitmap>();
        List<string> textList = new List<string>(); //show on screen
        List<List<Word>> wordList = new List<List<Word>>();
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            DialogResult result = openFileDialog1.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                for (int i = 0; i < imageList.Count; i++)
                {
                    imageList[i].Dispose();
                }
                imageList.Clear();
                textList.Clear();
                wordList.Clear();
                file = openFileDialog1.FileName;

                try
                {
                    text = File.ReadAllText(file);
                    this.label2.Text = file;
                    this.label2.Show();
                }
                catch (IOException)
                {
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (file == "")
                return;
            if (file.Substring(file.Length - 3) == "pdf")
                convert_pdf_to_images();
            else
                imageList.Add(PixConverter.ToBitmap(Pix.LoadFromFile(file)));
            for (int i = 0; i < imageList.Count; i++) {
                wordList.Add(new List<Word>());
            }
           

            if (imageList.Count >= 2)
            {
                List<Thread> threads = new List<Thread>();

                Thread t1 = new Thread(() => thread_ocr(0, imageList.Count / 2));
                t1.Start();//start thread and pass it the port
                threads.Add(t1);
                Thread t2 = new Thread(() => thread_ocr(imageList.Count / 2, imageList.Count));
                t2.Start();//start thread and pass it the port
                threads.Add(t2);

                foreach (var thread in threads)
                {
                    thread.Join();
                }
            }else
                thread_ocr(0, imageList.Count);

      
            this.button3.Visible = true;
            this.button4.Visible = true;
            this.label3.Visible = true;

            //show text on screen
           
            arrange_words();
            
            if (imageList.Count > page && textList.Count > page)
            {
                this.pictureBox1.Image = imageList[page];
                this.label3.Text = textList[page];
            }
                //image.Save("E:\\test2.jpg"); save to disk if necessary

            }

        void thread_ocr(int start, int end)
        {
            //List<Bitmap> local_images = images;
            using (var engine = new TesseractEngine("E:\\workspace_tra\\IT languages\\CSHARP\\ExtractTextFromFilePDForImage\\tessdata", "eng", EngineMode.Default))
            {
                for (int i = start; i < end; i++)
                {
                    var img = PixConverter.ToPix(imageList[i]);
                    var scaledImage = img.Scale(3, 3);
                    var grayImage = scaledImage.ConvertRGBToGray();
                    var thresholdedPix = grayImage.BinarizeOtsuAdaptiveThreshold(35, 35, 0, 0, 0.2f);
                    //thresholdedPix.Save("E:\\binary21.jpg");
                    using (var page = engine.Process(thresholdedPix, PageSegMode.Auto))
                    {
                        using (var iterator = page.GetIterator())
                        {
                            iterator.Begin();
                            do
                            {
                                string currentWord = iterator.GetText(PageIteratorLevel.Word);
  
                                //font = NULL always ??
                                FontAttributes font = iterator.GetWordFontAttributes();
                                iterator.TryGetBoundingBox(PageIteratorLevel.Word, out Rect bounds);
                                wordList[i].Add(new Word(currentWord, bounds, font));
                            }
                            while (iterator.Next(PageIteratorLevel.Word));
                        }
                    }
                }
            }
        }

        private void convert_pdf_to_images()
        {
            const string ThumbnailArguments = " -dBATCH -dNOPAUSE -sDEVICE=jpeg -dJPEGQ=95 -sOutputFile=\"{1}\\%03d.jpg\" \"{0}\"";

            // Build the command line arguments
            var arguments = string.Format(ThumbnailArguments, file, "E:\\workspace_tra\\IT languages\\CSHARP", 200, 300);

            using (GhostscriptRasterizer rasterizer = new GhostscriptRasterizer())
            {
                //  custom switches can be added before the file is opened

                // rasterizer.CustomSwitches.Add(arguments);

                byte[] buffer = File.ReadAllBytes(file);

                using (var fileStream = new FileStream(file,
                     FileMode.Open, FileAccess.ReadWrite, FileShare.Read))
                {
                    // now use that fileStream to save the pdf stream
                    fileStream.Write(buffer, 0, buffer.Length);
                    rasterizer.Open(fileStream);
                    var page_count = rasterizer.PageCount;
                    // var image = rasterizer.GetPage(0, 1);
                    for (int i = 1; i <= page_count; i++)
                    {
                        var image = rasterizer.GetPage(200, i);
                        imageList.Add((Bitmap)image);
                    }
                    // this.pictureBox1.Image = image;
                    fileStream.Close();
                }
                rasterizer.Close();
            }
        }

        //Prev Button
        private void button3_Click(object sender, EventArgs e)
        {
            if (page > 0)
            {
                {
                    page--;
                    this.pictureBox1.Image = imageList[page];
                    this.label3.Text = textList[page];

                }
            }
        }

        //Next Button
        private void button4_Click(object sender, EventArgs e)
        {
            if (page < imageList.Count() - 1)
            {
                page++;
                this.pictureBox1.Image = imageList[page];
                this.label3.Text = textList[page];

            }

        }

        private void arrange_words()
        {
            if (wordList.Count == 0) return;
            for (int i = 0; i < wordList.Count; i++)
            {
                List<Word> local_words = wordList[i];
                if (local_words.Count == 0) continue;
                int pointY = 0;
                string text_m = "";
                if (wordList.Count > 0)
                {
                    text_m = local_words[0].word_str;
                    pointY = (local_words[0].position.Y1 + local_words[0].position.Y2) / 2;
                }
                for (int ti = 1; ti < local_words.Count; ti++)
                {
                    Word word = local_words[ti];
                    if (pointY < word.position.Y1 || pointY > word.position.Y2)
                    {
                        text_m += "\n";
                        text_m += word.word_str;
                        pointY = (word.position.Y1 + word.position.Y2) / 2;
                    }
                    else
                    {
                        text_m += word.word_str;
                        text_m += " ";
                    }
                }
                textList.Add(text_m);
            }
                       
        }
    }
}
