using System.Runtime.InteropServices;

/// <summary>
/// Represents a printer and provides methods to interact with it via the Windows API.
/// </summary>
public partial class Printer
{
    /// <summary>
    /// Opens a connection to the specified printer.
    /// </summary>
    /// <param name="src">The name of the printer.</param>
    /// <param name="hPrinter">A handle to the printer.</param>
    /// <param name="pd">A pointer to a PRINTER_DEFAULTS structure.</param>
    /// <returns>True if the operation is successful; otherwise, false.</returns>
    [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool OpenPrinter(string src, out IntPtr hPrinter, IntPtr pd);

    /// <summary>
    /// Closes the connection to the specified printer.
    /// </summary>
    /// <param name="hPrinter">A handle to the printer.</param>
    /// <returns>True if the operation is successful; otherwise, false.</returns>
    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool ClosePrinter(IntPtr hPrinter);

    /// <summary>
    /// Starts a print job for the specified printer.
    /// </summary>
    /// <param name="hPrinter">A handle to the printer.</param>
    /// <param name="level">The level of the structure.</param>
    /// <param name="di">A DOCINFOA structure that contains information about the document.</param>
    /// <returns>True if the operation is successful; otherwise, false.</returns>
    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool StartDocPrinter(IntPtr hPrinter, Int32 level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

    /// <summary>
    /// Ends the print job for the specified printer.
    /// </summary>
    /// <param name="hPrinter">A handle to the printer.</param>
    /// <returns>True if the operation is successful; otherwise, false.</returns>
    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool EndDocPrinter(IntPtr hPrinter);

    /// <summary>
    /// Starts a page in the print job for the specified printer.
    /// </summary>
    /// <param name="hPrinter">A handle to the printer.</param>
    /// <returns>True if the operation is successful; otherwise, false.</returns>
    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool StartPagePrinter(IntPtr hPrinter);

    /// <summary>
    /// Ends a page in the print job for the specified printer.
    /// </summary>
    /// <param name="hPrinter">A handle to the printer.</param>
    /// <returns>True if the operation is successful; otherwise, false.</returns>
    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool EndPagePrinter(IntPtr hPrinter);

    /// <summary>
    /// Sends data to the specified printer.
    /// </summary>
    /// <param name="hPrinter">A handle to the printer.</param>
    /// <param name="pBytes">A pointer to the data to be sent.</param>
    /// <param name="dwCount">The number of bytes to send.</param>
    /// <param name="dwWritten">The number of bytes that were sent.</param>
    /// <returns>True if the operation is successful; otherwise, false.</returns>
    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, Int32 dwCount, out Int32 dwWritten);

    /// <summary>
    /// Contains information about a document to be printed.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class DOCINFOA
    {
        /// <summary>
        /// The name of the document.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public string pDocName = "PrintDoc";

        /// <summary>
        /// The name of the output file.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public string pOutputFile="";

        /// <summary>
        /// The data type of the document.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public string pDataType = "RAW";
    }
}
