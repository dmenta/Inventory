using MiInventario.Models.Capsules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Globalization;
using MiInventario.Code;
using System.Threading.Tasks;
using System.Web.UI.DataVisualization.Charting;
using System.IO;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Data.Entity;
using System.Drawing.Text;
using System.Net;

namespace MiInventario.Controllers {
  [Authorize]
  public class CapsulesController : BaseController {


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
        ItemInside = p.CapsulesItems.Count() == 1 ? new Models.ItemViewModel { CurrentItem = ItemsXml.Single(s => s.ItemID == p.CapsulesItems.First().ItemID) } : null,
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

      var unique = !string.IsNullOrEmpty(ItemsXml.Single(p => p.IsCapsule && p.ItemID == capsule.ItemId).UniqueID);

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
      var unique = !string.IsNullOrEmpty(ItemsXml.Single(p => p.IsCapsule && p.ItemID == capsule.ItemId).UniqueID);

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
        foreach (Models.ItemInventoryViewModel item in capsule.Items) {
          string itemID = item.CurrentItem.ItemID;
          var capsulaItem = Database.CapsulesItems.SingleOrDefault(p => p.CapsuleId == capsule.CapsuleId && p.ItemID == itemID);

          if (capsulaItem != null) {
            capsulaItem.Quantity = item.Quantity;
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
    public ActionResult Load(LoadViewModel capsula) {
      if (!ModelState.IsValid) {
        return View(capsula);
      }

      var capsulaDB = Database.Capsules.SingleOrDefault(p => p.CapsuleId == capsula.CapsuleId && p.UserId == Username);

      if (capsulaDB == null) {
        return new HttpNotFoundResult();
      }

      if (capsula.Items != null) {

        if (capsula.Items.Any(p => p.LoadQuantity > p.ItemQuantity)) {
          return View(capsula);
        }

        foreach (var item in capsula.Items.Where(p => p.LoadQuantity > 0)) {
          CapsulesItems capsulaItem = Database.CapsulesItems.SingleOrDefault(p => p.CapsuleId == capsula.CapsuleId && p.ItemID == item.CurrentItem.ItemID);

          if (capsulaItem != null) {
            capsulaItem.Quantity += item.LoadQuantity;
          }
          else {
            Database.CapsulesItems.Add(new CapsulesItems { CapsuleId = capsula.CapsuleId, ItemID = item.CurrentItem.ItemID, Quantity = item.LoadQuantity });
          }

          Inventarios inv = Database.Inventarios.SingleOrDefault(p => p.IdUsuario == Username && p.ItemID == item.CurrentItem.ItemID);

          if (inv != null) {
            if (inv.Cantidad == item.LoadQuantity) {
              Database.Inventarios.Remove(inv);
            }
            else {
              inv.Cantidad -= item.LoadQuantity;
            }
          }
        }

        Database.SaveChanges();
      }

      return RedirectToAction("List", new { id = capsula.CapsuleId });
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

      var capsulaDB = Database.Capsules.SingleOrDefault(p => p.CapsuleId == capsule.CapsuleId && p.UserId == Username);

      if (capsulaDB == null) {
        return new HttpNotFoundResult();
      }

      if (capsule.Items != null) {
        foreach (var item in capsule.Items.Where(p => p.UnloadQuantity > 0)) {
          CapsulesItems capsulaItem = Database.CapsulesItems.SingleOrDefault(p => p.CapsuleId == capsule.CapsuleId && p.ItemID == item.CurrentItem.ItemID);

          if (capsulaItem != null) {
            if (capsulaItem.Quantity == item.UnloadQuantity) {
              Database.CapsulesItems.Remove(capsulaItem);
            }
            else {
              capsulaItem.Quantity -= item.UnloadQuantity;
            }

            Inventarios inv = Database.Inventarios.SingleOrDefault(p => p.IdUsuario == Username && p.ItemID == item.CurrentItem.ItemID);

            if (inv != null) {
              inv.Cantidad += item.UnloadQuantity;
            }
            else {
              Database.Inventarios.Add(new Inventarios { IdUsuario = Username, ItemID = item.CurrentItem.ItemID, Cantidad = item.UnloadQuantity });
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

      var spawnables = ItemsXml.Where(p => p.PaysInterests).Select(s => s.ItemID);
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

      foreach (Models.ItemInventoryViewModel item in capsule.Items) {
        CapsulesItems capsulaItem = Database.CapsulesItems.SingleOrDefault(p => p.CapsuleId == capsule.CapsuleId && p.ItemID == item.CurrentItem.ItemID);

        if (capsulaItem != null) {
          if (capsulaItem.Quantity > item.Quantity) {
            ModelState.AddModelError("Masde100", "Quantity must be equal or greater than quantity on capsule.");
            return LogInterests(capsule.CapsuleId);
          }
          else if (capsulaItem.Quantity < item.Quantity) {
            nuevos.Add(item.CurrentItem.ItemID, item.Quantity - capsulaItem.Quantity);
            capsulaItem.Quantity = item.Quantity;
          }
        }
        else {
          break;
        }
      }

      if (nuevos.Count > 0) {
        DateTime ahora = DateTime.Now;
        DateTime diferencia = ahora.AddHours(-1);
        Reproducciones v_ultimaCercana = Database.Reproducciones.FirstOrDefault(p => p.IdUsuario == Username && p.Fecha >= diferencia);
        if (v_ultimaCercana != null) {
          ahora = v_ultimaCercana.Fecha;
        }

        Reproducciones v_repro = new Reproducciones { IdCapsula = capsuleDB.Code, IdUsuario = Username, Fecha = ahora };
        foreach (var kv in nuevos) {
          v_repro.ReproduccionesItems.Add(new ReproduccionesItems { ItemID = kv.Key, Cantidad = kv.Value });
        }
        Database.Reproducciones.Add(v_repro);

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

      var itemsCargados = capsuleDB.CapsulesItems.Select(p => p.ItemID).ToList();
      var isKey = ItemsXml.Where(p => p.IsKey).Select(s => s.ItemID);

      model.AddeableItems = ItemsXml.Where(p => !p.IsCapsule && !itemsCargados.Contains(p.ItemID) && (!model.Properties.IsKeyLocker || isKey.Contains(p.ItemID)))
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
        Database.CapsulesItems.Add(new CapsulesItems { CapsuleId = addedItem.CapsuleId, ItemID = addedItem.ItemID, Quantity = addedItem.Quantity });

        Database.SaveChanges();

        return RedirectToAction("List", new { id = addedItem.CapsuleId });
      }
      else {
        LoadCapsule(capsuleDB, addedItem);

        var itemsCargados = capsuleDB.CapsulesItems.Select(p => p.ItemID).ToList();
        var isKey = ItemsXml.Where(p => p.IsKey).Select(s => s.ItemID);

        addedItem.AddeableItems = ItemsXml.Where(p => !p.IsCapsule && !itemsCargados.Contains(p.ItemID) && (!addedItem.Properties.IsKeyLocker || isKey.Contains(p.ItemID)))
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

      var item = capsuleDB.CapsulesItems.SingleOrDefault(p => p.ItemID == itemID);

      if (item == null) {
        return new HttpNotFoundResult();
      }


      DeleteItemViewModel model = new DeleteItemViewModel();
      LoadCapsule(capsuleDB, model);

      Models.ItemViewModel summary = new Models.ItemViewModel {
        CurrentItem = ItemsXml.Single(z => z.ItemID == item.ItemID),
      };
      model.Item = summary;
      model.Quantity = item.Quantity;

      return View(model);
    }

    [HttpPost]
    public ActionResult DeleteItem(DeleteItemViewModel element) {

      var capsulaDB = Database.Capsules.SingleOrDefault(p => p.CapsuleId == element.CapsuleId && p.UserId == Username);

      if (capsulaDB == null) {
        return new HttpNotFoundResult();
      }

      var item = capsulaDB.CapsulesItems.SingleOrDefault(p => p.ItemID == element.Item.CurrentItem.ItemID);

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
      model.ItemInside = capsuleDB.CapsulesItems.Count() == 1 ? new Models.ItemViewModel { CurrentItem = ItemsXml.Single(s => s.ItemID == capsuleDB.CapsulesItems.First().ItemID) } : null;
    }

    private UnloadViewModel RecuperarItemsUnload(int id) {
      var capsuleDB = Database.Capsules.SingleOrDefault(p => p.CapsuleId == id && p.UserId == Username);

      if (capsuleDB == null) {
        return null;
      }

      UnloadViewModel model = new UnloadViewModel();
      LoadCapsule(capsuleDB, model);

      model.Items = capsuleDB.CapsulesItems.Select(p => new ItemUnloadViewModel {
        CurrentItem = ItemsXml.Single(z => z.ItemID == p.ItemID),
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
        CurrentItem = ItemsXml.Single(z => z.ItemID == p.ItemID),
        CapsuleQuantity = p.Quantity,
        ItemQuantity = 0,
        LoadQuantity = 0
      }).OrderBy(x => x.CurrentItem.Order).ToList();

      var isCapsule = ItemsXml.Where(p => p.IsCapsule).Select(s => s.ItemID);
      var isKey = ItemsXml.Where(p => p.IsKey).Select(s => s.ItemID);

      var inventoryDB = Database.Inventarios.Where(p => p.IdUsuario == Username && !isCapsule.Contains(p.ItemID) && (!model.Properties.IsKeyLocker || isKey.Contains(p.ItemID))).ToList();

      var inventories = inventoryDB.Select(q => new ItemLoadViewModel {
        CurrentItem = ItemsXml.Single(z => z.ItemID == q.ItemID),
        CapsuleQuantity = 0,
        ItemQuantity = q.Cantidad,
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
        var spawnables = ItemsXml.Where(p => p.PaysInterests).Select(s => s.ItemID);

        if (!spawnables.Contains(capsuleDB.ItemId)) {
          return null;
        }
      }

      ManageViewModel model = new ManageViewModel();
      LoadCapsule(capsuleDB, model);

      model.Items = capsuleDB.CapsulesItems.Select(p => new Models.ItemInventoryViewModel {
        CurrentItem = ItemsXml.Single(z => z.ItemID == p.ItemID),
        Quantity = p.Quantity
      }).OrderBy(x => x.CurrentItem.Order).ToList();

      return model;
    }

    private CapsuleProperties GetProperties(string capsuleItemID) {
      return ItemsXml.Where(p => p.ItemID == capsuleItemID).Select(q => new CapsuleProperties {
        Order = q.Order,
        IsKeyLocker = q.IsKeyLocker,
        IsTransferable = q.Transfer,
        PaysInterests = q.PaysInterests,
        UniqueID = q.UniqueID,
        UniqueName = string.IsNullOrEmpty(q.UniqueID) ? null : Resources.ItemsNames.ResourceManager.GetString(q.UniqueID)
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

    private IEnumerable<Item> GetValidNewCapsuleTypes(string capsuleItemId) {
      var existingTypes = Database.Capsules.Where(p => p.UserId == Username).Select(q => q.ItemId).Distinct();

      return ItemsXml.Where(p => p.IsCapsule && (p.ItemID == capsuleItemId || string.IsNullOrEmpty(p.UniqueID) || !existingTypes.Contains(p.ItemID)));
    }
  }
}