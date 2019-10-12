using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace PackingApi.Models.PDF
{
    public class Config
    {
       
        IHostingEnvironment _hostingEnvironment;
        public Config(IHostingEnvironment hostingEnvironment)
        {
            this._hostingEnvironment = hostingEnvironment; ;

        }
        public Document setPageSize()
        {
            return new Document(PageSize.A4, 20, 20, 20, 20);
        }
 
        public BaseFont getTHSarabun()
        {
            string fontName = "THSarabunNew";
            if (!FontFactory.IsRegistered(fontName))
            {
                var fontPath = _hostingEnvironment.ContentRootPath + "\\Fonts\\THSarabunNew.ttf";
                FontFactory.Register(fontPath);
            }
            FontFactory.GetFont(fontName, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            return BaseFont.CreateFont(fontName, BaseFont.IDENTITY_H, BaseFont.EMBEDDED); ;
        }
    }
}
