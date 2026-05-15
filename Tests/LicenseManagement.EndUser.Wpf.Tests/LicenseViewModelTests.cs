using LicenseManagement.EndUser.License;
using LicenseManagement.EndUser.Wpf.ViewModels;
using Xunit;

namespace LicenseManagement.EndUser.Wpf.Tests
{
    public class LicenseViewModelTests
    {
        [Fact]
        public void Constructor_DoesNotThrow()
        {
            // RelayCommand wires up CommandManager.RequerySuggested — verify no
            // STA or Application requirement at construction time.
            var vm = new LicenseViewModel();
            Assert.NotNull(vm);
        }

        [Fact]
        public void HasApiKey_False_WhenNoKeySet()
        {
            var vm = new LicenseViewModel();
            Assert.False(vm.HasApiKey);
        }

        [Fact]
        public void SetApiKey_MakesHasApiKeyTrue()
        {
            var vm = new LicenseViewModel();
            vm.SetApiKey("my-secret-key");
            Assert.True(vm.HasApiKey);
        }

        [Fact]
        public void Status_PropertyChanged_FiresOnChange()
        {
            var vm = new LicenseViewModel();
            var fired = false;
            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(vm.Status)) fired = true;
            };

            vm.Status = LicenseStatusTitles.Valid;

            Assert.True(fired);
        }

        [Fact]
        public void Status_PropertyChanged_DoesNotFire_WhenValueSame()
        {
            var vm = new LicenseViewModel();
            vm.Status = LicenseStatusTitles.Valid;

            var fired = false;
            vm.PropertyChanged += (_, _) => fired = true;

            vm.Status = LicenseStatusTitles.Valid; // same value — should no-op

            Assert.False(fired);
        }

        [Fact]
        public void ValidDays_DefaultIsZero()
        {
            var vm = new LicenseViewModel();
            Assert.Equal(0u, vm.ValidDays);
        }

        [Fact]
        public void ValidDays_CanBeSet()
        {
            var vm = new LicenseViewModel { ValidDays = 90 };
            Assert.Equal(90u, vm.ValidDays);
        }
    }
}
