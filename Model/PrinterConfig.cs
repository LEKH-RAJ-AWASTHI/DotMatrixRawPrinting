namespace PrintingRaw;

public class PrinterConfig
{
    public string PrinterName {get; set;}
    public double PageLengthMM {get; set;}
    public double PageWidthMM {get; set;}
    public double LeftMarginMM {get; set;}
    public double RightMarginMM {get; set;}
    public double TopMarginMM {get; set;}
    public double BottomMarginMM {get; set;}
    public int Pitch {get; set;}
    public int LineSpacing {get; set;}
    public double HeaderHeightMM{get; set;}
    public double FooterHeightMM{get; set;}
}
