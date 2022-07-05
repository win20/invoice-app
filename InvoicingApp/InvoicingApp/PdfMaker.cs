using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.IO;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.Printing;
using System.Drawing.Printing;
using System.Windows.Xps.Packaging;
using System.IO.Packaging;
using System.Windows.Xps;
using System.Collections.Specialized;

namespace InvoicingApp
{
    public class PdfMaker
    {
        public enum DocumentType
        {
            INVOICE,
            QUOTATION
        }
        public static PdfDocument CreatePDF(DocumentType docTypeP, int invoiceNumP, String clientNameP, String addressP, String postcodeP, List<JobItem> jobItemsP, float subtotalP, float vatP, float totalPriceP, StringCollection notesP)
        {
            PdfDocument document = new PdfDocument();
            document.Info.Title = "Invoice PDF";
            PdfPage page = document.AddPage();
            page.Size = PageSize.A4;
            XGraphics gfx = XGraphics.FromPdfPage(page);

            // ==== HEADER =====
            XFont fontHeader = new("Segoe UI Semibold", 20, XFontStyle.Regular);
            XFont fontSubHeader = new("Segoe UI", 12, XFontStyle.Regular);

            gfx.DrawString("East London Electrical Services LTD", fontHeader, XBrushes.Black,
                new XRect(0, 30, page.Width, page.Height),
                XStringFormats.TopCenter);

            gfx.DrawString("162 Stevens Road, Dagenham, London RM8 2PU", fontSubHeader, XBrushes.Black,
                new XRect(0, 60, page.Width, page.Height),
                XStringFormats.TopCenter);

            gfx.DrawString("Mobile No: 07908207915, Email: paban43@yahoo.com", fontSubHeader, XBrushes.Black,
                new XRect(0, 75, page.Width, page.Height),
                XStringFormats.TopCenter);

            XPen line = new XPen(XColors.Black, 0.5);
            gfx.DrawLine(line, 45, 100, 550, 100);

            // ==== INVOICE TO ===== 
            XFont fontInvoiceTo = new("Segoe UI Semibold", 12, XFontStyle.Regular);
            XFont fontPlaceholder = new("Segoe UI", 10, XFontStyle.Regular);

            if (docTypeP == DocumentType.INVOICE)
            {
                gfx.DrawString("INVOICE TO:", fontInvoiceTo, XBrushes.Black,
                new XRect(45, 140, page.Width, page.Height),
                XStringFormats.TopLeft);
            }
            else
            {
                gfx.DrawString("QUOTE TO:", fontInvoiceTo, XBrushes.Black,
                new XRect(45, 140, page.Width, page.Height),
                XStringFormats.TopLeft);
            }          

            gfx.DrawString(clientNameP, fontPlaceholder, XBrushes.Black,
                new XRect(45, 165, page.Width, page.Height),
                XStringFormats.TopLeft);

            gfx.DrawString(addressP, fontPlaceholder, XBrushes.Black,
                new XRect(45, 182, page.Width, page.Height),
                XStringFormats.TopLeft);

            gfx.DrawString(postcodeP, fontPlaceholder, XBrushes.Black,
                new XRect(45, 199, page.Width, page.Height),
                XStringFormats.TopLeft);

            // ==== INVOICE NUMBER =====
            XFont fontBold = new("Segoe UI Semibold", 11, XFontStyle.Regular);

            if (docTypeP == DocumentType.INVOICE)
            {
                gfx.DrawString("Invoice No", fontBold, XBrushes.Black,
                new XRect(400, 179, page.Width, page.Height),
                XStringFormats.TopLeft);
            }
            else
            {
                gfx.DrawString("Quote No", fontBold, XBrushes.Black,
                new XRect(400, 179, page.Width, page.Height),
                XStringFormats.TopLeft);
            }

            gfx.DrawString(invoiceNumP.ToString("0000"), fontPlaceholder, XBrushes.Black,
                new XRect(-40, 179, page.Width, page.Height),
                XStringFormats.TopRight);

            DateTime date = DateTime.Today;

            gfx.DrawString("Date", fontBold, XBrushes.Black,
                new XRect(400, 199, page.Width, page.Height),
                XStringFormats.TopLeft);

            gfx.DrawString(date.ToString("dd/MM/yyyy"), fontPlaceholder, XBrushes.Black,
                new XRect(-40, 199, page.Width, page.Height),
                XStringFormats.TopRight);

            // ==== JOBS HEADER =====
            XRect rectangle = new XRect(40, 250, 515, 30);
            XSolidBrush bgBrush = new XSolidBrush(XColors.Gray);
            gfx.DrawRectangle(bgBrush, rectangle);

            XFont fontJobHeader = new("Segoe UI Semibold", 13, XFontStyle.Regular);

            gfx.DrawString("Job Description", fontJobHeader, XBrushes.White,
                new XRect(45, 255, page.Width, page.Height),
                XStringFormats.TopLeft);

            gfx.DrawString("Qty", fontJobHeader, XBrushes.White,
                new XRect(350, 255, page.Width, page.Height),
                XStringFormats.TopLeft);

            gfx.DrawString("Rate", fontJobHeader, XBrushes.White,
                new XRect(430, 255, page.Width, page.Height),
                XStringFormats.TopLeft);

            gfx.DrawString("Price", fontJobHeader, XBrushes.White,
                new XRect(520, 255, page.Width, page.Height),
                XStringFormats.TopLeft);

            // ==== ITEMS =====
            if (jobItemsP.Count < 7)
            {
                int n = jobItemsP.Count;
                int yPos = 290;

                for (int i = 0; i < n; i++)
                {
                    int yDesc = yPos;
                    for (int j = 0; j < jobItemsP[i].descLines.Count; j++)
                    {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                        gfx.DrawString(jobItemsP[i].descLines[j].Trim(), fontPlaceholder, XBrushes.Black,
                        new XRect(45, yDesc, page.Width, page.Height),
                        XStringFormats.TopLeft);
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                        yDesc += 12;
                    }

                    gfx.DrawString(jobItemsP[i].qty, fontPlaceholder, XBrushes.Black,
                        new XRect(65, yPos, page.Width, page.Height),
                        XStringFormats.TopCenter);

                    gfx.DrawString("£" + jobItemsP[i].rate, fontPlaceholder, XBrushes.Black,
                        new XRect(145, yPos, page.Width, page.Height),
                        XStringFormats.TopCenter);

                    gfx.DrawString("£" + jobItemsP[i].price, fontPlaceholder, XBrushes.Black,
                        new XRect(-45, yPos, page.Width, page.Height),
                        XStringFormats.TopRight);

                    yPos += 50;

                    if (i == 5)
                    {
                        break;
                    }
                }
                // ==== TOTALS ====
                gfx.DrawString("Sub Total", fontBold, XBrushes.Black,
                    new XRect(135, yPos + 20, page.Width, page.Height),
                    XStringFormats.TopCenter);

                gfx.DrawString(subtotalP.ToString("C2"), fontPlaceholder, XBrushes.Black,
                    new XRect(-45, yPos + 20, page.Width, page.Height),
                    XStringFormats.TopRight);

                gfx.DrawString("VAT", fontBold, XBrushes.Black,
                    new XRect(148, yPos + 40, page.Width, page.Height),
                    XStringFormats.TopCenter);

                gfx.DrawString(vatP.ToString("C2"), fontPlaceholder, XBrushes.Black,
                    new XRect(-45, yPos + 40, page.Width, page.Height),
                    XStringFormats.TopRight);

                gfx.DrawString("Total", fontBold, XBrushes.Black,
                    new XRect(145, yPos + 60, page.Width, page.Height),
                    XStringFormats.TopCenter);

                gfx.DrawString(totalPriceP.ToString("C2"), fontPlaceholder, XBrushes.Black,
                    new XRect(-45, yPos + 60, page.Width, page.Height),
                    XStringFormats.TopRight);

                // ==== BANK DETAILS =====
                XFont fontStandard = new("Segoe UI", 11, XFontStyle.Regular);
                XFont bold = new("Segoe UI", 12, XFontStyle.Bold);

                XRect bankDetailsRect = new XRect(40, 700, 200, 90);       // x, y, width, height
                gfx.DrawRectangle(bgBrush, bankDetailsRect);

                gfx.DrawString("BANK DETAILS", bold, XBrushes.White,
                    new XRect(45, 705, page.Width, page.Height),
                    XStringFormats.TopLeft);

                gfx.DrawString("East London Electrical Services LTD", fontStandard, XBrushes.White,
                    new XRect(45, 730, page.Width, page.Height),
                    XStringFormats.TopLeft);

                gfx.DrawString("Sort Code: 30-94-51", fontStandard, XBrushes.White,
                    new XRect(45, 747, page.Width, page.Height),
                    XStringFormats.TopLeft);

                gfx.DrawString("Acc. Number: 25757760", fontStandard, XBrushes.White,
                    new XRect(45, 764, page.Width, page.Height),
                    XStringFormats.TopLeft);

                // ==== NOTES/COMMENTS =====
                gfx.DrawString("NOTES", fontStandard, XBrushes.Black,
                    new XRect(300, 680, page.Width, page.Height),
                    XStringFormats.TopLeft);

                XRect notes = new XRect(300, 700, 250, 90);       // x, y, width, height
                XPen border = new XPen(XColors.Black);
                gfx.DrawRectangle(border, notes);

                XFont fontNotes = new XFont("Segoe UI", 10, XFontStyle.Regular);

                int yNotes = 705;
                for (int i = 0; i < notesP.Count; i++)
                {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    gfx.DrawString(notesP[i].Trim(), fontNotes, XBrushes.Black,
                    new XRect(308, yNotes, page.Width, page.Height),
                    XStringFormats.TopLeft);
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                    yNotes += 12;
                }
            }

            else if (jobItemsP.Count < 9)
            {
                int n = jobItemsP.Count;
                int yPos = 290;

                for (int i = 0; i < n; i++)
                {
                    int yDesc = yPos;
                    for (int j = 0; j < jobItemsP[i].descLines.Count; j++)
                    {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                        gfx.DrawString(jobItemsP[i].descLines[j].Trim(), fontPlaceholder, XBrushes.Black,
                        new XRect(45, yDesc, page.Width, page.Height),
                        XStringFormats.TopLeft);
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                        yDesc += 12;
                    }

                    gfx.DrawString(jobItemsP[i].qty, fontPlaceholder, XBrushes.Black,
                        new XRect(65, yPos, page.Width, page.Height),
                        XStringFormats.TopCenter);

                    gfx.DrawString("£" + jobItemsP[i].rate, fontPlaceholder, XBrushes.Black,
                        new XRect(145, yPos, page.Width, page.Height),
                        XStringFormats.TopCenter);

                    gfx.DrawString("£" + jobItemsP[i].price, fontPlaceholder, XBrushes.Black,
                        new XRect(-45, yPos, page.Width, page.Height),
                        XStringFormats.TopRight);

                    yPos += 50;

                    if (i == 7)
                    {
                        break;
                    }
                }
                // ==== TOTALS ====
                gfx.DrawString("Sub Total", fontBold, XBrushes.Black,
                    new XRect(135, yPos + 20, page.Width, page.Height),
                    XStringFormats.TopCenter);

                gfx.DrawString(subtotalP.ToString("C2"), fontPlaceholder, XBrushes.Black,
                    new XRect(-45, yPos + 20, page.Width, page.Height),
                    XStringFormats.TopRight);

                gfx.DrawString("VAT", fontBold, XBrushes.Black,
                    new XRect(148, yPos + 40, page.Width, page.Height),
                    XStringFormats.TopCenter);

                gfx.DrawString(vatP.ToString("C2"), fontPlaceholder, XBrushes.Black,
                    new XRect(-45, yPos + 40, page.Width, page.Height),
                    XStringFormats.TopRight);

                gfx.DrawString("Total", fontBold, XBrushes.Black,
                    new XRect(145, yPos + 60, page.Width, page.Height),
                    XStringFormats.TopCenter);

                gfx.DrawString(totalPriceP.ToString("C2"), fontPlaceholder, XBrushes.Black,
                    new XRect(-45, yPos + 60, page.Width, page.Height),
                    XStringFormats.TopRight);

                page = document.AddPage();
                gfx = XGraphics.FromPdfPage(page);

                // ==== BANK DETAILS =====
                XFont fontStandard = new("Segoe UI", 11, XFontStyle.Regular);
                XFont bold = new("Segoe UI", 12, XFontStyle.Bold);

                XRect bankDetailsRect = new XRect(40, 100, 200, 90);       // x, y, width, height
                gfx.DrawRectangle(bgBrush, bankDetailsRect);

                gfx.DrawString("BANK DETAILS", bold, XBrushes.White,
                    new XRect(45, 105, page.Width, page.Height),
                    XStringFormats.TopLeft);

                gfx.DrawString("East London Electrical Services LTD", fontStandard, XBrushes.White,
                    new XRect(45, 130, page.Width, page.Height),
                    XStringFormats.TopLeft);

                gfx.DrawString("Sort Code: 30-94-51", fontStandard, XBrushes.White,
                    new XRect(45, 147, page.Width, page.Height),
                    XStringFormats.TopLeft);

                gfx.DrawString("Acc. Number: 25757760", fontStandard, XBrushes.White,
                    new XRect(45, 164, page.Width, page.Height),
                    XStringFormats.TopLeft);

                // ==== NOTES/COMMENTS =====
                gfx.DrawString("NOTES", fontStandard, XBrushes.Black,
                    new XRect(300, 80, page.Width, page.Height),
                    XStringFormats.TopLeft);

                XRect notes = new XRect(300, 100, 250, 90);       // x, y, width, height
                XPen border = new XPen(XColors.Black);
                gfx.DrawRectangle(border, notes);

                XFont fontNotes = new XFont("Segoe UI", 10, XFontStyle.Regular);

                int yNotes = 705;
                for (int i = 0; i < notesP.Count; i++)
                {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    gfx.DrawString(notesP[i].Trim(), fontNotes, XBrushes.Black,
                    new XRect(308, yNotes, page.Width, page.Height),
                    XStringFormats.TopLeft);
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                    yNotes += 12;
                }
            }
            else
            {
                int n = jobItemsP.Count;
                int yPos = 290;

                for (int i = 0; i < n; i++)
                {
                    int yDesc = yPos;
                    for (int j = 0; j < jobItemsP[i].descLines.Count; j++)
                    {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                        gfx.DrawString(jobItemsP[i].descLines[j].Trim(), fontPlaceholder, XBrushes.Black,
                        new XRect(45, yDesc, page.Width, page.Height),
                        XStringFormats.TopLeft);
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                        yDesc += 12;
                    }

                    gfx.DrawString(jobItemsP[i].qty, fontPlaceholder, XBrushes.Black,
                        new XRect(65, yPos, page.Width, page.Height),
                        XStringFormats.TopCenter);

                    gfx.DrawString("£" + jobItemsP[i].rate, fontPlaceholder, XBrushes.Black,
                        new XRect(145, yPos, page.Width, page.Height),
                        XStringFormats.TopCenter);

                    gfx.DrawString("£" + jobItemsP[i].price, fontPlaceholder, XBrushes.Black,
                        new XRect(-45, yPos, page.Width, page.Height),
                        XStringFormats.TopRight);

                    yPos += 50;

                    if (i == 7)
                    {
                        break;
                    }
                }


                page = document.AddPage();
                gfx = XGraphics.FromPdfPage(page);

                yPos = 50;

                for (int i = 8; i < n; i++)
                {
                    int yDesc = yPos;
                    for (int j = 0; j < jobItemsP[i].descLines.Count; j++)
                    {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                        gfx.DrawString(jobItemsP[i].descLines[j].Trim(), fontPlaceholder, XBrushes.Black,
                        new XRect(45, yDesc, page.Width, page.Height),
                        XStringFormats.TopLeft);
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                        yDesc += 12;
                    }

                    gfx.DrawString(jobItemsP[i].qty, fontPlaceholder, XBrushes.Black,
                        new XRect(65, yPos, page.Width, page.Height),
                        XStringFormats.TopCenter);

                    gfx.DrawString("£" + jobItemsP[i].rate, fontPlaceholder, XBrushes.Black,
                        new XRect(145, yPos, page.Width, page.Height),
                        XStringFormats.TopCenter);

                    gfx.DrawString("£" + jobItemsP[i].price, fontPlaceholder, XBrushes.Black,
                        new XRect(-45, yPos, page.Width, page.Height),
                        XStringFormats.TopRight);

                    yPos += 50;

                }

                // ==== TOTALS ====
                gfx.DrawString("Sub Total", fontBold, XBrushes.Black,
                    new XRect(135, yPos + 20, page.Width, page.Height),
                    XStringFormats.TopCenter);

                gfx.DrawString(subtotalP.ToString("C2"), fontPlaceholder, XBrushes.Black,
                    new XRect(-45, yPos + 20, page.Width, page.Height),
                    XStringFormats.TopRight);

                gfx.DrawString("VAT", fontBold, XBrushes.Black,
                    new XRect(148, yPos + 40, page.Width, page.Height),
                    XStringFormats.TopCenter);

                gfx.DrawString(vatP.ToString("C2"), fontPlaceholder, XBrushes.Black,
                    new XRect(-45, yPos + 40, page.Width, page.Height),
                    XStringFormats.TopRight);

                gfx.DrawString("Total", fontBold, XBrushes.Black,
                    new XRect(145, yPos + 60, page.Width, page.Height),
                    XStringFormats.TopCenter);

                gfx.DrawString(totalPriceP.ToString("C2"), fontPlaceholder, XBrushes.Black,
                    new XRect(-45, yPos + 60, page.Width, page.Height),
                    XStringFormats.TopRight);



                // ==== BANK DETAILS =====
                XFont fontStandard = new("Segoe UI", 11, XFontStyle.Regular);
                XFont bold = new("Segoe UI", 12, XFontStyle.Bold);

                XRect bankDetailsRect = new XRect(40, 700, 200, 90);       // x, y, width, height
                gfx.DrawRectangle(bgBrush, bankDetailsRect);

                gfx.DrawString("BANK DETAILS", bold, XBrushes.White,
                    new XRect(45, 705, page.Width, page.Height),
                    XStringFormats.TopLeft);

                gfx.DrawString("East London Electrical Services LTD", fontStandard, XBrushes.White,
                    new XRect(45, 730, page.Width, page.Height),
                    XStringFormats.TopLeft);

                gfx.DrawString("Sort Code: 30-94-51", fontStandard, XBrushes.White,
                    new XRect(45, 747, page.Width, page.Height),
                    XStringFormats.TopLeft);

                gfx.DrawString("Acc. Number: 25757760", fontStandard, XBrushes.White,
                    new XRect(45, 764, page.Width, page.Height),
                    XStringFormats.TopLeft);

                // ==== NOTES/COMMENTS =====
                gfx.DrawString("NOTES", fontStandard, XBrushes.Black,
                    new XRect(300, 680, page.Width, page.Height),
                    XStringFormats.TopLeft);

                XRect notes = new XRect(300, 700, 250, 90);       // x, y, width, height
                XPen border = new XPen(XColors.Black);
                gfx.DrawRectangle(border, notes);

                XFont fontNotes = new XFont("Segoe UI", 10, XFontStyle.Regular);

                int yNotes = 705;
                for (int i = 0; i < notesP.Count; i++)
                {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    gfx.DrawString(notesP[i].Trim(), fontNotes, XBrushes.Black,
                    new XRect(308, yNotes, page.Width, page.Height),
                    XStringFormats.TopLeft);
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                    yNotes += 12;
                }
            }
            return document;
        }
    }
}
