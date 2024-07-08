using System.Runtime.InteropServices;
using System.Text;

public partial class Printer
{
    private string PrinterName;
    IntPtr hPrinter;
    DOCINFOA di = new DOCINFOA
    {
        pDocName = "PrintDoc",
        pDataType = "RAW"
    };


    public Printer(string printerName)
    {
        this.PrinterName = printerName;
    }
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

