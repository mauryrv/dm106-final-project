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
    [Authorize]
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
                return NotFound();
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
                return StatusCode(HttpStatusCode.Forbidden);
            }
        }

        [ResponseType(typeof(List<Order>))]
        [HttpGet]
        [Route("byEmail")]
        public IHttpActionResult GetOrderbyEmail(string email)
        {
            List<Order> orders = new List<Order>();
            orders = db.Orders.Where(c => c.userMail == email).ToList(); ;

            if (orders.Count == 0)
            {
                return NotFound();
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
                return StatusCode(HttpStatusCode.Forbidden);
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

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Orders
        [ResponseType(typeof(Order))]
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
        [ResponseType(typeof(Order))]
        public IHttpActionResult DeleteOrder(int id)
        {
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return NotFound();
            }


            var usrID = HttpContext.Current.User.Identity.GetUserId();
            ApplicationDbContext dbContext = new ApplicationDbContext();
            var mailUserLogged = dbContext.Users.Find(usrID).Email;

            if (mailUserLogged == order.userMail || User.IsInRole("ADMIN"))
            {

                db.Orders.Remove(order);
                db.SaveChanges();

                return Ok(order);
            }
            else
            {
                return StatusCode(HttpStatusCode.Forbidden);
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
                return NotFound();
            }

            var usrID = HttpContext.Current.User.Identity.GetUserId();
            ApplicationDbContext dbContext = new ApplicationDbContext();
            var mailUserLogged = dbContext.Users.Find(usrID).Email;

            if (mailUserLogged == order.userMail || User.IsInRole("ADMIN"))
            {
                if(order.precoFrete!=0)
                {
                    order.status = "fechado";

                    PutOrder(id, order);
                    order = db.Orders.Find(id);

                    return Ok(order);
                }
                else
                {
                    return BadRequest("Frete não calculado!");

                }
               
            }
            else
            {
                return StatusCode(HttpStatusCode.Forbidden);
            }
        }



        [ResponseType(typeof(string))]
        [HttpGet]
        [Route("frete")]
        public IHttpActionResult CalculaFrete()
        {
            string frete;
            CalcPrecoPrazoWS correios = new CalcPrecoPrazoWS();
            cResultado resultado = correios.CalcPrecoPrazo("", "",
            "40010", "37540000", "37002970", "1", 1, 30, 30, 30, 30, "N", 100, "S");
            if (resultado.Servicos[0].Erro.Equals("0"))
            {
                frete = "Valor do frete: " + resultado.Servicos[0]
                .Valor + " - Prazo de entrega: " + resultado.Servicos[0].PrazoEntrega + " dia(s)";
                return Ok(frete);
            }
            else
            {
                return BadRequest("Código do erro: " + resultado.Servicos[0].Erro + "-" + resultado.Servicos[0].MsgErro);
            }
        }


        [ResponseType(typeof(string))]
        [HttpGet]
        [Route("cep")]
        public IHttpActionResult getCEP()
        {
            CRMRestClient crmClient = new CRMRestClient();
            Customer customer = crmClient.GetCustomerByEmail(User.
            Identity.Name);
            if (customer != null)
            {
                return Ok(customer.zip);
            }
            else
            {
                return BadRequest("Falha ao consultar o CRM");
            }
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