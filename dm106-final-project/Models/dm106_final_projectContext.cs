using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace dm106_final_project.Models
{
    public class dm106_final_projectContext : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx
    
        public dm106_final_projectContext() : base("name=dm106_final_projectContext")
        {
        }

        public System.Data.Entity.DbSet<dm106_final_project.Models.Product> Products { get; set; }

        public System.Data.Entity.DbSet<dm106_final_project.Models.Order> Orders { get; set; }

    }
}
