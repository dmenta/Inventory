using MiInventario.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace MiInventario.Controllers
{
    [Authorize]
    public class GruposController : BaseController
    {
        [HttpGet]
        public ActionResult Index()
        {
            DateTime inicio = DateTime.Now;

            string user = User.Identity.GetUserName();
            InventarioEntities db = new InventarioEntities();

            var enCapsulas = db.CapsulasItems
                .Where(s => s.Capsulas.IdUsuario == user)
                .GroupBy(t => t.ItemID)
                .Select(u => new { ItemID = u.Key, CantidadCapsula = u.Sum(v => v.Cantidad) }).ToList();

            var inventario = db.Inventarios
                .Where(s => s.IdUsuario == user)
                .Select(u => new { ItemID = u.ItemID, Cantidad = u.Cantidad }).ToList();

            var model = GroupsXml.Select(p => new GrupoViewModel
                            {
                                GroupID = p.GroupID,
                                Total = inventario.Where(s => p.Types.SelectMany(t => t.Items).Any(u => u.ItemID == s.ItemID)).DefaultIfEmpty().Sum(v => v == null ? 0 : v.Cantidad),
                                TotalCapsulas = enCapsulas.Where(s => p.Types.SelectMany(t => t.Items).Any(u => u.ItemID == s.ItemID)).DefaultIfEmpty().Sum(v => v == null ? 0 : v.CantidadCapsula)
                            }).ToList();

            double elapsed = (DateTime.Now - inicio).TotalMilliseconds;

            return View(model);
        }

        [HttpGet]
        public ActionResult ViewAll()
        {
            return View(RecuperarGrupos());
        }

        [HttpGet]
        public ActionResult List(string id)
        {
            var grupo = RecuperarGrupo(id);
            if (grupo == null)
            {
                return new HttpNotFoundResult();
            }

            return View(grupo);
        }

        [HttpGet]
        public ActionResult Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return new HttpNotFoundResult();
            }
            if (!GroupsXml.Any(p => p.GroupID == id))
            {
                return new HttpNotFoundResult();
            }

            InventarioEntities db = new InventarioEntities();
            string user = User.Identity.GetUserName();

            var enCapsulas = db.CapsulasItems
                        .Where(s => s.Capsulas.IdUsuario == user)
                        .GroupBy(t => t.ItemID)
                        .Select(u => new { ItemID = u.Key, CantidadCapsula = u.Sum(v => v.Cantidad) }).ToList();

            var inventario = db.Inventarios
                .Where(s => s.IdUsuario == user)
                .Select(u => new { ItemID = u.ItemID, Cantidad = u.Cantidad }).ToList();

            var grupo = GroupsXml.Where(z => z.GroupID == id)
                .Select(p => new
                {
                    GroupID = p.GroupID,
                    Items = p.Types.SelectMany(z => z.Items).Select(r => new ItemEditViewModel
                    {
                        CurrentItem = r,
                        Cantidad = inventario.SingleOrDefault(s => s.ItemID == r.ItemID) == null ? 0 : inventario.Single(s => s.ItemID == r.ItemID).Cantidad,
                        CantidadCapsulas = enCapsulas.SingleOrDefault(s => s.ItemID == r.ItemID) == null ? 0 : enCapsulas.Single(s => s.ItemID == r.ItemID).CantidadCapsula
                    })
                }).Single();

            GrupoEditViewModel model = new GrupoEditViewModel();
            model.GroupID = grupo.GroupID;
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

            if (ModelState.IsValid)
            {
                foreach (ItemEditViewModel item in grupo.Items)
                {
                    string itemID = item.CurrentItem.ItemID;
                    Inventarios inv = db.Inventarios.SingleOrDefault(p => p.IdUsuario == user && p.ItemID == itemID);

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
                            db.Inventarios.Add(new Inventarios { IdUsuario = user, ItemID = itemID, Cantidad = item.Cantidad });
                        }
                    }
                }

                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(grupo);
        }

        private GrupoViewModel RecuperarGrupo(string id)
        {
            var grupo = RecuperacionGrupos(id);
            if (grupo == null)
            {
                return null;
            }
            else
            {
                return grupo.Single();
            }
        }
        private List<GrupoViewModel> RecuperarGrupos()
        {
            return RecuperacionGrupos(null);
        }

        private List<GrupoViewModel> RecuperacionGrupos(string id)
        {
            if (!string.IsNullOrWhiteSpace(id) && !GroupsXml.Any(p => p.GroupID == id))
            {
                return null;
            }

            DateTime inicio = DateTime.Now;
            InventarioEntities db = new InventarioEntities();
            string user = User.Identity.GetUserName();

            var enCapsulas = db.CapsulasItems
                        .Where(s => s.Capsulas.IdUsuario == user)
                        .GroupBy(t => t.ItemID)
                        .Select(u => new { ItemID = u.Key, CantidadCapsula = u.Sum(v => v.Cantidad) }).ToList();

            var inventario = db.Inventarios
                .Where(s => s.IdUsuario == user)
                .Select(u => new { ItemID = u.ItemID, Cantidad = u.Cantidad }).ToList();


            var grupos = GroupsXml.Where(z => string.IsNullOrWhiteSpace(id) || z.GroupID == id)
                .Select(p => new GrupoViewModel
            {
                GroupID = p.GroupID,
                Tipos = p.Types.Select(q => new TipoViewModel
                {
                    TypeID = q.TypeID,
                    Items = q.Items.Select(r => new ItemInventoryViewModel
                    {
                        CurrentItem = r,
                        Cantidad = inventario.SingleOrDefault(s => s.ItemID == r.ItemID) == null ? 0 : inventario.Single(s => s.ItemID == r.ItemID).Cantidad,
                        CantidadCapsulas = enCapsulas.SingleOrDefault(s => s.ItemID == r.ItemID) == null ? 0 : enCapsulas.Single(s => s.ItemID == r.ItemID).CantidadCapsula
                    })
                })
            });
            double elapsed = (DateTime.Now - inicio).TotalMilliseconds;

            var lista = new List<GrupoViewModel>();
            if (!string.IsNullOrWhiteSpace(id))
            {
                var model = grupos.Single();
                model.Total = model.Tipos.SelectMany(q => q.Items).Sum(r => r.Cantidad);
                model.TotalCapsulas = model.Tipos.SelectMany(q => q.Items).Sum(r => r.CantidadCapsulas);

                lista.Add(model);
            }
            else
            {
                lista = grupos.ToList();
            }

            return lista;
        }

        public ActionResult Difference()
        {
            InventarioEntities db = new InventarioEntities();
            string user = User.Identity.GetUserName();
            if (user != "diegomenta@gmail.com" && user != "pceriani@gmail.com")
            {
                return new HttpNotFoundResult();
            }

            DifferenceViewModel model = new DifferenceViewModel();
            string usuarioA = user;
            string usuarioB = (user == "diegomenta@gmail.com" ? "pceriani@gmail.com" : "diegomenta@gmail.com");

            model.UsuarioA = usuarioA;
            model.UsuarioB = usuarioB;

            var inventarioA = db.Inventarios
    .Where(s => s.IdUsuario == usuarioA)
    .Select(u => new { ItemID = u.ItemID, Cantidad = u.Cantidad }).ToList();
            var inventarioB = db.Inventarios
    .Where(s => s.IdUsuario == usuarioB)
    .Select(u => new { ItemID = u.ItemID, Cantidad = u.Cantidad }).ToList();


            model.Grupos = GroupsXml
                .Select(p => new GrupoDifferenceViewModel
                {
                    GroupID = p.GroupID,
                    Tipos = p.Types.Select(q => new TipoDifferenceViewModel
                    {
                        TypeID = q.TypeID,
                        Items = q.Items.Select(r => new ItemDifferenceViewModel
                        {
                            CurrentItem = r,
                            CantidadUsuarioA = inventarioA.SingleOrDefault(s => s.ItemID == r.ItemID) == null ? 0 : inventarioA.Single(s => s.ItemID == r.ItemID).Cantidad,
                            CantidadUsuarioB = inventarioB.SingleOrDefault(s => s.ItemID == r.ItemID) == null ? 0 : inventarioB.Single(s => s.ItemID == r.ItemID).Cantidad
                        })
                    })
                }).ToList();

            return View(model);
        }
    }
}