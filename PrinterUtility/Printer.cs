using System.Runtime.InteropServices;
using System.Text;

/// <summary>
/// Represents a printer and provides methods to send commands to it.
/// </summary>
public partial class Printer
{
    /// <summary>
    /// Name of the printer.
    /// </summary>
    private string PrinterName;

    /// <summary>
    /// Handle to the printer.
    /// </summary>
    IntPtr hPrinter;

    /// <summary>
    /// Document information for the print job.
    /// </summary>
    DOCINFOA di = new DOCINFOA
    {
        pDocName = "PrintDoc",
        pDataType = "RAW"
    };

    /// <summary>
    /// Initializes a new instance of the Printer class with the specified printer name.
    /// </summary>
    /// <param name="printerName">The name of the printer.</param>
    public Printer(string printerName)
    {
        this.PrinterName = printerName;
    }

    /// <summary>
    /// Sends a string command to the printer.
    /// </summary>
    /// <param name="data">The string data to send to the printer.</param>
    /// <returns>True if the command was successfully sent; otherwise, false.</returns>
    public bool SendCommand(string data)
    {
        bool success = false;
        if (OpenPrinter(PrinterName, out hPrinter, IntPtr.Zero))
        {
            if (StartDocPrinter(hPrinter, 1, di))
            {
                if (StartPagePrinter(hPrinter))
                {
                    byte[] bytes = Encoding.ASCII.GetBytes(data);
                    IntPtr pUnmanagedBytes = Marshal.AllocCoTaskMem(bytes.Length);
                    Marshal.Copy(bytes, 0, pUnmanagedBytes, bytes.Length);
                    int dwWritten = 0;
                    success = WritePrinter(hPrinter, pUnmanagedBytes, bytes.Length, out dwWritten);
                    Marshal.FreeCoTaskMem(pUnmanagedBytes);
                    EndPagePrinter(hPrinter);
                }
                EndDocPrinter(hPrinter);
            }
            ClosePrinter(hPrinter);
        }
        return success;
    }

    /// <summary>
    /// Sends a byte array command to the printer.
    /// </summary>
    /// <param name="bytes">The byte array data to send to the printer.</param>
    /// <returns>True if the command was successfully sent; otherwise, false.</returns>
    public bool SendCommandBytes(byte[] bytes)
    {
        bool success = false;
        if (OpenPrinter(PrinterName, out hPrinter, IntPtr.Zero))
        {
            if (StartDocPrinter(hPrinter, 1, di))
            {
                if (StartPagePrinter(hPrinter))
                {
                    IntPtr pUnmanagedBytes = Marshal.AllocCoTaskMem(bytes.Length);
                    Marshal.Copy(bytes, 0, pUnmanagedBytes, bytes.Length);
                    int dwWritten = 0;
                    success = WritePrinter(hPrinter, pUnmanagedBytes, bytes.Length, out dwWritten);
                    Marshal.FreeCoTaskMem(pUnmanagedBytes);
                    EndPagePrinter(hPrinter);
                    EndDocPrinter(hPrinter);
                    ClosePrinter(hPrinter);
                }
                EndDocPrinter(hPrinter);
            }
            ClosePrinter(hPrinter);
        }
        return success;
    }
}
