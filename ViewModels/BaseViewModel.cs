using LicenseManagement.EndUser.Wpf.Views;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace LicenseManagement.EndUser.Wpf.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        DateTime? _updated;
        DateTime? _created;

        /// <summary>
        /// the date this object was first created in db
        /// </summary>
        public virtual DateTime? Created
        {
            get => _created; set
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
            get => _updated; set
            {
                if (_updated != value)
                {
                    _updated = value;
                    OnPropertyChanged();
                }
            }
        }

        protected void ShowErrorView(Window obj, Exception exception)
        {
            if (!obj.IsActive)
            {
                return;
            }
            var view = new ErrorView
            {
                Owner = obj,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                DataContext = exception
            };
            view.ShowDialog();
        }

        public event PropertyChangedEventHandler PropertyChanged;


        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
