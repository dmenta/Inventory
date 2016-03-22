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
                             ItemId = c.Attribute("ItemId").Value,
                             Order = int.Parse(c.Attribute("Order").Value),
                             GroupId = c.Attribute("GroupId").Value,
                             TypeId = c.Attribute("TypeId").Value,
                             Level = c.Attribute("Level")==null?0:int.Parse(c.Attribute("Level").Value),
                             Rarity = c.Attribute("Rarity").Value,
                             IsKey = c.Attribute("IsKey") != null && bool.Parse(c.Attribute("IsKey").Value),
                             IsCapsule = c.Attribute("IsCapsule") != null && bool.Parse(c.Attribute("IsCapsule").Value),
                             PaysInterests = c.Attribute("PaysInterests") != null && bool.Parse(c.Attribute("PaysInterests").Value),
                             IsKeyLocker = c.Attribute("IsKeyLocker") != null && bool.Parse(c.Attribute("IsKeyLocker").Value),
                             UniqueId = c.Attribute("UniqueId") == null || c.Attribute("UniqueId").Value.Length==0 ? null : c.Attribute("UniqueId").Value,
                             Transfer = c.Attribute("Transfer") == null || bool.Parse(c.Attribute("Transfer").Value),
                         }).ToList().AsReadOnly();

            HttpContext.Current.Application["ItemsXml"] = query;

            HttpContext.Current.Application["ItemGroupsXml"] = query.GroupBy(p => p.GroupId)
                .Select(q => new ItemGroup
                {
                    GroupId = q.Key,
                    Types = q.GroupBy(r => r.TypeId).Select(s => new ItemType
                    {
                        TypeId = s.Key,
                        Items = s
                    })
                }).ToList().AsReadOnly();
        }
    }
}
