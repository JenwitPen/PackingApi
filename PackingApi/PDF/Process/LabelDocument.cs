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
    public class LabelDocument
    {
        private IHostingEnvironment _hostingEnvironment;

        Document _document;
        string[] headers;
        BaseFont _bf;
        public LabelDocument(IHostingEnvironment hostingEnvironment)
        {
            this._hostingEnvironment = hostingEnvironment;
            Config config = new Config(this._hostingEnvironment);
            this._document = config.setPageSize();
            this._bf = config.getTHSarabun();
        }

        public Stream CreatePDF(LabelDocumentModel labelDocumentModel)
        {
            System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
            Document document = this._document;
            PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
            document.Open();

            float[] widthBoxs = new float[] { 75f, 20f };
            PdfPTable tableBox = new PdfPTable(widthBoxs.Count());

            tableBox.SetWidths(widthBoxs);
            tableBox.WidthPercentage = 95f;

            PdfPTable tableTopLeft = new PdfPTable(1);
            tableTopLeft.WidthPercentage = 90f;
            tableTopLeft.AddCell(bulidColumnTitleBox(@"ผู้รับเงิน", 18, 1, Element.ALIGN_LEFT, 0));
            tableTopLeft.AddCell(bulidColumnTitleBox("   " + labelDocumentModel.CardName, 16, 1, Element.ALIGN_LEFT, 0));
            tableTopLeft.AddCell(bulidColumnTitleBox("   " + labelDocumentModel.Address, 16, 1, Element.ALIGN_LEFT, 0));
            tableTopLeft.AddCell(bulidColumnTitleBox("   " + @"โทรศัพท์", 16, 1, Element.ALIGN_LEFT, 0));

            PdfPTable tableTopRight = new PdfPTable(1);
            tableTopRight.WidthPercentage = 90f;
            tableTopRight.AddCell(bulidColumnTitleBox(@"จำนวน", 18, 1, Element.ALIGN_CENTER, 1));
            tableTopRight.AddCell(bulidColumnTitleBox(@"1", 30, 1, Element.ALIGN_CENTER, 1));
            tableTopRight.AddCell(bulidColumnTitleBox(@"1", 30, 1, Element.ALIGN_CENTER, 1));

            PdfPCell cellLeft = new PdfPCell(tableTopLeft);
            tableBox.AddCell(cellLeft);

            PdfPCell cellRight = new PdfPCell(tableTopRight);
            tableBox.AddCell(cellRight);

            document.Add(tableBox);
            //------------------------------------------------------
            float[] widths = new float[] { 20f, 70f };
            PdfPTable tableDetail = new PdfPTable(widths.Count());

            tableDetail.WidthPercentage = 90f;
            tableDetail.SetWidths(widths);

            tableDetail.AddCell(bulidColumnDetail(@"PO", 16));
            tableDetail.AddCell(bulidColumnDetail(@"", 14));

            tableDetail.AddCell(bulidColumnDetail(@"ใบเสร็จ", 16));
            tableDetail.AddCell(bulidColumnDetail(labelDocumentModel.DocNum, 14));

            tableDetail.AddCell(bulidColumnDetail(@"วันที่ออกใบเสร็จ", 16));
            tableDetail.AddCell(bulidColumnDetail(labelDocumentModel.DocDate == null ? "" : labelDocumentModel.DocDate.Value.ToString("dd/MM/yyyy"), 14));

            tableDetail.AddCell(bulidColumnDetail(@"วันที่ส่งสินค้า", 16));
            tableDetail.AddCell(bulidColumnDetail(labelDocumentModel.DocDueDate == null ? "" : labelDocumentModel.DocDueDate.Value.ToString("dd/MM/yyyy"), 14));

            tableDetail.AddCell(bulidColumnDetail(@"คำสั่งพิเศษ(ระบุ)", 16));
            tableDetail.AddCell(bulidColumnDetail(labelDocumentModel.Remark, 14));
            document.Add(tableDetail);

            //------------------------------------------------------
            float[] Itemwidths = new float[] { 10f, 75f, 10f };
            string[] headers = new string[] { "No.", "รายการ", "จำนวน" };
            PdfPTable tableItem = new PdfPTable(Itemwidths.Count());
            tableItem.WidthPercentage = 95f;
            tableItem.SetWidths(Itemwidths);
            tableItem.HeaderRows = 2;
            tableItem.AddCell(bulidTextDeteail(@"โปรดตรวจสอบสินค้าทันที หากไม่ถูกต้องกรุณาแจ้งบริษัทภายใน 15 วัน", 22, headers.Count()));


            foreach (string h in headers)
            {
                tableItem.AddCell(bulidColumnHeader(h, 16));
            }
            int no = 1;
            foreach (var i in labelDocumentModel.labelItems)
            {
                bool isLast = false;
                if (no == labelDocumentModel.labelItems.Count())
                {
                    isLast = true;
                }

                tableItem.AddCell(bulidColumnRows(no.ToString(), 14, Element.ALIGN_CENTER, isLast));
                tableItem.AddCell(bulidColumnRows(i.Dscription, 14, Element.ALIGN_LEFT, isLast));
                tableItem.AddCell(bulidColumnRows(i.Quantity.ToString(), 14, Element.ALIGN_CENTER, isLast));
                no++;
            }
            document.Add(tableItem);
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
            cell.Padding = 8;
            cell.Border = border;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Align;
            return cell;
        }
        private PdfPCell bulidTextDeteail(string str, int size, int col)
        {

            Font font = new Font(this._bf, size, Font.BOLD);

            Phrase pharse = new Phrase(str, font);


            PdfPCell cell = new PdfPCell(pharse);
            cell.Colspan = col;
            cell.Border = 0;
            cell.Padding = 20f;
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            return cell;
        }
        private PdfPCell bulidColumnDetail(string str, int size)
        {

            Font font = new Font(this._bf, size);
            Phrase pharse = new Phrase(str, font);


            PdfPCell cell = new PdfPCell(pharse);
            cell.Colspan = 1;
            cell.Border = 0;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            return cell;
        }
        private PdfPCell bulidColumnHeader(string str, int size)
        {
            Font font = new Font(this._bf, size);
            Phrase pharse = new Phrase(str, font);

            PdfPCell cell = new PdfPCell(pharse);
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.FixedHeight = 25f;
            cell.Border = Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER | Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER;
            return cell;
        }
        private PdfPCell bulidColumnRows(string str, int size, int align, bool lastRow)
        {

            Font font = new Font(this._bf, size);
            Phrase pharse = new Phrase(str, font);

            PdfPCell cell = new PdfPCell(pharse);
            cell.HorizontalAlignment = align;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.Padding = 5;
            if (lastRow)
            {
                cell.Border = Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER | Rectangle.BOTTOM_BORDER;
            }
            else
            {
                cell.Border = Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER;
            }
            return cell;
        }
    }

}
