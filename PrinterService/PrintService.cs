namespace PrintingRaw
{
    /// <summary>
    /// Class that provide service for setting up page, sending print command to printer and some utility functions
    /// </summary>
    public class PrintService
    {
        /// <summary>
        /// CRLF is short form of carriage return and line feed.
        /// </summary>
        private static readonly string crlf = "\x0D\x0A";
        /// <summary>
        /// Object of Printer class which send data to the printer
        /// </summary>
        private readonly Printer _printer;
        /// <summary>
        /// Variable that defines the unit which help in setting margins and length of page.
        /// </summary>
        private int definedUnit;
        /// <summary>
        /// Name of the printer
        /// </summary>
        private string PrinterName;
        /// <summary>
        /// ESC is the command. Mostly command in the ESC/P2 starts with ESC i.e. \x1B
        /// </summary>
        private string ESC = "\x1B";
        /// <summary>
        /// Length of page in inch.
        /// </summary>
        private double pageLengthInch;
        /// <summary>
        /// Width of page in inch.
        /// </summary>
        private double pageWidthInch;
        /// <summary>
        /// Left margin calculated in inch.
        /// </summary>
        private double leftMarginInch;
        /// <summary>
        /// right margin calculated in inch
        /// </summary>
        private double rightMarginInch;
        /// <summary>
        /// top margin calculated in inch
        /// </summary>
        private double topMarginInch;
        /// <summary>
        /// bottom margin calculated in inch
        /// </summary>
        private double bottomMarginInch;
        /// <summary>
        /// Integer value of the characters per inch (CPI). Available values are 10, 12, 15
        /// </summary>
        private int pitch; // Characters per inch
        /// <summary>
        /// Line spacing number of line per inch in the page. Available values are 6, 8
        /// </summary>
        private int lineSpacing; // Lines per inch
        /// <summary>
        /// Header height measured in inch
        /// </summary>
        private double headerHeightInch;
        /// <summary>
        /// footer height measured in inch
        /// </summary>
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
        /// <summary>
        /// Functions that will convert mm into inches.
        /// </summary>
        /// <param name="mmSize"></param>
        /// <returns></returns>
        private double ConvertMMtoInches(double mmSize)
        {
            return mmSize / 25.4;
        }
        
        #region Initialize and Reset Printer
        /// <summary>
        /// Fucntion that initialize printer. Resets all previous settings
        /// </summary>
        private void InitializePrinter()
        {
            string initializePrinter = "\x1B\x40";
            _printer.SendCommand(initializePrinter);
        }
        #endregion

        #region Setting Page In Printer
        /// <summary>
        /// Send command to printer to set Line spacing.
        /// </summary>
        private void SetLineSpacing()
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
        }
        /// <summary>
        /// Send command to printer to set the pitch.
        /// </summary>
        private void SetPitchCPI()
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
        }
        /// <summary>
        /// Send command to define unit for measuring length and width of page.
        /// </summary>
        /// <returns></returns>
        private void DefineUnit()
        {
            string unit = "\x1B\x28\x55\x01\x00\x0A";
            definedUnit = 360;
            _printer.SendCommand(unit);
        }
        /// <summary>
        /// Send Command to printer to set left margin.
        /// </summary>
        private void SetLeftMargin()
        {
            int numberOfCharacters = (int)(leftMarginInch * pitch);
            byte[] setLeftMargin = { 0x1B, 0x6C, (byte)numberOfCharacters };
            _printer.SendCommandBytes(setLeftMargin);
        }
        /// <summary>
        /// Send Command to printer to set right margin.
        /// </summary>
        private void SetRightMargin()
        {
            int numberOfCharacters = (int)((pageWidthInch - rightMarginInch) * pitch);
            byte[] setRightMargin = { 0x1B, 0x51, (byte)numberOfCharacters };
            _printer.SendCommandBytes(setRightMargin);
        }
        /// <summary>
        /// Send Command to printer to set page Length.
        /// </summary>
        private void SetPageLength()
        {
            double pageLengthUnits = pageLengthInch * definedUnit;
            int nL = 2;
            int nH = 0;
            int mL = (int)pageLengthUnits % 256;
            int mH = (int)pageLengthUnits / 256;
            byte[] bytes = { 0x1B, 0x28, 0x43, (byte)nL, (byte)nH, (byte)mL, (byte)mH };
            _printer.SendCommandBytes(bytes);
        }
        /// <summary>
        /// Send Command to printer to set top and bottom margin.
        /// </summary>
        private void SetYMargin()
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
        }
        /// <summary>
        /// Send Command to printer to select font.
        /// </summary>
        private void SelectFont()
        {
            string selectFont = "\x1B\x4B\x39";
            _printer.SendCommand(selectFont);
        }
        #endregion
        /// <summary>
        /// Function that helps to execute all required function and send raw text to printer.
        /// </summary>
        /// <param name="printText">String that is processed with escp commands for formatting</param>
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
            _printer.SendCommand("\x0C");
            InitializePrinter();
            // _printer.SendCommand(emphasized+"A quick brown fox jumps"+emphasizedCancel+" over the lazy dog."+crlf);
            // _printer.SendCommand(double_strike+"A quick brown fox jumps"+double_strike_cancel+" over the lazy dog."+crlf);
            // _printer.SendCommand(proportionalMode+"A quick brown fox jumps"+proportionalModeCancel+" over the lazy dog."+crlf);
            // _printer.SendCommand(condensed+"A quick brown fox jumps"+cancelCondensed+" over the lazy dog."+crlf);
            // _printer.SendCommand(doubleWidth+"A quick brown fox jumps"+doublWidthCancel+" over the lazy dog."+crlf);
            // _printer.SendCommand(superScript+"A quick brown fox jumps"+subScript+" over the lazy dog."+crlf);
            // _printer.SendCommand(doubleHeight+"A quick brown fox jumps"+doubleHeightCancel+" over the lazy dog."+crlf);
            // _printer.SendCommand("2"+superScript+"2"+normalizeCommand+ " is 4."+crlf);
            // _printer.SendCommand("11001110101010"+subScript+"2"+normalizeCommand+" is 1973"+crlf);
        }
            // _printer.SendCommand("A quick brown fox jumps over the lazy dog."+crlf);
            // _printer.SendCommand("A quick brown fox jumps over the lazy dog."+crlf);
            // _printer.SendCommand("A quick brown fox jumps over the lazy dog."+crlf);
            // _printer.SendCommand("A quick brown fox jumps over the lazy dog."+crlf);

        #region Utility Functions
        /// <summary>
        /// Function that calculates and return number of printable characters per line.
        /// </summary>
        /// <returns>Integer variable of number of printable characters per line</returns>
        public int NumberOfPrintableCharacters()
        {
            double printableWidth = pageWidthInch - rightMarginInch - leftMarginInch;
            double printableCharacters = printableWidth * pitch;
            return (int)printableCharacters;
        }
        /// <summary>
        /// Function that calculates and return number of lines per Page.
        /// </summary>
        /// <returns>Integer variable of number of lines per Page</returns>
        public int NumberOfLinesPerPage()
        {
            double printableHeight = pageLengthInch - topMarginInch - bottomMarginInch;
            double printableLines = printableHeight * lineSpacing;
            return (int)printableLines;
        }
        /// <summary>
        /// Function that calculates and return number of lines occupied header.
        /// </summary>
        /// <returns>Integer variable of number of lines occupied header</returns>
        public int HeaderHeightLines()
        {
            double headerLines = headerHeightInch * lineSpacing;
            return (int)headerLines + 1;
        }
        /// <summary>
        /// Function that calculates and return number of lines occupied footer.
        /// </summary>
        /// <returns>Integer variable of number of lines occupied footer</returns>
        public int FooterHeightLines()
        {
            double footerLines = footerHeightInch * lineSpacing;
            return (int)footerLines + 1;
        }
        #endregion
    
    }
}
