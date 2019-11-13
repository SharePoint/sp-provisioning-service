using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SharePoint.Portal.Web.Models
{
    public class BaseModel<TKey>
    {
        [Key]
        public TKey Id { get; set; }
    }
}
