using OpenCvSharp;
using Sunny.UI.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTMent.Core.GameSystem.GameHelper 
{
    public class ImageHelper:Singleton<ImageHelper>
    {
        /// <summary>
        /// 将图片转成mat
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public  Mat  Bitmap2Mat(Bitmap img)
        {
            return OpenCvSharp.Extensions.BitmapConverter.ToMat(img);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mat"></param>
        /// <returns></returns>
        public  Bitmap Mat2Img(Mat mat) 
        { 
            return OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mat);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public Bitmap BitmapDeepCopy(Bitmap bitmap)
        {
            if (bitmap == null) return null;
            Bitmap dstBitmap = bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), bitmap.PixelFormat);
            return dstBitmap;
        }
        /// <summary>
        /// bitmap转stream
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public  MemoryStream Bitmap2MemoryStream(Bitmap bmp)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            try
            {
                bmp.Save(ms, ImageFormat.Bmp);
            }
            catch (Exception ex)
            {
                ms = null;
                LoggerHelper.Debug(ex);
            }

            return ms;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public  Image MemoryStream2Bitmap(MemoryStream ms)
        {
            Image bmp = null;
            try
            {
                bmp = System.Drawing.Bitmap.FromStream(ms);
            }
            catch (Exception ex)
            {
                bmp = null;
                LoggerHelper.Debug(ex);
            }
            return bmp;
        }
    }
}
