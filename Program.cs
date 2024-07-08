// See https://aka.ms/new-console-template for more information
using System.Text;
using GenerateRawTextToPrint;
using Newtonsoft.Json;
using PrintingRaw;

// Console.WriteLine("Hello, World!");
// Printer printer = new Printer("EPSON LQ-310 ESC/P2");
// string s= "Hello World\n";
// byte[] bytes = Encoding.ASCII.GetBytes(s);
// printer.SendCommandBytes(bytes);
var PrinterSetting = @"{
                    'PrinterName': 'EPSON LQ-310 ESC/P2',
                    'PageLengthMM': '150',
                    'pageWidthMM': '230',
                    'LeftMarginMM': '10',
                    'RightMarginMM': '10',
                    'TopMarginMM': '15',
                    'BottomMarginMM': '15',
        'Pitch': '10',
        'lineSpacing': '6',
                    'HeaderHeightMM': '10',
                    'FooterHeightMM': '10'
    }";
var patientAndinvoiceDetails = @"{
				'HospitalNo': { 'DisplayLabel': 'HospitalNo', 'value': '24000624' },
				'PatientName': { 'DisplayLabel': 'PatientName', 'value': 'Monday New Patient' },
				'Address': { 'DisplayLabel': 'Address', 'value': 'Dillibazar, Kathmandu' },
				'AgeSex': { 'DisplayLabel': 'Age/Sex', 'value': '19Y/M' },
				'Contact': { 'DisplayLabel': 'Phone Number', 'value': '980000000' },
				'PolicyNo': { 'DisplayLabel': 'Policy/Member No', 'value': '1720256' },
				'Type': { 'DisplayLabel': 'Scheme', 'value': 'General' },
				'InvoiceNo': { 'DisplayLabel': 'InvoiceNo', 'value': '80/81-BL23456' },
				'InvoiceDate': { 'DisplayLabel': 'InvoiceDate', 'value': '2024-06-24' },
				'ClaimCode': { 'DisplayLabel': 'ClaimCode', 'value': '12365478' },
				'Department': { 'DisplayLabel': 'Dept', 'value': 'ENT' },
				'PaymentMode': { 'DisplayLabel': 'Method Of Payment', 'value': 'Cash' }
			}";

// Sample settings
var patientAndInvoiceSettings = @"{
    'HospitalNo': { 'Position': '20mm;20mm', 'DisplaySeq': '1' },
    'PatientName': { 'Position': '20mm;40mm', 'DisplaySeq': '2' },
    'Address': { 'Position': '20mm;60mm', 'DisplaySeq': '5' },
    'AgeSex': { 'Position': '20mm;80mm', 'DisplaySeq': '7' },
    'Contact': { 'Position': '20mm;100mm', 'DisplaySeq': '9' },
    'PolicyNo': { 'Position': '20mm;120mm', 'DisplaySeq': '11' },
    'Type': { 'Position': '20mm;140mm', 'DisplaySeq': '13' },
    'InvoiceNo': { 'Position': '105mm;20mm', 'DisplaySeq': '2' },
    'InvoiceDate': { 'Position': '105mm;40mm', 'DisplaySeq': '4' },
    'ClaimCode': { 'Position': '105mm;60mm', 'DisplaySeq': '6' },
    'Department': { 'Position': '105mm;80mm', 'DisplaySeq': '8' },
    'PaymentMode': { 'Position': '105mm;100mm', 'DisplaySeq': '10' }
}";

var InvoiceItemCharacterWidth= @"
{
    ""SN"": 5,
    ""Particular"": 30,
    ""Qty"": 5,
    ""Dis%"": 6,
    ""DisAmt"": 8,
    ""TotalAmt"": 10
}";
// var HeaderFooter=@"{
// 	'Letterpad': {'height': '50mm', 'lineSpacing': '10'},
// 	'Footer': {'height': '30mm', 'lineSpacing': '10'}
// }";
var invoiceItemsData = @"[
{
    ""SN"": 1,
    ""Particular"": ""CBC"",
    ""Qty"": 1,
    ""Dis%"":0,
    ""DisAmt"": 0,
    ""TotalAmt"": 250
},

{
    ""SN"": 2,
    ""Particular"": ""RFT kjhdh khieoi ioejojl"",
    ""Qty"": 1,
    ""Dis%"":0,
    ""DisAmt"": 0,
    ""TotalAmt"": 250
}]
// {
//     ""SN"": 3,
//     ""Particular"": ""Bili fgd fgdf fdh dgd"",
//     ""Qty"": 1,
//     ""Dis%"":0,
//     ""DisAmt"": 0,
//     ""TotalAmt"": 250
// },
// {
//     ""SN"": 4,
//     ""Particular"": ""X-Ray lkjd jalkjdsgsdgc lkjalk"",
//     ""Qty"": 1,
//     ""Dis%"":0,
//     ""DisAmt"": 0,
//     ""TotalAmt"": 250
// },
// {
//     ""SN"": 5,
//     ""Particular"": ""Ultra Sound ./dflks lkja"",
//     ""Qty"": 1,
//     ""Dis%"":0,
//     ""DisAmt"": 0,
//     ""TotalAmt"": 250
// }]";

var BillTotalDetail= @"{
            'SubTotal': '1250',
            'Dis': '0',
            'NetTotal': '1250',
            'AmountInWords':'One Thousand Two Hundred and Fifty only'
            }";
var DisplayOrder= @"{
            'patientAndinvoiceDetails': '1',
            'invoiceItemsData': '2',
            'BillTotalDetail': '3'
        }";
PrinterConfig pConfig = JsonConvert.DeserializeObject<PrinterConfig>(PrinterSetting);
PrintService setPrinter = new PrintService(pConfig);
int linesPerPage = setPrinter.NumberOfLinesPerPage();
int printableCharacters=setPrinter.NumberOfPrintableCharacters();
int headerLines = setPrinter.HeaderHeightLines();
int footerLines = setPrinter.FooterHeightLines();

InvoiceService invoiceService = new InvoiceService();
string invoiceText = invoiceService.GenerateInvoiceRawText(patientAndinvoiceDetails, patientAndInvoiceSettings, invoiceItemsData, linesPerPage, printableCharacters, InvoiceItemCharacterWidth, headerLines, footerLines, BillTotalDetail);	
Console.WriteLine(invoiceText);
setPrinter.PrintRawText(invoiceText);