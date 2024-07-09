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
        public string GenerateInvoiceRawText(string patientAndInvoiceInfo, string patientAndInvoiceInfoSettings, string invoiceItemsData, int linesPerPage, int printableCharacters, string invoiceItemCharacterWidth, int headerLines, int footerLines, string BillTotalDetail)
        {
            LinePrintableCharacters = printableCharacters;
            numberOfLines = linesPerPage - footerLines;
            HeaderLines = headerLines;
            FooterLines = footerLines;

            var patientAndInvoiceInfoData = JsonConvert.DeserializeObject<Dictionary<string, Field>>(patientAndInvoiceInfo);
            var patientAndInvoiceInfoSectionSettings = JsonConvert.DeserializeObject<Dictionary<string, FieldSetting>>(patientAndInvoiceInfoSettings);
            var InvoiceFieldWidth = JsonConvert.DeserializeObject<Dictionary<string, int>>(invoiceItemCharacterWidth);
            var invoiceItems = JsonConvert.DeserializeObject(invoiceItemsData);
            BillTotal billTotal = JsonConvert.DeserializeObject<BillTotal>(BillTotalDetail);

            //checking if the invoiceItemCharacterWidth is exceeding printable character width
            if (InvoiceFieldWidth.Values.Sum() > printableCharacters)
            {
                return "Column width exceeds the printable characters in a line";
            }

            if (patientAndInvoiceInfoData is null || patientAndInvoiceInfoSectionSettings is null)
            {
                return string.Empty;
            }

            return BuildRawInvoiceText(patientAndInvoiceInfoData, patientAndInvoiceInfoSectionSettings, invoiceItems, printableCharacters, InvoiceFieldWidth, billTotal);
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
        private string BuildRawInvoiceText(Dictionary<string, Field> data, Dictionary<string, FieldSetting> settings, dynamic invoiceItems, int printableCharacters, Dictionary<string, int> invoiceFieldColumnWidth, BillTotal billTotal)
        {
            PrintHeader(crlf, HeaderLines);

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

            return rawTextString.ToString();
        }
        /// <summary>
        /// Prints patient details. And add styling to the it. 
        /// </summary>
        /// <param name="printDetail">Dynamic object containing patient details to be printed.</param>
        /// <param name="printableCharacters">Number of printable characters per line.</param>

        private void PrintPatientDetails(dynamic printDetail, int printableCharacters)
        {
            int eachSideWidth = printableCharacters / 2;

            for (int i = 0; i < printDetail.Count; i += 2)
            {
                string lineLeftText = ApplyTextStyles(printDetail[i].DisplayLabel, TextStyling.bold) + ": " + printDetail[i].Value;
                string lineRightText = ApplyTextStyles(printDetail[i + 1].DisplayLabel, TextStyling.bold) + ": " + printDetail[i + 1].Value;
                rawTextString.AppendFormat($"{{0,-{eachSideWidth}}}{{1,{eachSideWidth}}}\x0A", lineLeftText, lineRightText);
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
                PrintFooter(crlf, FooterLines);
                rawTextString.AppendLine("\x0C");
                PrintHeader(crlf, HeaderLines);
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
        private void PrintHeader(string headerContent, int numberOfLines)
        {
            rawTextString.Append(CenterAlignText(headerContent, LinePrintableCharacters) + crlf);
            for (int i = 0; i < numberOfLines - 1; i++)
            {
                rawTextString.Append(CenterAlignText(headerContent, LinePrintableCharacters) + crlf);
            }
            currentLine += numberOfLines;
        }
        /// <summary>
        /// Prints the footer.
        /// </summary>
        /// <param name="footerContent">Footer content.</param>
        /// <param name="numberOfLines">Number of footer lines.</param>
        //To do: PrintFooter for now is only adding blank lines but when other parameter come then it can become a big function to print the whole footer

        private void PrintFooter(string footerContent, int numberOfLines)
        {
            rawTextString.Append(CenterAlignText(footerContent, LinePrintableCharacters) + crlf);
            for (int i = 0; i < numberOfLines - 1; i++)
            {
                rawTextString.Append(CenterAlignText(footerContent, LinePrintableCharacters) + crlf);
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
            int padding = (width - text.Length) / 2;
            return text.PadLeft(padding).PadRight(padding);
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