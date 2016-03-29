using MyInventory.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Xml.Linq;

namespace MyInventory {
  public class MvcApplication : System.Web.HttpApplication {
    protected void Application_Start() {
      AreaRegistration.RegisterAllAreas();
      FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
      RouteConfig.RegisterRoutes(RouteTable.Routes);
      BundleConfig.RegisterBundles(BundleTable.Bundles);

      LoadItemsDefinitions();
    }

    private void LoadItemsDefinitions() {
      XDocument itemsData = XDocument.Load(Server.MapPath("~/App_Data/Items.xml"));
      var itemsNoCapsule = (from c in itemsData.Descendants("item").Where(d => d.Attribute("IsCapsule") == null || !bool.Parse(d.Attribute("IsCapsule").Value))
                            select new Item {
                              ItemId = c.Attribute("ItemId").Value,
                              Order = int.Parse(c.Attribute("Order").Value),
                              GroupId = c.Attribute("GroupId").Value,
                              TypeId = c.Attribute("TypeId").Value,
                              Level = c.Attribute("Level") == null ? 0 : int.Parse(c.Attribute("Level").Value),
                              Rarity = c.Attribute("Rarity").Value,
                              IsKey = c.Attribute("IsKey") != null && bool.Parse(c.Attribute("IsKey").Value),
                            }).ToList();

      var itemsCapsule = (from c in itemsData.Descendants("item").Where(d => d.Attribute("IsCapsule") != null && bool.Parse(d.Attribute("IsCapsule").Value))
                          select new ItemCapsule {
                            ItemId = c.Attribute("ItemId").Value,
                            Order = int.Parse(c.Attribute("Order").Value),
                            GroupId = c.Attribute("GroupId").Value,
                            TypeId = c.Attribute("TypeId").Value,
                            Level = c.Attribute("Level") == null ? 0 : int.Parse(c.Attribute("Level").Value),
                            Rarity = c.Attribute("Rarity").Value,
                            IsKey = false,
                            PaysInterests = c.Attribute("PaysInterests") != null && bool.Parse(c.Attribute("PaysInterests").Value),
                            IsKeyLocker = c.Attribute("IsKeyLocker") != null && bool.Parse(c.Attribute("IsKeyLocker").Value),
                            UniqueId = c.Attribute("UniqueId") == null || c.Attribute("UniqueId").Value.Length == 0 ? null : c.Attribute("UniqueId").Value,
                            Transfer = c.Attribute("Transfer") == null || bool.Parse(c.Attribute("Transfer").Value),
                          }).ToList();


      var allItems = itemsNoCapsule.Union(itemsCapsule).OrderBy(p => p.Order).ToList();

      HttpContext.Current.Application["ItemsXml"] = allItems;

      HttpContext.Current.Application["ItemGroupsXml"] = allItems.GroupBy(p => p.GroupId)
          .Select(q => new ItemGroup {
            GroupId = q.Key,
            Types = q.GroupBy(r => r.TypeId).Select(s => new ItemType {
              TypeId = s.Key,
              Items = s
            })
          }).ToList().AsReadOnly();
    }
  }
}
