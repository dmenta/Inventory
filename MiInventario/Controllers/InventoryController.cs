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

    private const string GroupIdAll = "G_ALL";

    [HttpGet]
    public ActionResult Index() {
      return View();
    }

    [HttpGet]
    public JsonResult GroupsTotals() {
      DateTime inicio = DateTime.Now;

      var enCapsules = Database.CapsulesItems
          .Where(s => s.Capsules.UserId == Username)
          .GroupBy(t => t.ItemId)
          .Select(u => new { ItemId = u.Key, CapsuleQuantity = u.Sum(v => v.Quantity) }).ToList();

      var inventario = Database.InventoriesItems
          .Where(s => s.Inventories.UserId == Username)
          .Select(u => new { ItemId = u.ItemId, Quantity = u.Quantity }).ToList();

      var model = GroupsXml.Select(p => new GroupViewModel {
        GroupId = p.GroupId,
        GroupName = Resources.Groups.ResourceManager.GetString(p.GroupId),
        TotalQuantity = inventario.Where(s => p.Types.SelectMany(t => t.Items).Any(u => u.ItemId == s.ItemId)).DefaultIfEmpty().Sum(v => v == null ? 0 : v.Quantity),
        CapsulesTotal = enCapsules.Where(s => p.Types.SelectMany(t => t.Items).Any(u => u.ItemId == s.ItemId)).DefaultIfEmpty().Sum(v => v == null ? 0 : v.CapsuleQuantity)
      }).ToList();

      model.Add(new GroupViewModel {
        GroupId = GroupIdAll,
        GroupName = Resources.Groups.ResourceManager.GetString(GroupIdAll),
        TotalQuantity = model.Sum(p => p.TotalQuantity),
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
      string userIdA = Username;
      string userIdB = (Username == "diegomenta@gmail.com" ? "pceriani@gmail.com" : "diegomenta@gmail.com");

      model.OriginUserId = userIdA;
      model.DestinationUserId = userIdB;

      var inventarioA = Database.InventoriesItems
                        .Where(s => s.Inventories.UserId == userIdA)
                        .Select(u => new ItemQuantity { ItemId = u.ItemId, Quantity = u.Quantity }).ToList();
      var inventarioB = Database.InventoriesItems
                        .Where(s => s.Inventories.UserId == userIdB)
                        .Select(u => new ItemQuantity { ItemId = u.ItemId, Quantity = u.Quantity }).ToList();

      var capsulesA = Database.CapsulesItems
                        .Where(s => s.Capsules.UserId == userIdA)
                        .GroupBy(t => t.ItemId)
                        .Select(u => new ItemQuantity { ItemId = u.Key, Quantity = u.Sum(v => v.Quantity) });
      var capsulesB = Database.CapsulesItems
                        .Where(s => s.Capsules.UserId == userIdB)
                        .GroupBy(t => t.ItemId)
                        .Select(u => new ItemQuantity { ItemId = u.Key, Quantity = u.Sum(v => v.Quantity) });

      foreach (var itemC in capsulesA) {
        var itemI = inventarioA.SingleOrDefault(f => f.ItemId == itemC.ItemId);
        if (itemI == null) {
          inventarioA.Add(new ItemQuantity { ItemId = itemC.ItemId, Quantity = itemC.Quantity });
        }
        else {
          itemI.Quantity += itemC.Quantity;
        }
      }

      foreach (var itemC in capsulesB) {
        var itemI = inventarioB.SingleOrDefault(f => f.ItemId == itemC.ItemId);
        if (itemI == null) {
          inventarioB.Add(new ItemQuantity { ItemId = itemC.ItemId, Quantity = itemC.Quantity });
        }
        else {
          itemI.Quantity += itemC.Quantity;
        }
      }
      model.Groups = GroupsXml
          .Select(p => new GroupDifferenceViewModel {
            GroupId = p.GroupId,
            Types = p.Types.Select(q => new TypeDifferenceViewModel {
              TypeId = q.TypeId,
              Items = q.Items.Select(r => new ItemDifferenceViewModel {
                CurrentItem = r,
                OriginQuantity = inventarioA.SingleOrDefault(s => s.ItemId == r.ItemId) == null ? 0 : inventarioA.Single(s => s.ItemId == r.ItemId).Quantity,
                DestinationQuantity = inventarioB.SingleOrDefault(s => s.ItemId == r.ItemId) == null ? 0 : inventarioB.Single(s => s.ItemId == r.ItemId).Quantity
              })
            })
          }).ToList();

      return View(model);

    }

    [HttpGet]
    public ActionResult Manage(string id, bool? editing) {
      Dictionary<string, string> groups = GroupsXml.Select(p => new { GroupId = p.GroupId, Name = Resources.Groups.ResourceManager.GetString(p.GroupId) }).ToDictionary(p => p.GroupId, q => q.Name);
      groups.Add(GroupIdAll, Resources.Groups.ResourceManager.GetString(GroupIdAll));

      var model = new ManageViewModel { Groups = groups };
      if (!string.IsNullOrEmpty(id)) {
        if (groups.ContainsKey(id)) {
          model.GroupId = id;
          model.Editing = editing.GetValueOrDefault();
        }
        else {
          return new HttpStatusCodeResult(System.Net.HttpStatusCode.NotFound, "GroupId does not exists");
        }
      }
      else {
        model.GroupId = GroupsXml.First().GroupId;
        model.Editing = false;
      }

      return View(model);
    }

    [HttpPost]
    public ActionResult Items(string groupId) {
      return GetItems(groupId, false);
    }

    [HttpPost]
    public ActionResult ItemsEdit(string groupId) {
      return GetItems(groupId, true);
    }

    private ActionResult GetItems(string groupId, bool ignoreZeroQuantity) {
      bool allGroups = groupId == GroupIdAll;

      if (!allGroups && !GroupsXml.Any(p => p.GroupId == groupId)) {
        return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest, "GroupId does not exists");
      }

      DateTime inicio = DateTime.Now;

      var enCapsules = Database.CapsulesItems
                  .Where(s => s.Capsules.UserId == Username)
                  .GroupBy(t => t.ItemId)
                  .Select(u => new { ItemId = u.Key, CapsulesQuantity = u.Sum(v => v.Quantity) }).ToList();

      var inventario = Database.InventoriesItems
                       .Where(s => s.Inventories.UserId == Username)
                       .Select(u => new { ItemId = u.ItemId, Quantity = u.Quantity }).ToList();

      double elapsed = (DateTime.Now - inicio).TotalMilliseconds;

      var itemsGroups = ItemsXml
        .Where(z => (allGroups || z.GroupId == groupId) && (ignoreZeroQuantity || (inventario.Any(y => y.ItemId == z.ItemId) || enCapsules.Any(y => y.ItemId == z.ItemId))))
        .Select(y => new {
          y.GroupId,
          y.TypeId,
          y.ItemId,
          y.Order,
          y.Level,
          y.Rarity,
          y.UniqueId,
          y.IsKeyLocker,
          Quantity = inventario.SingleOrDefault(s => s.ItemId == y.ItemId) == null ? 0 : inventario.Single(s => s.ItemId == y.ItemId).Quantity,
          CapsulesQuantity = enCapsules.SingleOrDefault(s => s.ItemId == y.ItemId) == null ? 0 : enCapsules.Single(s => s.ItemId == y.ItemId).CapsulesQuantity
        })
        .ToList();

      if (itemsGroups.Count == 0) {
        return Json(new { Result = false }, JsonRequestBehavior.DenyGet);
      }

      var groups = itemsGroups
        .GroupBy(d => d.GroupId)
        .Select(p => new {
          GroupId = p.Key,
          Types = p.GroupBy(e => e.TypeId)
          .Select(q => new {
            TypeId = q.Key,
            Quantity = q.Sum(f => f.Quantity),
            CapsulesQuantity = q.Sum(f => f.CapsulesQuantity),
            Items = q.Select(r => new {
              CurrentItem = new Item {
                ItemId = r.ItemId,
                TypeId = r.TypeId,
                Order = r.Order,
                Level = r.Level,
                Rarity = r.Rarity,
                UniqueId = r.UniqueId,
                IsKeyLocker = r.IsKeyLocker,
              },
              Quantity = r.Quantity,
              CapsulesQuantity = r.CapsulesQuantity
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
        if (ItemsXml.Any(p => p.ItemId == item.ItemId && !string.IsNullOrEmpty(p.UniqueId))) {
          item.Quantity = 1;
        }

        string itemId = item.ItemId;
        InventoriesItems invItem = Database.InventoriesItems.SingleOrDefault(p => p.Inventories.UserId == Username && p.ItemId == itemId);

        if (invItem != null) {
          if (item.Quantity == 0) {
            Database.InventoriesItems.Remove(invItem);
          }
          else {
            invItem.Quantity = item.Quantity;
          }
        }
        else {
          if (item.Quantity > 0) {
            Inventories inv = Database.Inventories.SingleOrDefault(p => p.UserId == Username);
            if (inv == null) {
              inv = new Inventories { UserId = Username };
              Database.Inventories.Add(inv);
            }

            Database.InventoriesItems.Add(new InventoriesItems { Inventories = inv, ItemId = itemId, Quantity = item.Quantity });
          }
        }
      }

      Database.SaveChanges();

      return Json("Successfully saved!");
    }
  }
}