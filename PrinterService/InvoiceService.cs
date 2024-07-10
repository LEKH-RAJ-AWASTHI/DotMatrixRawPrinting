using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrintingRaw;

namespace GenerateRawTextToPrint
{
    /// <summary>
    /// A service class to generate raw text for printing invoices for dot matrix printers
    /// </summary>
    public class InvoiceService
    {
        /// <summary>
        /// CurrentLine is variable that will help to track the current line in the page. And will help to find end of the page.
        /// </summary>
        private int currentLine = 0;
        /// <summary>
        /// Number of lines per page
        /// </summary>
        private int numberOfLines = 0;
        /// <summary>
        /// Number of characters printed in a line.
        /// </summary>
        private int LinePrintableCharacters;
        /// <summary>
        /// Number of lines in a page that will occupy the header.
        /// </summary>
        private int HeaderLines;
        /// <summary>
        /// Number of line in a page that will occupy footer.
        /// </summary>
        private int FooterLines;
        /// <summary>
        /// CRLF is short form of carriage return and line feed.
        /// </summary>
        private readonly string crlf = string.Format("\x0D\x0A");
        private string HeaderData;
        private string FooterData;
        private StringBuilder rawTextString = new StringBuilder();

        /// <summary>
        /// Generates raw text for an invoice.
        /// </summary>
        /// <param name="patientAndInvoiceInfo">JSON string containing patient and invoice information.</param>
        /// <param name="patientAndInvoiceInfoSettings">JSON string containing settings for patient and invoice information.</param>
        /// <param name="invoiceItemsData">JSON string containing invoice items data.</param>
        /// <param name="linesPerPage">Number of lines per page.</param>
        /// <param name="printableCharacters">Number of printable characters per line.</param>
        /// <param name="invoiceItemCharacterWidth">JSON string containing character width for invoice items.</param>
        /// <param name="headerLines">Number of header lines.</param>
        /// <param name="footerLines">Number of footer lines.</param>
        /// <returns>Raw text string for the invoice.</returns>
        public string GenerateInvoiceRawText(string WhatToPrint, string patientAndInvoiceInfo, string patientAndInvoiceInfoSettings, string invoiceItemsData, int linesPerPage, int printableCharacters, string invoiceItemCharacterWidth, int headerLines, int footerLines, string BillTotalDetail, string headerDataJson, string footerDataJson)
        {
            LinePrintableCharacters = printableCharacters;
            numberOfLines = linesPerPage - footerLines;
            HeaderLines = headerLines;
            FooterLines = footerLines;
            Invoice printSection = JsonConvert.DeserializeObject<Invoice>(WhatToPrint);

            HeaderData = printSection.Header ?headerDataJson : null;
            FooterData = printSection.Footer ?footerDataJson: null;

            var patientAndInvoiceInfoData = printSection.CustomerDetail ? JsonConvert.DeserializeObject<Dictionary<string, Field>>(patientAndInvoiceInfo): null;
            var patientAndInvoiceInfoSectionSettings = printSection.CustomerDetail ? JsonConvert.DeserializeObject<Dictionary<string, FieldSetting>>(patientAndInvoiceInfoSettings): null;
            var InvoiceFieldWidth = printSection.InvoiceItem ? JsonConvert.DeserializeObject<Dictionary<string, int>>(invoiceItemCharacterWidth): null;
            var invoiceItems = printSection.InvoiceItem ? JsonConvert.DeserializeObject(invoiceItemsData): null;
            BillTotal billTotal = printSection.TotalAmount ? JsonConvert.DeserializeObject<BillTotal>(BillTotalDetail): null;

            // checking if the invoiceItemCharacterWidth is exceeding printable character width
            if (InvoiceFieldWidth.Values.Sum() > printableCharacters)
            {
                return "Column width exceeds the printable characters in a line";
            }

            if (patientAndInvoiceInfoData is null || patientAndInvoiceInfoSectionSettings is null)
            {
                return string.Empty;
            }

            return BuildRawInvoiceText(HeaderData, patientAndInvoiceInfoData, patientAndInvoiceInfoSectionSettings, invoiceItems, printableCharacters, InvoiceFieldWidth, billTotal, FooterData);
        }
        /// <summary>
        /// Builds the raw text for an invoice.
        /// </summary>
        /// <param name="data">Dictionary containing patient and invoice information.</param>
        /// <param name="settings">Dictionary containing settings for patient and invoice information.</param>
        /// <param name="invoiceItems">Dynamic object containing invoice items data.</param>
        /// <param name="printableCharacters">Number of printable characters per line.</param>
        /// <param name="invoiceFieldColumnWidth">Dictionary containing column width for invoice items.</param>
        /// <returns>Raw text string for the invoice.</returns>
        private string BuildRawInvoiceText(string header, Dictionary<string, Field> data, Dictionary<string, FieldSetting> settings, dynamic invoiceItems, int printableCharacters, Dictionary<string, int> invoiceFieldColumnWidth, BillTotal billTotal, string footer)
        {
            if(data is null || settings is null || invoiceItems is null)
            {
                return  rawTextString.ToString();
            }
            if(header is null)
            {
                PrintHeaderOrFooter(crlf, HeaderLines);
            }
            else
            {
                PrintHeaderOrFooter(header, HeaderLines);
            }

            // Create a list of items with position and sequence
            var printItems = data.Select(d => new
            {
                DisplayLabel = d.Value.DisplayLabel,
                Value = d.Value.Value,
                DisplaySeq = int.Parse(settings[d.Key].DisplaySeq)
            }).OrderBy(item => item.DisplaySeq).ToList();

            // Print patient detail section
            PrintPatientDetails(printItems, printableCharacters);

            // Print invoice items

            PrintInvoiceItems(invoiceItems, printableCharacters, invoiceFieldColumnWidth);
            PrintBillTotalSection(billTotal, printableCharacters, invoiceFieldColumnWidth);
            PrintFooter(FooterData, FooterLines);

            return rawTextString.ToString();
        }
        /// <summary>
        /// Prints patient details. And add styling to the it. 
        /// </summary>
        /// <param name="printDetail">Dynamic object containing patient details to be printed.</param>
        /// <param name="printableCharacters">Number of printable characters per line.</param>

        private void PrintPatientDetails(dynamic printDetail, int printableCharacters)
        {
            if(printDetail is null)
            {
                return;
            }
            int eachSideWidth = printableCharacters / 2;
            for (int i = 0; i < printDetail.Count; i += 2)
            {
                // string lineLeftText = ApplyTextStyles(printDetail[i].DisplayLabel, TextStyling.bold) + ": " + printDetail[i].Value;
                // string lineRightText = ApplyTextStyles(printDetail[i + 1].DisplayLabel, TextStyling.bold) + ": " + printDetail[i + 1].Value;
                // Console.WriteLine(rawTextString.ToString());
                // rawTextString.AppendFormat($"{{0,-{eachSideWidth}}}{{1,{eachSideWidth}}}\x0A", lineLeftText, lineRightText);
                // Console.WriteLine(rawTextString.ToString());
                string lineLeftText = $"{printDetail[i].DisplayLabel}: {printDetail[i].Value}";
                string lineRightText = string.Empty;

                if (i + 1 < printDetail.Count)
                {
                    lineRightText = $"{printDetail[i + 1].DisplayLabel}: {printDetail[i + 1].Value}";
                }

                // Format the text with spaces
                string formattedText = string.Format($"{{0,-{eachSideWidth}}}{{1,{eachSideWidth}}}\x0A", lineLeftText, lineRightText);

                // Apply styling to the text
                string styledText = formattedText;
                styledText = styledText.Replace(printDetail[i].DisplayLabel, ApplyTextStyles(printDetail[i].DisplayLabel, TextStyling.bold));

                if (i + 1 < printDetail.Count)
                {
                    styledText = styledText.Replace(printDetail[i + 1].DisplayLabel, ApplyTextStyles(printDetail[i + 1].DisplayLabel, TextStyling.bold));
                }
                rawTextString.Append(styledText);
                CheckPageEnd();
            }
        }
        /// <summary>
        /// Prints invoice items.
        /// </summary>
        /// <param name="invoiceItems">Dynamic object containing invoice items data.</param>
        /// <param name="printableCharacters">Number of printable characters per line.</param>
        /// <param name="colWidth">Dictionary containing column width for invoice items.</param>

        private void PrintInvoiceItems(dynamic invoiceItems, int printableCharacters, Dictionary<string, int> colWidth)
        {
            if(invoiceItems is null)
            {
                return;
            }
            int TotalAmt = 0;
            if (invoiceItems is not null && invoiceItems.Count > 0)
            {
                rawTextString.AppendLine("\n");
                CheckPageEnd();

                // Dynamically generate headers based on properties in the first item
                var firstItem = (JObject)invoiceItems.First;
                List<string> headers = new List<string>();
                foreach (var prop in firstItem.Properties())
                {
                    headers.Add(prop.Name);
                }

                // Print headers
                FormatInvoiceRow(headers.ToArray(), printableCharacters, colWidth);

                // Print data rows
                foreach (var item in invoiceItems)
                {
                    // List<string> rowData = new List<string>();
                    // var rowData= headers.ToDictionary(header=> header, header => WrapTextToWidth(item[header].ToString(), colWidth[header], lea));
                    Dictionary<string, string> rowData = new Dictionary<string, string>();
                    int leadingSpaces = 0;
                    foreach (var prop in headers)
                    {
                        rowData.Add(prop, WrapTextToWidth(item[prop].ToString(), colWidth[prop], leadingSpaces));
                        leadingSpaces += colWidth[prop];
                        if (prop.ToString() == "TotalAmt")
                        {
                            TotalAmt += (int)item[prop];
                        }
                    }
                    // FormatInvoiceRow(rowData.ToArray(),printableCharacters, colWidth);
                    rawTextString.Append(Helper.FormatRow(rowData, printableCharacters, colWidth));
                    CheckPageEnd();
                }
                // int totalWidth= colWidth.Sum(kv => kv.Value); // calculates the total width of the column in the invoice item.
                // int eachSideWidth = totalWidth/2;
                // string lineLeftText = ApplyTextStyles("Total Amount", TextStyling.Bold) + ": ";
                // // string lineRightText = ApplyTextStyles(TotalAmt.ToString(), TextStyling.Bold);
                // rawTextString.AppendFormat($"{{0,{totalWidth}}}\x0A", lineLeftText+ TotalAmt);
            }
        }
        /// <summary>
        /// Applies text styles.
        /// </summary>
        /// <param name="text">Text to be styled.</param>
        /// <param name="styling">Styling to be applied.</param>
        /// <returns>Styled text.</returns>

        private void PrintBillTotalSection(BillTotal billTotal, int printableCharacters, Dictionary<string, int> colWidth)
        {
            if(billTotal is null)
            {
                return;
            }
            int totalColWidth = colWidth.Values.Sum();
            // Type type = typeof(BillTotal);
            // PropertyInfo[] properties= type.GetProperties();
            // foreach(PropertyInfo property in properties)
            // {
            //     string rowData= property.ToString().PadRight(totalColWidth)+crlf;
            //     rawTextString.Append(rowData);
            // }
            string subTotal = (ApplyTextStyles("Sub Total: ",TextStyling.bold)+billTotal.SubTotal.ToString()).PadLeft(totalColWidth)+crlf;
            string dis= (ApplyTextStyles("Discount %: ",TextStyling.bold)+billTotal.Dis.ToString()).PadLeft(totalColWidth)+crlf;
            string totalAmt= (ApplyTextStyles("Total Amount: ",TextStyling.bold)+billTotal.NetTotal.ToString()).PadLeft(totalColWidth)+crlf;
            string amtInWords = (ApplyTextStyles("Amount In Words: ",TextStyling.bold)+billTotal.AmountInWords).PadLeft(totalColWidth)+crlf;

            rawTextString.Append(subTotal);
            CheckPageEnd();
            rawTextString.Append(dis);
            CheckPageEnd();
            rawTextString.Append(totalAmt);
            CheckPageEnd();
            rawTextString.Append(amtInWords);
            CheckPageEnd(); 
        }
        private string ApplyTextStyles(string text, TextStyling styling)
        {
            StringBuilder lineText = new StringBuilder(text);
            switch (styling)
            {
                case TextStyling.bold:
                    lineText.Insert(0, "\x1B\x45");
                    lineText.Append("\x1B\x46");
                    break;
                case TextStyling.italic:
                    lineText.Insert(0, "\x1B\x34");
                    lineText.Append("\x1B\x35");
                    break;
                case TextStyling.double_strike:
                    lineText.Insert(0, "\x1B\x47");
                    lineText.Append("\x1B\x48");
                    break;
                case TextStyling.proportionalMode:
                    lineText.Insert(0, "\x1B\x70\x31");
                    lineText.Append("\x1B\x70\x30");
                    break;
                case TextStyling.codensed:
                    lineText.Insert(0, "\x0F");
                    lineText.Append("\x12");
                    break;
                case TextStyling.doubleWidth:
                    lineText.Insert(0, "\x0E");
                    lineText.Append("\x14");
                    break;
                case TextStyling.superScript:
                    lineText.Insert(0, "\x1B\x53\x30");
                    lineText.Append("\x1B\x54");
                    break;
                case TextStyling.subScript:
                    lineText.Insert(0, "\x1B\x53\x31");
                    lineText.Append("\x1B\x54");
                    break;
                case TextStyling.doubleHeight:
                    lineText.Insert(0, "\x1B\x77\x31");
                    lineText.Append("\x1B\x77\x30");
                    break;
                default:
                    break;
            }
            return lineText.ToString();
        }

        private enum TextStyling
        {
            bold,
            italic,
            // emphasized, == bold
            double_strike,
            proportionalMode,
            codensed,
            doubleWidth,
            doubleHeight,
            superScript,
            subScript,
        }
        /// <summary>
        /// Wraps text to a specified column width.
        /// </summary>
        /// <param name="text">Text to be wrapped.</param>
        /// <param name="columnWidth">Width of the column.</param>
        /// <param name="leadingSpacesCount">Number of leading spaces for wrapped lines.</param>
        /// <returns>Wrapped text.</returns>
        public string WrapTextToWidth(string text, int columnWidth, int leadingSpacesCount)
        {
            text = text.Trim();
            // Base condition: if the text is shorter than the column width, return it as is
            if (text.Length <= columnWidth)
            {
                // text = text.Trim();
                // text = string.Format($"{{0, -{columnWidth}}}", text);
                return text.PadRight(columnWidth);
            }

            CheckPageEnd();

            // Find the last space within the allowed column width
            int wrapIndex = text.LastIndexOf(' ', columnWidth);

            // If no space is found within the column width, break at the column width
            if (wrapIndex == -1)
            {
                wrapIndex = columnWidth;
            }

            // Generating leading spaces in next line
            string leadingSpaces = new string(' ', leadingSpacesCount);

            // Get the current line and the remaining text
            string currentLine = text.Substring(0, wrapIndex).TrimStart() + "\n" + leadingSpaces;
            string remainingText = text.Substring(wrapIndex);

            // Recursive call to handle the remaining text
            return currentLine + WrapTextToWidth(remainingText, columnWidth, leadingSpacesCount);
        }
        /// <summary>
        /// Checks if the current page has ended and handles page break logic. Adds footer to the end of page and header when next page begins.
        /// </summary>
        public void CheckPageEnd()
        {
            ++currentLine;
            if (numberOfLines <= currentLine)
            {
                rawTextString.AppendLine(@$"Continue{crlf}");
                //printing footer
                if(FooterData is null)
                {
                    PrintHeaderOrFooter(crlf, FooterLines);
                }
                else
                {
                    PrintHeaderOrFooter(FooterData, FooterLines);
                }
                rawTextString.AppendLine("\x0C");
                Console.WriteLine("page end");
                if(HeaderData is null)
                {
                    PrintHeaderOrFooter(crlf, HeaderLines);
                }
                else
                {
                    PrintHeaderOrFooter(HeaderData, HeaderLines);
                }
                rawTextString.AppendLine("Continue");
                currentLine = 1;
            }
        }
        /// <summary>
        /// Formats a row of invoice items for printing.
        /// </summary>
        /// <param name="columns">Array of column names.</param>
        /// <param name="printableCharacters">Number of printable characters per line.</param>
        /// <param name="colWidth">Dictionary containing column width for invoice items.</param>

        public void FormatInvoiceRow(string[] columns, int printableCharacters, Dictionary<string, int> colWidth)
        {
            int invoiceItemColumnWidth = Helper.GetEachColumnWidth(printableCharacters, columns.Length);
            foreach (var column in columns)
            {
                rawTextString.AppendFormat($"{{0, -{colWidth[column]}}}", column);
            }
            rawTextString.Append(crlf);
            CheckPageEnd();
        }
        /// <summary>
        /// Prints the header.
        /// </summary>
        /// <param name="headerContent">Header content.</param>
        /// <param name="numberOfLines">Number of header lines.</param>
        //To do: PrintHeader for now is only adding blank lines but when other parameter come then it can become a big function to print the whole header
        private void PrintHeaderOrFooter(string headerContent, int numberOfLines)
        {
            if(headerContent == crlf)
            {
                rawTextString.Append(CenterAlignText(headerContent, LinePrintableCharacters) + crlf);
                for (int i = 0; i < numberOfLines - 1; i++)
                {
                    rawTextString.Append(CenterAlignText(headerContent, LinePrintableCharacters) + crlf);
                }
                currentLine += numberOfLines;
            }
            else
            {
                int leftWidth = LinePrintableCharacters * 25 / 100;
                int rightWidth = LinePrintableCharacters * 25 / 100;
                int centerWidth = LinePrintableCharacters - leftWidth - rightWidth;
                Header headerData = JsonConvert.DeserializeObject<Header>(headerContent);
                int maxLength = Math.Max(headerData.Left.Length, Math.Max(headerData.Center.Length, headerData.Right.Length));

                for (int i = 0; i < maxLength; i++)
                {
                    string leftText = i< headerData.Left.Length && headerData.Left[i].Value.Length > 0 ? ApplyTextStyles(headerData.Left[i].Value, TextStyling.bold).PadRight(leftWidth) :BlankSpaces(leftWidth);
                    string centerText = i< headerData.Center.Length && headerData.Center[i].Value.Length > 0 ? ApplyTextStyles(CenterAlignText(headerData.Center[i].Value,centerWidth), TextStyling.bold) :BlankSpaces(centerWidth);
                    string rightText = i< headerData.Right.Length && headerData.Right[i].Value.Length > 0 ? ApplyTextStyles(headerData.Right[i].Value, TextStyling.bold).PadLeft(rightWidth) :BlankSpaces(rightWidth);
                    // string leftText = i< headerData.Left.Length ? ApplyTextStyles(headerData.Left[i].Value, TextStyling.bold) : BlankSpaces(leftWidth);
                    // string centerText = i< headerData.Center.Length ? ApplyTextStyles(headerData.Center[i].Value, TextStyling.bold) : BlankSpaces(centerWidth);
                    // string rightText = i< headerData.Right.Length ? ApplyTextStyles(headerData.Right[i].Value, TextStyling.bold) : BlankSpaces(rightWidth);
                    // rawTextString.AppendFormat($"{{0, -{leftWidth}}}{{1}}{{2,{rightWidth}}}", leftText, CenterAlignText(centerText, centerWidth), rightText);
                    rawTextString.Append(leftText);
                    rawTextString.Append(centerText);
                    rawTextString.Append(rightText);

                    rawTextString.Append(crlf);

                    currentLine++;
                }
            }
        }
        private string BlankSpaces(int numberOfSpaces)
        {
            string s = new string(' ', numberOfSpaces);
            return s;
        }
       
        public static string HeaderApplyStyling(string text, string styles)
        {
            StringBuilder lineText = new StringBuilder(text);
            string[] stylings = styles.Split(';');

            foreach (var style in stylings)
            {
                string trimmedStyle = style.Trim().ToLower();
                switch (trimmedStyle)
                {
                    case "bold":
                        lineText.Insert(0, "\x1B\x45");
                        lineText.Append("\x1B\x46");
                        break;
                    case "italic":
                        lineText.Insert(0, "\x1B\x34");
                        lineText.Append("\x1B\x35");
                        break;
                    // case TextStyling.double_strike:
                    //     lineText.Insert(0, "\x1B\x47");
                    //     lineText.Append("\x1B\x48");
                    //     break;
                    // case TextStyling.proportionalMode:
                    //     lineText.Insert(0, "\x1B\x70\x31");
                    //     lineText.Append("\x1B\x70\x30");
                    //     break;
                    // case TextStyling.codensed:
                    //     lineText.Insert(0, "\x0F");
                    //     lineText.Append("\x12");
                    //     break;
                    case "doublewidth":
                        lineText.Insert(0, "\x0E");
                        lineText.Append("\x14");
                        break;
                    // case TextStyling.superScript:
                    //     lineText.Insert(0, "\x1B\x53\x30");
                    //     lineText.Append("\x1B\x54");
                    //     break;
                    // case TextStyling.subScript:
                    //     lineText.Insert(0, "\x1B\x53\x31");
                    //     lineText.Append("\x1B\x54");
                    //     break;
                    case "doubleheight":
                        lineText.Insert(0, "\x1B\x77\x31");
                        lineText.Append("\x1B\x77\x30");
                        break;
                    default:
                        break;
                }
            }

            return lineText.ToString();
        }
        /// <summary>
        /// Prints the footer.
        /// </summary>
        /// <param name="footerContent">Footer content.</param>
        /// <param name="numberOfLines">Number of footer lines.</param>
        //To do: PrintFooter for now is only adding blank lines but when other parameter come then it can become a big function to print the whole footer

        private void PrintFooter(string footerContent, int footerLines)
        {
            if(numberOfLines- currentLine > footerLines)
            {
                    if(footerContent is null)
                    {
                        PrintHeaderOrFooter(crlf, footerLines);
                    }
                    else
                    {
                        PrintHeaderOrFooter(footerContent, footerLines);
                    }
            }
        }
        /// <summary>
        /// Centers text within a specified width.
        /// </summary>
        /// <param name="text">Text to be centered.</param>
        /// <param name="width">Width for centering the text.</param>
        /// <returns>Centered text.</returns>
        private static string CenterAlignText(string text, int width)
        {
            if (text.Length >= width)
        {
            return text; // If the text is longer than the width, just return the text
        }

        int leftPadding = (width - text.Length) / 2;
        int rightPadding = width - text.Length - leftPadding;

        string paddedText = new string(' ', leftPadding) + text + new string(' ', rightPadding);
        return paddedText;
        }
    }
}
    // string emphasized= "\x1B\x45";
    // string emphasizedCancel = "\x1B\x46";
    // string double_strike= "\x1B\x47";
    // string double_strike_cancel = "\x1B\x48";
    // string proportionalMode = "\x1B\x70\x31";
    // string proportionalModeCancel = "\x1B\x70\x30";
    // string condensed ="\x0F";
    // string cancelCondensed = "\x12";
    // string doubleWidth = "\x0E";
    // string doublWidthCancel ="\x14";
    // string superScript = "\x1B\x53\x30";
    // string subScript ="\x1B\x53\x31";
    // string normalizeCommand = "\x1B\x54";
    // string doubleHeight= "\x1B\x77\x31";
    // string doubleHeightCancel = "\x1B\x77\x30";