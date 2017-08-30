using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace dm106_final_project.Models
{
    public class Product
    {

 
        public int Id { get; set; }

        [Required(ErrorMessage = "O campo nome é obrigatório")]
        public string nome { get; set; }
        public string descricao { get; set; }
        public string color { get; set; }

        [Required(ErrorMessage = "O campo modelo é obrigatório")]
        public string modelo { get; set; }

        [Required(ErrorMessage = "O campo código é obrigatório")]
        //[StringLength(8, ErrorMessage = "O tamanho máximo do código é 8 caracteres")]
        public string codigo { get; set; }
        public decimal preco { get; set; }
        public decimal peso { get; set; }
        public decimal altura { get; set; }
        public decimal largura { get; set; }
        public decimal comprimento { get; set; }
        public decimal diametro { get; set; }
        public string url { get; set; }

    }
}