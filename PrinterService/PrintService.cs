using System;
using System.Drawing.Printing;

namespace PrintingRaw
{
    public class PrintService
    {
        private static readonly string crlf = "\x0D\x0A";
        private readonly Printer _printer;
        private int definedUnit;
        private string PrinterName;
        private string ESC = "\x1B";

        private double pageLengthInch;
        private double pageWidthInch;
        private double leftMarginInch;
        private double rightMarginInch;
        private double topMarginInch;
        private double bottomMarginInch;
        private int pitch; // Characters per inch
        private int lineSpacing; // Lines per inch
        private double headerHeightInch;
        private double footerHeightInch;

        public PrintService(PrinterConfig p)
        {
            this.pageLengthInch = ConvertMMtoInches(p.PageLengthMM);
            this.pageWidthInch = ConvertMMtoInches(p.PageWidthMM);
            this.leftMarginInch = ConvertMMtoInches(p.LeftMarginMM);
            this.rightMarginInch = ConvertMMtoInches(p.RightMarginMM);
            this.topMarginInch = ConvertMMtoInches(p.TopMarginMM);
            this.bottomMarginInch = ConvertMMtoInches(p.BottomMarginMM);
            this.headerHeightInch = ConvertMMtoInches(p.HeaderHeightMM);
            this.footerHeightInch = ConvertMMtoInches(p.FooterHeightMM);
            this.pitch = p.Pitch;
            this.lineSpacing = p.LineSpacing;
            this.PrinterName = p.PrinterName;
            _printer = new Printer(PrinterName);
        }

        private double ConvertMMtoInches(double mmSize)
        {
            return mmSize / 25.4;
        }

        #region Initialize and Reset Printer
        private string InitializePrinter()
        {
            string initializePrinter = "\x1B\x40";
            _printer.SendCommand(initializePrinter);
            return initializePrinter;
        }
        #endregion

        #region Setting Page In Printer
        private string SetLineSpacing()
        {
            string lineSpacingCmd = ESC;
            switch (lineSpacing)
            {
                case 6:
                    lineSpacingCmd += "\x32";
                    break;
                case 8:
                    lineSpacingCmd += "\x30";
                    break;
                default:
                    Console.WriteLine("Invalid line spacing");
                    break;
            }
            _printer.SendCommand(lineSpacingCmd);
            return lineSpacingCmd;
        }

        private string SetPitchCPI()
        {
            string selectCPI = ESC;
            switch (pitch)
            {
                case 10:
                    selectCPI += "\x50";
                    break;
                case 12:
                    selectCPI += "\x4D";
                    break;
                case 15:
                    selectCPI += "\x67";
                    break;
                default:
                    Console.WriteLine("Invalid CPI");
                    break;
            }
            _printer.SendCommand(selectCPI);
            return selectCPI;
        }

        private string DefineUnit()
        {
            string unit = "\x1B\x28\x55\x01\x00\x0A";
            definedUnit = 360;
            _printer.SendCommand(unit);
            return unit;
        }

        private string SetLeftMargin()
        {
            int numberOfCharacters = (int)(leftMarginInch * pitch);
            byte[] setLeftMargin = { 0x1B, 0x6C, (byte)numberOfCharacters };
            _printer.SendCommandBytes(setLeftMargin);
            return string.Format("\\x1B\\x6C\\x{0:X2}", numberOfCharacters);
        }

        private string SetRightMargin()
        {
            int numberOfCharacters = (int)((pageWidthInch - rightMarginInch) * pitch);
            byte[] setRightMargin = { 0x1B, 0x51, (byte)numberOfCharacters };
            _printer.SendCommandBytes(setRightMargin);
            return string.Format("\\x1B\\x51\\x{0:X2}", numberOfCharacters);
        }

        private string SetPageLength()
        {
            double pageLengthUnits = pageLengthInch * definedUnit;
            int nL = 2;
            int nH = 0;
            int mL = (int)pageLengthUnits % 256;
            int mH = (int)pageLengthUnits / 256;
            byte[] bytes = { 0x1B, 0x28, 0x43, (byte)nL, (byte)nH, (byte)mL, (byte)mH };
            _printer.SendCommandBytes(bytes);
            return string.Format("\\x1B\\x28\\x43\\x{0:X2}\\x{1:X2}\\x{2:X2}\\x{3:X2}", nL, nH, mL, mH);
        }

        private string SetYMargin()
        {
            double topMarginUnits = topMarginInch * definedUnit;
            int th = (int)topMarginUnits / 256;
            int tl = (int)topMarginUnits % 256;

            double bottomMarginUnits = bottomMarginInch * definedUnit;
            int bh = (int)bottomMarginUnits / 256;
            int bl = (int)bottomMarginUnits % 256;

            byte nL = 4;
            byte nH = 0;

            byte[] setMarginsCommand = new byte[]
            {
                0x1B, 0x28, 0x63, nL, nH, (byte)tl, (byte)th, (byte)bl, (byte)bh
            };
            _printer.SendCommandBytes(setMarginsCommand);
            return string.Format("\\x1B\\x28\\x63\\x{0:X2}\\x{1:X2}\\x{2:X2}\\x{3:X2}\\x{4:X2}\\x{5:X2}", nL, nH, tl, th, bl, bh);
        }

        private string SelectFont()
        {
            string selectFont = "\x1B\x4B\x39";
            _printer.SendCommand(selectFont);
            return selectFont;
        }
        #endregion

        public void PrintRawText(string printText)
        {
            InitializePrinter();
            SetLineSpacing();
            SetPitchCPI();
            DefineUnit();
            SetLeftMargin();
            SetRightMargin();
            SetPageLength();
            SetYMargin();
            SelectFont();
            _printer.SendCommand(printText + crlf);
            // _printer.SendCommand("\x0C");
            InitializePrinter();
        }

        #region Utility Functions
        public int NumberOfPrintableCharacters()
        {
            double printableWidth = pageWidthInch - rightMarginInch - leftMarginInch;
            double printableCharacters = printableWidth * pitch;
            return (int)printableCharacters;
        }

        public int NumberOfLinesPerPage()
        {
            double printableHeight = pageLengthInch - topMarginInch - bottomMarginInch;
            double printableLines = printableHeight * lineSpacing;
            return (int)printableLines;
        }

        public int HeaderHeightLines()
        {
            double headerLines = headerHeightInch * lineSpacing;
            return (int)headerLines + 1;
        }

        public int FooterHeightLines()
        {
            double footerLines = footerHeightInch * lineSpacing;
            return (int)footerLines + 1;
        }
        #endregion
    }
}
