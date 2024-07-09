# DotMatrixRawPrinting
This is a console application project that will help you create an formatted invoice on the basis of data you provide.

In this project I have used dotnet 8.0 sdk. I have used Newtonsoft.Json package for deserializing the json provided by user.

Make sure you have dotnet 8.0 installed in your device.



Here is the input required and output given by the application
![InputOutput](https://github.com/LEKH-RAJ-AWASTHI/DotMatrixRawPrinting/assets/104682699/feadbbe9-b2eb-4305-ab32-8ad43588196f)

## Features
- Generate raw text for printing invoices.
- Support for headers and footers in the invoice.
- Automatic text wrapping and alignment.
- Customizable printable characters per line and lines per page.
- Handling of multiline invoice items and patient details.
- Support for text styling (Bold, Italic).

## Program Flow of Application
![Program Flow of PrintingRaw](https://github.com/LEKH-RAJ-AWASTHI/DotMatrixRawPrinting/assets/104682699/6bf658f7-0dff-4147-bec8-e4862199ce9c)




## Installation
To use the InvoiceService application, clone the repository and add the necessary dependencies.

```sh
git clone https://github.com/yourusername/InvoiceService.git
cd InvoiceService 
```
Make sure you have required dependencies (dotnet 8.0, Newtonsoft.json)
You can add the dependencies via NuGet
- dotnet add package Newtonsoft.Json
Just that's it.

## Program Usage

### Data required:

#### Printer Setting
```
var PrinterSetting = @"{
                    'PrinterName': 'EPSON LQ-310 ESC/P2',
                    'PageLengthMM': '210',
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
```
### Customer Detail Data
```
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
```
#### Customer Detail Invoice setting
```
// Sample settings
var patientAndInvoiceSettings = @"{
    'HospitalNo': {'DisplaySeq': '1' },
    'PatientName': {'DisplaySeq': '2' },
    'Address': {'DisplaySeq': '5' },
    'AgeSex': {'DisplaySeq': '7' },
    'Contact': {'DisplaySeq': '9' },
    'PolicyNo': {'DisplaySeq': '11' },
    'Type': {'DisplaySeq': '13' },
    'InvoiceNo': {'DisplaySeq': '2' },
    'InvoiceDate': {'DisplaySeq': '4' },
    'ClaimCode': {'DisplaySeq': '6' },
    'Department': {'DisplaySeq': '8' },
    'PaymentMode': {'DisplaySeq': '10' }
}";
```
#### Column Width of invoice item
```
var InvoiceItemCharacterWidth= @"
{
    ""SN"": 5,
    ""Particular"": 30,
    ""Qty"": 5,
    ""Dis%"": 6,
    ""DisAmt"": 8,
    ""TotalAmt"": 10
}";
```
#### Invoice Item Data to be shown in tabular form
```
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
}];
```
#### Column Width of invoice item table
```
var InvoiceItemCharacterWidth= @"
{
    ""SN"": 5,
    ""Particular"": 30,
    ""Qty"": 5,
    ""Dis%"": 6,
    ""DisAmt"": 8,
    ""TotalAmt"": 10
}";
```

#### Bill Total Section Data
```
var BillTotalDetail= @"{
            'SubTotal': '1250',
            'Dis': '0',
            'NetTotal': '1250',
            'AmountInWords':'One Thousand Two Hundred and Fifty only'
            }";
```

**Important:** You can update those data as per your requirement but the format should be same.