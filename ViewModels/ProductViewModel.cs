using LicenseManagement.EndUser.Models;

namespace LicenseManagement.EndUser.Wpf.ViewModels
{
    public class ProductViewModel : BaseViewModel
    {
        public static ProductViewModel FromProductModel(ProductModel model)
        {
            if (model == null)
                return null;

            return new ProductViewModel
            {
                Id = model.Id,
                Name = model.Name,
                Created = model.Created,
                Updated = model.Updated
            };
        }

        private string _id;
        private string _name;

        /// <summary>
        /// this is actually the Ulid of the product with prefix PRD_
        /// </summary>
        public string Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// this is the user defined name of the product
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
