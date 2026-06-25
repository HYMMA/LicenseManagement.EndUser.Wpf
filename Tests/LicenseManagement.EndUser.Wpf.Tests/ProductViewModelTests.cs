using LicenseManagement.EndUser.License;
using LicenseManagement.EndUser.Models;
using LicenseManagement.EndUser.Wpf.ViewModels;
using System;
using Xunit;

namespace LicenseManagement.EndUser.Wpf.Tests
{
    public class ProductViewModelTests
    {
        [Fact]
        public void FromProductModel_Null_ReturnsNull()
        {
            var result = ProductViewModel.FromProductModel(null);
            Assert.Null(result);
        }

        [Fact]
        public void FromProductModel_ValidModel_MapsIdAndName()
        {
            var model = new ProductModel { Id = "PRD_001", Name = "Test Product" };
            var vm = ProductViewModel.FromProductModel(model);

            Assert.NotNull(vm);
            Assert.Equal("PRD_001", vm.Id);
            Assert.Equal("Test Product", vm.Name);
        }

        [Fact]
        public void FromProductModel_MapsCreatedDate()
        {
            var created = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);
            var model = new ProductModel { Id = "PRD_002", Name = "Dated", Created = created };
            var vm = ProductViewModel.FromProductModel(model);

            Assert.Equal(created, vm.Created);
        }

        [Fact]
        public void Id_PropertyChanged_FiresOnAssignment()
        {
            var vm = new ProductViewModel();
            var fired = false;
            vm.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(vm.Id)) fired = true; };

            vm.Id = "PRD_999";

            Assert.True(fired);
        }

        [Fact]
        public void Name_PropertyChanged_DoesNotFire_WhenValueUnchanged()
        {
            var vm = new ProductViewModel { Name = "Same" };
            var fired = false;
            vm.PropertyChanged += (_, _) => fired = true;

            vm.Name = "Same"; // same value — setter should no-op

            Assert.False(fired);
        }

        [Fact]
        public void IsChecked_False_BeforeAnyCheck()
        {
            Assert.False(new ProductViewModel().IsChecked);
        }

        [Fact]
        public void IsChecked_True_AfterDeterminateStatus()
        {
            var vm = new ProductViewModel();
            vm.UpdateLicenseSnapshot(new LicenseModel { Status = LicenseStatusTitles.Valid }, 90);
            Assert.True(vm.IsChecked);
        }

        [Fact]
        public void IsChecked_False_WhenStatusUnknown()
        {
            // A check that came back Unknown must not look "loaded" — the card should
            // offer to check again rather than masquerade as a determinate status.
            var vm = new ProductViewModel();
            vm.UpdateLicenseSnapshot(new LicenseModel { Status = LicenseStatusTitles.Unknown }, 90);
            Assert.False(vm.IsChecked);
        }

        [Fact]
        public void MetaText_Unknown_DiffersFromNeverChecked_AndMentionsVerify()
        {
            var neverChecked = new ProductViewModel().MetaText;

            var unverified = new ProductViewModel();
            unverified.UpdateLicenseSnapshot(new LicenseModel { Status = LicenseStatusTitles.Unknown }, 90);

            Assert.NotEqual(neverChecked, unverified.MetaText);
            Assert.Contains("verify", unverified.MetaText, StringComparison.OrdinalIgnoreCase);
        }
    }
}
