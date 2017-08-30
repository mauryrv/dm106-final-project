using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace dm106_final_project.Models
{
    public class OrderItem
    {

        public int Id { get; set; }
        public int quantidade { get; set; }
        // Foreign Key
        public int ProductId { get; set; }
        // Navigation property
        public int OrderId { get; set; }
        public virtual Product Product { get; set; }
       
    }
}
 