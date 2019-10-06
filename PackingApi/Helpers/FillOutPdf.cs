using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PackingApi.Helpers
{
    public class FillOutPdf
    {
        string path = "";
        public FillOutPdf(string p)
        {
            this.path = p;
        }
        public ICollection GetFormFields(Stream pdfStream)
        {
            PdfReader reader = null;
            try
            {
                PdfReader pdfReader = new PdfReader(pdfStream);
                AcroFields acroFields = pdfReader.AcroFields;
                return acroFields.Fields.Keys;
            }
            finally
            {
                reader?.Close();
            }
        }
        public  byte[] ReadFully(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }
        public Stream FillForm(Stream inputStream, Models.PDF.Invoice model)
        {
            Stream outStream = new MemoryStream();
            PdfReader pdfReader = null;
            PdfStamper pdfStamper = null;
            Stream inStream = null;
            try
            {
                
             

                pdfReader = new PdfReader(inputStream);
                pdfStamper = new PdfStamper(pdfReader, outStream);
          
                AcroFields form = pdfStamper.AcroFields;
                Font font= GetTHSarabunNew();
                foreach (string key in form.Fields.Keys)
                {
                 
                    form.SetFieldProperty(key, "textfont", font.BaseFont, null);
                    form.RegenerateField(key);
                }
                form.SetField("txt_CardCode", model.CardCode);
                form.SetField("txt_CardName", model.CardName);
                form.SetField("txt_DocNum", model.Docnum);
                // set this if you want the result PDF to not be editable. 
                pdfStamper.FormFlattening = true;
                return outStream;
            }
            finally
            {
                pdfStamper?.Close();
                pdfReader?.Close();
                inStream?.Close();
            }

        }
        public Font GetTHSarabunNew()
        {
            var fontName = "THSarabunNew";
            if (!FontFactory.IsRegistered(fontName))
            {
                var fontPath = this.path + "\\Fonts\\THSarabunNew.ttf";
                FontFactory.Register(fontPath);
            }
            return FontFactory.GetFont(fontName, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

        }
    }
}
