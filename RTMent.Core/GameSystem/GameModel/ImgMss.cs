using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTMent.Core.GameSystem.GameModel
{
    public class ImgMss
    {
        public int imgIndex { get; set; }
        public MemoryStream img {  get; set; }
        public DateTime DateTime { get; set; }
        public string Name { get; set; }
        public bool[][] isHand { get; set; }

    }
    public struct ImgAndIndex
    {
        public int imgIndex { get; set; }
        public Bitmap bmp { get; set; }
    }
}
