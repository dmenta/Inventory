using MiInventario2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace MiInventario2.Controllers
{
    [Authorize]
    public class GruposController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            string user=User.Identity.GetUserName();
            InventarioEntities db = new InventarioEntities();

            var model = db.Grupos.Select(p => new GrupoViewModel
                            {
                                IdGrupo = p.IdGrupo,
                                Descripcion = p.Descripcion,
                                Total = p.Tipos.SelectMany(q => q.Items).SelectMany(r => r.Inventarios.Where(s => s.IdUsuario == user)).DefaultIfEmpty().Sum(t => t == null ? 0 : t.Cantidad),
                                TotalCapsulas = p.Tipos.SelectMany(q => q.Items).SelectMany(r => r.CapsulasItems.Where(s => s.Capsulas.IdUsuario == user)).DefaultIfEmpty().Sum(t => t == null ? 0 : t.Cantidad)
                            }).ToList();

            return View(model);
        }

        [HttpGet]
        public ActionResult ViewAll()
        {
            return RecuperarGrupo(null);
        }

        [HttpGet]
        public ActionResult List(int id)
        {
            return RecuperarGrupo(id);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            InventarioEntities db = new InventarioEntities();
            string user = User.Identity.GetUserName();

            var grupo = db.Grupos.Where(p => p.IdGrupo == id).Select(p => new 
            {
                IdGrupo = p.IdGrupo,
                Descripcion = p.Descripcion,
                Items = p.Tipos.SelectMany(q=> q.Items).Select(r => new ItemViewModel
                    {
                        IdItem = r.IdItem,
                        IdTipo = r.IdTipo,
                        IdGrupo = p.IdGrupo,
                        Descripcion = r.Nombre ?? r.Tipos.Descripcion,
                        MostrarNivel = r.Tipos.MostrarNivel,
                        Nivel = r.IdNivel,
                        MostrarRareza = r.Tipos.MostrarRareza,
                        IdRareza = r.IdRareza,
                        Rareza = r.Rarezas.Descripcion,
                        Cantidad = r.Inventarios.Where(s => s.IdUsuario == user).DefaultIfEmpty().Sum(t => t == null ? 0 : t.Cantidad),
                        CantidadCapsulas = r.CapsulasItems.Where(s => s.Capsulas.IdUsuario == user).DefaultIfEmpty().Sum(t => t == null ? 0 : t.Cantidad)
                })
            }).Single();

            GrupoEditViewModel model = new GrupoEditViewModel();
            model.IdGrupo = grupo.IdGrupo;
            model.Descripcion= grupo.Descripcion;
            model.Items = grupo.Items.ToList();
            model.Total = model.Items.Sum(r => r.Cantidad);
            model.TotalCapsulas = model.Items.Sum(r => r.CantidadCapsulas);

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(GrupoEditViewModel grupo)
        {
            InventarioEntities db = new InventarioEntities();
            string user = User.Identity.GetUserName();

            var inventarios = db.Inventarios.Where(p => p.IdUsuario == user && p.Items.Tipos.IdGrupo == grupo.IdGrupo).ToList();

            foreach (ItemViewModel item in grupo.Items)
            {
                Inventarios inv = inventarios.SingleOrDefault(p => p.IdItem == item.IdItem);

                if (inv != null)
                {
                    if (item.Cantidad == 0)
                    {
                        db.Inventarios.Remove(inv);
                    }
                    else
                    {
                        inv.Cantidad = item.Cantidad;
                    }
                }
                else
                {
                    if (item.Cantidad > 0)
                    {
                        db.Inventarios.Add(new Inventarios { IdUsuario = user, IdItem = item.IdItem, Cantidad = item.Cantidad });
                    }
                }
            }

            db.SaveChanges();

            return RedirectToAction("Index", new { id = grupo.IdGrupo });
        }

        private ActionResult RecuperarGrupo(int? id)
        {
            InventarioEntities db = new InventarioEntities();
            string user = User.Identity.GetUserName();

            var grupos = db.Grupos.Where(o => (!id.HasValue || o.IdGrupo == id.Value)).Select(p => new GrupoViewModel
            {
                IdGrupo = p.IdGrupo,
                Descripcion = p.Descripcion,
                Tipos = p.Tipos.Select(q => new TipoViewModel
                {
                    IdTipo = q.IdTipo,
                    Items = q.Items.Select(r => new ItemViewModel
                    {
                        IdItem = r.IdItem,
                        IdTipo = r.IdTipo,
                        IdGrupo = p.IdGrupo,
                        Descripcion = r.Nombre ?? q.Descripcion,
                        MostrarNivel = q.MostrarNivel,
                        Nivel = r.IdNivel,
                        MostrarRareza = q.MostrarRareza,
                        IdRareza = r.IdRareza,
                        Rareza = r.Rarezas.Descripcion,
                        Cantidad = r.Inventarios.Where(s => s.IdUsuario == user).DefaultIfEmpty().Sum(t => t == null ? 0 : t.Cantidad),
                        CantidadCapsulas = r.CapsulasItems.Where(s => s.Capsulas.IdUsuario == user).DefaultIfEmpty().Sum(t => t == null ? 0 : t.Cantidad)
                    })
                })
            });

            if (id.HasValue)
            {
                var model = grupos.Single();
                model.Total = model.Tipos.SelectMany(q => q.Items).Sum(r => r.Cantidad);
                model.TotalCapsulas = model.Tipos.SelectMany(q => q.Items).Sum(r => r.CantidadCapsulas);

                return View(model);
            }
            else
            {
                var model = grupos.ToList();

                return View(model);
            }
        }
    }
}