using PackingApi.PDF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using PackingApi.Helpers;

namespace PackingApi.Models.PDF.Process
{
    public class InvoiceDocument
    {
        private IHostingEnvironment _hostingEnvironment;

        Document _document;

        BaseFont _bf;
        public InvoiceDocument(IHostingEnvironment hostingEnvironment)
        {
            this._hostingEnvironment = hostingEnvironment;
            Config config = new Config(this._hostingEnvironment);
            this._document = config.setPageSize();
            this._bf = config.getTHSarabun();
        }

        public Stream CreatePDF(InvoiceDocumentModel invoiceDocumentModel)
        {
            System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
            Document document = this._document;
            PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
            document.Open();
       
            PdfPTable table = new PdfPTable(1);
            table.WidthPercentage = 95f;
            table.HeaderRows = 1;
            //table.FooterRows = 2;


            PdfPTable tabletitle = new PdfPTable(4);
            tabletitle.AddCell(bulidColumnDetail("", 10, 1, Element.ALIGN_CENTER));
            tabletitle.AddCell(bulidColumnDetail("", 10, 1, Element.ALIGN_LEFT));
            tabletitle.AddCell(bulidColumnDetail("", 10, 1, Element.ALIGN_CENTER));
            tabletitle.AddCell(bulidColumnDetail(invoiceDocumentModel.County, 10, 1, Element.ALIGN_LEFT));

            tabletitle.AddCell(bulidColumnDetail("", 10, 1, Element.ALIGN_CENTER));
            tabletitle.AddCell(bulidColumnDetail(invoiceDocumentModel.CardCode+" เลขประจำตัวผู้เสียภาษี", 10, 1, Element.ALIGN_LEFT));
            tabletitle.AddCell(bulidColumnDetail("", 10, 1, Element.ALIGN_CENTER));
            tabletitle.AddCell(bulidColumnDetail("", 10, 1, Element.ALIGN_CENTER));

            tabletitle.AddCell(bulidColumnDetail("", 10, 1, Element.ALIGN_CENTER));
            tabletitle.AddCell(bulidColumnDetail(invoiceDocumentModel.CardName, 10, 1, Element.ALIGN_LEFT));
            tabletitle.AddCell(bulidColumnDetail("", 10, 1, Element.ALIGN_CENTER));
            tabletitle.AddCell(bulidColumnDetail(invoiceDocumentModel.DocDate.Value.ToString("dd/MM/yyyy"), 10, 1, Element.ALIGN_LEFT));

            tabletitle.AddCell(bulidColumnDetail("", 10, 1, Element.ALIGN_CENTER));
            tabletitle.AddCell(bulidColumnDetail(invoiceDocumentModel.Address, 10, 1, Element.ALIGN_LEFT,2));
            tabletitle.AddCell(bulidColumnDetail("", 10, 1, Element.ALIGN_CENTER));
            tabletitle.AddCell(bulidColumnDetail("", 10, 1, Element.ALIGN_CENTER));

            tabletitle.AddCell(bulidColumnDetail("", 10, 1, Element.ALIGN_CENTER));

            tabletitle.AddCell(bulidColumnDetail("", 10, 1, Element.ALIGN_CENTER));
            tabletitle.AddCell(bulidColumnDetail(invoiceDocumentModel.DocDueDate.Value.ToString("dd/MM/yyyy"), 10, 1, Element.ALIGN_LEFT));

            tabletitle.AddCell(bulidColumnDetail("", 10, 1, Element.ALIGN_CENTER));
            tabletitle.AddCell(bulidColumnDetail("", 10, 1, Element.ALIGN_CENTER));
            tabletitle.AddCell(bulidColumnDetail("", 10, 1, Element.ALIGN_CENTER));
            tabletitle.AddCell(bulidColumnDetail("ขายส่ง", 10, 1, Element.ALIGN_LEFT));

            tabletitle.AddCell(bulidColumnDetail("", 10, 1, Element.ALIGN_CENTER));
            tabletitle.AddCell(bulidColumnDetail(invoiceDocumentModel.Address, 10, 1, Element.ALIGN_LEFT, 2));
            tabletitle.AddCell(bulidColumnDetail("", 10, 1, Element.ALIGN_CENTER));
            tabletitle.AddCell(bulidColumnDetail("60 วัน", 10, 1, Element.ALIGN_LEFT));

            tabletitle.AddCell(bulidColumnDetail("", 10, 1, Element.ALIGN_CENTER));

            tabletitle.AddCell(bulidColumnDetail("", 10, 1, Element.ALIGN_CENTER));
            tabletitle.AddCell(bulidColumnDetail(invoiceDocumentModel.DocDueDate.Value.ToString("dd/MM/yyyy"), 10, 1, Element.ALIGN_LEFT));

            tabletitle.AddCell(bulidColumnDetail("", 10, 1, Element.ALIGN_CENTER));
            tabletitle.AddCell(bulidColumnDetail("", 10, 1, Element.ALIGN_CENTER));
            tabletitle.AddCell(bulidColumnDetail("", 10, 1, Element.ALIGN_CENTER));
            tabletitle.AddCell(bulidColumnDetail("", 10, 1, Element.ALIGN_CENTER));
            table.AddCell(tabletitle);

            int count = 1;
            float[] widthBoxs = new float[] { 7f, 15F, 47F, 7f, 7f, 7f, 10f };
            PdfPTable tableDetail = new PdfPTable(widthBoxs.Count());
            tableDetail.WidthPercentage = 95f;
            foreach (var item in invoiceDocumentModel.invoiceitems) {
               
                tableDetail.AddCell(bulidColumnDetail(count+":"+item.ItemGrpCode, 10, 1, Element.ALIGN_CENTER));
                tableDetail.AddCell(bulidColumnDetail(item.ItemCode, 10, 1, Element.ALIGN_CENTER));
                tableDetail.AddCell(bulidColumnDetail(item.Dscription, 10, 1, Element.ALIGN_LEFT));
                tableDetail.AddCell(bulidColumnDetail(item.Qty.ToString(), 10, 1, Element.ALIGN_RIGHT));
                tableDetail.AddCell(bulidColumnDetail(item.Price.ToString(), 10, 1, Element.ALIGN_RIGHT));
                tableDetail.AddCell(bulidColumnDetail("%", 10, 1, Element.ALIGN_CENTER));
                tableDetail.AddCell(bulidColumnDetail((item.Qty* item.Price).ToString(), 10, 1, Element.ALIGN_RIGHT));
                if (count == 12)
                {

                    tableDetail.AddCell(bulidColumnDetail("", 10, 1, Element.ALIGN_CENTER));
                    tableDetail.AddCell(bulidColumnDetail("", 10, 1, Element.ALIGN_CENTER));
                    tableDetail.AddCell(bulidColumnDetail(NumberToText.ThaiBahtText(invoiceDocumentModel.invoiceitems.Select(x => x.Qty).Sum().ToString()), 10, 1, Element.ALIGN_LEFT));
                    tableDetail.AddCell(bulidColumnDetail(invoiceDocumentModel.invoiceitems.Select(x => x.Qty).Sum().ToString(), 10, 1, Element.ALIGN_RIGHT));
                    tableDetail.AddCell(bulidColumnDetail("", 10, 2, Element.ALIGN_CENTER));
                    tableDetail.AddCell(bulidColumnDetail(invoiceDocumentModel.invoiceitems.Select(x => x.Qty * x.Price).Sum().ToString("n2"), 10, 1, Element.ALIGN_RIGHT));

                    tableDetail.AddCell(bulidColumnDetail("", 10, 1, Element.ALIGN_CENTER));
                    tableDetail.AddCell(bulidColumnDetail(invoiceDocumentModel.Remark, 10, 3, Element.ALIGN_LEFT,2));
                    tableDetail.AddCell(bulidColumnDetail("", 10, 2, Element.ALIGN_CENTER));
                    tableDetail.AddCell(bulidColumnDetail("", 10, 1, Element.ALIGN_CENTER));

                    tableDetail.AddCell(bulidColumnDetail("", 10, 1, Element.ALIGN_CENTER));

                    tableDetail.AddCell(bulidColumnDetail("", 10, 2, Element.ALIGN_CENTER));
                    tableDetail.AddCell(bulidColumnDetail(invoiceDocumentModel.invoiceitems.Select(x => x.Qty * x.Price).Sum().ToString("n2"), 10, 1, Element.ALIGN_RIGHT));

                    tableDetail.AddCell(bulidColumnDetail("", 10, 1, Element.ALIGN_CENTER));
                    tableDetail.AddCell(bulidColumnDetail("", 10, 1, Element.ALIGN_CENTER));
                    tableDetail.AddCell(bulidColumnDetail("", 10, 1, Element.ALIGN_CENTER));
                    tableDetail.AddCell(bulidColumnDetail("", 10, 1, Element.ALIGN_CENTER));
                    tableDetail.AddCell(bulidColumnFooster(3 ,Element.ALIGN_RIGHT));
           
              
                    table.AddCell(tableDetail);


                    document.NewPage();
                    document.Add(table);
                    tableDetail = new PdfPTable(widthBoxs.Count());
                    tableDetail.WidthPercentage = 95f;
                    count = 0;                  
                }
                count++;
            }

        

            //PdfPTable tablefooster = new PdfPTable(widthBoxs.Count());
            //tablefooster.WidthPercentage = 95f;

            document.Close();
            return memoryStream;
        }
        private PdfPCell bulidEmptryRows(int Col)
        {
            PdfPCell cell = new PdfPCell();
            cell.Colspan = Col;
            cell.Border = 0;
            cell.FixedHeight = 20f;
            return cell;
        }
        private PdfPCell bulidColumnTitleBox(string str, int size, int Colspan, int Align, int border)
        {

            Font font = new Font(this._bf, size);
            Phrase pharse = new Phrase(str, font);


            PdfPCell cell = new PdfPCell(pharse);
            cell.Colspan = Colspan;
            cell.Border = border;
            cell.VerticalAlignment = Element.ALIGN_TOP;
            cell.HorizontalAlignment = Align;
            return cell;
        }
        private PdfPCell bulidColumnFooster(string str, int size, int Colspan)
        {

            Font font = new Font(this._bf, size);
            Phrase pharse = new Phrase(str, font);


            PdfPCell cell = new PdfPCell(pharse);
            cell.Colspan = Colspan;
            cell.Border = 0;
            cell.VerticalAlignment = Element.ALIGN_TOP;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            return cell;
        }
        public PdfPCell bulidColumnFooster(int col, int Align)
        {
         
            var ImagePath = _hostingEnvironment.ContentRootPath + "\\PDF\\Image\\license_Invioce.jpg";

            iTextSharp.text.Image pic = iTextSharp.text.Image.GetInstance(ImagePath);
            pic.ScaleAbsolute(70, 35);


            PdfPCell cell = new PdfPCell(pic);
            cell.Colspan = col;
            cell.Border = 0;
            cell.HorizontalAlignment = Align;
            return cell;
        }
        public PdfPCell bulidColumnDetail(string str, int size, int col, int Align)
        {

            Font font = new Font(this._bf, size);
            Phrase pharse = new Phrase(str, font);


            PdfPCell cell = new PdfPCell(pharse);
            cell.Colspan = col;

            cell.Border = 0;
            cell.HorizontalAlignment = Align;
            return cell;
        }
        public PdfPCell bulidColumnDetail(string str, int size, int col, int Align,int row)
        {

            Font font = new Font(this._bf, size);
            Phrase pharse = new Phrase(str, font);


            PdfPCell cell = new PdfPCell(pharse);
            cell.Colspan = col;
            cell.Rowspan = row;
            cell.Border = 0;
            cell.HorizontalAlignment = Align;
            return cell;
        }
        private PdfPCell bulidColumnHeader(string str, int size)
        {
            Font font = new Font(this._bf, size);
            Phrase pharse = new Phrase(str, font);

            PdfPCell cell = new PdfPCell(pharse);
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.FixedHeight = 25f;
            cell.Border = Rectangle.BOTTOM_BORDER;
            return cell;
        }
        public class HeaderFooterPageEvent : PdfPageEventHelper
        {

         public void onStartPage(PdfWriter writer, Document document)
            {
             

                //tabletitle.AddCell(bulidColumnDetail("", 10, 1, Element.ALIGN_CENTER));
                //tabletitle.AddCell(bulidColumnDetail(invoiceDocumentModel.DocNum, 10, 1, Element.ALIGN_CENTER));
                //tabletitle.AddCell(bulidColumnDetail("", 10, 1, Element.ALIGN_CENTER));
                //tabletitle.AddCell(bulidColumnDetail(invoiceDocumentModel.DocNum, 10, 1, Element.ALIGN_CENTER));
                //document.Add(tabletitle);
            }

            public void onEndPage(PdfWriter writer, Document document)
        {
            //ColumnText.showTextAligned(writer.getDirectContent(), Element.ALIGN_CENTER, new Phrase("http://www.xxxx-your_example.com/"), 110, 30, 0);
            //ColumnText.showTextAligned(writer.getDirectContent(), Element.ALIGN_CENTER, new Phrase("page " + document.getPageNumber()), 550, 30, 0);
        }

    }
}

}
