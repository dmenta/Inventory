using MiInventario2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace MiInventario2.Controllers {
  [Authorize]
  public class CapsulasController : Controller {
    public const int IdGrupoCapsulas = 6;
    public static readonly List<int> CapsulasSpawnables = new List<int>(new[] { 51 });
    [HttpGet]
    public ActionResult Index() {
      InventarioEntities db = new InventarioEntities();
      string user = User.Identity.GetUserName();
      var capsulas = db.Capsulas.Where(p => p.IdUsuario == user);

      List<CapsulasViewModel> model = new List<CapsulasViewModel>();
      foreach (var capsula in capsulas) {
        int cantidad = capsula.CapsulasItems.Sum(s => s.Cantidad);

        string itemEncapsulado = capsula.Descripcion;

        if (itemEncapsulado == null) {
          if (capsula.CapsulasItems.Count() == 1) {
            var elemento = capsula.CapsulasItems.First();
            itemEncapsulado = elemento.Items.Nombre ?? elemento.Items.Tipos.Descripcion;
            if (elemento.Items.Tipos.MostrarNivel) {
              itemEncapsulado += string.Format(" L{0}", elemento.Items.IdNivel);
            }
            if (elemento.Items.Tipos.MostrarRareza) {
              itemEncapsulado += " " + elemento.Items.Rarezas.Descripcion;
            }
          }
          else if (capsula.CapsulasItems.Count() > 1) {
            itemEncapsulado = string.Format("({0} items)", capsula.CapsulasItems.Count());
          }
        }

        model.Add(new CapsulasViewModel { IdCapsula = capsula.IdCapsula, ItemEncapsulado = itemEncapsulado, Total = cantidad, Spawnable = CapsulasSpawnables.Contains(capsula.IdItem) });
      }

      return View(model);
    }
    [HttpGet]
    public ActionResult Add() {
      InventarioEntities db = new InventarioEntities();
      var model = new CapsulasCreateViewModel();
      model.Capsulas = db.Grupos.Where(p => p.IdGrupo == IdGrupoCapsulas)
          .SelectMany(q => q.Tipos)
          .SelectMany(r => r.Items)
          .Select(s => new { s.IdItem, Nombre = s.Nombre ?? s.Tipos.Descripcion }).ToDictionary(t => t.IdItem, u => u.Nombre);

      return View(model);
    }
    public ActionResult Add(CapsulasCreateViewModel capsula) {
      InventarioEntities db = new InventarioEntities();

      if (ModelState.IsValid) {
        string user = User.Identity.GetUserName();

        db.Capsulas.Add(new Capsulas { IdUsuario = user, IdCapsula = capsula.IdCapsula.ToUpper(), Descripcion = capsula.Descripcion, IdItem = capsula.IdItem });

        db.SaveChanges();

        return RedirectToAction("Index");
      }

      capsula.Capsulas = db.Grupos.Where(p => p.IdGrupo == IdGrupoCapsulas)
                      .SelectMany(q => q.Tipos)
                      .SelectMany(r => r.Items)
                      .Select(s => new { s.IdItem, Nombre = s.Nombre ?? s.Tipos.Descripcion }).ToDictionary(t => t.IdItem, u => u.Nombre);

      return View(capsula);
    }

    [HttpGet]
    public ActionResult Edit(string id) {
      InventarioEntities db = new InventarioEntities();
      string user = User.Identity.GetUserName();
      var capsulaDB = db.Capsulas.SingleOrDefault(p => p.IdCapsula == id && p.IdUsuario == user);

      if (capsulaDB == null) {
        return new HttpNotFoundResult();
      }

      var model = new CapsulasEditViewModel();
      model.IdCapsula = capsulaDB.IdCapsula;
      model.Descripcion = capsulaDB.Descripcion;
      model.IdItem = capsulaDB.IdItem;
      model.Capsulas = db.Grupos.Where(p => p.IdGrupo == IdGrupoCapsulas)
          .SelectMany(q => q.Tipos)
          .SelectMany(r => r.Items)
          .Select(s => new { s.IdItem, Nombre = s.Nombre ?? s.Tipos.Descripcion }).ToDictionary(t => t.IdItem, u => u.Nombre);

      return View(model);
    }
    public ActionResult Edit(CapsulasEditViewModel capsula) {
      InventarioEntities db = new InventarioEntities();
      string user = User.Identity.GetUserName();
      var capsulaDB = db.Capsulas.SingleOrDefault(p => p.IdCapsula == capsula.IdCapsula && p.IdUsuario == user);

      if (capsulaDB == null) {
        return new HttpNotFoundResult();
      }

      if (ModelState.IsValid) {
        capsulaDB.Descripcion = capsula.Descripcion;
        capsulaDB.IdItem = capsula.IdItem;

        db.SaveChanges();

        return RedirectToAction("Index");
      }

      capsula.Capsulas = db.Grupos.Where(p => p.IdGrupo == IdGrupoCapsulas)
                      .SelectMany(q => q.Tipos)
                      .SelectMany(r => r.Items)
                      .Select(s => new { s.IdItem, Nombre = s.Nombre ?? s.Tipos.Descripcion }).ToDictionary(t => t.IdItem, u => u.Nombre);
      return View(capsula);
    }

    [HttpGet]
    public ActionResult List(string id) {
      return RecuperarItems(id, null);

    }
    [HttpGet]
    public ActionResult ManageItems(string id) {
      return RecuperarItems(id, null);
    }

    [HttpGet]
    public ActionResult LogInterests(string id) {
      return RecuperarItems(id, true);
    }

    [HttpGet]
    public ActionResult AddItem(string id) {
      if (id == null) {
        return new HttpNotFoundResult();
      }

      InventarioEntities db = new InventarioEntities();
      string user = User.Identity.GetUserName();
      var capsula = db.Capsulas.SingleOrDefault(p => p.IdCapsula == id && p.IdUsuario == user);

      if (capsula == null) {
        return new HttpNotFoundResult();
      }

      var itemsCargados = capsula.CapsulasItems.Select(p => p.IdItem).ToList();

      CapsulasAddItemViewModel model = new CapsulasAddItemViewModel();
      model.IdCapsula = id;
      model.Total = capsula.CapsulasItems.Sum(s => s.Cantidad);
      var agregables = db.Items.Where(p => !itemsCargados.Contains(p.IdItem));
      foreach (Items item in agregables) {
        string descripcion = item.Nombre ?? item.Tipos.Descripcion;
        if (item.Tipos.MostrarNivel) {
          descripcion += string.Format(" L{0}", item.IdNivel);
        }
        if (item.Tipos.MostrarRareza) {
          descripcion += " " + item.Rarezas.Descripcion;
        }

        model.AddeableItems.Add(new ItemSummaryViewModel { IdItem = item.IdItem, Descripcion = descripcion });
      }

      return View(model);
    }

    [HttpPost]
    public ActionResult AddItem(CapsulasAddItemViewModel addedItem) {
      InventarioEntities db = new InventarioEntities();
      string user = User.Identity.GetUserName();
      var capsula = db.Capsulas.SingleOrDefault(p => p.IdCapsula == addedItem.IdCapsula && p.IdUsuario == user);

      if (capsula == null) {
        return new HttpNotFoundResult();
      }

      db.CapsulasItems.Add(new CapsulasItems { IdCapsula = addedItem.IdCapsula, IdItem = addedItem.IdItem, Cantidad = addedItem.Cantidad });

      db.SaveChanges();

      return RedirectToAction("List", new { id = addedItem.IdCapsula });
    }

    private ActionResult RecuperarItems(string id, bool? spawnable) {
      if (id == null) {
        return new HttpNotFoundResult();
      }

      InventarioEntities db = new InventarioEntities();
      string user = User.Identity.GetUserName();
      var capsula = db.Capsulas.SingleOrDefault(p => p.IdCapsula == id && p.IdUsuario == user && (!spawnable.HasValue || CapsulasSpawnables.Contains(p.IdItem)));

      if (capsula == null) {
        return new HttpNotFoundResult();
      }

      CapsulasManageViewModel model = new CapsulasManageViewModel();
      model.IdCapsula = id;
      model.Descripcion = capsula.Descripcion;
      model.Total = capsula.CapsulasItems.Sum(s => s.Cantidad);

      foreach (var item in capsula.CapsulasItems) {
        var itemM = new ItemViewModel {
          IdItem = item.IdItem,
          IdTipo = item.Items.IdTipo,
          Descripcion = item.Items.Nombre ?? item.Items.Tipos.Descripcion,
          MostrarNivel = item.Items.Tipos.MostrarNivel,
          Nivel = item.Items.IdNivel,
          MostrarRareza = item.Items.Tipos.MostrarRareza,
          IdRareza = item.Items.IdRareza,
          Rareza = item.Items.Rarezas.Descripcion,
          Cantidad = item.Cantidad
        };

        model.Items.Add(itemM);
      }

      return View(model);
    }

    [HttpPost]
    public ActionResult ManageItems(CapsulasManageViewModel capsula) {
      InventarioEntities db = new InventarioEntities();
      string user = User.Identity.GetUserName();
      var capsulaDB = db.Capsulas.SingleOrDefault(p => p.IdCapsula == capsula.IdCapsula && p.IdUsuario == user);

      if (capsulaDB == null) {
        return new HttpNotFoundResult();
      }

      foreach (ItemViewModel item in capsula.Items) {
        CapsulasItems capsulaItem = db.CapsulasItems.SingleOrDefault(p => p.IdCapsula == capsula.IdCapsula && p.IdItem == item.IdItem);

        if (capsulaItem != null) {
          capsulaItem.Cantidad = item.Cantidad;
        }
      }

      db.SaveChanges();

      return RedirectToAction("List", new { id = capsula.IdCapsula });
    }

    [HttpPost]
    public ActionResult LogInterests(CapsulasManageViewModel capsula) {
      InventarioEntities db = new InventarioEntities();
      string user = User.Identity.GetUserName();

      var capsulaDB = db.Capsulas.SingleOrDefault(p => p.IdCapsula == capsula.IdCapsula && p.IdUsuario == user);

      if (capsulaDB == null) {
        return new HttpNotFoundResult();
      }

      Dictionary<int, int> v_nuevos = new Dictionary<int, int>();

      foreach (ItemViewModel item in capsula.Items) {
        CapsulasItems capsulaItem = db.CapsulasItems.SingleOrDefault(p => p.IdCapsula == capsula.IdCapsula && p.IdItem == item.IdItem);

        if (capsulaItem != null) {
          if (capsulaItem.Cantidad > item.Cantidad) {
            break;
          }
          else if (capsulaItem.Cantidad < item.Cantidad) {
            v_nuevos.Add(item.IdItem, item.Cantidad - capsulaItem.Cantidad);
            capsulaItem.Cantidad = item.Cantidad;
          }
        }
        else {
          break;
        }
      }

      if (v_nuevos.Count > 0) {
        DateTime ahora = DateTime.Now;
        DateTime diferencia = ahora.AddMinutes(-15);
        Reproducciones v_ultimaCercana = db.Reproducciones.FirstOrDefault(p => p.IdUsuario == user && p.Fecha >= diferencia);
        if (v_ultimaCercana != null) {
          ahora = v_ultimaCercana.Fecha;
        }

        Reproducciones v_repro = new Reproducciones { IdCapsula = capsula.IdCapsula, IdUsuario = user, Fecha = ahora };
        foreach (var kv in v_nuevos) {
          v_repro.ReproduccionesItems.Add(new ReproduccionesItems { IdItem = kv.Key, Cantidad = kv.Value });
        }
        db.Reproducciones.Add(v_repro);

        db.SaveChanges();

        return RedirectToAction("List", new { id = capsula.IdCapsula });
      }

      return RedirectToAction("LogInterests", new { id = capsula.IdCapsula });
    }

    [HttpGet]
    public ActionResult DeleteItem(string id, int? idItem) {
      if (id == null || !idItem.HasValue) {
        return new HttpNotFoundResult();
      }

      InventarioEntities db = new InventarioEntities();
      string user = User.Identity.GetUserName();

      var capsulaDB = db.Capsulas.SingleOrDefault(p => p.IdCapsula == id && p.IdUsuario == user);

      if (capsulaDB == null) {
        return new HttpNotFoundResult();
      }

      var item = capsulaDB.CapsulasItems.SingleOrDefault(p => p.IdItem == idItem);

      if (item == null) {
        return new HttpNotFoundResult();
      }

      ItemSummaryViewModel summary = new ItemSummaryViewModel();
      string descripcion = item.Items.Nombre ?? item.Items.Tipos.Descripcion;
      if (item.Items.Tipos.MostrarNivel) {
        descripcion += string.Format(" L{0}", item.Items.IdNivel);
      }
      if (item.Items.Tipos.MostrarRareza) {
        descripcion += " " + item.Items.Rarezas.Descripcion;
      }
      summary.IdItem = idItem.Value;
      summary.Descripcion = descripcion;

      return View(new CapsulasDeleteItemViewModel { IdCapsula = id, Total = capsulaDB.CapsulasItems.Sum(s => s.Cantidad), Item = summary, Cantidad = item.Cantidad });
    }

    [HttpPost]
    public ActionResult DeleteItem(CapsulasDeleteItemViewModel element) {
      InventarioEntities db = new InventarioEntities();
      string user = User.Identity.GetUserName();

      var capsulaDB = db.Capsulas.SingleOrDefault(p => p.IdCapsula == element.IdCapsula && p.IdUsuario == user);

      if (capsulaDB == null) {
        return new HttpNotFoundResult();
      }

      var item = capsulaDB.CapsulasItems.SingleOrDefault(p => p.IdItem == element.Item.IdItem);

      if (item == null) {
        return new HttpNotFoundResult();
      }

      db.CapsulasItems.Remove(item);

      db.SaveChanges();

      return RedirectToAction("ManageItems", new { id = element.IdCapsula });
    }

    [HttpGet]
    public ActionResult Delete(string id) {
      InventarioEntities db = new InventarioEntities();
      string user = User.Identity.GetUserName();

      var capsula = db.Capsulas.SingleOrDefault(p => p.IdCapsula == id && p.IdUsuario == user);

      if (capsula == null) {
        return new HttpNotFoundResult();
      }

      CapsulasDeleteViewModel model = new CapsulasDeleteViewModel();
      model.IdCapsula = id;
      model.Total = capsula.CapsulasItems.Sum(s => s.Cantidad);

      return View(model);
    }

    [HttpPost]
    public ActionResult Delete(CapsulasDeleteViewModel element) {
      InventarioEntities db = new InventarioEntities();
      string user = User.Identity.GetUserName();

      var capsula = db.Capsulas.SingleOrDefault(p => p.IdUsuario == user && p.IdCapsula == element.IdCapsula);

      if (capsula == null) {
        return new HttpNotFoundResult();
      }

      while (capsula.CapsulasItems.Count > 0) {
        db.CapsulasItems.Remove(capsula.CapsulasItems.First());
      }

      db.Capsulas.Remove(capsula);

      db.SaveChanges();

      return RedirectToAction("Index");
    }

    [HttpGet]
    public ActionResult InterestsDayCapsule(string id) {
      DateTime inicio = DateTime.Now;

      InventarioEntities db = new InventarioEntities();
      string user = User.Identity.GetUserName();

      var reproducciones = db.ReproduccionesItems.Where(p => (id == null || p.Reproducciones.IdCapsula == id) && p.Reproducciones.IdUsuario == user)
          .Select(s => new {
            IdCapsula = s.Reproducciones.IdCapsula,
            Fecha = s.Reproducciones.Fecha,
            IdItem = s.IdItem,
            Cantidad = s.Cantidad,
            IdNivel = s.Items.IdNivel,
            Nombre = s.Items.Nombre ?? s.Items.Tipos.Descripcion,
            Rareza = s.Items.Rarezas.Descripcion
          }).ToList();

      var filas = reproducciones
                  .GroupBy(r => new { r.IdCapsula, r.Fecha })
                  .Select(s => new CapsulaFechaTotalViewModel {
                    IdCapsula = s.Key.IdCapsula,
                    Fecha = s.Key.Fecha,
                    Elementos = s.GroupBy(u => u.IdItem).OrderBy(q => q.Key).Select(v => string.Format("{0} L{1} {2} {3}", v.Sum(w => w.Cantidad), v.First().IdNivel, v.First().Nombre, v.First().Rareza))
                  }).ToList();
      var totales = reproducciones
          .GroupBy(r => r.IdCapsula)
          .Select(s => new CapsulaFechaTotalViewModel {
            IdCapsula = s.Key,
            Fecha = DateTime.MinValue,
            Elementos = s.GroupBy(u => u.IdItem).Select(v => string.Format("{0} L{1} {2} {3}", v.Sum(w => w.Cantidad), v.First().IdNivel, v.First().Nombre, v.First().Rareza))
          }).ToList();

      var model = new CapsulasInterests2ViewModel();
      model.Fechas = reproducciones.Select(p => p.Fecha).Distinct();
      model.Capsulas = reproducciones.Select(p => p.IdCapsula).Distinct().ToList();
      model.Filas = filas;
      model.Totales = totales;

      TimeSpan tiempo = DateTime.Now.Subtract(inicio);

      return View(model);
    }

    [HttpGet]
    public ActionResult LastSpawn() {
      InventarioEntities db = new InventarioEntities();
      string user = User.Identity.GetUserName();

      DateTime fechaMax = db.Reproducciones.Where(p => p.IdUsuario == user).DefaultIfEmpty().Max(q => q == null ? DateTime.MinValue : q.Fecha);

      CapsulasLastSpawnViewModel model = new CapsulasLastSpawnViewModel();
      model.Total = 0;

      if (fechaMax == DateTime.MinValue) {
        return View(model);
      }

      model.Fecha = fechaMax;
      var reproducciones = db.ReproduccionesItems.Where(p => p.Reproducciones.Fecha == fechaMax && p.Reproducciones.IdUsuario == user)
          .OrderBy(q => q.IdItem)
                   .Select(s => new {
                     IdCapsula = s.Reproducciones.IdCapsula,
                     IdItem = s.IdItem,
                     Cantidad = s.Cantidad,
                     IdNivel = s.Items.IdNivel,
                     Nombre = s.Items.Nombre ?? s.Items.Tipos.Descripcion,
                     Rareza = s.Items.Rarezas.Descripcion
                   }).ToList();
      model.Capsulas = reproducciones.GroupBy(r => r.IdCapsula)
                   .Select(s => new CapsulaFechaTotalViewModel {
                     IdCapsula = s.Key,
                     Total = s.Sum(p => p.Cantidad),
                     Elementos = s.GroupBy(u => u.IdItem).Select(v => string.Format("{0} L{1} {2} {3}", v.Sum(w => w.Cantidad), v.First().IdNivel, v.First().Nombre, v.First().Rareza))
                   }).ToList();

      var totales = reproducciones
                      .GroupBy(r => r.IdItem)
                      .Select(s => string.Format("{0} L{1} {2} {3}", s.Sum(w => w.Cantidad), s.First().IdNivel, s.First().Nombre, s.First().Rareza));
      model.Total = model.Capsulas.Sum(p => p.Total);
      model.Totales = totales;

      return View(model);
    }

    [HttpGet]
    public ActionResult InterestsTotalCapsule(string id) {
      InventarioEntities db = new InventarioEntities();
      string user = User.Identity.GetUserName();

      var reproducciones = db.ReproduccionesItems.Where(p => (id == null || p.Reproducciones.IdCapsula == id) && p.Reproducciones.IdUsuario == user)
                  .OrderBy(q => q.IdItem)
                   .Select(s => new {
                     IdCapsula = s.Reproducciones.IdCapsula,
                     IdItem = s.IdItem,
                     Cantidad = s.Cantidad,
                     IdNivel = s.Items.IdNivel,
                     Nombre = s.Items.Nombre ?? s.Items.Tipos.Descripcion,
                     Rareza = s.Items.Rarezas.Descripcion
                   }).ToList();
      var model = reproducciones.GroupBy(r => r.IdCapsula)
                   .Select(s => new CapsulaFechaTotalViewModel {
                     IdCapsula = s.Key,
                     Total = s.Sum(p => p.Cantidad),
                     Elementos = s.GroupBy(u => u.IdItem).Select(v => string.Format("{0} L{1} {2} {3}", v.Sum(w => w.Cantidad), v.First().IdNivel, v.First().Nombre, v.First().Rareza))
                   }).ToList();

      return View(model);
    }

    [HttpGet]
    public ActionResult InterestsTotalByItem() {
      InventarioEntities db = new InventarioEntities();
      string user = User.Identity.GetUserName();


      var totales = db.Reproducciones.Where(p => p.IdUsuario == user)
          .SelectMany(p => p.ReproduccionesItems)
          .GroupBy(p => p.IdItem)
          .Select(q => new ItemViewModel {
            IdItem = q.Key,
            IdGrupo = q.FirstOrDefault().Items.Tipos.IdGrupo,
            IdTipo = q.FirstOrDefault().Items.IdTipo,
            Descripcion = q.FirstOrDefault().Items.Nombre ?? q.FirstOrDefault().Items.Tipos.Descripcion,
            MostrarNivel = q.FirstOrDefault().Items.Tipos.MostrarNivel,
            Nivel = q.FirstOrDefault().Items.IdNivel,
            MostrarRareza = q.FirstOrDefault().Items.Tipos.MostrarRareza,
            IdRareza = q.FirstOrDefault().Items.IdRareza,
            Rareza = q.FirstOrDefault().Items.Rarezas.Descripcion,
            Cantidad = q.Sum(r => r.Cantidad)
          }).ToList();

      return View(totales);
    }
  }
}