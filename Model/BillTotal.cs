namespace PrintingRaw;
/// <summary>
/// Contains the information about the bill total.
/// </summary>
public class BillTotal
{
    /// <summary>
    /// Subtotal before discount
    /// </summary>
    public decimal SubTotal {get; set;}
    /// <summary>
    /// Discount percent that will be applied on the subtotal
    /// </summary>
    public decimal Dis {get; set;}
    /// <summary>
    /// Amount after discount
    /// </summary>
    public decimal NetTotal {get; set;}
    /// <summary>
    /// Net total amount in words
    /// </summary>
    public string AmountInWords {get; set;}="";
}
