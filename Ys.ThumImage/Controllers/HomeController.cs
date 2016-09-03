using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using System.IO;
using Ys.Common;
using System.Threading.Tasks;

namespace Ys.ThumImage.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// 生成图片缩率图Action
        /// </summary>
        /// <param name="p">原图url</param>
        /// <param name="id">图片尺寸以及生成缩率图的类型</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> Index(string p, string id)
        {
            if (string.IsNullOrEmpty(p))
            {
                return new HttpStatusCodeResult(404);
            }

            string oPath = Regex.Replace(p, @"http[s]?://(.*?)/", "/", RegexOptions.IgnoreCase);
            int? oWidth = 200, oHeight = 200;
            int cutMode = 3;
            string pPath;
            string oDir;

            if (!string.IsNullOrEmpty(id))
            {
                string[] ss = id.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                if (ss.Length < 2)
                {
                    return new HttpStatusCodeResult(404);
                }
                if (ss.Length > 2)
                {
                    cutMode = int.Parse(ss[2]);
                }
                oPath = oPath.Insert(oPath.LastIndexOf('/') + 1, string.Format("{0}_{1}_{2}_", ss[0], ss[1], cutMode));
                oWidth = int.Parse(ss[0]);
                oHeight = int.Parse(ss[1]);
            }

            oPath= string.Format("/{0}{1}", AppConfigHelper.GetAppConfig<string>("cacheDir"), oPath);

            pPath = Server.MapPath(oPath);
            
            oDir = Path.GetDirectoryName(pPath);

            if (!System.IO.File.Exists(pPath))
            {
                byte[] imagebytes =await FileHelper.DownLoadFileAsync(p);
                if (!Directory.Exists(oDir))
                {
                    Directory.CreateDirectory(oDir);
                }
                FileHelper.MakeThumbnail(FileHelper.BytToImg(imagebytes), oWidth.Value, oHeight.Value, (ThumbnailMode)cutMode, pPath, true);
            }

            return File(pPath, FileHelper.GetContentTypeByExtension(Path.GetExtension(pPath).ToLower()));
        }

       
    }
}
