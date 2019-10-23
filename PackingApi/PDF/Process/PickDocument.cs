using PackingApi.PDF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using Microsoft.AspNetCore.Hosting;


namespace PackingApi.Models.PDF.Process
{
    public class PickDocument
    {
        private IHostingEnvironment _hostingEnvironment;

        Document _document;
        string[] headers;
        BaseFont _bf;
        public PickDocument(IHostingEnvironment hostingEnvironment)
        {
            this._hostingEnvironment = hostingEnvironment;
            Config config = new Config(this._hostingEnvironment);
            this._document = config.setPageSize();
            this._bf = config.getTHSarabun();
        }

        public Stream CreatePDF(PickDocumentModel pickDocumentModel)
        {
            System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
            Document document = this._document;
            PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
            document.Open();

            headers = new string[] { "No.", "รหัสลูกค้า", "ชื่อสินค้า", "ISBN", "ราคา", "Order" };
            PdfPTable table = new PdfPTable(headers.Count());
            float[] widths = new float[] { 7f, 20f, 45f, 20f, 7, 7f };

            table.SetWidths(widths);
            table.HeaderRows = 6;

            table.WidthPercentage = 95f;
            table.AddCell(bulidColumnTitle(@"บริษัท แม็คเอ็ดดูเคชั่น จำกัด", 20));
            table.AddCell(bulidColumnTitle(@"ใบรวมใบจัดสินค้า (PICK LIST SUMMARY)", 20));
            table.AddCell(bulidColumnTitle(@"วันที่ส่ง " + pickDocumentModel.DocDueDate.Value.ToString("dd/MM/yyyy"), 18));
            table.AddCell(bulidEmptryRows());
            table.AddCell(bulidColumnSubTitle(@"รหัส/ชื่อกลุ่มสินค้า " + pickDocumentModel.ItemGrpCode + "-" + pickDocumentModel.ItemGrpName, 16, Element.ALIGN_LEFT));
            table.AddCell(bulidColumnSubTitle(@"ใบจัดซื้อสินค้า " + pickDocumentModel.PickNo, 16, Element.ALIGN_RIGHT));


            foreach (string h in headers)
            {
                table.AddCell(bulidColumnHeader(h, 16));
            }
            int no = 1;
            foreach (var i in pickDocumentModel.selectPickItems)
            {
                table.AddCell(bulidColumnRows(no.ToString(), 14, Element.ALIGN_CENTER));
                table.AddCell(bulidColumnRows(i.ItemCode, 14, Element.ALIGN_CENTER));
                table.AddCell(bulidColumnRows(i.Dscription, 14, Element.ALIGN_LEFT));
                table.AddCell(bulidColumnRows(i.Isbn, 14, Element.ALIGN_CENTER));
                table.AddCell(bulidColumnRows(i.Price.ToString(), 14, Element.ALIGN_CENTER));
                table.AddCell(bulidColumnRows(i.Quantity.ToString(), 14, Element.ALIGN_CENTER));
                no++;
            }
            document.Add(table);
            document.Close();
            return memoryStream;
        }
        private PdfPCell bulidEmptryRows()
        {
            PdfPCell cell = new PdfPCell();
            cell.Colspan = headers.Count();
            cell.Border = 0;
            cell.FixedHeight = 20f;
            return cell;
        }
        private PdfPCell bulidColumnTitle(string str, int size)
        {
     
            Font font = new Font(this._bf, size);
            Phrase pharse = new Phrase(str, font);


            PdfPCell cell = new PdfPCell(pharse);
            cell.Colspan = headers.Count();
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.Border = 0;
            return cell;
        }
        private PdfPCell bulidColumnSubTitle(string str, int size, int align)
        {

            Font font = new Font(this._bf, size);
            Phrase pharse = new Phrase(str, font);

            PdfPCell cell = new PdfPCell(pharse);
            cell.Colspan = headers.Count() / 2;
            cell.HorizontalAlignment = align;
            cell.Border = 0;
            return cell;
        }
        private PdfPCell bulidColumnHeader(string str, int size)
        {
            Font font = new Font(this._bf, size);
            Phrase pharse = new Phrase(str, font);

            PdfPCell cell = new PdfPCell(pharse);
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.Border = 0;
            cell.FixedHeight = 25f;
            return cell;
        }
        private PdfPCell bulidColumnRows(string str, int size, int align)
        {

            Font font = new Font(this._bf, size);
            Phrase pharse = new Phrase(str, font);

            PdfPCell cell = new PdfPCell(pharse);
            cell.HorizontalAlignment = align;
            cell.Border = 0;
            return cell;
        }


    }

}
