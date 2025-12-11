using Hymma.Lm.EndUser.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hymma.Lm.EndUser.Wpf.ViewModels
{
    public class ProductViewModel : BaseViewModel
    {
        public static ProductViewModel FromProductModel(Models.ProductModel model)
        {
            if (model ==null)
                return null;
            
            var p = new ProductViewModel
            {
                Id = model.Id,
                Name = model.Name,
                Created = model.Created,
                Updated = model.Updated
            };
            return p;
        }

        string _id;
        string _name;
        /// <summary>
        /// this is actually the Ulid of the product with prefix PRD_
        /// </summary>
        public string Id
        {
            get => _id; set
            {
                if (_id != value)
                {
                    _id = value;
                }
            }
        }

        /// <summary>
        /// this is the user defined name of the product
        /// </summary>
        public string Name
        {
            get => _name; set
            {
                if (_name != value)
                {
                    _name = value;
                }
            }
        }
        //public override string ToString()
        //{
        //    return Id + Name; 
        //}
    }
}
