using MiInventario.Models.Capsules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Globalization;
using MiInventario.Code;
using System.Threading.Tasks;
using System.Web.UI.DataVisualization.Charting;
using System.IO;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Data.Entity;
using System.Drawing.Text;

namespace MiInventario.Controllers {
  [Authorize]
  public class CapsulesController : BaseController {


    [HttpGet]
    public ActionResult Index() {
      using (InventarioEntities db = new InventarioEntities()) {
        string user = User.Identity.GetUserName();
        var capsulas = db.Capsulas.Where(p => p.IdUsuario == user).ToList();

        List<CapsulesViewModel> model = new List<CapsulesViewModel>();
        foreach (var capsula in capsulas) {
          int cantidad = capsula.CapsulasItems.Sum(s => s.Cantidad);

          var capsulaV = new CapsulesViewModel();
          capsulaV.IdCapsula = capsula.IdCapsula;
          capsulaV.Total = cantidad;
          capsulaV.Spawnable = ItemsXml.Any(p => p.ItemID == capsula.ItemID && p.PaysInterests);
          capsulaV.Descripcion = capsula.Descripcion;

          if (capsulaV.Descripcion == null) {
            if (!capsula.CapsulasItems.Any()) {
              capsulaV.Descripcion = Resources.General.ResourceManager.GetString("Capsule_Empty");
            }
            else if (capsula.CapsulasItems.Count() == 1) {
              var itemIn = ItemsXml.Single(p => p.ItemID == capsula.CapsulasItems.Single().ItemID);
              capsulaV.ItemEncapsulado = new Models.ItemViewModel { CurrentItem = itemIn };
            }
            else if (capsula.CapsulasItems.Count() > 1) {
              capsulaV.Descripcion = string.Format("({0} items)", capsula.CapsulasItems.Count());
            }
          }

          model.Add(capsulaV);
        }

        return View(model);
      }
    }

    [HttpGet]
    public ActionResult SendToUser(string id, string sendToUser) {
      using (InventarioEntities db = new InventarioEntities()) {
        string user = User.Identity.GetUserName();
        var capsula = db.Capsulas.SingleOrDefault(p => p.IdUsuario == user && p.IdCapsula == id);

        if (capsula == null) {
          return new HttpNotFoundResult();
        }

        SendToViewModel model = new SendToViewModel();
        model.IdCapsula = capsula.IdCapsula;
        model.UserID = sendToUser;
        model.Descripcion = capsula.Descripcion;

        if (model.Descripcion == null) {
          if (!capsula.CapsulasItems.Any()) {
            model.Descripcion = Resources.General.ResourceManager.GetString("Capsule_Empty");
          }
          else if (capsula.CapsulasItems.Count() == 1) {
            var itemIn = ItemsXml.Single(p => p.ItemID == capsula.CapsulasItems.Single().ItemID);
            model.ItemEncapsulado = new Models.ItemViewModel { CurrentItem = itemIn };
          }
          else if (capsula.CapsulasItems.Count() > 1) {
            model.Descripcion = string.Format("({0} items)", capsula.CapsulasItems.Count());
          }
        }

        return View(model);
      }
    }

    [HttpPost]
    public ActionResult SendToUser(SendToViewModel capsula) {
      using (InventarioEntities db = new InventarioEntities()) {
        string user = User.Identity.GetUserName();
        var capsulaDB = db.Capsulas.SingleOrDefault(p => p.IdUsuario == user && p.IdCapsula == capsula.IdCapsula);

        if (capsulaDB == null) {
          return new HttpNotFoundResult();
        }

        capsulaDB.IdUsuario = capsula.UserID;

        db.SaveChanges();

        return RedirectToAction("Index");
      }
    }

    [HttpGet]
    public ActionResult Add() {

      using (InventarioEntities db = new InventarioEntities()) {
        var model = new CreateViewModel();
        model.Capsulas = ItemsXml.Where(p => p.IsCapsule)
            .Select(s => new {
              ItemID = s.ItemID,
              Nombre = s.Nombre
            }).ToDictionary(x => x.ItemID, y => y.Nombre);

        return View(model);
      }
    }
    public ActionResult Add(CreateViewModel capsula) {
      using (InventarioEntities db = new InventarioEntities()) {

        if (db.Capsulas.Any(p => p.IdCapsula == capsula.IdCapsula)) {
          ModelState.AddModelError("Duplicate", "There is another capsule with the same ID");
        }

        if (ModelState.IsValid) {
          string user = User.Identity.GetUserName();

          db.Capsulas.Add(new Capsulas { IdUsuario = user, IdCapsula = capsula.IdCapsula.ToUpper(), Descripcion = capsula.Descripcion, ItemID = capsula.ItemID });

          db.SaveChanges();

          return RedirectToAction("Index");
        }

        capsula.Capsulas = ItemsXml.Where(p => p.IsCapsule)
            .Select(s => new {
              ItemID = s.ItemID,
              Nombre = s.Nombre
            }).ToDictionary(x => x.ItemID, y => y.Nombre);


        return View(capsula);
      }
    }

    [HttpGet]
    public ActionResult Edit(string id) {
      using (InventarioEntities db = new InventarioEntities()) {
        string user = User.Identity.GetUserName();
        var capsulaDB = db.Capsulas.SingleOrDefault(p => p.IdCapsula == id && p.IdUsuario == user);

        if (capsulaDB == null) {
          return new HttpNotFoundResult();
        }

        var model = new EditViewModel();
        model.IdCapsula = capsulaDB.IdCapsula;
        model.Descripcion = capsulaDB.Descripcion;
        model.ItemID = capsulaDB.ItemID;
        model.Capsulas = ItemsXml.Where(p => p.IsCapsule)
            .Select(s => new {
              ItemID = s.ItemID,
              Nombre = s.Nombre
            }).ToDictionary(x => x.ItemID, y => y.Nombre);

        return View(model);
      }
    }
    public ActionResult Edit(EditViewModel capsula) {
      using (InventarioEntities db = new InventarioEntities()) {
        string user = User.Identity.GetUserName();
        var capsulaDB = db.Capsulas.SingleOrDefault(p => p.IdCapsula == capsula.IdCapsula && p.IdUsuario == user);

        if (capsulaDB == null) {
          return new HttpNotFoundResult();
        }

        if (ModelState.IsValid) {
          capsulaDB.Descripcion = capsula.Descripcion;
          capsulaDB.ItemID = capsula.ItemID;

          db.SaveChanges();

          return RedirectToAction("Index");
        }

        capsula.Capsulas = ItemsXml.Where(p => p.IsCapsule)
            .Select(s => new {
              ItemID = s.ItemID,
              Nombre = s.Nombre
            }).ToDictionary(x => x.ItemID, y => y.Nombre);

        return View(capsula);
      }
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
    public ActionResult Unload(string id) {
      return RecuperarItemsUnload(id);
    }
    [HttpGet]
    public ActionResult Load(string id) {
      return RecuperarItemsLoad(id);
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

      using (InventarioEntities db = new InventarioEntities()) {
        string user = User.Identity.GetUserName();
        var capsula = db.Capsulas.SingleOrDefault(p => p.IdCapsula == id && p.IdUsuario == user);

        if (capsula == null) {
          return new HttpNotFoundResult();
        }

        var itemsCargados = capsula.CapsulasItems.Select(p => p.ItemID).ToList();

        AddItemViewModel model = new AddItemViewModel();
        model.IdCapsula = id;
        model.Total = capsula.CapsulasItems.Sum(s => s.Cantidad);
        model.AddeableItems = ItemsXml.Where(p => !p.IsCapsule && !itemsCargados.Contains(p.ItemID))
            .Select(q => new Models.ItemViewModel {
              CurrentItem = q,
            }).ToList();

        return View(model);
      }
    }

    [HttpPost]
    public ActionResult AddItem(AddItemViewModel addedItem) {
      using (InventarioEntities db = new InventarioEntities()) {
        string user = User.Identity.GetUserName();
        var capsula = db.Capsulas.SingleOrDefault(p => p.IdCapsula == addedItem.IdCapsula && p.IdUsuario == user);

        if (capsula == null) {
          return new HttpNotFoundResult();
        }

        if (ModelState.IsValid) {
          db.CapsulasItems.Add(new CapsulasItems { IdCapsula = addedItem.IdCapsula, ItemID = addedItem.ItemID, Cantidad = addedItem.Cantidad });

          db.SaveChanges();

          return RedirectToAction("List", new { id = addedItem.IdCapsula });
        }
        else {
          var itemsCargados = capsula.CapsulasItems.Select(p => p.ItemID).ToList();

          addedItem.Total = capsula.CapsulasItems.Sum(s => s.Cantidad);
          addedItem.AddeableItems = ItemsXml.Where(p => !p.IsCapsule && !itemsCargados.Contains(p.ItemID))
              .Select(q => new Models.ItemViewModel {
                CurrentItem = q,
              }).ToList();

          return View(addedItem);
        }
      }
    }

    private ActionResult RecuperarItemsUnload(string id) {
      if (id == null) {
        return new HttpNotFoundResult();
      }

      using (InventarioEntities db = new InventarioEntities()) {
        string user = User.Identity.GetUserName();
        var capsula = db.Capsulas.SingleOrDefault(p => p.IdCapsula == id && p.IdUsuario == user);

        if (capsula == null) {
          return new HttpNotFoundResult();
        }

        UnloadViewModel model = new UnloadViewModel();
        model.IdCapsula = id;
        model.Descripcion = capsula.Descripcion;
        model.Total = capsula.CapsulasItems.Sum(s => s.Cantidad);
        model.Items = capsula.CapsulasItems.Select(p => new ItemUnloadViewModel {
          CurrentItem = ItemsXml.Single(z => z.ItemID == p.ItemID),
          Cantidad = p.Cantidad,
          CantidadDescargar = 0
        }).OrderBy(n => n.CurrentItem.Order).ToList();

        return View(model);
      }
    }
    private ActionResult RecuperarItemsLoad(string id) {
      if (id == null) {
        return new HttpNotFoundResult();
      }

      using (InventarioEntities db = new InventarioEntities()) {
        string user = User.Identity.GetUserName();
        var capsula = db.Capsulas.SingleOrDefault(p => p.IdCapsula == id && p.IdUsuario == user);

        if (capsula == null) {
          return new HttpNotFoundResult();
        }

        LoadViewModel model = new LoadViewModel();
        model.IdCapsula = id;
        model.Descripcion = capsula.Descripcion;
        model.Total = capsula.CapsulasItems.Sum(s => s.Cantidad);

        var enCapsula = capsula.CapsulasItems.Select(q => new ItemLoadViewModel {
          CurrentItem = ItemsXml.Single(z => z.ItemID == q.ItemID),
          CantidadEnCapsula = q.Cantidad,
          CantidadCargar = 0
        }).ToList();

        var esCapsula = ItemsXml.Where(p => p.IsCapsule).Select(s => s.ItemID);

        var inventariosDB = db.Inventarios.Where(p => p.IdUsuario == user && !esCapsula.Contains(p.ItemID))
            .Select(q => new {
              ItemID = q.ItemID,
              CantidadSuelta = q.Cantidad,
              CantidadCargar = 0
            }).ToList();

        var inventarios = inventariosDB.Select(p => new ItemLoadViewModel {
          CurrentItem = ItemsXml.FirstOrDefault(z => z.ItemID == p.ItemID),
          CantidadSuelta = p.CantidadSuelta,
          CantidadCargar = p.CantidadCargar
        }).ToList();

        foreach (var item in enCapsula) {
          var inv = inventarios.SingleOrDefault(p => p.CurrentItem.ItemID == item.CurrentItem.ItemID);
          if (inv == null) {
            inventarios.Add(item);
          }
          else {
            inv.CantidadEnCapsula = item.CantidadEnCapsula;
          }
        }

        model.Items = inventarios.OrderBy(p => p.CurrentItem.Order).ToList();

        return View(model);
      }
    }
    private ActionResult RecuperarItems(string id, bool? spawnable) {
      if (id == null) {
        return new HttpNotFoundResult();
      }

      using (InventarioEntities db = new InventarioEntities()) {
        string user = User.Identity.GetUserName();

        var capsulasInterests = ItemsXml.Where(p => p.PaysInterests).Select(s => s.ItemID);

        var capsula = db.Capsulas.SingleOrDefault(p => p.IdCapsula == id && p.IdUsuario == user && (!spawnable.HasValue || capsulasInterests.Contains(p.ItemID)));

        if (capsula == null) {
          return new HttpNotFoundResult();
        }

        ManageViewModel model = new ManageViewModel();
        model.IdCapsula = id;
        model.Descripcion = capsula.Descripcion;
        model.Total = capsula.CapsulasItems.Sum(s => s.Cantidad);
        model.Items = capsula.CapsulasItems.Select(p => new Models.ItemInventoryViewModel {
          CurrentItem = ItemsXml.Single(z => z.ItemID == p.ItemID),
          Cantidad = p.Cantidad
        }).OrderBy(x => x.CurrentItem.Order).ToList();

        return View(model);
      }
    }

    [HttpPost]
    public ActionResult ManageItems(ManageViewModel capsula) {
      using (InventarioEntities db = new InventarioEntities()) {
        string user = User.Identity.GetUserName();
        var capsulaDB = db.Capsulas.SingleOrDefault(p => p.IdCapsula == capsula.IdCapsula && p.IdUsuario == user);

        if (capsulaDB == null) {
          return new HttpNotFoundResult();
        }

        if (capsula.Items != null) {
          foreach (Models.ItemInventoryViewModel item in capsula.Items) {
            string itemID = item.CurrentItem.ItemID;
            CapsulasItems capsulaItem = db.CapsulasItems.SingleOrDefault(p => p.IdCapsula == capsula.IdCapsula && p.ItemID == itemID);

            if (capsulaItem != null) {
              capsulaItem.Cantidad = item.Cantidad;
            }
          }

          db.SaveChanges();
        }

        return RedirectToAction("List", new { id = capsula.IdCapsula });
      }
    }

    [HttpPost]
    public ActionResult Unload(UnloadViewModel capsula) {
      if (!ModelState.IsValid) {
        return RecuperarItemsUnload(capsula.IdCapsula);
      }

      using (InventarioEntities db = new InventarioEntities()) {
        string user = User.Identity.GetUserName();
        var capsulaDB = db.Capsulas.SingleOrDefault(p => p.IdCapsula == capsula.IdCapsula && p.IdUsuario == user);

        if (capsulaDB == null) {
          return new HttpNotFoundResult();
        }

        if (capsula.Items != null) {
          foreach (var item in capsula.Items.Where(p => p.CantidadDescargar > 0)) {
            CapsulasItems capsulaItem = db.CapsulasItems.SingleOrDefault(p => p.IdCapsula == capsula.IdCapsula && p.ItemID == item.CurrentItem.ItemID);

            if (capsulaItem != null) {
              if (capsulaItem.Cantidad == item.CantidadDescargar) {
                db.CapsulasItems.Remove(capsulaItem);
              }
              else {
                capsulaItem.Cantidad -= item.CantidadDescargar;
              }

              Inventarios inv = db.Inventarios.SingleOrDefault(p => p.IdUsuario == user && p.ItemID == item.CurrentItem.ItemID);

              if (inv != null) {
                inv.Cantidad += item.CantidadDescargar;
              }
              else {
                db.Inventarios.Add(new Inventarios { IdUsuario = user, ItemID = item.CurrentItem.ItemID, Cantidad = item.CantidadDescargar });
              }
            }
          }

          db.SaveChanges();
        }

        return RedirectToAction("List", new { id = capsula.IdCapsula });
      }
    }

    [HttpPost]
    public ActionResult Load(LoadViewModel capsula) {
      if (!ModelState.IsValid) {
        return View(capsula);
      }

      using (InventarioEntities db = new InventarioEntities()) {
        string user = User.Identity.GetUserName();
        var capsulaDB = db.Capsulas.SingleOrDefault(p => p.IdCapsula == capsula.IdCapsula && p.IdUsuario == user);

        if (capsulaDB == null) {
          return new HttpNotFoundResult();
        }

        if (capsula.Items != null) {

          if (capsula.Items.Any(p => p.CantidadCargar > p.CantidadSuelta)) {
            return View(capsula);
          }

          foreach (var item in capsula.Items.Where(p => p.CantidadCargar > 0)) {
            CapsulasItems capsulaItem = db.CapsulasItems.SingleOrDefault(p => p.IdCapsula == capsula.IdCapsula && p.ItemID == item.CurrentItem.ItemID);

            if (capsulaItem != null) {
              capsulaItem.Cantidad += item.CantidadCargar;
            }
            else {
              db.CapsulasItems.Add(new CapsulasItems { IdCapsula = capsula.IdCapsula, ItemID = item.CurrentItem.ItemID, Cantidad = item.CantidadCargar });
            }

            Inventarios inv = db.Inventarios.SingleOrDefault(p => p.IdUsuario == user && p.ItemID == item.CurrentItem.ItemID);

            if (inv != null) {
              if (inv.Cantidad == item.CantidadCargar) {
                db.Inventarios.Remove(inv);
              }
              else {
                inv.Cantidad -= item.CantidadCargar;
              }
            }
          }

          db.SaveChanges();
        }

        return RedirectToAction("List", new { id = capsula.IdCapsula });
      }
    }

    [HttpPost]
    public ActionResult LogInterests(ManageViewModel capsula) {
      using (InventarioEntities db = new InventarioEntities()) {
        string user = User.Identity.GetUserName();

        var capsulaDB = db.Capsulas.SingleOrDefault(p => p.IdCapsula == capsula.IdCapsula && p.IdUsuario == user);

        if (capsulaDB == null) {
          return new HttpNotFoundResult();
        }

        if (capsula.Items == null) {
          return RedirectToAction("List", new { id = capsula.IdCapsula });
        }

        if (capsula.Items.Sum(p => p.Cantidad) > 100) {
          ModelState.AddModelError("Masde100", "Total Quantity on capsule exceeds 100.");
          return LogInterests(capsula.IdCapsula);
        }

        Dictionary<string, int> v_nuevos = new Dictionary<string, int>();

        foreach (Models.ItemInventoryViewModel item in capsula.Items) {
          CapsulasItems capsulaItem = db.CapsulasItems.SingleOrDefault(p => p.IdCapsula == capsula.IdCapsula && p.ItemID == item.CurrentItem.ItemID);

          if (capsulaItem != null) {
            if (capsulaItem.Cantidad > item.Cantidad) {
              ModelState.AddModelError("Masde100", "Quantity must be equal or greater than quantity on capsule.");
              return LogInterests(capsula.IdCapsula);
            }
            else if (capsulaItem.Cantidad < item.Cantidad) {
              v_nuevos.Add(item.CurrentItem.ItemID, item.Cantidad - capsulaItem.Cantidad);
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
            v_repro.ReproduccionesItems.Add(new ReproduccionesItems { ItemID = kv.Key, Cantidad = kv.Value });
          }
          db.Reproducciones.Add(v_repro);

          db.SaveChanges();

          return RedirectToAction("List", new { id = capsula.IdCapsula });
        }

        return RedirectToAction("LogInterests", new { id = capsula.IdCapsula });
      }
    }

    [HttpGet]
    public ActionResult DeleteItem(string id, string itemID) {
      if (id == null || string.IsNullOrWhiteSpace(itemID)) {
        return new HttpNotFoundResult();
      }

      using (InventarioEntities db = new InventarioEntities()) {
        string user = User.Identity.GetUserName();

        var capsulaDB = db.Capsulas.SingleOrDefault(p => p.IdCapsula == id && p.IdUsuario == user);

        if (capsulaDB == null) {
          return new HttpNotFoundResult();
        }

        var item = capsulaDB.CapsulasItems.SingleOrDefault(p => p.ItemID == itemID);

        if (item == null) {
          return new HttpNotFoundResult();
        }

        Models.ItemViewModel summary = new Models.ItemViewModel {
          CurrentItem = ItemsXml.Single(z => z.ItemID == item.ItemID),
        };

        return View(new DeleteItemViewModel { IdCapsula = id, Total = capsulaDB.CapsulasItems.Sum(s => s.Cantidad), Item = summary, Cantidad = item.Cantidad });
      }
    }

    [HttpPost]
    public ActionResult DeleteItem(DeleteItemViewModel element) {
      using (InventarioEntities db = new InventarioEntities()) {
        string user = User.Identity.GetUserName();

        var capsulaDB = db.Capsulas.SingleOrDefault(p => p.IdCapsula == element.IdCapsula && p.IdUsuario == user);

        if (capsulaDB == null) {
          return new HttpNotFoundResult();
        }

        var item = capsulaDB.CapsulasItems.SingleOrDefault(p => p.ItemID == element.Item.CurrentItem.ItemID);

        if (item == null) {
          return new HttpNotFoundResult();
        }

        db.CapsulasItems.Remove(item);

        db.SaveChanges();

        return RedirectToAction("ManageItems", new { id = element.IdCapsula });
      }
    }

    [HttpGet]
    public ActionResult Delete(string id) {
      using (InventarioEntities db = new InventarioEntities()) {
        string user = User.Identity.GetUserName();

        var capsula = db.Capsulas.SingleOrDefault(p => p.IdCapsula == id && p.IdUsuario == user);

        if (capsula == null) {
          return new HttpNotFoundResult();
        }

        DeleteViewModel model = new DeleteViewModel();
        model.IdCapsula = id;
        model.Total = capsula.CapsulasItems.Sum(s => s.Cantidad);

        return View(model);
      }
    }

    [HttpPost]
    public ActionResult Delete(DeleteViewModel element) {
      using (InventarioEntities db = new InventarioEntities()) {
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
    }


  }
}