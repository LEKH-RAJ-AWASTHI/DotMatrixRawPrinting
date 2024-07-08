namespace PrintingRaw;
/// <summary>
/// This class contains properties that allows the setting printing area in the paper.
/// </summary>
public class PrinterConfig
{
    /// <summary>
    /// Name of the printer
    /// </summary>
    public string PrinterName {get; set;}
    /// <summary>
    /// Page Length measured in Millimeter
    /// </summary>
    public double PageLengthMM {get; set;}
    /// <summary>
    /// Page Width measured in Millimeter
    /// </summary>
    public double PageWidthMM {get; set;}
    /// <summary>
    /// Left Margin measured in Millimeter
    /// </summary>
    public double LeftMarginMM {get; set;}
    /// <summary>
    /// Right Margin measured in Millimeter
    /// </summary>
    public double RightMarginMM {get; set;}
    /// <summary>
    /// Top Margin measured in Millimeter
    /// </summary>
    public double TopMarginMM {get; set;}
    /// <summary>
    /// Bottom Margin measured in Millimeter
    /// </summary>
    public double BottomMarginMM {get; set;}
    /// <summary>
    /// Integer value of the characters per inch (CPI). Available values are 10, 12, 15
    /// </summary>
    public int Pitch {get; set;}
    /// <summary>
    /// Line spacing number of line per inch in the page. Available values are 6, 8
    /// </summary>
    public int LineSpacing {get; set;}
    /// <summary>
    /// Header height measured in Millimeter. It will be converted into number of lines according to line spacing given. If header have to be printed give header height as 0.00;
    /// </summary>
    public double HeaderHeightMM{get; set;}
    /// <summary>
    /// Footer height measured in Millimeter. It will be converted into number of lines according to line spacing given. If footer have to be printed give header height as 0.00;
    /// </summary>
    public double FooterHeightMM{get; set;}
}
