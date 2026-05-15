using LicenseManagement.EndUser.Wpf.Views;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace LicenseManagement.EndUser.Wpf.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        private DateTime? _updated;
        private DateTime? _created;

        /// <summary>
        /// the date this object was first created in db
        /// </summary>
        public virtual DateTime? Created
        {
            get => _created;
            set
            {
                if (_created != value)
                {
                    _created = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// represents the date this row was updated in db
        /// </summary>
        public virtual DateTime? Updated
        {
            get => _updated;
            set
            {
                if (_updated != value)
                {
                    _updated = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Resolves the owning <see cref="Window"/> for the given source (which may be
        /// a Window, a UserControl, or null) and shows an error dialog for the
        /// supplied exception. No-op if no owner can be resolved.
        /// </summary>
        protected void ShowErrorView(DependencyObject source, Exception exception)
        {
            if (exception == null) return;

            var owner = source is Window w ? w : (source != null ? Window.GetWindow(source) : null);
            if (owner == null || !owner.IsActive) return;

            var view = new ErrorView
            {
                Owner = owner,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                DataContext = exception
            };
            view.ShowDialog();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
