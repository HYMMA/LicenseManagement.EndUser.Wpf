namespace LicenseManagement.EndUser.Wpf.ViewModels
{
    /// <summary>
    /// Arrangement used to present the grouped product cards. The integrating
    /// developer picks one when wiring up the control; all three render the same
    /// cards, only the grouping arrangement differs.
    /// </summary>
    public enum ProductLayout
    {
        /// <summary>
        /// Full-width tinted bands stacked top to bottom, each group's products in a
        /// wrapping row beneath its header. Scales best for many groups/products.
        /// Default.
        /// </summary>
        Bands = 0,

        /// <summary>
        /// One coloured column per group, side by side. Strongest visual separation;
        /// best for two or three groups. Columns wrap on narrow windows.
        /// </summary>
        Lanes = 1,

        /// <summary>
        /// A segmented control that shows one group's cards at a time. Most compact —
        /// keeps the window short regardless of how many products exist.
        /// </summary>
        Switcher = 2
    }
}
