using MyInventory.Models.Capsules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Globalization;
using MyInventory.Code;
using System.Threading.Tasks;
using System.Web.UI.DataVisualization.Charting;
using System.IO;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Data.Entity;
using System.Drawing.Text;
using System.Net;
using MyInventory.Models;

namespace MyInventory.Controllers {
  [Authorize]
  public class CapsulesController : BaseController {

    private IEnumerable<ItemCapsule> ItemsCapsule {
      get {
        return ItemsXml.Where(p => p is ItemCapsule).Cast<ItemCapsule>();
      }
    }

    [HttpGet]
    public ActionResult Index() {
      return View();
    }

    [HttpGet]
    public JsonResult CapsulesSummary() {
      var capsulesDB = Database.Capsules.Where(p => p.UserId == Username).ToList();


      var capsules = capsulesDB.Select(p => new CapsuleViewModel {
        CapsuleId = p.CapsuleId,
        Code = p.Code,
        TotalQuantity = p.CapsulesItems.Sum(q => q.Quantity),
        Name = ResolveCapsuleName(p),
        Properties = GetProperties(p.ItemId),
        ItemInside = p.CapsulesItems.Count() == 1 ? Models.ItemViewModelLight.Create(ItemsXml, p.CapsulesItems.First().ItemId) : null,
      }).OrderBy(m => m.Properties.Order).ThenBy(n => n.Code);

      return Json(capsules, JsonRequestBehavior.AllowGet);
    }

    [HttpGet]
    public ActionResult List(int id) {
      var model = RecuperarItems(id, false);

      if (model == null) {
        return new HttpNotFoundResult();
      }

      return View(model);
    }

    [HttpGet]
    public ActionResult Add() {
      var model = new AddEditViewModel();

      model.ItemId = "CAPS_RA";
      model.CapsuleTypes = GetValidNewCapsuleTypes(null);

      return View(model);
    }

    [HttpPost]
    public ActionResult Add(AddEditViewModel capsule) {
      if (Database.Capsules.Any(p => p.UserId == Username && p.Code == capsule.Code)) {
        ModelState.AddModelError("Duplicate", "There is another capsule with the same Code");
      }

      var unique = !string.IsNullOrEmpty(ItemsCapsule.Single(q => q.ItemId == capsule.ItemId).UniqueId);

      if (!unique && capsule.Code.Length != 8) {
        ModelState.AddModelError("Code", "Code must be 8 characters long.");
      }

      if (ModelState.IsValid) {
        Database.Capsules.Add(new Capsules { UserId = Username, Code = capsule.Code.ToUpper(), Name = capsule.Name, ItemId = capsule.ItemId });

        Database.SaveChanges();

        return RedirectToAction("Index");
      }

      capsule.CapsuleTypes = GetValidNewCapsuleTypes(null);

      return View(capsule);
    }

    [HttpGet]
    public ActionResult Edit(int id) {
      var capsuleDB = Database.Capsules.SingleOrDefault(p => p.CapsuleId == id && p.UserId == Username);

      if (capsuleDB == null) {
        return new HttpNotFoundResult();
      }

      var model = new AddEditViewModel();
      LoadCapsule(capsuleDB, model);

      model.ItemId = capsuleDB.ItemId;
      model.CapsuleTypes = GetValidNewCapsuleTypes(model.ItemId);

      return View(model);
    }

    [HttpPost]
    public ActionResult Edit(AddEditViewModel capsule) {
      var unique = !string.IsNullOrEmpty(ItemsCapsule.Single(q => q.ItemId == capsule.ItemId).UniqueId);

      if (!unique && capsule.Code.Length != 8) {
        ModelState.AddModelError("Code", "Code must be 8 characters long.");
      }

      var capsuleDB = Database.Capsules.SingleOrDefault(p => p.CapsuleId == capsule.CapsuleId && p.UserId == Username);

      if (capsuleDB == null) {
        return new HttpNotFoundResult();
      }

      if (capsuleDB.Code != capsule.Code && Database.Capsules.Any(p => p.UserId == Username && p.Code == capsule.Code)) {
        ModelState.AddModelError("Duplicate", "There is another capsule with the same Code");
      }

      if (ModelState.IsValid) {
        capsuleDB.Code = capsule.Code;
        capsuleDB.Name = capsule.Name;
        capsuleDB.ItemId = capsule.ItemId;

        Database.SaveChanges();

        return RedirectToAction("Index");
      }

      capsule.CapsuleTypes = GetValidNewCapsuleTypes(capsuleDB.ItemId);

      return View(capsule);
    }

    [HttpGet]
    public ActionResult ManageItems(int id) {
      var model = RecuperarItems(id, false);

      if (model == null) {
        return new HttpNotFoundResult();
      }

      return View(model);
    }

    [HttpPost]
    public ActionResult ManageItems(ManageViewModel capsule) {
      var capsuleDB = Database.Capsules.SingleOrDefault(p => p.CapsuleId == capsule.CapsuleId && p.UserId == Username);

      if (capsuleDB == null) {
        return new HttpNotFoundResult();
      }

      if (capsule.Items != null) {
        foreach (ItemCapsuleViewModel item in capsule.Items) {
          string itemID = item.CurrentItem.ItemId;
          var capsuleItem = Database.CapsulesItems.SingleOrDefault(p => p.CapsuleId == capsule.CapsuleId && p.ItemId == itemID);

          if (capsuleItem != null) {
            capsuleItem.Quantity = item.Quantity;
          }
        }

        Database.SaveChanges();
      }

      return RedirectToAction("List", new { id = capsule.CapsuleId });
    }

    [HttpGet]
    public ActionResult Delete(int id) {
      var capsuleDB = Database.Capsules.SingleOrDefault(p => p.CapsuleId == id && p.UserId == Username);

      if (capsuleDB == null) {
        return new HttpNotFoundResult();
      }

      CapsuleViewModel model = new CapsuleViewModel();
      LoadCapsule(capsuleDB, model);

      return View(model);
    }

    [HttpPost]
    public ActionResult Delete(CapsuleViewModel capsule) {
      var capsuleDB = Database.Capsules.SingleOrDefault(p => p.CapsuleId == capsule.CapsuleId && p.UserId == Username);

      if (capsuleDB == null) {
        return new HttpNotFoundResult();
      }

      Database.CapsulesItems.RemoveRange(capsuleDB.CapsulesItems);
      Database.Capsules.Remove(capsuleDB);

      Database.SaveChanges();

      return RedirectToAction("Index");
    }

    [HttpGet]
    public ActionResult Load(int id) {
      var model = RecuperarItemsLoad(id);

      if (model == null) {
        return new HttpNotFoundResult();
      }

      return View(model);
    }

    [HttpPost]
    public ActionResult Load(LoadViewModel capsule) {
      if (!ModelState.IsValid) {
        return View(capsule);
      }

      var capsuleDB = Database.Capsules.SingleOrDefault(p => p.CapsuleId == capsule.CapsuleId && p.UserId == Username);

      if (capsuleDB == null) {
        return new HttpNotFoundResult();
      }

      if (capsule.Items != null) {

        if (capsule.Items.Any(p => p.LoadQuantity > p.ItemQuantity)) {
          return View(capsule);
        }

        foreach (var item in capsule.Items.Where(p => p.LoadQuantity > 0)) {
          CapsulesItems capsuleItem = Database.CapsulesItems.SingleOrDefault(p => p.CapsuleId == capsule.CapsuleId && p.ItemId == item.CurrentItem.ItemId);

          if (capsuleItem != null) {
            capsuleItem.Quantity += item.LoadQuantity;
          }
          else {
            Database.CapsulesItems.Add(new CapsulesItems { CapsuleId = capsule.CapsuleId, ItemId = item.CurrentItem.ItemId, Quantity = item.LoadQuantity });
          }

          InventoriesItems invItem = Database.InventoriesItems.SingleOrDefault(p => p.Inventories.UserId == Username && p.ItemId == item.CurrentItem.ItemId);

          if (invItem != null) {
            if (invItem.Quantity == item.LoadQuantity) {
              Database.InventoriesItems.Remove(invItem);
            }
            else {
              invItem.Quantity -= item.LoadQuantity;
            }
          }
        }

        Database.SaveChanges();
      }

      return RedirectToAction("List", new { id = capsule.CapsuleId });
    }

    [HttpGet]
    public ActionResult Unload(int id) {
      var model = RecuperarItemsUnload(id);

      if (model == null) {
        return new HttpNotFoundResult();
      }

      return View(model);
    }

    [HttpPost]
    public ActionResult Unload(UnloadViewModel capsule) {
      if (!ModelState.IsValid) {
        var model = RecuperarItemsUnload(capsule.CapsuleId);

        if (model == null) {
          return new HttpNotFoundResult();
        }

        return View(model);
      }

      var capsuleDB = Database.Capsules.SingleOrDefault(p => p.CapsuleId == capsule.CapsuleId && p.UserId == Username);

      if (capsuleDB == null) {
        return new HttpNotFoundResult();
      }

      if (capsule.Items != null) {
        foreach (var item in capsule.Items.Where(p => p.UnloadQuantity > 0)) {
          CapsulesItems capsuleItem = Database.CapsulesItems.SingleOrDefault(p => p.CapsuleId == capsule.CapsuleId && p.ItemId == item.CurrentItem.ItemId);

          if (capsuleItem != null) {
            if (capsuleItem.Quantity == item.UnloadQuantity) {
              Database.CapsulesItems.Remove(capsuleItem);
            }
            else {
              capsuleItem.Quantity -= item.UnloadQuantity;
            }

            InventoriesItems invItem = Database.InventoriesItems.SingleOrDefault(p => p.Inventories.UserId == Username && p.ItemId == item.CurrentItem.ItemId);

            if (invItem != null) {
              invItem.Quantity += item.UnloadQuantity;
            }
            else {
              Inventories inv = Database.Inventories.SingleOrDefault(p => p.UserId == Username);
              if (inv == null) {
                inv = new Inventories { UserId = Username };
                Database.Inventories.Add(inv);
              }
              Database.InventoriesItems.Add(new InventoriesItems { Inventories = inv, ItemId = item.CurrentItem.ItemId, Quantity = item.UnloadQuantity });
            }
          }
        }

        Database.SaveChanges();
      }

      return RedirectToAction("List", new { id = capsule.CapsuleId });
    }

    [HttpGet]
    public ActionResult LogInterests(int id) {
      var model = RecuperarItems(id, true);

      if (model == null) {
        return new HttpNotFoundResult();
      }

      if (!model.Items.Any()) {
        return RedirectToAction("List", new { id = id });
      }

      return View(model);
    }

    [HttpPost]
    public ActionResult LogInterests(ManageViewModel capsule) {
      var capsuleDB = Database.Capsules.SingleOrDefault(p => p.CapsuleId == capsule.CapsuleId && p.UserId == Username);

      if (capsuleDB == null) {
        return new HttpNotFoundResult();
      }

      var spawnables = ItemsCapsule.Where(p => p.PaysInterests).Select(s => s.ItemId);
      if (!spawnables.Contains(capsuleDB.ItemId)) {
        return RedirectToAction("List", new { id = capsule.CapsuleId });
      }

      if (!capsuleDB.CapsulesItems.Any()) {
        return RedirectToAction("List", new { id = capsule.CapsuleId });
      }

      if (capsule.Items == null) {
        return RedirectToAction("List", new { id = capsule.CapsuleId });
      }

      if (capsule.Items.Sum(p => p.Quantity) > 100) {
        ModelState.AddModelError("Masde100", "Total Quantity on capsule exceeds 100.");
        return LogInterests(capsule.CapsuleId);
      }

      Dictionary<string, int> nuevos = new Dictionary<string, int>();

      foreach (ItemCapsuleViewModel item in capsule.Items) {
        CapsulesItems capsuleItem = Database.CapsulesItems.SingleOrDefault(p => p.CapsuleId == capsule.CapsuleId && p.ItemId == item.CurrentItem.ItemId);

        if (capsuleItem != null) {
          if (capsuleItem.Quantity > item.Quantity) {
            ModelState.AddModelError("Masde100", "Quantity must be equal or greater than quantity on capsule.");
            return LogInterests(capsule.CapsuleId);
          }
          else if (capsuleItem.Quantity < item.Quantity) {
            nuevos.Add(item.CurrentItem.ItemId, item.Quantity - capsuleItem.Quantity);
            capsuleItem.Quantity = item.Quantity;
          }
        }
        else {
          break;
        }
      }

      if (nuevos.Count > 0) {
        DateTime currentLapse = DateTime.Now.AddHours(-1);
        Spawns current = Database.Spawns.SingleOrDefault(p => p.UserId == Username && p.Date >= currentLapse);
        if (current == null) {
          current = Database.Spawns.Add(new Spawns { UserId = Username, Date = DateTime.Now });
          Database.Spawns.Add(current);
        }

        SpawnsCapsules capsuleSpawn = current.SpawnsCapsules.SingleOrDefault(p => p.CapsuleCode == capsuleDB.Code);

        if (capsuleSpawn == null) {
          capsuleSpawn = Database.SpawnsCapsules.Add(new SpawnsCapsules { Spawns = current, CapsuleCode = capsuleDB.Code });
          Database.SpawnsCapsules.Add(capsuleSpawn);
        }

        foreach (var kv in nuevos) {
          SpawnsCapsulesItems capsuleSpawnItem = capsuleSpawn.SpawnsCapsulesItems.SingleOrDefault(p => p.ItemId == kv.Key);
          if (capsuleSpawnItem != null) {
            capsuleSpawnItem.Quantity += kv.Value;
          }
          else {
            capsuleSpawn.SpawnsCapsulesItems.Add(new SpawnsCapsulesItems { SpawnsCapsules = capsuleSpawn, ItemId = kv.Key, Quantity = kv.Value });
          }
        }

        Database.SaveChanges();

        return RedirectToAction("List", new { id = capsule.CapsuleId });
      }

      return RedirectToAction("LogInterests", new { id = capsule.CapsuleId });
    }


    [HttpGet]
    public ActionResult AddItem(int id) {
      var capsuleDB = Database.Capsules.SingleOrDefault(p => p.CapsuleId == id && p.UserId == Username);

      if (capsuleDB == null) {
        return new HttpNotFoundResult();
      }

      AddItemViewModel model = new AddItemViewModel();
      LoadCapsule(capsuleDB, model);

      var itemsCargados = capsuleDB.CapsulesItems.Select(p => p.ItemId).ToList();
      var isKey = ItemsXml.Where(p => p.IsKey).Select(s => s.ItemId);

      model.AddeableItems = ItemsCapsule.Where(q => !itemsCargados.Contains(q.ItemId) && (!model.Properties.IsKeyLocker || isKey.Contains(q.ItemId)))
          .Select(q => new Models.ItemViewModel {
            CurrentItem = q,
          }).ToList();

      return View(model);
    }

    [HttpPost]
    public ActionResult AddItem(AddItemViewModel addedItem) {
      var capsuleDB = Database.Capsules.SingleOrDefault(p => p.CapsuleId == addedItem.CapsuleId && p.UserId == Username);

      if (capsuleDB == null) {
        return new HttpNotFoundResult();
      }

      if (ModelState.IsValid) {
        Database.CapsulesItems.Add(new CapsulesItems { CapsuleId = addedItem.CapsuleId, ItemId = addedItem.ItemId, Quantity = addedItem.Quantity });

        Database.SaveChanges();

        return RedirectToAction("List", new { id = addedItem.CapsuleId });
      }
      else {
        LoadCapsule(capsuleDB, addedItem);

        var itemsCargados = capsuleDB.CapsulesItems.Select(p => p.ItemId).ToList();
        var isKey = ItemsXml.Where(p => p.IsKey).Select(s => s.ItemId);

        addedItem.AddeableItems = ItemsCapsule.Where(q => !itemsCargados.Contains(q.ItemId) && (!addedItem.Properties.IsKeyLocker || isKey.Contains(q.ItemId)))
              .Select(q => new Models.ItemViewModel {
                CurrentItem = q,
              }).ToList();

        return View(addedItem);
      }
    }

    [HttpGet]
    public ActionResult DeleteItem(int id, string itemID) {
      if (string.IsNullOrWhiteSpace(itemID)) {
        return new HttpNotFoundResult();
      }

      var capsuleDB = Database.Capsules.SingleOrDefault(p => p.CapsuleId == id && p.UserId == Username);

      if (capsuleDB == null) {
        return new HttpNotFoundResult();
      }

      var item = capsuleDB.CapsulesItems.SingleOrDefault(p => p.ItemId == itemID);

      if (item == null) {
        return new HttpNotFoundResult();
      }


      DeleteItemViewModel model = new DeleteItemViewModel();
      LoadCapsule(capsuleDB, model);

      model.Item = ItemViewModelLight.Create(ItemsXml,item.ItemId);
      model.Quantity = item.Quantity;

      return View(model);
    }

    [HttpPost]
    public ActionResult DeleteItem(DeleteItemViewModel element) {

      var capsuleDB = Database.Capsules.SingleOrDefault(p => p.CapsuleId == element.CapsuleId && p.UserId == Username);

      if (capsuleDB == null) {
        return new HttpNotFoundResult();
      }

      var item = capsuleDB.CapsulesItems.SingleOrDefault(p => p.ItemId == element.Item.CurrentItem.ItemId);

      if (item == null) {
        return new HttpNotFoundResult();
      }

      Database.CapsulesItems.Remove(item);

      Database.SaveChanges();

      return RedirectToAction("ManageItems", new { id = element.CapsuleId });
    }

    [HttpGet]
    public ActionResult SendToUser(int id, string sendToUser) {
      var capsuleDB = Database.Capsules.SingleOrDefault(p => p.UserId == Username && p.CapsuleId == id);

      if (capsuleDB == null) {
        return new HttpNotFoundResult();
      }

      bool hasCode = Database.Capsules.Any(p => p.UserId == sendToUser && p.Code == capsuleDB.Code);

      if (hasCode) {
        return new HttpNotFoundResult();
      }

      SendToViewModel model = new SendToViewModel();
      LoadCapsule(capsuleDB, model);
      model.UserId = sendToUser;

      return View(model);
    }

    [HttpPost]
    public ActionResult SendToUser(SendToViewModel capsule) {
      var capsuleDB = Database.Capsules.SingleOrDefault(p => p.UserId == Username && p.CapsuleId == capsule.CapsuleId);

      if (capsuleDB == null) {
        return new HttpNotFoundResult();
      }

      capsuleDB.UserId = capsule.UserId;

      Database.SaveChanges();

      return RedirectToAction("Index");
    }


    private void LoadCapsule(Capsules capsuleDB, CapsuleViewModel model) {
      model.CapsuleId = capsuleDB.CapsuleId;
      model.Code = capsuleDB.Code;
      model.Name = ResolveCapsuleName(capsuleDB);
      model.TotalQuantity = capsuleDB.CapsulesItems.Sum(q => q.Quantity);
      model.Properties = GetProperties(capsuleDB.ItemId);
      model.ItemInside = capsuleDB.CapsulesItems.Count() == 1 ? Models.ItemViewModelLight.Create(ItemsXml, capsuleDB.CapsulesItems.First().ItemId) : null;
    }

    private UnloadViewModel RecuperarItemsUnload(int id) {
      var capsuleDB = Database.Capsules.SingleOrDefault(p => p.CapsuleId == id && p.UserId == Username);

      if (capsuleDB == null) {
        return null;
      }

      UnloadViewModel model = new UnloadViewModel();
      LoadCapsule(capsuleDB, model);

      model.Items = capsuleDB.CapsulesItems.Select(p => new ItemUnloadViewModel {
        CurrentItem = ItemBase.Create(ItemsXml, p.ItemId),
        CapsuleQuantity = p.Quantity,
        UnloadQuantity = 0
      }).OrderBy(x => x.CurrentItem.Order).ToList();

      return model;
    }

    private LoadViewModel RecuperarItemsLoad(int id) {
      var capsuleDB = Database.Capsules.SingleOrDefault(p => p.CapsuleId == id && p.UserId == Username);

      if (capsuleDB == null) {
        return null;
      }

      LoadViewModel model = new LoadViewModel();
      LoadCapsule(capsuleDB, model);

      model.Items = capsuleDB.CapsulesItems.Select(p => new ItemLoadViewModel {
        CurrentItem = ItemBase.Create(ItemsXml, p.ItemId),
        CapsuleQuantity = p.Quantity,
        ItemQuantity = 0,
        LoadQuantity = 0
      }).OrderBy(x => x.CurrentItem.Order).ToList();

      var isCapsule = ItemsCapsule.Select(s => s.ItemId);
      var isKey = ItemsXml.Where(p => p.IsKey).Select(s => s.ItemId);

      var inventoryDB = Database.InventoriesItems.Where(p => p.Inventories.UserId == Username && !isCapsule.Contains(p.ItemId) && (!model.Properties.IsKeyLocker || isKey.Contains(p.ItemId))).ToList();

      var inventories = inventoryDB.Select(q => new ItemLoadViewModel {
        CurrentItem = ItemBase.Create(ItemsXml, q.ItemId),
        CapsuleQuantity = 0,
        ItemQuantity = q.Quantity,
        LoadQuantity = 0
      }).OrderBy(p => p.CurrentItem.Order).ToList();

      foreach (var item in model.Items) {
        ItemLoadViewModel inv = inventories.SingleOrDefault(p => p.CurrentItem == item.CurrentItem);
        if (inv != null) {
          item.ItemQuantity = inv.ItemQuantity;
          inventories.Remove(inv);
        }
      }

      model.Items.AddRange(inventories);

      return model;
    }

    private ManageViewModel RecuperarItems(int id, bool onlySpawnables) {
      var capsuleDB = Database.Capsules.SingleOrDefault(p => p.CapsuleId == id && p.UserId == Username);

      if (capsuleDB == null) {
        return null;
      }

      if (onlySpawnables) {
        var spawnables = ItemsCapsule.Where(p => p.PaysInterests).Select(s => s.ItemId);

        if (!spawnables.Contains(capsuleDB.ItemId)) {
          return null;
        }
      }

      ManageViewModel model = new ManageViewModel();
      LoadCapsule(capsuleDB, model);

      model.Items = capsuleDB.CapsulesItems.Select(p => new ItemCapsuleViewModel {
        CurrentItem = ItemBase.Create(ItemsXml, p.ItemId),
        Quantity = p.Quantity
      }).OrderBy(x => x.CurrentItem.Order).ToList();

      return model;
    }

    private CapsuleProperties GetProperties(string capsuleItemId) {
      return ItemsCapsule.Where(p => p.ItemId == capsuleItemId).Select(q => new CapsuleProperties {
        Order = q.Order,
        IsKeyLocker = q.IsKeyLocker,
        IsTransferable = q.Transfer,
        PaysInterests = q.PaysInterests,
        UniqueId = q.UniqueId,
        UniqueName = string.IsNullOrEmpty(q.UniqueId) ? null : Resources.ItemsNames.ResourceManager.GetString(q.UniqueId)
      }).Single();
    }

    private string ResolveCapsuleName(Capsules capsule) {
      if (!string.IsNullOrEmpty(capsule.Name)) {
        return capsule.Name;
      }

      if (!capsule.CapsulesItems.Any()) {
        return Resources.General.ResourceManager.GetString("Capsule_Empty");
      }

      if (capsule.CapsulesItems.Count() > 1) {
        return string.Format("({0} items)", capsule.CapsulesItems.Count());
      }

      return string.Empty;
    }

    private IEnumerable<ItemCapsule> GetValidNewCapsuleTypes(string capsuleItemId) {
      var existingTypes = Database.Capsules.Where(p => p.UserId == Username).Select(q => q.ItemId).Distinct();

      return ItemsCapsule.Where(p => p.ItemId == capsuleItemId || string.IsNullOrEmpty(p.UniqueId) || !existingTypes.Contains(p.ItemId));
    }
  }
}