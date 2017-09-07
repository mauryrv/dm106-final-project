using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using dm106_final_project.Models;
using Microsoft.AspNet.Identity;
using System.Web;
using dm106_final_project.CRMClient;
using dm106_final_project.br.com.correios.ws;

namespace dm106_final_project.Controllers
{
    [Authorize(Roles = "ADMIN, USER")]
    [RoutePrefix("api/orders")]
    public class OrdersController : ApiController
    {
        private dm106_final_projectContext db = new dm106_final_projectContext();

        // GET: api/Orders
        [Authorize(Roles = "ADMIN")]
        public IQueryable<Order> GetOrders()
        {
            return db.Orders;
        }

        // GET: api/Orders/5
        [ResponseType(typeof(Order))]
        public IHttpActionResult GetOrder(int id)
        {
            Order order = db.Orders.Find(id);

            if (order == null)
            {
                return BadRequest("Pedido não encontrado!");
            }

            var usrID = HttpContext.Current.User.Identity.GetUserId();
            ApplicationDbContext dbContext = new ApplicationDbContext();
            var mailUserLogged = dbContext.Users.Find(usrID).Email;

            if (mailUserLogged == order.userMail || User.IsInRole("ADMIN"))
            {

                return Ok(order);
            }
            else
            {
                return BadRequest("Acesso negado!");
            }
        }
        
        [ResponseType(typeof(List<Order>))]
        [Route("byEmail")]
        public IHttpActionResult GetOrderbyEmail(string email)
        {
            List<Order> orders = new List<Order>();
            orders = db.Orders.Where(c => c.userMail == email).ToList(); ;

            if (orders.Count == 0)
            {
                return BadRequest("Não há pedidos para o email pesquisado!");
            }

            var usrID = HttpContext.Current.User.Identity.GetUserId();
            ApplicationDbContext dbContext = new ApplicationDbContext();
            var mailUserLogged = dbContext.Users.Find(usrID).Email;

            if (mailUserLogged == orders[0].userMail || User.IsInRole("ADMIN"))
            {

                return Ok(orders);
            }
            else
            {
                return BadRequest("Acesso negado!");
            }
        }
        
        // PUT: api/Orders/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutOrder(int id, Order order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != order.Id)
            {
                return BadRequest();
            }

            db.Entry(order).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return BadRequest("Pedido atualizado!");
            //return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Orders
        [ResponseType(typeof(Order))]
        [HttpPost]
        public IHttpActionResult PostOrder(Order order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            order.status = "novo";
            order.pesoTotal = 0;
            order.precoFrete = 0;
            order.precoTotal = 0;
            order.dataPedido = DateTime.Now;

            var usrID = HttpContext.Current.User.Identity.GetUserId();
            ApplicationDbContext dbContext = new ApplicationDbContext();
            var mailUserLogged = dbContext.Users.Find(usrID).Email;

            order.userMail = mailUserLogged;

            db.Orders.Add(order);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = order.Id }, order);
        }

        // DELETE: api/Orders/5
        [ResponseType(typeof(string))]
        public IHttpActionResult DeleteOrder(int id)
        {
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return BadRequest("Pedido não encontrado!");
            }


            var usrID = HttpContext.Current.User.Identity.GetUserId();
            ApplicationDbContext dbContext = new ApplicationDbContext();
            var mailUserLogged = dbContext.Users.Find(usrID).Email;

            if (mailUserLogged == order.userMail || User.IsInRole("ADMIN"))
            {

                db.Orders.Remove(order);
                db.SaveChanges();

                return Ok("Pedido deletado!");
            }
            else
            {
                return BadRequest("Acesso negado!");

            }



        }

        [ResponseType(typeof(string))]
        [HttpPut]
        [Route("close")]
        public IHttpActionResult CloseOrder(int id)
        {

            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return BadRequest("Pedido não encontrado!");
            }

            var usrID = HttpContext.Current.User.Identity.GetUserId();
            ApplicationDbContext dbContext = new ApplicationDbContext();
            var mailUserLogged = dbContext.Users.Find(usrID).Email;

            if (mailUserLogged == order.userMail || User.IsInRole("ADMIN"))
            {
                if (order.precoFrete != 0)
                {
                    order.status = "fechado";
                    db.Entry(order).State = EntityState.Modified;

                    try
                    {
                        db.SaveChanges();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!OrderExists(id))
                        {
                            return NotFound();
                        }
                    }

                    return Ok("Pedido fechado!");
                }
                else
                {
                    return BadRequest("Frete não calculado!");

                }

            }
            else
            {
                return BadRequest("Acesso negado!");
                //return StatusCode(HttpStatusCode.Forbidden);
            }
        }
        
        [ResponseType(typeof(string))]
        [HttpGet]
        [Route("frete")]
        public IHttpActionResult CalculaFrete(int id)
        {

            Order order = db.Orders.Find(id);

            var usrID = HttpContext.Current.User.Identity.GetUserId();
            ApplicationDbContext dbContext = new ApplicationDbContext();
            var mailUserLogged = dbContext.Users.Find(usrID).Email;

            if (mailUserLogged == order.userMail || User.IsInRole("ADMIN"))
            {

                if (order == null)
                {
                    return BadRequest("Pedido não existe!");
                }

                if (order.OrderItems.Count == 0)
                {
                    return BadRequest("Pedido sem itens!");
                }

                if (order.status != "novo")
                {
                    return BadRequest("Pedido não está com status de novo!");
                }

                string cepDest = getCEP(order.userMail);
                if (cepDest == "0")
                {
                    return BadRequest("Falha ao consultar o CRM!");
                }

                decimal valorTotal = calcValorTotal(order);
                decimal peso = calcPesoTotal(order);
                //para calculo do comprimento, altura, largura e diametro sempre pego o valor mais alto de cada item no pedido.
                decimal comprimento = calcComprimento(order);
                decimal altura = calcAltura(order);
                decimal largura = calcLargura(order);
                decimal diametro = calcDiametro(order);

                string freteMsg;
                string frete;
                DateTime prazoEntrega;
                CalcPrecoPrazoWS correios = new CalcPrecoPrazoWS();
                cResultado resultado = correios.CalcPrecoPrazo("", "", "40010", "37540000", cepDest, peso.ToString(), 1,
                    comprimento, altura, largura, diametro, "N", valorTotal, "S");

                if (resultado.Servicos[0].Erro.Equals("0"))
                {
                    frete = resultado.Servicos[0].Valor;
                    prazoEntrega = DateTime.Today.AddDays(Convert.ToInt16(resultado.Servicos[0].PrazoEntrega));
                    freteMsg = "Valor do frete: " + resultado.Servicos[0]
                    .Valor + " - Prazo de entrega: " + resultado.Servicos[0].PrazoEntrega + " dia(s)";

                    order.pesoTotal = peso;
                    order.precoFrete = decimal.Parse(frete);
                    order.precoTotal = valorTotal;

                    db.Entry(order).State = EntityState.Modified;
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!OrderExists(id))
                        {
                            return NotFound();
                        }
                    }

                    return Ok(freteMsg);
                }
                else
                {
                    return BadRequest("Código do erro: " + resultado.Servicos[0].Erro + "-" + resultado.Servicos[0].MsgErro);
                }
            }
            else
            {
                return BadRequest("Acesso negado!");
            }
        }

        private string getCEP(string mail)
        {
            string cep = "0";
            CRMRestClient crmClient = new CRMRestClient();
            Customer customer = crmClient.GetCustomerByEmail(mail);
            if (customer != null)
            {
                cep = customer.zip;
            }

            return cep;
        }

        private decimal calcValorTotal(Order order)
        {
            decimal valorTotal = 0;
            foreach(OrderItem item in order.OrderItems )
            {
                valorTotal += item.quantidade * item.Product.preco;

            }
            return valorTotal;

        }

        private decimal calcPesoTotal(Order order)
        {

            decimal pesoTotal = 0;
            foreach (OrderItem item in order.OrderItems)
            {
                pesoTotal += item.Product.peso;

            }
            return pesoTotal;

        }

        private decimal calcComprimento(Order order)
        {

            List<decimal> comprimentoList = new List<decimal>();
            decimal comprimento = 0;
            foreach (OrderItem item in order.OrderItems)
            {
                comprimentoList.Add(item.Product.comprimento);

            }
            comprimento = comprimentoList.Max();
            return comprimento;

        }

        private decimal calcAltura(Order order)
        {

            List<decimal> alturaList = new List<decimal>();
            decimal altura = 0;
            foreach (OrderItem item in order.OrderItems)
            {
                alturaList.Add(item.Product.altura);

            }
            altura = alturaList.Max();
            return altura;
        }

        private decimal calcLargura(Order order)
        {

            List<decimal> larguraList = new List<decimal>();
            decimal largura = 0;
            foreach (OrderItem item in order.OrderItems)
            {
                larguraList.Add(item.Product.largura);

            }
            largura = larguraList.Max();
            return largura;

        }

        private decimal calcDiametro(Order order)
        {

            List<decimal> diametroList = new List<decimal>();
            decimal diametro = 0;
            foreach (OrderItem item in order.OrderItems)
            {
                diametroList.Add(item.Product.diametro);

            }
            diametro = diametroList.Max();
            return diametro;

        }
       
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool OrderExists(int id)
        {
            return db.Orders.Count(e => e.Id == id) > 0;
        }
    }
}