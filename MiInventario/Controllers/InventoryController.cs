using MiInventario.Models.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Net;
using MiInventario.Code;


namespace MiInventario.Controllers {
  [Authorize]
  public class InventoryController : BaseController {

    private const string GroupIDAll = "G_ALL";

    [HttpGet]
    public ActionResult Index() {
      DateTime inicio = DateTime.Now;

      string user = User.Identity.GetUserName();
      using (InventarioEntities db = new InventarioEntities()) {

        var enCapsulas = db.CapsulasItems
            .Where(s => s.Capsulas.IdUsuario == user)
            .GroupBy(t => t.ItemID)
            .Select(u => new { ItemID = u.Key, CantidadCapsula = u.Sum(v => v.Cantidad) }).ToList();

        var inventario = db.Inventarios
            .Where(s => s.IdUsuario == user)
            .Select(u => new { ItemID = u.ItemID, Cantidad = u.Cantidad }).ToList();

        var model = GroupsXml.Select(p => new GroupViewModel {
          GroupID = p.GroupID,
          Total = inventario.Where(s => p.Types.SelectMany(t => t.Items).Any(u => u.ItemID == s.ItemID)).DefaultIfEmpty().Sum(v => v == null ? 0 : v.Cantidad),
          TotalCapsulas = enCapsulas.Where(s => p.Types.SelectMany(t => t.Items).Any(u => u.ItemID == s.ItemID)).DefaultIfEmpty().Sum(v => v == null ? 0 : v.CantidadCapsula)
        }).ToList();

        double elapsed = (DateTime.Now - inicio).TotalMilliseconds;

        return View(model);
      }
    }

    public ActionResult Difference() {
      using (InventarioEntities db = new InventarioEntities()) {
        string user = User.Identity.GetUserName();
        if (user != "diegomenta@gmail.com" && user != "pceriani@gmail.com") {
          return new HttpNotFoundResult();
        }

        DifferenceViewModel model = new DifferenceViewModel();
        string usuarioA = user;
        string usuarioB = (user == "diegomenta@gmail.com" ? "pceriani@gmail.com" : "diegomenta@gmail.com");

        model.UsuarioA = usuarioA;
        model.UsuarioB = usuarioB;

        var inventarioA = db.Inventarios
                          .Where(s => s.IdUsuario == usuarioA)
                          .Select(u => new ItemBasico { ItemID = u.ItemID, Cantidad = u.Cantidad }).ToList();
        var inventarioB = db.Inventarios
                          .Where(s => s.IdUsuario == usuarioB)
                          .Select(u => new ItemBasico { ItemID = u.ItemID, Cantidad = u.Cantidad }).ToList();

        var capsulasA = db.CapsulasItems
                          .Where(s => s.Capsulas.IdUsuario == usuarioA)
                          .GroupBy(t => t.ItemID)
                          .Select(u => new ItemBasico { ItemID = u.Key, Cantidad = u.Sum(v => v.Cantidad) });
        var capsulasB = db.CapsulasItems
                          .Where(s => s.Capsulas.IdUsuario == usuarioB)
                          .GroupBy(t => t.ItemID)
                          .Select(u => new ItemBasico { ItemID = u.Key, Cantidad = u.Sum(v => v.Cantidad) });

        foreach (var itemC in capsulasA) {
          var itemI = inventarioA.SingleOrDefault(f => f.ItemID == itemC.ItemID);
          if (itemI == null) {
            inventarioA.Add(new ItemBasico { ItemID = itemC.ItemID, Cantidad = itemC.Cantidad });
          }
          else {
            itemI.Cantidad += itemC.Cantidad;
          }
        }

        foreach (var itemC in capsulasB) {
          var itemI = inventarioB.SingleOrDefault(f => f.ItemID == itemC.ItemID);
          if (itemI == null) {
            inventarioB.Add(new ItemBasico { ItemID = itemC.ItemID, Cantidad = itemC.Cantidad });
          }
          else {
            itemI.Cantidad += itemC.Cantidad;
          }
        }
        model.Groups = GroupsXml
            .Select(p => new GroupDifferenceViewModel {
              GroupID = p.GroupID,
              Types = p.Types.Select(q => new TypeDifferenceViewModel {
                TypeID = q.TypeID,
                Items = q.Items.Select(r => new ItemDifferenceViewModel {
                  CurrentItem = r,
                  CantidadUsuarioA = inventarioA.SingleOrDefault(s => s.ItemID == r.ItemID) == null ? 0 : inventarioA.Single(s => s.ItemID == r.ItemID).Cantidad,
                  CantidadUsuarioB = inventarioB.SingleOrDefault(s => s.ItemID == r.ItemID) == null ? 0 : inventarioB.Single(s => s.ItemID == r.ItemID).Cantidad
                })
              })
            }).ToList();

        return View(model);
      }
    }

    [HttpGet]
    public ActionResult Manage(string id, bool? editing) {
      Dictionary<string, string> groups = GroupsXml.Select(p => new { GroupID = p.GroupID, Name = Resources.Groups.ResourceManager.GetString(p.GroupID) }).ToDictionary(p => p.GroupID, q => q.Name);
      groups.Add(GroupIDAll, Resources.Groups.ResourceManager.GetString(GroupIDAll));

      var model = new ManageViewModel { Groups = groups };
      if (!string.IsNullOrEmpty(id)) {
        if (groups.ContainsKey(id)) {
          model.GroupID = id;
          model.Editing = editing.GetValueOrDefault();
        }
        else {
          return new HttpStatusCodeResult(System.Net.HttpStatusCode.NotFound, "GroupID does not exists");
        }
      }
      else {
        model.GroupID = GroupsXml.First().GroupID;
        model.Editing = false;
      }

      return View(model);
    }

    [HttpPost]
    public ActionResult Items(string groupID) {
      return GetItems(groupID, false);
    }

    [HttpPost]
    public ActionResult ItemsEdit(string groupID) {
      return GetItems(groupID, true);
    }

    private ActionResult GetItems(string groupID, bool ignoreZeroQuantity) {
      bool allGroups = groupID == GroupIDAll;

      if (!allGroups && !GroupsXml.Any(p => p.GroupID == groupID)) {
        return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest, "GroupID does not exists");
      }

      DateTime inicio = DateTime.Now;
      using (InventarioEntities db = new InventarioEntities()) {
        string user = User.Identity.GetUserName();

        var enCapsulas = db.CapsulasItems
                    .Where(s => s.Capsulas.IdUsuario == user)
                    .GroupBy(t => t.ItemID)
                    .Select(u => new { ItemID = u.Key, CantidadCapsulas = u.Sum(v => v.Cantidad) }).ToList();

        var inventario = db.Inventarios
            .Where(s => s.IdUsuario == user)
            .Select(u => new { ItemID = u.ItemID, Cantidad = u.Cantidad }).ToList();
        double elapsed = (DateTime.Now - inicio).TotalMilliseconds;

        var itemsGroups = ItemsXml
          .Where(z => (allGroups || z.GroupID == groupID) && (ignoreZeroQuantity || (inventario.Any(y => y.ItemID == z.ItemID) || enCapsulas.Any(y => y.ItemID == z.ItemID))))
          .Select(y => new {
            y.GroupID,
            y.TypeID,
            y.ItemID,
            y.Order,
            y.Level,
            y.Rarity,
            Cantidad = inventario.SingleOrDefault(s => s.ItemID == y.ItemID) == null ? 0 : inventario.Single(s => s.ItemID == y.ItemID).Cantidad,
            CantidadCapsulas = enCapsulas.SingleOrDefault(s => s.ItemID == y.ItemID) == null ? 0 : enCapsulas.Single(s => s.ItemID == y.ItemID).CantidadCapsulas
          })
          .ToList();

        if (itemsGroups.Count == 0) {
          return Json(new { Result = false }, JsonRequestBehavior.DenyGet);
        }

        var groups = itemsGroups
          .GroupBy(d => d.GroupID)
          .Select(p => new {
            GroupID = p.Key,
            Types = p.GroupBy(e => e.TypeID)
            .Select(q => new {
              TypeID = q.Key,
              Cantidad = q.Sum(f => f.Cantidad),
              CantidadCapsulas = q.Sum(f => f.CantidadCapsulas),
              Items = q.Select(r => new {
                CurrentItem = new Item {
                  ItemID = r.ItemID,
                  TypeID = r.TypeID,
                  Order = r.Order,
                  Level = r.Level,
                  Rarity = r.Rarity,
                },
                Cantidad = r.Cantidad,
                CantidadCapsulas = r.CantidadCapsulas
              })
            })
          });

        return Json(new { Result = true, Groups = groups }, JsonRequestBehavior.DenyGet);
      }
    }

    [HttpPost]
    public ActionResult SaveNewQty(IList<ItemsQty> items) {
      using (InventarioEntities db = new InventarioEntities()) {
        string user = User.Identity.GetUserName();

        if (items == null) {
          return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "There are no items.");
        }

        if (items.Any(p => p.Qty > 2000 || p.Qty < 0)) {
          return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "The item quantity must be a value between 0 and 2000");
        }

        foreach (ItemsQty item in items) {
          string itemID = item.ItemID;
          Inventarios inv = db.Inventarios.SingleOrDefault(p => p.IdUsuario == user && p.ItemID == itemID);

          if (inv != null) {
            if (item.Qty == 0) {
              db.Inventarios.Remove(inv);
            }
            else {
              inv.Cantidad = item.Qty;
            }
          }
          else {
            if (item.Qty > 0) {
              db.Inventarios.Add(new Inventarios { IdUsuario = user, ItemID = itemID, Cantidad = item.Qty });
            }
          }
        }

        db.SaveChanges();

        return Json("Successfully saved!");
      }
    }
  }
}