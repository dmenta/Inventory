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
      return View();
    }

    [HttpGet]
    public JsonResult GroupsTotals() {
      DateTime inicio = DateTime.Now;

      var enCapsulas = Database.CapsulesItems
          .Where(s => s.Capsules.UserId == Username)
          .GroupBy(t => t.ItemID)
          .Select(u => new { ItemID = u.Key, CantidadCapsula = u.Sum(v => v.Quantity) }).ToList();

      var inventario = Database.Inventarios
          .Where(s => s.IdUsuario == Username)
          .Select(u => new { ItemID = u.ItemID, Cantidad = u.Cantidad }).ToList();

      var model = GroupsXml.Select(p => new GroupViewModel {
        GroupID = p.GroupID,
        GroupName = Resources.Groups.ResourceManager.GetString(p.GroupID),
        Total = inventario.Where(s => p.Types.SelectMany(t => t.Items).Any(u => u.ItemID == s.ItemID)).DefaultIfEmpty().Sum(v => v == null ? 0 : v.Cantidad),
        CapsulesTotal = enCapsulas.Where(s => p.Types.SelectMany(t => t.Items).Any(u => u.ItemID == s.ItemID)).DefaultIfEmpty().Sum(v => v == null ? 0 : v.CantidadCapsula)
      }).ToList();

      model.Add(new GroupViewModel {
        GroupID = GroupIDAll,
        GroupName = Resources.Groups.ResourceManager.GetString(GroupIDAll),
        Total = model.Sum(p => p.Total),
        CapsulesTotal = model.Sum(p => p.CapsulesTotal)
      });

      double elapsed = (DateTime.Now - inicio).TotalMilliseconds;

      return Json(model, JsonRequestBehavior.AllowGet);
    }

    public ActionResult Difference() {
      if (Username != "diegomenta@gmail.com" && Username != "pceriani@gmail.com") {
        return new HttpNotFoundResult();
      }

      DifferenceViewModel model = new DifferenceViewModel();
      string usuarioA = Username;
      string usuarioB = (Username == "diegomenta@gmail.com" ? "pceriani@gmail.com" : "diegomenta@gmail.com");

      model.UsuarioA = usuarioA;
      model.UsuarioB = usuarioB;

      var inventarioA = Database.Inventarios
                        .Where(s => s.IdUsuario == usuarioA)
                        .Select(u => new ItemQuantity { ItemID = u.ItemID, Quantity = u.Cantidad }).ToList();
      var inventarioB = Database.Inventarios
                        .Where(s => s.IdUsuario == usuarioB)
                        .Select(u => new ItemQuantity { ItemID = u.ItemID, Quantity = u.Cantidad }).ToList();

      var capsulasA = Database.CapsulesItems
                        .Where(s => s.Capsules.UserId == usuarioA)
                        .GroupBy(t => t.ItemID)
                        .Select(u => new ItemQuantity { ItemID = u.Key, Quantity = u.Sum(v => v.Quantity) });
      var capsulasB = Database.CapsulesItems
                        .Where(s => s.Capsules.UserId == usuarioB)
                        .GroupBy(t => t.ItemID)
                        .Select(u => new ItemQuantity { ItemID = u.Key, Quantity = u.Sum(v => v.Quantity) });

      foreach (var itemC in capsulasA) {
        var itemI = inventarioA.SingleOrDefault(f => f.ItemID == itemC.ItemID);
        if (itemI == null) {
          inventarioA.Add(new ItemQuantity { ItemID = itemC.ItemID, Quantity = itemC.Quantity });
        }
        else {
          itemI.Quantity += itemC.Quantity;
        }
      }

      foreach (var itemC in capsulasB) {
        var itemI = inventarioB.SingleOrDefault(f => f.ItemID == itemC.ItemID);
        if (itemI == null) {
          inventarioB.Add(new ItemQuantity { ItemID = itemC.ItemID, Quantity = itemC.Quantity });
        }
        else {
          itemI.Quantity += itemC.Quantity;
        }
      }
      model.Groups = GroupsXml
          .Select(p => new GroupDifferenceViewModel {
            GroupID = p.GroupID,
            Types = p.Types.Select(q => new TypeDifferenceViewModel {
              TypeID = q.TypeID,
              Items = q.Items.Select(r => new ItemDifferenceViewModel {
                CurrentItem = r,
                CantidadUsuarioA = inventarioA.SingleOrDefault(s => s.ItemID == r.ItemID) == null ? 0 : inventarioA.Single(s => s.ItemID == r.ItemID).Quantity,
                CantidadUsuarioB = inventarioB.SingleOrDefault(s => s.ItemID == r.ItemID) == null ? 0 : inventarioB.Single(s => s.ItemID == r.ItemID).Quantity
              })
            })
          }).ToList();

      return View(model);

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

      var enCapsulas = Database.CapsulesItems
                  .Where(s => s.Capsules.UserId == Username)
                  .GroupBy(t => t.ItemID)
                  .Select(u => new { ItemID = u.Key, CantidadCapsulas = u.Sum(v => v.Quantity) }).ToList();

      var inventario = Database.Inventarios
          .Where(s => s.IdUsuario == Username)
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
          y.UniqueID,
          y.IsKeyLocker,
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
                UniqueID = r.UniqueID,
                IsKeyLocker = r.IsKeyLocker,
              },
              Cantidad = r.Cantidad,
              CantidadCapsulas = r.CantidadCapsulas
            })
          })
        });

      return Json(new { Result = true, Groups = groups }, JsonRequestBehavior.DenyGet);

    }

    [HttpPost]
    public ActionResult SaveNewQty(IList<ItemQuantity> items) {
      if (items == null) {
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "There are no items.");
      }

      if (items.Any(p => p.Quantity > 2000 || p.Quantity < 0)) {
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "The item quantity must be a value between 0 and 2000");
      }

      foreach (ItemQuantity item in items) {
        if (ItemsXml.Any(p => p.ItemID == item.ItemID && !string.IsNullOrEmpty(p.UniqueID))) {
          item.Quantity = 1;
        }

        string itemID = item.ItemID;
        Inventarios inv = Database.Inventarios.SingleOrDefault(p => p.IdUsuario == Username && p.ItemID == itemID);

        if (inv != null) {
          if (item.Quantity == 0) {
            Database.Inventarios.Remove(inv);
          }
          else {
            inv.Cantidad = item.Quantity;
          }
        }
        else {
          if (item.Quantity > 0) {
            Database.Inventarios.Add(new Inventarios { IdUsuario = Username, ItemID = itemID, Cantidad = item.Quantity });
          }
        }
      }

      Database.SaveChanges();

      return Json("Successfully saved!");
    }
  }
}