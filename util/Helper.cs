using System.Text;

namespace PrintingRaw;

public class Helper
{
    public static string crlf = string.Format("\x0D\x0A");

    public static string FormatRow(Dictionary<string, string> rowData, int printableCharacters, Dictionary<string, int> colWidth)
    {
        // InvoiceItemColumnWidth= GetEachColumnWidth(printableCharacters, columns.Length);
        StringBuilder rowBuilder = new StringBuilder("");
        // int leadingSpacesCount= 0;
        foreach (var column in rowData)
        {
            // rowBuilder.Append($" {column,-15} |"); // Adjust width as 
            //the commented line of below code is to wrap the long column name 
            // rowBuilder.AppendFormat($"{{0, -{InvoiceItemColumnWidth}}}", WrapText(column,InvoiceItemColumnWidth, leadingSpacesCount));

            rowBuilder.AppendFormat($"{{0, -{colWidth[column.Key]}}}", column.Value);

            // leadingSpacesCount+=column.Length;
        }
        rowBuilder.Append(crlf);
        return rowBuilder.ToString();
    }
    public static int GetEachColumnWidth(int printableWidth, int numberOfColumn)
    {
        int EachColumnWidth= printableWidth/numberOfColumn;
        return EachColumnWidth;
    }

}

