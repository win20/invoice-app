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
using System.Collections;
using System.Text.RegularExpressions;
using System.Drawing.Printing;
using System.IO;
using Microsoft.Win32;
using System.Xml.XPath;
using System.Xml;
using PdfSharp.Pdf;
using System.Diagnostics;
using System.Collections.Specialized;

namespace InvoicingApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        public static List<StackPanel> stackArr = new List<StackPanel>();
        
        public static List<float> priceArr = new List<float>();
        public float subTotal;
        public static float vat;
        public float totalPrice;

        public const string UserSettingsFilename = "settings.xml";
        public static int invoiceNum = int.Parse(UserSettings.ReadValueFromXML("invoiceNum"));
        public static PdfMaker.DocumentType DocumentType = PdfMaker.DocumentType.INVOICE;

        public MainWindow()
        {
            InitializeComponent();
            InitializeData();         
        }

        private void InitializeData()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            stackArr.Add(itemsStackInner);

            // Set date
            DateTime date = DateTime.Today;
            dateTxt.Text = date.ToString("d");

            // Set invoice no
            invoiceNumTxt.Text = invoiceNum.ToString("0000");


        }

        private void TextBox_MouseEnter(object sender, MouseEventArgs e)
        {
            MessageBox.Show("mouse enter");
        }

        private void Quote_Btn(object sender, RoutedEventArgs e)
        {
            invoiceTo.Text = "QUOTE TO:";
            InvoiceNum.Text = "Quote No";
            DocumentType = PdfMaker.DocumentType.QUOTATION;
        }

        private void Invoice_Btn(object sender, RoutedEventArgs e)
        {
            invoiceTo.Text = "INVOICE TO:";
            InvoiceNum.Text = "Invoice No";
            DocumentType = PdfMaker.DocumentType.INVOICE;
        }

        // Validate and calculate price
        private void calculatePrice(object sender, RoutedEventArgs e)
        {
            float qtyParseOut = 0;

            if (float.TryParse(qtyTxt.Text, out qtyParseOut) == true)
            {
                if (rateTxt.Text != "" && qtyTxt.Text != "")
                {
                    float price = float.Parse(qtyTxt.Text) * float.Parse(rateTxt.Text);
                    float rate = float.Parse(rateTxt.Text);
                    priceTxt.Text = price.ToString("0.00");
                    rateTxt.Text = rate.ToString("0.00");
                }
                else
                {
                    qtyTxt.Text = qtyTxt.Text;
                }
            }
            else if (float.TryParse(rateTxt.Text, out qtyParseOut) == false)
            {
                rateTxt.Text = String.Empty;
            }
            else
            {
                qtyTxt.Text = qtyTxt.Text;

                float rate = float.Parse(rateTxt.Text);
                priceTxt.Text = rate.ToString("0.00");
                rateTxt.Text = rate.ToString("0.00");
            }

            // Calculate prices and update textblocks
            priceArr.Clear();
            for (int i = 0; i < stackArr.Count; i++)
            {
                foreach (object child in stackArr[i].Children)
                {
                    if (((FrameworkElement)child).Name == "priceTxt")
                    {
                        priceArr.Add( float.Parse(((TextBlock) child).Text) );
                    }
                }
            }

            subTotal = priceArr.Sum();
            subTotalTxt.Text = subTotal.ToString("C2");

            totalPrice = subTotal + vat;
            totalTxt.Text = totalPrice.ToString("C2");
        }

        private void calcPrice(Object sender, TextBox qtyTxt, TextBox rateTxt, TextBlock priceTxt) 
        {
            float qtyParseOut = 0;

            if (float.TryParse(qtyTxt.Text, out qtyParseOut) == true)
            {
                if (rateTxt.Text != "" && qtyTxt.Text != "")
                {
                    float price = float.Parse(qtyTxt.Text) * float.Parse(rateTxt.Text);
                    float rate = float.Parse(rateTxt.Text);

                    priceTxt.Text = price.ToString("0.00");
                    rateTxt.Text = rate.ToString("0.00");

                }
                else
                {
                    qtyTxt.Text = qtyTxt.Text;
                }
            }
            else if (float.TryParse(rateTxt.Text, out qtyParseOut) == false)
            {
                rateTxt.Text = String.Empty;
            }
            else
            {
                qtyTxt.Text = qtyTxt.Text;

                float rate = float.Parse(rateTxt.Text);
                priceTxt.Text = rate.ToString("0.00");
                rateTxt.Text = rate.ToString("0.00");
            }

            // Update all the price values
            priceArr.Clear();

            for (int i = 0; i < stackArr.Count; i++)
            {
                foreach (object child in stackArr[i].Children)
                {
                    if (((FrameworkElement)child).Name == "priceTxt")
                    {
                        priceArr.Add(float.Parse(((TextBlock)child).Text));
                    }
                }
            }

            subTotal = priceArr.Sum();
            subTotalTxt.Text = subTotal.ToString("C2");

            totalPrice = subTotal + vat;
            totalTxt.Text = totalPrice.ToString("C2");
        }

        // Used for VAT textbox for placeholder
        private void clearVAT(object sender, RoutedEventArgs e)
        {
            if (vatTxt.Text == "£0.00")
            {
                vatTxt.Text = String.Empty;
            }
            else
            {
                vatTxt.Text = vatTxt.Text;
            }
        }

        // Update VAT box with default values depending on context
        private void updateVAT(object sender, RoutedEventArgs e)
        {
            String vatStr = vatTxt.Text.ToString();
            String vatFloat = vatStr.Replace("£", "");      // holds the value of the vat after removing the £ sign

            //vatFloat = vatStr.Replace("£", "");

            float vatParseOut = 0;
            if (vatStr == String.Empty)
            {
                vatTxt.Text = "£0.00";
            }
            else if (float.TryParse(vatFloat, out vatParseOut) == false)
            {
                vatTxt.Text = "£0.00";
            }
            else
            {
                vat = float.Parse(vatFloat);
                vatTxt.Text = vat.ToString("C2");

                totalPrice = subTotal + vat;

                totalTxt.Text = totalPrice.ToString("C2");
            }
        }

        // Remove data line and data associated with it
        private void deleteItem(Object sender, RoutedEventArgs e)
        {
            if (stackArr.Count > 1) {

                StackPanel lastPanel = stackArr.Last();
                
                foreach (Object item in lastPanel.Children)
                {
                    if (((FrameworkElement)item).Name == "priceTxt")
                    {
                        if (((TextBlock)item).Text != "0.00")
                        {
                            if (stackArr.Count > 4)
                            {
                                stackArr.Remove(stackArr.Last());

                                priceArr.Remove(priceArr.Last());
                                subTotal = priceArr.Sum();
                                subTotalTxt.Text = subTotal.ToString("C2");

                                totalTxt.Text = (subTotal + vat).ToString("C2");
                                itemsStackOut.Children.RemoveAt(stackArr.Count);

                                //Update the page elements
                                double currentPageHeight = pageCanvas.ActualHeight;
                                pageCanvas.Height = currentPageHeight - 60;

                                double currentPageBGHeight = pageCanvasBG.ActualHeight;
                                pageCanvasBG.Height = currentPageBGHeight - 60;
                            }
                            else
                            {
                                stackArr.Remove(stackArr.Last());

                                priceArr.Remove(priceArr.Last());
                                subTotal = priceArr.Sum();
                                subTotalTxt.Text = subTotal.ToString("C2");

                                totalTxt.Text = (subTotal + vat).ToString("C2");
                                itemsStackOut.Children.RemoveAt(stackArr.Count);
                            }           
                        }
                        else
                        {
                            if (stackArr.Count > 4)
                            {
                                itemsStackOut.Children.Remove(stackArr.Last());
                                stackArr.Remove(stackArr.Last());

                                //Update the page elements
                                double currentPageHeight = pageCanvas.ActualHeight;
                                pageCanvas.Height = currentPageHeight - 60;

                                double currentPageBGHeight = pageCanvasBG.ActualHeight;
                                pageCanvasBG.Height = currentPageBGHeight - 60;
                            }
                            else
                            {
                                itemsStackOut.Children.Remove(stackArr.Last());
                                stackArr.Remove(stackArr.Last());
                            }
                        }
                        
                    }
                }                
            }         
        }

        private void addItem(object sender, RoutedEventArgs e)
        {
            // Get watermark style
            Style watermarkStyle = (Style)FindResource("WatermarkTextBox");

            // Line stack
            StackPanel itemsLine = new StackPanel();
            itemsLine.Orientation = Orientation.Horizontal;
            itemsLine.Margin = new Thickness(0, 0, 0, 30);

            // Job description
            TextBox jobDesc = new TextBox();
            jobDesc.Name = "jobDescTxt";
            jobDesc.Style = watermarkStyle;
            jobDesc.Tag = "Enter Job Description";
            jobDesc.TextWrapping = TextWrapping.Wrap;
            jobDesc.AcceptsReturn = true;
            jobDesc.VerticalAlignment = VerticalAlignment.Top;
            jobDesc.MinWidth = 395;
            jobDesc.MaxWidth = 395;
            jobDesc.MinHeight = 25;
            jobDesc.MaxLength = 160;
            jobDesc.Padding = new Thickness(3);
            jobDesc.Foreground = Brushes.Black;

            // QTY
            TextBox qty = new TextBox();
            qty.Style = watermarkStyle;
            qty.Tag = "0";
            qty.Margin = new Thickness(52, 0, 0, 0);
            qty.Width = 46;
            qty.Height = 25;
            qty.VerticalAlignment = VerticalAlignment.Top;
            qty.Padding = new Thickness(3);
            qty.Foreground = Brushes.Black;

            TextBox rate = new TextBox();
            TextBlock price = new TextBlock();
            qty.LostFocus += (sender, e) => calcPrice(sender, qty, rate, price);

            // £ Sign
            TextBlock poundSign = new TextBlock();
            poundSign.FontSize = 13;
            poundSign.Margin = new Thickness(40, 3, 0, 0);
            poundSign.VerticalAlignment = VerticalAlignment.Top;
            poundSign.Text = "£";

            // Rate
            rate = new TextBox();
            rate.Style = watermarkStyle;
            rate.Tag = "0.00";
            rate.VerticalAlignment = VerticalAlignment.Top;
            rate.Margin = new Thickness(5, 0, 0, 0);
            rate.Width = 64;
            rate.Height = 25;
            rate.TextAlignment = TextAlignment.Left;
            rate.Padding = new Thickness(3);
            rate.LostFocus += (sender, e) => calcPrice(sender, qty, rate, price);
            rate.Foreground = Brushes.Black;

            // £ Sign
            TextBlock poundSign2 = new TextBlock();
            poundSign2.FontSize = 13;
            poundSign2.Margin = new Thickness(0, 3, 0, 0);
            poundSign2.VerticalAlignment = VerticalAlignment.Top;
            poundSign2.Text = "£";

            // Price
            price = new TextBlock();
            price.Name = "priceTxt";
            price.Text = "0.00";
            price.FontSize = 13;
            price.TextAlignment = TextAlignment.Right;
            price.Margin = new Thickness(0, 3, 0, 0);
            price.Height = 25;
            price.Width = 62;
            price.VerticalAlignment = VerticalAlignment.Top;
            price.Foreground = Brushes.Black;

            // Add items to line stack
            itemsLine.Children.Add(jobDesc);
            itemsLine.Children.Add(qty);
            itemsLine.Children.Add(poundSign);
            itemsLine.Children.Add(rate);
            itemsLine.Children.Add(poundSign2);
            itemsLine.Children.Add(price);

            // Add line stack to main stack
            itemsStackOut.Children.Add(itemsLine);
            stackArr.Add(itemsLine);         

            if (stackArr.Count > 4)
            {
                //Update the page elements
                double currentPageHeight = pageCanvas.ActualHeight;
                pageCanvas.Height = currentPageHeight + 60;

                double currentPageBGHeight = pageCanvasBG.ActualHeight;
                pageCanvasBG.Height = currentPageBGHeight + 60;
            }
        }

        private void ChangeFolder(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.ShowDialog();

            UserSettings.WriteValueTOXML("outputPath", dialog.SelectedPath);
        }

        
        private void SavePDF(object sender, RoutedEventArgs e)
        {
            String outPath = UserSettings.ReadValueFromXML("outputPath");
            String filename = invoiceNum.ToString() + ".pdf";
            String fullPath = outPath + "\\" + filename;


            if (outPath != String.Empty)
            {
                List<JobItem> jobItemList = new List<JobItem>();

                for (int i = 0; i < stackArr.Count; i++)
                {
                    JobItem jobItem = new JobItem();
                    StringCollection lines = new StringCollection();
                    List<string> items = new List<string>();
                    foreach (object child in stackArr[i].Children)
                    {
                        if (((FrameworkElement)child).Name == "jobDescTxt")
                        {
                            lines = GetNumberOfLines((TextBox)child);
                        }
                    }

                    foreach (var tb in stackArr[i].Children.OfType<TextBox>())
                    {
                        items.Add(tb.Text);
                    }
                    foreach (var tb in stackArr[i].Children.OfType<TextBlock>())
                    {
                        if (tb.Name == priceTxt.Name)
                        {
                            items.Add(tb.Text);
                        }
                    }

                    jobItem.descLines = lines;
                    jobItem.description = items[0];
                    jobItem.qty = items[1];
                    jobItem.rate = items[2];
                    jobItem.price = items[3];

                    jobItemList.Add(jobItem);
                }
          
                String clientName = clientNameTxt.Text;
                String address = addressTxt.Text;
                String postcode = postcodeTxt.Text;
                // invoiceNum
                String date = DateTime.Now.ToString("dd/MM/yyyy");
                // subtotal
                // vat;
                // totalPrice;
                StringCollection notes = GetNumberOfLines(notesTxt);

                PdfDocument document = PdfMaker.CreatePDF(DocumentType, invoiceNum, clientName, address, postcode, jobItemList, subTotal, vat, totalPrice, notes);

                try
                {
                    document.Save(fullPath);
                    invoiceNum++;

                    UserSettings.WriteValueTOXML("invoiceNum", invoiceNum.ToString());
                    invoiceNumTxt.Text = invoiceNum.ToString("0000");

                    MessageBox.Show("Document saved as: " + fullPath);
                }
                catch (IOException ex)
                {
                    MessageBox.Show("ERROR: Directory doesn't exist.");
                }            
            }
            else
            {
                MessageBox.Show("Please set a destination folder using the \"Change Folder\" button.");
            }         
        }

        private void EditPDF(object sender, RoutedEventArgs e)
        {

        }      

        private StringCollection GetNumberOfLines(TextBox textBox)
        {
            StringCollection lines = new StringCollection();
            int lineCount = textBox.LineCount;

            for (int i = 0; i < lineCount; i++)
            {
                lines.Add(textBox.GetLineText(i));
            }
            return lines;
        } 
    }  
}
