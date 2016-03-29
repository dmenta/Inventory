using MyInventory.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace MyInventory.Controllers {
  public class BaseController : Controller {
    public IReadOnlyList<Item> ItemsXml { get { return (IReadOnlyList<Item>)HttpContext.Application["ItemsXml"]; } }
    public IReadOnlyList<ItemGroup> GroupsXml { get { return (IReadOnlyList<ItemGroup>)HttpContext.Application["ItemGroupsXml"]; } }

    public string Username {
      get {
        return User.Identity.GetUserName();
      }
    }

    public BaseController() {
      Database = new InventoryEntities();
    }

    protected InventoryEntities Database { get; set; }

    protected override void Dispose(bool disposing) {
      if (Database != null) {
        Database.Dispose();
      }
      base.Dispose(disposing);
    }
  }
}