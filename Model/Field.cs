namespace PrintingRaw;
/// <summary>
/// Contains display label and values for the detail.
/// </summary>
public class Field
{
    /// <summary>
    /// Field Name such as Name, address etc
    /// </summary>
    public string DisplayLabel { get; set; }
    /// <summary>
    /// Value of display label
    /// </summary>
    public string Value { get; set; }
}

/// <summary>
/// Contains Position and display sequence of the detail field.
/// </summary>
public class FieldSetting
{
    /// <summary>
    /// Display order of the displayLabel
    /// </summary>
    public string DisplaySeq { get; set; }
}