using MiInventario.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Net;
using MiInventario.Code;


namespace MiInventario.Controllers
{
  [Authorize]
  public class InventoryController : BaseController
  {

    [HttpGet]
    public ActionResult Index()
    {
      var grupos = GroupsXml.Select(p => new GroupViewModel { GroupID = p.GroupID, Name = Resources.Groups.ResourceManager.GetString(p.GroupID) }).ToList();
      grupos.Add(new GroupViewModel { GroupID = "", Name = Resources.Groups.G_ALL });

      return View(grupos);
    }

    [HttpPost]
    public ActionResult Items(string groupID)
    {
      return GetItems(groupID, false);
    }

    [HttpPost]
    public ActionResult ItemsEdit(string groupID)
    {
      return GetItems(groupID, true);
    }

    private ActionResult GetItems(string groupID, bool ignoreZeroQuantity)
    {
      if (!string.IsNullOrWhiteSpace(groupID) && !GroupsXml.Any(p => p.GroupID == groupID))
      {
        return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest, "GroupID does not exists");
      }

      DateTime inicio = DateTime.Now;
      using (InventarioEntities db = new InventarioEntities())
      {
        string user = User.Identity.GetUserName();

        var enCapsulas = db.CapsulasItems
                    .Where(s => s.Capsulas.IdUsuario == user)
                    .GroupBy(t => t.ItemID)
                    .Select(u => new { ItemID = u.Key, CantidadCapsulas = u.Sum(v => v.Cantidad) }).ToList();

        var inventario = db.Inventarios
            .Where(s => s.IdUsuario == user)
            .Select(u => new { ItemID = u.ItemID, Cantidad = u.Cantidad }).ToList();

        var itemsGrupos = ItemsXml
          .Where(z => (string.IsNullOrWhiteSpace(groupID) || z.GroupID == groupID) && (ignoreZeroQuantity || (inventario.Any(y => y.ItemID == z.ItemID) || enCapsulas.Any(y => y.ItemID == z.ItemID))))
          .Select(y => new
          {
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

        var grupos = itemsGrupos
          .GroupBy(d => d.GroupID)
          .Select(p => new
          {
            GroupID = p.Key,
            Tipos = p.GroupBy(e => e.TypeID)
            .Select(q => new
            {
              TypeID = q.Key,
              Cantidad = q.Sum(f => f.Cantidad),
              CantidadCapsulas = q.Sum(f => f.CantidadCapsulas),
              Items = q.Select(r => new
              {
                CurrentItem = new Item
                           {
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
        double elapsed = (DateTime.Now - inicio).TotalMilliseconds;

        return Json(grupos, JsonRequestBehavior.DenyGet);
      }
    }

    [HttpPost]
    public ActionResult SaveNewQty(IList<ItemsQty> items)
    {
      using (InventarioEntities db = new InventarioEntities())
      {
        string user = User.Identity.GetUserName();

        if (items == null)
        {
          return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "There are no items.");
        }

        if (items.Any(p => p.Qty > 2000 || p.Qty < 0))
        {
          return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "The item quantity must be a value between 0 and 2000");
        }

        foreach (ItemsQty item in items)
        {
          string itemID = item.ItemID;
          Inventarios inv = db.Inventarios.SingleOrDefault(p => p.IdUsuario == user && p.ItemID == itemID);

          if (inv != null)
          {
            if (item.Qty == 0)
            {
              db.Inventarios.Remove(inv);
            }
            else
            {
              inv.Cantidad = item.Qty;
            }
          }
          else
          {
            if (item.Qty > 0)
            {
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