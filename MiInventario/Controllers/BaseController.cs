using MiInventario.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MiInventario.Controllers
{
    public class BaseController : Controller
    {
        public IReadOnlyList<Item> ItemsXml { get { return (IReadOnlyList<Item>)HttpContext.Application["ItemsXml"]; } }
        public IReadOnlyList<ItemGroup> GroupsXml { get { return (IReadOnlyList<ItemGroup>)HttpContext.Application["ItemGroupsXml"]; } }

    }
}