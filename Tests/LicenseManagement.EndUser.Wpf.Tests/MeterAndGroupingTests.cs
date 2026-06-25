using LicenseManagement.EndUser.License;
using LicenseManagement.EndUser.Models;
using LicenseManagement.EndUser.Wpf.Configuration;
using LicenseManagement.EndUser.Wpf.ViewModels;
using System;
using System.Collections.ObjectModel;
using Xunit;

namespace LicenseManagement.EndUser.Wpf.Tests
{
    public class MeterAndGroupingTests
    {
        private static ProductViewModel Product(string id) => new ProductViewModel { Id = id, Name = id };

        // ---------- validity meter ----------

        [Fact]
        public void MeterFraction_Subscription_UsesReceiptTerm_IgnoringDbCreated()
        {
            var now = DateTime.UtcNow;
            var vm = Product("P");
            vm.UpdateLicenseSnapshot(new LicenseModel
            {
                Status = LicenseStatusTitles.Valid,
                Created = now.AddDays(-200), // far in the past — must NOT define the window
                Updated = now.AddDays(-200),
                Receipt = new ReceiptModel { Created = now.AddDays(-15), Expires = now.AddDays(15) }
            }, 90);

            Assert.InRange(vm.MeterFraction, 0.45, 0.55); // ~15 of a 30-day term
            Assert.InRange(vm.DaysLeft, 14, 15);
            Assert.True(vm.HasMeter);
        }

        [Fact]
        public void MeterFraction_Trial_UsesTrialWindow()
        {
            var now = DateTime.UtcNow;
            var vm = Product("P");
            vm.UpdateLicenseSnapshot(new LicenseModel
            {
                Status = LicenseStatusTitles.ValidTrial,
                Created = now.AddDays(-5),
                TrialEndDate = now.AddDays(5)
            }, 90);

            Assert.InRange(vm.MeterFraction, 0.45, 0.55); // ~5 of a 10-day trial
        }

        [Fact]
        public void MeterFraction_FallsBackToValidDays_WhenNoPeriodStartKnown()
        {
            var now = DateTime.UtcNow;
            var vm = Product("P");
            // Paid license, no receipt and no Created/Updated -> window = ValidDays (90).
            vm.UpdateLicenseSnapshot(new LicenseModel
            {
                Status = LicenseStatusTitles.Valid,
                Expires = now.AddDays(45)
            }, 90);

            Assert.InRange(vm.MeterFraction, 0.45, 0.55); // 45 of 90 days
        }

        [Fact]
        public void MeterFraction_Zero_WhenAlreadyExpired()
        {
            var now = DateTime.UtcNow;
            var vm = Product("P");
            vm.UpdateLicenseSnapshot(new LicenseModel
            {
                Status = LicenseStatusTitles.Valid,
                Receipt = new ReceiptModel { Created = now.AddDays(-30), Expires = now.AddDays(-1) }
            }, 90);

            Assert.Equal(0d, vm.MeterFraction);
            Assert.Equal(0, vm.DaysLeft);
        }

        [Fact]
        public void HasMeter_False_WhenNotRegistered()
        {
            var vm = Product("P");
            vm.UpdateLicenseSnapshot(new LicenseModel { Status = LicenseStatusTitles.ReceiptUnregistered }, 90);
            Assert.False(vm.HasMeter);
        }

        // ---------- grouping ----------

        [Fact]
        public void ApplyGrouping_AssignsMembers_SetsLayout_AndTrailingDefaultForLeftovers()
        {
            var vm = new LicenseViewModel
            {
                Products = new ObservableCollection<ProductViewModel> { Product("P1"), Product("P2"), Product("P3") }
            };
            var defs = new[]
            {
                new ProductGroupDefinition { Key = "a", Label = "A", ProductIds = { "P1" } },
                new ProductGroupDefinition { Key = "b", Label = "B", ProductIds = { "P2" } }
            };

            vm.ApplyGrouping(defs, ProductLayout.Lanes);

            Assert.Equal(ProductLayout.Lanes, vm.Layout);
            Assert.Equal(3, vm.ProductGroups.Count); // A, B, then a trailing default for P3

            Assert.Equal("A", vm.ProductGroups[0].Label);
            Assert.Equal("P1", vm.ProductGroups[0].Products[0].Id);
            Assert.Equal("a", vm.ProductGroups[0].Products[0].GroupKey);

            Assert.Equal("Other", vm.ProductGroups[2].Label); // leftovers when some groups exist
            Assert.Equal("P3", vm.ProductGroups[2].Products[0].Id);
        }

        [Fact]
        public void ApplyGrouping_Null_BuildsSingleUnlabeledGroupWithAllProducts()
        {
            var vm = new LicenseViewModel
            {
                Products = new ObservableCollection<ProductViewModel> { Product("P1"), Product("P2") }
            };

            vm.ApplyGrouping(null);

            Assert.Single(vm.ProductGroups);
            Assert.Null(vm.ProductGroups[0].Label); // unlabeled default => bare cards, no band header
            Assert.Equal(2, vm.ProductGroups[0].Products.Count);
        }

        [Fact]
        public void ApplyGrouping_IgnoresUnknownIds_AndKeepsDefinitionOrder()
        {
            var vm = new LicenseViewModel
            {
                Products = new ObservableCollection<ProductViewModel> { Product("P1"), Product("P2") }
            };
            var defs = new[]
            {
                new ProductGroupDefinition { Key = "annual", Label = "Annual", ProductIds = { "P2", "PRD_DOES_NOT_EXIST" } },
                new ProductGroupDefinition { Key = "monthly", Label = "Monthly", ProductIds = { "P1" } }
            };

            vm.ApplyGrouping(defs);

            // No leftovers => exactly the two defined groups, in definition order.
            Assert.Equal(2, vm.ProductGroups.Count);
            Assert.Equal("Annual", vm.ProductGroups[0].Label);
            Assert.Single(vm.ProductGroups[0].Products); // the bogus id was skipped
            Assert.Equal("P2", vm.ProductGroups[0].Products[0].Id);
            Assert.Equal("Monthly", vm.ProductGroups[1].Label);
        }
    }
}
