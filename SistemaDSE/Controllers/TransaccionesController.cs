﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SistemaDSE.Models;
using SistemaDSE.Tools;

namespace SistemaDSE.Controllers
{
    public class TransaccionesController : Controller
    {
        // GET: TransaccionesController
        public async Task<IActionResult> Index(int? pageNumber)
        {
            string numeroTarjeta = HttpContext.Session.GetString("numeroTarjeta");
            string tipoDeTransaccion = HttpContext.Session.GetString("tipoTransaccion");
            ViewBag.tipoDeTransaccion = tipoDeTransaccion;

            List<TransaccionModel> transaccionesResult = await GetTransacciones(numeroTarjeta, tipoDeTransaccion);
            var transacciones = from s in transaccionesResult select s;

            transacciones = transacciones.OrderBy(s => s.Fecha);
            int pageSize = 6;

            return View(await PaginatedList<TransaccionModel>.CreateAsync(transacciones.ToList(), pageNumber ?? 1, pageSize));
        }

        // GET: TransaccionesController/Create
        public ActionResult Create()
        {
            ViewBag.tipoDeTransaccion = HttpContext.Session.GetString("tipoTransaccion");

            return View();
        }


        [HttpGet]
        public async Task<List<TransaccionModel>> GetTransacciones(string numeroTarjeta, string tipoTransaccion)
        {
            List<TransaccionModel> transaccionesResponse = new List<TransaccionModel>();
            int mesActual = DateTime.Now.Month;

            using (HttpClient client = new HttpClient())
            {
                string urlAPI = $"https://localhost:7001/api/transacciones/{numeroTarjeta}?mes={mesActual}&tipo={tipoTransaccion}";

                HttpResponseMessage response = await client.GetAsync(urlAPI);

                if (response.IsSuccessStatusCode)
                {
                    string responseJSON = await response.Content.ReadAsStringAsync();

                    transaccionesResponse = JsonConvert.DeserializeObject<List<TransaccionModel>>(responseJSON);
                }
            }

            return transaccionesResponse;
        }

        [HttpPost]
        public ActionResult Create(TransaccionModel transaccionModel)
        {
            string urlAPI = $"https://localhost:7001/api/transacciones";

            string numeroTarjeta = HttpContext.Session.GetString("numeroTarjeta");
            string tipoDeTransaccion = HttpContext.Session.GetString("tipoTransaccion");
            DateTime fechaActual = DateTime.UtcNow;

            transaccionModel.NumeroTarjeta = numeroTarjeta;
            transaccionModel.Tipo = tipoDeTransaccion;
            transaccionModel.Fecha = fechaActual;

            if (tipoDeTransaccion == "Pago")
            {
                transaccionModel.Descripcion = tipoDeTransaccion;
            }

            using (HttpClient client = new HttpClient())
            {
                var response = client.PostAsJsonAsync<TransaccionModel>(urlAPI, transaccionModel);
                response.Wait();

                if (response.Result.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index", "Transacciones");
                }
            }

            ModelState.AddModelError(string.Empty, "No se pudo procesar su transacción, revise los datos e intente de nuevo.");
            ViewBag.tipoDeTransaccion = HttpContext.Session.GetString("tipoTransaccion");
            return View();
        }

    }
}
