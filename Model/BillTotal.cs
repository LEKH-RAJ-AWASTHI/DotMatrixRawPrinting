namespace PrintingRaw;
/// <summary>
/// Contains the information about the bill total.
/// </summary>
public class BillTotal
{
    /// <summary>
    /// Subtotal before discount
    /// </summary>
    public decimal SubTotal;
    /// <summary>
    /// Discount percent that will be applied on the subtotal
    /// </summary>
    public decimal Dis;
    /// <summary>
    /// Amount after discount
    /// </summary>
    public decimal NetTotal;
    /// <summary>
    /// Net total amount in words
    /// </summary>
    public string AmountInWords;
}
