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
    }
}
