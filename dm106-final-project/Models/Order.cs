using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace dm106_final_project.Models
{
    public class Order
    {

        public Order()
        {
            this.OrderItems = new HashSet<OrderItem>();
        }
        public int Id { get; set; }
        public string userMail { get; set; }
        public DateTime dataPedido { get; set; }
        public DateTime dataEntrega { get; set; }
        public string status { get; set; }
        public decimal precoTotal { get; set; }
        public decimal pesoTotal { get; set; }
        public decimal precoFrete { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }
}