using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tesseract;
namespace ExtractText
{
    internal class Position
    {
        public int x;
        public int y;
        public int width;
        public int height;
        public Position(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }
    }

    internal class Word
    {
        public string word_str { get; set;}
        public Rect position { get; set; }
        public FontAttributes font { get; set; }
        public Word(string word_str, Rect position, FontAttributes font)
        {
            this.word_str = word_str;
            this.position = position;
            this.font = font;
        }
    }
}
