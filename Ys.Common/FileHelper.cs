using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;
using System.Drawing;
using System.Net;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;

namespace Ys.Common
{
    public class FileHelper
    {


        /// <summary>
        /// 会产生graphics异常的PixelFormat
        /// </summary>
        private static PixelFormat[] indexedPixelFormats = { PixelFormat.Undefined, PixelFormat.DontCare,
PixelFormat.Format16bppArgb1555, PixelFormat.Format1bppIndexed, PixelFormat.Format4bppIndexed,
PixelFormat.Format8bppIndexed
                                                           };
        /// <summary>
        /// 图片后缀和ContentType对应字典
        /// </summary>
        static Dictionary<string, string> extensionContentTypeDic;


        static FileHelper()
        {
            if (extensionContentTypeDic == null)
            {
                //.jpg", ".png", ".gif", ".jpeg
                extensionContentTypeDic = new Dictionary<string, string>();
                extensionContentTypeDic.Add(".jpg", "image/jpeg");
                extensionContentTypeDic.Add(".png", "image/png");
                extensionContentTypeDic.Add(".gif", "image/gif");
                extensionContentTypeDic.Add(".jpeg", "image/jpeg");
            }

        }
        /// <summary>
        /// 根据后缀名获取extension
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static string GetContentTypeByExtension(string extension)
        {
            if (extensionContentTypeDic.ContainsKey(extension))
            {
                return extensionContentTypeDic[extension];
            }
            return null;
        }

        /// <summary>
        /// 得到缩略图
        /// </summary>
        /// <param name="picPath"></param>
        /// <returns></returns>
        public static string GetThumbnail(string picPath, string thumbnailPrefix = "s")
        {
            if (string.IsNullOrEmpty(picPath))
                return null;
            return picPath.Insert(picPath.LastIndexOf('/') + 1, thumbnailPrefix);
        }


        ///  <summary > 
        /// 将Image对象转化成二进制流 
        ///  </summary > 
        ///  <param name="image" > </param > 
        ///  <returns > </returns > 
        public static byte[] ImageToByteArray(Image image)
        {
            MemoryStream imageStream = new MemoryStream();
            Bitmap bmp = new Bitmap(image.Width, image.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.DrawImage(image, new System.Drawing.Rectangle(0, 0, image.Width, image.Height));
            try
            {
                bmp.Save(imageStream, image.RawFormat);
            }
            catch (Exception e)
            {

                bmp.Save(imageStream, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            byte[] byteImg = imageStream.GetBuffer();
            bmp.Dispose();
            g.Dispose();
            imageStream.Close();
            return byteImg;

            #region 老方法有bug
            //    //实例化流 
            //    using (MemoryStream imageStream = new MemoryStream())
            //    {

            //        //将图片的实例保存到流中            
            //        image.Save(imageStream, image.RawFormat);
            //        //System.Drawing.Imaging.ImageFormat.Jpeg
            //        //保存流的二进制数组 
            //        byte[] imageContent = new Byte[imageStream.Length];

            //        imageStream.Position = 0;
            //        //将流泻如数组中 
            //        imageStream.Read(imageContent, 0, (int)imageStream.Length);

            //        return imageStream.ToArray();
            //}
            #endregion
        }

        /// <summary>
        /// 图片旋转90
        /// </summary>
        /// <param name="PhyPath"></param>
        public static void ImageRotate90(string PhyPath)
        {
            Image image = Image.FromFile(PhyPath);
            image.RotateFlip(RotateFlipType.Rotate90FlipNone);
            image.Save(PhyPath);
            image.Dispose();
        }

        /// <summary>
        /// 图片旋转90
        /// </summary>
        /// <param name="image"></param>
        public static void ImageRotate90(Image image)
        {
            image.RotateFlip(RotateFlipType.Rotate90FlipNone);
        }

        /// <summary> 
        /// 字节流转换成图片 
        /// </summary> 
        /// <param name="byt">要转换的字节流</param> 
        /// <returns>转换得到的Image对象</returns> 
        public static Image BytToImg(byte[] byt)
        {
            MemoryStream ms = new MemoryStream(byt);
            Image img = Image.FromStream(ms);
            ms.Close();
            return img;
        }



        /// <summary>
        /// 获取自定义目录的上传路径
        /// </summary>
        /// <param name="customDir"></param>
        public static string GetUploadPath(string customDir)
        {
            return string.Format("/upload{{1}}/{0}/{{0}}/", customDir.Trim('/'));
        }


        /// <summary>
        /// 获取amr的播放时长
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static long GetAMRFileDuration(byte[] bytes)
        {
            long duration = 0;
            MemoryStream stream = new MemoryStream(bytes);
            {
                byte[] packed_size = new byte[16] { 12, 13, 15, 17, 19, 20, 26, 31, 5, 0, 0, 0, 0, 0, 0, 0 };
                int pos = 0;
                pos += 6;
                long lenth = stream.Length;
                byte[] toc = new byte[1];
                int framecount = 0;
                byte ft;
                while (pos < lenth)
                {
                    stream.Seek(pos, SeekOrigin.Begin);
                    if (1 != stream.Read(toc, 0, 1))
                    {
                        duration = lenth > 0 ? ((lenth - 6) / 650) : 0;
                        stream.Close();
                        break;
                    }
                    ft = (byte)((toc[0] / 8) & 0x0F);
                    pos += packed_size[ft] + 1;
                    framecount++;
                }
                duration = framecount * 20 / 1000;
            }
            stream.Close();
            return duration;
        }

        /// <summary>
        /// 生成缩率图
        /// </summary>
        /// <param name="originalImage">原始图片Image</param>
        /// <param name="width">缩率图宽</param>
        /// <param name="height">缩率图高</param>
        /// <param name="mode">生成缩率图的方式</param>
        /// <param name="thumbnailPath">缩率图存放的地址</param>
        public static Image MakeThumbnail(Image originalImage, int width, int height, ThumbnailMode mode, string thumbnailPath, bool isSave = true)
        {

            int towidth = width;
            int toheight = height;

            int x = 0;
            int y = 0;
            int ow = originalImage.Width;
            int oh = originalImage.Height;
            switch (mode)
            {
                case ThumbnailMode.HW://指定高宽缩放（可能变形）                  
                    break;
                case ThumbnailMode.W://指定宽，高按比例                      
                    toheight = originalImage.Height * width / originalImage.Width;
                    break;
                case ThumbnailMode.H://指定高，宽按比例  
                    towidth = originalImage.Width * height / originalImage.Height;
                    break;
                case ThumbnailMode.Cut://指定高宽裁减（不变形）                  
                    if ((double)originalImage.Width / (double)originalImage.Height > (double)towidth / (double)toheight)
                    {
                        oh = originalImage.Height;
                        ow = originalImage.Height * towidth / toheight;
                        y = 0;
                        x = (originalImage.Width - ow) / 2;
                    }
                    else
                    {
                        ow = originalImage.Width;
                        oh = originalImage.Width * height / towidth;
                        x = 0;
                        y = (originalImage.Height - oh) / 2;
                    }
                    break;

                default:
                    break;
            }

            //新建一个bmp图片  
            System.Drawing.Image bitmap = new System.Drawing.Bitmap(towidth, toheight);
            //新建一个画板  
            Graphics g = System.Drawing.Graphics.FromImage(bitmap);
            //设置高质量插值法  
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
            //设置高质量,低速度呈现平滑程度  
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            //清空画布并以透明背景色填充  
            g.Clear(Color.Transparent);
            //在指定位置并且按指定大小绘制原图片的指定部分  
            g.DrawImage(originalImage, new Rectangle(0, 0, towidth, toheight),
             new Rectangle(x, y, ow, oh),
             GraphicsUnit.Pixel);
            if (!isSave)
            {
                return bitmap;
            }
            try
            {
                //以jpg格式保存缩略图  
                //bitmap.Save(thumbnailPath, bitmap.RawFormat);
                bitmap.Save(thumbnailPath, ImageFormat.Jpeg);
                return bitmap;

            }
            catch (System.Exception e)
            {
                throw e;
            }
            finally
            {
                originalImage.Dispose();
                bitmap.Dispose();
                g.Dispose();
            }
            return null;
        }

        /// <summary>
        /// 填充一个圆角矩形
        /// </summary>
        /// <param name="g"></param>
        /// <param name="brush"></param>
        /// <param name="rect"></param>
        /// <param name="cornerRadius"></param>
        public static void FillRoundRectangle(Graphics g, Brush brush, Rectangle rect, int cornerRadius)
        {
            using (GraphicsPath path = CreateRoundedRectanglePath(rect, cornerRadius))
            {
                g.FillPath(brush, path);
            }
        }
        /// <summary>
        /// 画一个圆角矩形
        /// </summary>
        /// <param name="g"></param>
        /// <param name="pen"></param>
        /// <param name="rect"></param>
        /// <param name="cornerRadius"></param>
        public static void DrawRoundRectangle(Graphics g, Pen pen, Rectangle rect, int cornerRadius)
        {
            using (GraphicsPath path = CreateRoundedRectanglePath(rect, cornerRadius))
            {
                g.DrawPath(pen, path);
            }
        }




        #region  私有方法
        /// <summary>
        /// 判断图片的PixelFormat 是否在 引发异常的 PixelFormat 之中
        /// </summary>
        /// <param name="imgPixelFormat">原图片的PixelFormat</param>
        /// <returns></returns>
        private static bool IsPixelFormatIndexed(PixelFormat imgPixelFormat)
        {
            foreach (PixelFormat pf in indexedPixelFormats)
            {
                if (pf.Equals(imgPixelFormat)) return true;
            }

            return false;
        }


        /// <summary>
        /// 获得默认的图片上传路径
        /// </summary>
        /// <param name="uploadDriectory"></param>
        /// <returns></returns>
        private static string GetDefaultUploadDriectory(string uploadDriectory)
        {

            if (string.IsNullOrEmpty(uploadDriectory))
            {
                return null;
            }

            return Regex.Replace(uploadDriectory, @"/upload(?<index>[\d])+?/", "/upload/");
        }

        /// <summary>
        /// 判断路径是否本地地址
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static string GetFileServerIndexFromPath(string path)
        {
            Match match = Regex.Match(path, @"/upload(?<index>[\d])*?/");
            if (match == null)
            {
                return "";
            }

            return match.Groups["index"].Value;
        }

        /// <summary>
        /// 计算绘图路径 GraphicsPath
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="radius"></param>
        /// <param name="sPosition"></param>
        /// <returns></returns>
        private static GraphicsPath CreateRoundRectanglePath(Rectangle rect, int radius, CornerLocation cornerLocation)
        {
            GraphicsPath rectPath = new GraphicsPath();
            switch (cornerLocation)
            {
                case CornerLocation.TopLeft:
                    {
                        rectPath.AddArc(rect.Left, rect.Top, radius * 2, radius * 2, 180, 90);
                        rectPath.AddLine(rect.Left, rect.Top, rect.Left, rect.Top + radius);
                        break;
                    }

                case CornerLocation.TopRight:
                    {
                        rectPath.AddArc(rect.Right - radius * 2, rect.Top, radius * 2, radius * 2, 270, 90);
                        rectPath.AddLine(rect.Right, rect.Top, rect.Right - radius, rect.Top);
                        break;
                    }

                case CornerLocation.BottomLeft:
                    {
                        rectPath.AddArc(rect.Left, rect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
                        rectPath.AddLine(rect.Left, rect.Bottom - radius, rect.Left, rect.Bottom);
                        break;
                    }

                case CornerLocation.BottomRight:
                    {
                        rectPath.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
                        rectPath.AddLine(rect.Right - radius, rect.Bottom, rect.Right, rect.Bottom);
                        break;
                    }

            }
            return rectPath;
        }
        /// <summary>
        /// 创建圆角图形path
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="cornerRadius"></param>
        /// <returns></returns>
        static GraphicsPath CreateRoundedRectanglePath(Rectangle rect, int cornerRadius)
        {
            GraphicsPath roundedRect = new GraphicsPath();
            roundedRect.AddArc(rect.X, rect.Y, cornerRadius * 2, cornerRadius * 2, 180, 90);
            roundedRect.AddLine(rect.X + cornerRadius, rect.Y, rect.Right - cornerRadius * 2, rect.Y);
            roundedRect.AddArc(rect.X + rect.Width - cornerRadius * 2, rect.Y, cornerRadius * 2, cornerRadius * 2, 270, 90);
            roundedRect.AddLine(rect.Right, rect.Y + cornerRadius * 2, rect.Right, rect.Y + rect.Height - cornerRadius * 2);
            roundedRect.AddArc(rect.X + rect.Width - cornerRadius * 2, rect.Y + rect.Height - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 0, 90);
            roundedRect.AddLine(rect.Right - cornerRadius * 2, rect.Bottom, rect.X + cornerRadius * 2, rect.Bottom);
            roundedRect.AddArc(rect.X, rect.Bottom - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 90, 90);
            roundedRect.AddLine(rect.X, rect.Bottom - cornerRadius * 2, rect.X, rect.Y + cornerRadius * 2);
            roundedRect.CloseFigure();
            return roundedRect;
        }
        #endregion



        /// <summary>
        /// 下载指定文件到指定的地方
        /// </summary>
        /// <param name="remoteUrl"></param>
        /// <param name="ss"></param>
        public static void DownLoadFile(string remoteUrl, string sss)
        {
            WebClient wc = new WebClient();
            try
            {
                string path = Path.GetDirectoryName(sss);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                wc.DownloadFile(remoteUrl, sss);
            }
            catch (Exception e)
            {

                throw new Exception("下载文件失败");
            }
        }

        /// <summary>
        /// 下载指定文件
        /// </summary>
        /// <param name="remoteUrl"></param>
        /// <param name="ss"></param>
        public static byte[] DownLoadFile(string remoteUrl)
        {
            WebClient wc = new WebClient();
            try
            {
                return wc.DownloadData(remoteUrl);
            }
            catch (Exception e)
            {
                throw new Exception("下载文件失败");
            }

        }

        /// <summary>
        /// 异步任务下载指定文件
        /// </summary>
        /// <param name="remoteUrl"></param>
        /// <returns></returns>
        public static Task<byte[]> DownLoadFileAsync(string remoteUrl)
        {
            WebClient wc = new WebClient();
            try
            {             
                return wc.DownloadDataTaskAsync(remoteUrl);
            }
            catch (Exception e)
            {
                throw new Exception("下载文件失败");
            }
        }




    }
    public enum ThumbnailMode
    {
        /// <summary>
        /// 指定高宽缩放（可能变形）
        /// </summary>
        HW,
        /// <summary>
        /// 指定高，宽按比例
        /// </summary>
        H,
        /// <summary>
        /// 指定宽，高按比例
        /// </summary>
        W,
        /// <summary>
        /// 指定高宽裁减（不变形）   
        /// </summary>
        Cut,

    }
    /// <summary>
    ///文件类型
    /// </summary>
    public enum FileType
    {
        /// <summary>
        /// 音频、视频
        /// </summary>
        Audio,
        /// <summary>
        /// 图片
        /// </summary>
        Image,
        /// <summary>
        /// 药品库图片
        /// </summary>
        Flash,
        /// <summary>
        /// 其它文件
        /// </summary>
        File,
    }

    enum CornerLocation
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }
}
