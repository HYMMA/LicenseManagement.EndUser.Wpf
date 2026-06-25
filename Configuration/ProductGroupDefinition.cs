using System.Collections.Generic;

namespace LicenseManagement.EndUser.Wpf.Configuration
{
    /// <summary>
    /// Developer-supplied description of one product group (e.g. "Monthly" or
    /// "Annual"). Passed to <c>LicenseViewModel.ApplyGrouping</c> at startup. Grouping
    /// is purely a presentation concern — no server or license-model change is needed,
    /// the publisher just maps the product ids it already knows.
    /// </summary>
    public sealed class ProductGroupDefinition
    {
        public ProductGroupDefinition()
        {
            ProductIds = new List<string>();
        }

        /// <summary>Stable key used to tag each product (e.g. "monthly").</summary>
        public string Key { get; set; }

        /// <summary>Heading shown to the end user (e.g. "Monthly").</summary>
        public string Label { get; set; }

        /// <summary>Short line under the heading (e.g. "Renews every month").</summary>
        public string Caption { get; set; }

        /// <summary>
        /// Accent colour for this group as a hex string (e.g. "#0E8F9C"). Drives the
        /// card rail, header glyph and count chip so groups read apart at a glance.
        /// </summary>
        public string Accent { get; set; }

        /// <summary>Ids of the products that belong to this group, in display order.</summary>
        public IList<string> ProductIds { get; set; }
    }
}
