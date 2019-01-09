using SharePointPnP.ProvisioningApp.DomainModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SharePointPnP.ProvisioningApp.WebSite.Models
{
    public class IndexViewModel
    {
        private List<Package> _packages;
        public List<Package> Packages
        {
            get
            {
                if(_packages == null)
                {
                    _packages = new List<Package>();
                }

                return _packages;
            }
            set
            {
                _packages = value;
            }
        }

        public String ServiceDescription { get; set; }
    }
}