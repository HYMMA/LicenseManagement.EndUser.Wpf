namespace LicenseManagement.EndUser.Wpf.Properties
{
    /// <summary>
    /// net8 build of the two string resources actually used by the library
    /// (<see cref="Rules.ReceiptCodeRule"/>). The original Properties\Resources.resx mixes
    /// these with unused System.Drawing-typed template entries (Color1/Bitmap1/Icon1) that
    /// net8 resgen + BinaryFormatter can't process, so the net8 sibling project skips the
    /// .resx/.Designer and supplies the strings here (identical values to the net481 resx).
    /// </summary>
    internal static class Resources
    {
        public static string ReceiptCodeEmpty => "Product code cannot be empty.";
        public static string ReceiptCodeTooLong => "Product code cannot be more than {0} characters.";
    }
}
