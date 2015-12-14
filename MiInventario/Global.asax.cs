using MiInventario.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Xml.Linq;

namespace MiInventario
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            LoadItemsDefinitions();
        }

        private void LoadItemsDefinitions()
        {
            //  <item IdItem="1" Group="G1" Type="T01" Level="1" Rarity="R1" IsCapsule="false" PaysInterests="false" />

            XDocument items = XDocument.Load(Server.MapPath("~/App_Data/Items.xml"));
            var query = (from c in items.Descendants("item")
                         select new Item
                         {
                             ItemID = c.Attribute("ItemID").Value,
                             Order = int.Parse(c.Attribute("Order").Value),
                             GroupID = c.Attribute("GroupID").Value,
                             TypeID = c.Attribute("TypeID").Value,
                             Level = int.Parse(c.Attribute("Level").Value),
                             Rarity = c.Attribute("Rarity").Value,
                             IsCapsule = bool.Parse(c.Attribute("IsCapsule").Value),
                             PaysInterests = bool.Parse(c.Attribute("PaysInterests").Value)
                         }).ToList().AsReadOnly();

            HttpContext.Current.Application["ItemsXml"] = query;

            HttpContext.Current.Application["ItemGroupsXml"] = query.GroupBy(p => p.GroupID)
                .Select(q => new ItemGroup
                {
                    GroupID = q.Key,
                    Types = q.GroupBy(r => r.TypeID).Select(s => new ItemType
                    {
                        TypeID = s.Key,
                        Items = s
                    })
                }).ToList().AsReadOnly();
        }
    }
}
