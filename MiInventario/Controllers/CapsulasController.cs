using MiInventario.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Globalization;
using MiInventario.Code;
using System.Threading.Tasks;
using System.Web.UI.DataVisualization.Charting;
using System.IO;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Data.Entity;
using System.Drawing.Text;

namespace MiInventario.Controllers
{
    [Authorize]
    public class CapsulasController : BaseController
    {
        [HttpGet]
        public ActionResult InterestsCharts()
        {
            using (InventarioEntities db = new InventarioEntities())
            {
                string user = User.Identity.GetUserName();

                var reproduccionesDB = db.ReproduccionesItems
                    .Where(p => p.Reproducciones.IdUsuario == user)
                    .Select(s => s.ItemID)
                    .Distinct()
                    .ToList();

                CapsulesInterestsChartsViewModel model = new CapsulesInterestsChartsViewModel();
                model.Grouping = DateGrouping.Week;
                model.Accumulate = true;
                model.ViewableItems = ItemsXml.Where(p => reproduccionesDB.Contains(p.ItemID)).Select(q => new { q.ItemID, Description = q.Description() }).ToDictionary(r => r.ItemID, s => s.Description);

                return View(model);
            }
        }

        [HttpGet]
        public ActionResult _InterestsChartTitle(DateGrouping grouping, string itemID, bool accumulative)
        {
            var model = new ChartTitleViewModel();
            model.Grouping = grouping;
            model.Item = new ItemViewModel() { CurrentItem = ItemsXml.SingleOrDefault(p => p.ItemID == itemID) };
            model.Accumulative = accumulative;

            return PartialView("_InterestsChartTitle", model);
        }

        [HttpGet]
        public ActionResult InterestsDateItemChart(DateGrouping grouping, string itemID, bool accumulate)
        {
            DateTime inicio = DateTime.Now;

            using (InventarioEntities db = new InventarioEntities())
            {
                string user = User.Identity.GetUserName();

                var reproduccionesDB = db.ReproduccionesItems.Where(p => p.Reproducciones.IdUsuario == user)
                    .GroupBy(z => new { z.Reproducciones.Fecha, z.Reproducciones.IdCapsula })
                    .Select(s => new
                    {
                        IdCapsula = s.Key.IdCapsula,
                        Fecha = s.Key.Fecha,
                        Items = s.GroupBy(x => x.ItemID).Select(y => new { ItemID = y.Key, Cantidad = y.Sum(t => t.Cantidad) }),
                    }).ToList();

                var model = new CapsulasInterestsByDateViewModel();
                model.Grouping = grouping;
                model.DateInfo = reproduccionesDB
                            .GroupBy(r => GetResolvedDate(grouping, r.Fecha))
                            .Select(s => new DateInfoModel
                            {
                                Fecha = s.Key,
                                TotalCapsules = s.Where(n => n.Items.Any(q => string.IsNullOrEmpty(itemID) || q.ItemID == itemID)).Select(z => z.IdCapsula).Distinct().Count(),
                                DifferentItems = s.SelectMany(n => n.Items).Where(b => string.IsNullOrEmpty(itemID) || b.ItemID == itemID).Select(h => h.ItemID).Distinct().Count(),
                                TotalItems = s.SelectMany(n => n.Items).Where(b => string.IsNullOrEmpty(itemID) || b.ItemID == itemID).DefaultIfEmpty().Sum(p => p == null ? 0 : p.Cantidad)
                            }).OrderBy(i => i.Fecha).ToList();

                if (accumulate)
                {
                    int actualItemQty = 0;
                    int actualCapsQty = 0;
                    foreach (DateInfoModel info in model.DateInfo)
                    {
                        info.TotalItems += actualItemQty;
                        actualItemQty = info.TotalItems;

                        info.TotalCapsules += actualCapsQty;
                        actualCapsQty = info.TotalCapsules;
                    }
                }

                TimeSpan tiempo = DateTime.Now.Subtract(inicio);

                return View(model);
            }
        }
        public ActionResult InterestsChart(DateGrouping grouping, string itemID, bool accumulate)
        {
            DateTime inicio = DateTime.Now;

            using (InventarioEntities db = new InventarioEntities())
            {
                string user = User.Identity.GetUserName();

                var reproduccionesDB = db.ReproduccionesItems.Where(p => p.Reproducciones.IdUsuario == user)
                    .GroupBy(z => new { z.Reproducciones.Fecha, z.Reproducciones.IdCapsula })
                    .Select(s => new
                    {
                        IdCapsula = s.Key.IdCapsula,
                        Fecha = s.Key.Fecha,
                        Items = s.GroupBy(x => x.ItemID).Select(y => new { ItemID = y.Key, Cantidad = y.Sum(t => t.Cantidad) }),
                    }).ToList();

                var model = new CapsulasInterestsByDateViewModel();
                model.Grouping = grouping;
                model.DateInfo = reproduccionesDB
                            .GroupBy(r => GetResolvedDate(grouping, r.Fecha))
                            .Select(s => new DateInfoModel
                            {
                                Fecha = s.Key,
                                TotalCapsules = s.Where(n => n.Items.Any(q => string.IsNullOrEmpty(itemID) || q.ItemID == itemID)).Select(z => z.IdCapsula).Distinct().Count(),
                                DifferentItems = s.SelectMany(n => n.Items).Where(b => string.IsNullOrEmpty(itemID) || b.ItemID == itemID).Select(h => h.ItemID).Distinct().Count(),
                                TotalItems = s.SelectMany(n => n.Items).Where(b => string.IsNullOrEmpty(itemID) || b.ItemID == itemID).DefaultIfEmpty().Sum(p => p == null ? 0 : p.Cantidad)
                            }).OrderBy(i => i.Fecha).ToList();

                if (accumulate)
                {
                    int actualItemQty = 0;
                    int actualCapsQty = 0;
                    foreach (DateInfoModel info in model.DateInfo)
                    {
                        info.TotalItems += actualItemQty;
                        actualItemQty = info.TotalItems;

                        info.TotalCapsules += actualCapsQty;
                        actualCapsQty = info.TotalCapsules;
                    }
                }

                TimeSpan tiempo = DateTime.Now.Subtract(inicio);

                string dateFormat = Resources.General.ResourceManager.GetString(string.Format("InterestsChart_DateFormat_{0}", model.Grouping.ToString()));

                PrivateFontCollection pfc = new PrivateFontCollection();
                pfc.AddFontFile(Server.MapPath("~/Content/Coda-Regular.ttf"));

                using (Chart chart = new Chart())
                {
                    chart.Font.Name = pfc.Families[0].Name;
                    chart.Font.Size = FontUnit.Point(8);
                    chart.Width = 1000;
                    chart.Height = 400;
                    chart.BackColor = Color.FromArgb(255, 0x27, 0x2B, 0x30);
                    chart.BorderlineDashStyle = ChartDashStyle.Solid;
                    chart.BorderlineColor = Color.Gray;
                    chart.BorderlineWidth = 1;
                    chart.Palette = ChartColorPalette.None;
                    chart.PaletteCustomColors = new[] { Color.Orange, Color.LightGray };

                    using (Font fuente = new Font(pfc.Families[0], 8, GraphicsUnit.Point))
                    {
                        ChartArea area = new ChartArea();
                        area.BackColor = Color.Transparent;
                        area.ShadowColor = Color.Transparent;
                        area.BorderColor = Color.FromArgb(255, 0x88, 0x88, 0x88);
                        area.BorderDashStyle = ChartDashStyle.Solid;

                        var ejeX = area.AxisX;
                        ejeX.LabelStyle.Font = fuente;
                        ejeX.LabelStyle.ForeColor = Color.White;
                        ejeX.LineColor = Color.FromArgb(255, 0x99, 0x99, 0x99);
                        ejeX.IsLabelAutoFit = false;
                        ejeX.IsMarginVisible = true;
                        ejeX.MajorGrid.LineColor = Color.FromArgb(255, 0x99, 0x99, 0x99);
                        ejeX.MajorTickMark.LineColor = Color.FromArgb(255, 0xAA, 0xAA, 0xAA);

                        var interval = Math.Max(model.DateInfo.Count() / 25, 1);
                        ejeX.Interval = interval;
                        ejeX.LabelStyle.Angle = -90;

                        var ejeY = area.AxisY;
                        ejeY.LabelStyle.Font = fuente;
                        ejeY.LabelStyle.ForeColor = Color.White;
                        ejeY.LineColor = Color.FromArgb(255, 0x99, 0x99, 0x99);
                        ejeY.IsLabelAutoFit = false;
                        ejeY.IsMarginVisible = true;
                        ejeY.MajorGrid.LineColor = Color.FromArgb(255, 0x99, 0x99, 0x99);
                        ejeY.MajorTickMark.LineColor = Color.FromArgb(255, 0xAA, 0xAA, 0xAA);

                        chart.ChartAreas.Add(area);

                        Series itemsSerie = new Series("Items");
                        itemsSerie.Font = fuente;
                        itemsSerie.ChartType = SeriesChartType.Spline;
                        itemsSerie.Points.DataBindXY(model.DateInfo.Select(p => p.Fecha.ToString(dateFormat)).ToArray(), model.DateInfo.Select(p => p.TotalItems).ToArray());
                        itemsSerie.IsValueShownAsLabel = grouping != DateGrouping.Day;
                        itemsSerie.LabelForeColor = Color.Orange;
                        chart.Series.Add(itemsSerie);

                        Series capsulasSerie = new Series("Capsules");
                        capsulasSerie.Font = fuente;
                        capsulasSerie.ChartType = SeriesChartType.Column;
                        capsulasSerie.Points.DataBindXY(model.DateInfo.Select(p => p.Fecha.ToString(dateFormat)).ToArray(), model.DateInfo.Select(p => p.TotalCapsules).ToArray());
                        capsulasSerie.IsValueShownAsLabel = grouping != DateGrouping.Day;
                        capsulasSerie.LabelForeColor = Color.LightGray;
                        chart.Series.Add(capsulasSerie);

                        MemoryStream ms = new MemoryStream();
                        chart.SaveImage(ms, ChartImageFormat.Png);
                        return File(ms.ToArray(), "image/png");
                    }
                }
            }
        }

        /*
string temp = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Chart>
  <ChartAreas>    
    <ChartArea>      
      <AxisY>        
        <MajorGrid />   
        <MajorTickMark /> 
        <LabelStyle />      
      </AxisY>      
      <AxisX>        
        <MajorGrid /> 
        <MajorTickMark  />   
        <LabelStyle />      
      </AxisX>     
     </ChartArea>  
    </ChartAreas>  
    <Legends>    
      <Legend _Template_=""All"" 
        Alignment=""Center"" 
        BackColor=""Transparent"" 
		ForeColor=""White""
        Docking=""Bottom"" 
        Font=""Arial, 24 px"" 
        IsTextAutoFit =""False"" 
        LegendStyle=""Row"">   
    </Legend>  
  </Legends> 
</Chart>";
         * */

        [HttpGet]
        public ActionResult Index()
        {
            using (InventarioEntities db = new InventarioEntities())
            {
                string user = User.Identity.GetUserName();
                var capsulas = db.Capsulas.Where(p => p.IdUsuario == user).ToList();

                List<CapsulasViewModel> model = new List<CapsulasViewModel>();
                foreach (var capsula in capsulas)
                {
                    int cantidad = capsula.CapsulasItems.Sum(s => s.Cantidad);

                    var capsulaV = new CapsulasViewModel();
                    capsulaV.IdCapsula = capsula.IdCapsula;
                    capsulaV.Total = cantidad;
                    capsulaV.Spawnable = ItemsXml.Any(p => p.ItemID == capsula.ItemID && p.PaysInterests);
                    capsulaV.Descripcion = capsula.Descripcion;

                    if (capsulaV.Descripcion == null)
                    {
                        if (!capsula.CapsulasItems.Any())
                        {
                            capsulaV.Descripcion = Resources.General.ResourceManager.GetString("Capsule_Empty");
                        }
                        else if (capsula.CapsulasItems.Count() == 1)
                        {
                            var itemIn = ItemsXml.Single(p => p.ItemID == capsula.CapsulasItems.Single().ItemID);
                            capsulaV.ItemEncapsulado = new ItemViewModel { CurrentItem = itemIn };
                        }
                        else if (capsula.CapsulasItems.Count() > 1)
                        {
                            capsulaV.Descripcion = string.Format("({0} items)", capsula.CapsulasItems.Count());
                        }
                    }

                    model.Add(capsulaV);
                }

                return View(model);
            }
        }

        [HttpGet]
        public ActionResult SendToUser(string id, string sendToUser)
        {
            using (InventarioEntities db = new InventarioEntities())
            {
                string user = User.Identity.GetUserName();
                var capsula = db.Capsulas.SingleOrDefault(p => p.IdUsuario == user && p.IdCapsula == id);

                if (capsula == null)
                {
                    return new HttpNotFoundResult();
                }

                CapsulasSendToViewModel model = new CapsulasSendToViewModel();
                model.IdCapsula = capsula.IdCapsula;
                model.UserID = sendToUser;
                model.Descripcion = capsula.Descripcion;

                if (model.Descripcion == null)
                {
                    if (!capsula.CapsulasItems.Any())
                    {
                        model.Descripcion = Resources.General.ResourceManager.GetString("Capsule_Empty");
                    }
                    else if (capsula.CapsulasItems.Count() == 1)
                    {
                        var itemIn = ItemsXml.Single(p => p.ItemID == capsula.CapsulasItems.Single().ItemID);
                        model.ItemEncapsulado = new ItemViewModel { CurrentItem = itemIn };
                    }
                    else if (capsula.CapsulasItems.Count() > 1)
                    {
                        model.Descripcion = string.Format("({0} items)", capsula.CapsulasItems.Count());
                    }
                }

                return View(model);
            }
        }

        [HttpPost]
        public ActionResult SendToUser(CapsulasSendToViewModel capsula)
        {
            using (InventarioEntities db = new InventarioEntities())
            {
                string user = User.Identity.GetUserName();
                var capsulaDB = db.Capsulas.SingleOrDefault(p => p.IdUsuario == user && p.IdCapsula == capsula.IdCapsula);

                if (capsulaDB == null)
                {
                    return new HttpNotFoundResult();
                }

                capsulaDB.IdUsuario = capsula.UserID;

                db.SaveChanges();

                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public ActionResult Add()
        {

            using (InventarioEntities db = new InventarioEntities())
            {
                var model = new CapsulasCreateViewModel();
                model.Capsulas = ItemsXml.Where(p => p.IsCapsule)
                    .Select(s => new
                    {
                        ItemID = s.ItemID,
                        Nombre = s.Nombre
                    }).ToDictionary(x => x.ItemID, y => y.Nombre);

                return View(model);
            }
        }
        public ActionResult Add(CapsulasCreateViewModel capsula)
        {
            using (InventarioEntities db = new InventarioEntities())
            {

                if (db.Capsulas.Any(p => p.IdCapsula == capsula.IdCapsula))
                {
                    ModelState.AddModelError("Duplicate", "There is another capsule with the same ID");
                }

                if (ModelState.IsValid)
                {
                    string user = User.Identity.GetUserName();

                    db.Capsulas.Add(new Capsulas { IdUsuario = user, IdCapsula = capsula.IdCapsula.ToUpper(), Descripcion = capsula.Descripcion, ItemID = capsula.ItemID });

                    db.SaveChanges();

                    return RedirectToAction("Index");
                }

                capsula.Capsulas = ItemsXml.Where(p => p.IsCapsule)
                    .Select(s => new
                    {
                        ItemID = s.ItemID,
                        Nombre = s.Nombre
                    }).ToDictionary(x => x.ItemID, y => y.Nombre);


                return View(capsula);
            }
        }

        [HttpGet]
        public ActionResult Edit(string id)
        {
            using (InventarioEntities db = new InventarioEntities())
            {
                string user = User.Identity.GetUserName();
                var capsulaDB = db.Capsulas.SingleOrDefault(p => p.IdCapsula == id && p.IdUsuario == user);

                if (capsulaDB == null)
                {
                    return new HttpNotFoundResult();
                }

                var model = new CapsulasEditViewModel();
                model.IdCapsula = capsulaDB.IdCapsula;
                model.Descripcion = capsulaDB.Descripcion;
                model.ItemID = capsulaDB.ItemID;
                model.Capsulas = ItemsXml.Where(p => p.IsCapsule)
                    .Select(s => new
                    {
                        ItemID = s.ItemID,
                        Nombre = s.Nombre
                    }).ToDictionary(x => x.ItemID, y => y.Nombre);

                return View(model);
            }
        }
        public ActionResult Edit(CapsulasEditViewModel capsula)
        {
            using (InventarioEntities db = new InventarioEntities())
            {
                string user = User.Identity.GetUserName();
                var capsulaDB = db.Capsulas.SingleOrDefault(p => p.IdCapsula == capsula.IdCapsula && p.IdUsuario == user);

                if (capsulaDB == null)
                {
                    return new HttpNotFoundResult();
                }

                if (ModelState.IsValid)
                {
                    capsulaDB.Descripcion = capsula.Descripcion;
                    capsulaDB.ItemID = capsula.ItemID;

                    db.SaveChanges();

                    return RedirectToAction("Index");
                }

                capsula.Capsulas = ItemsXml.Where(p => p.IsCapsule)
                    .Select(s => new
                    {
                        ItemID = s.ItemID,
                        Nombre = s.Nombre
                    }).ToDictionary(x => x.ItemID, y => y.Nombre);

                return View(capsula);
            }
        }

        [HttpGet]
        public ActionResult List(string id)
        {
            return RecuperarItems(id, null);

        }
        [HttpGet]
        public ActionResult ManageItems(string id)
        {
            return RecuperarItems(id, null);
        }

        [HttpGet]
        public ActionResult Unload(string id)
        {
            return RecuperarItemsUnload(id);
        }
        [HttpGet]
        public ActionResult Load(string id)
        {
            return RecuperarItemsLoad(id);
        }

        [HttpGet]
        public ActionResult LogInterests(string id)
        {
            return RecuperarItems(id, true);
        }

        [HttpGet]
        public ActionResult AddItem(string id)
        {
            if (id == null)
            {
                return new HttpNotFoundResult();
            }

            using (InventarioEntities db = new InventarioEntities())
            {
                string user = User.Identity.GetUserName();
                var capsula = db.Capsulas.SingleOrDefault(p => p.IdCapsula == id && p.IdUsuario == user);

                if (capsula == null)
                {
                    return new HttpNotFoundResult();
                }

                var itemsCargados = capsula.CapsulasItems.Select(p => p.ItemID).ToList();

                CapsulasAddItemViewModel model = new CapsulasAddItemViewModel();
                model.IdCapsula = id;
                model.Total = capsula.CapsulasItems.Sum(s => s.Cantidad);
                model.AddeableItems = ItemsXml.Where(p => !p.IsCapsule && !itemsCargados.Contains(p.ItemID))
                    .Select(q => new ItemViewModel
                    {
                        CurrentItem = q,
                    }).ToList();

                return View(model);
            }
        }

        [HttpPost]
        public ActionResult AddItem(CapsulasAddItemViewModel addedItem)
        {
            using (InventarioEntities db = new InventarioEntities())
            {
                string user = User.Identity.GetUserName();
                var capsula = db.Capsulas.SingleOrDefault(p => p.IdCapsula == addedItem.IdCapsula && p.IdUsuario == user);

                if (capsula == null)
                {
                    return new HttpNotFoundResult();
                }

                if (ModelState.IsValid)
                {
                    db.CapsulasItems.Add(new CapsulasItems { IdCapsula = addedItem.IdCapsula, ItemID = addedItem.ItemID, Cantidad = addedItem.Cantidad });

                    db.SaveChanges();

                    return RedirectToAction("List", new { id = addedItem.IdCapsula });
                }
                else
                {
                    var itemsCargados = capsula.CapsulasItems.Select(p => p.ItemID).ToList();

                    addedItem.Total = capsula.CapsulasItems.Sum(s => s.Cantidad);
                    addedItem.AddeableItems = ItemsXml.Where(p => !p.IsCapsule && !itemsCargados.Contains(p.ItemID))
                        .Select(q => new ItemViewModel
                        {
                            CurrentItem = q,
                        }).ToList();

                    return View(addedItem);
                }
            }
        }

        private ActionResult RecuperarItemsUnload(string id)
        {
            if (id == null)
            {
                return new HttpNotFoundResult();
            }

            using (InventarioEntities db = new InventarioEntities())
            {
                string user = User.Identity.GetUserName();
                var capsula = db.Capsulas.SingleOrDefault(p => p.IdCapsula == id && p.IdUsuario == user);

                if (capsula == null)
                {
                    return new HttpNotFoundResult();
                }

                CapsulaUnloadViewModel model = new CapsulaUnloadViewModel();
                model.IdCapsula = id;
                model.Descripcion = capsula.Descripcion;
                model.Total = capsula.CapsulasItems.Sum(s => s.Cantidad);
                model.Items = capsula.CapsulasItems.Select(p => new ItemUnloadViewModel
                    {
                        CurrentItem = ItemsXml.Single(z => z.ItemID == p.ItemID),
                        Cantidad = p.Cantidad,
                        CantidadDescargar = 0
                    }).OrderBy(n => n.CurrentItem.Order).ToList();

                return View(model);
            }
        }
        private ActionResult RecuperarItemsLoad(string id)
        {
            if (id == null)
            {
                return new HttpNotFoundResult();
            }

            using (InventarioEntities db = new InventarioEntities())
            {
                string user = User.Identity.GetUserName();
                var capsula = db.Capsulas.SingleOrDefault(p => p.IdCapsula == id && p.IdUsuario == user);

                if (capsula == null)
                {
                    return new HttpNotFoundResult();
                }

                CapsulaLoadViewModel model = new CapsulaLoadViewModel();
                model.IdCapsula = id;
                model.Descripcion = capsula.Descripcion;
                model.Total = capsula.CapsulasItems.Sum(s => s.Cantidad);

                var enCapsula = capsula.CapsulasItems.Select(q => new ItemLoadViewModel
                {
                    CurrentItem = ItemsXml.Single(z => z.ItemID == q.ItemID),
                    CantidadEnCapsula = q.Cantidad,
                    CantidadCargar = 0
                }).ToList();

                var esCapsula = ItemsXml.Where(p => p.IsCapsule).Select(s => s.ItemID);

                var inventariosDB = db.Inventarios.Where(p => p.IdUsuario == user && !esCapsula.Contains(p.ItemID))
                    .Select(q => new
                    {
                        ItemID = q.ItemID,
                        CantidadSuelta = q.Cantidad,
                        CantidadCargar = 0
                    }).ToList();

                var inventarios = inventariosDB.Select(p => new ItemLoadViewModel
                {
                    CurrentItem = ItemsXml.FirstOrDefault(z => z.ItemID == p.ItemID),
                    CantidadSuelta = p.CantidadSuelta,
                    CantidadCargar = p.CantidadCargar
                }).ToList();

                foreach (var item in enCapsula)
                {
                    var inv = inventarios.SingleOrDefault(p => p.CurrentItem.ItemID == item.CurrentItem.ItemID);
                    if (inv == null)
                    {
                        inventarios.Add(item);
                    }
                    else
                    {
                        inv.CantidadEnCapsula = item.CantidadEnCapsula;
                    }
                }

                model.Items = inventarios.OrderBy(p => p.CurrentItem.Order).ToList();

                return View(model);
            }
        }
        private ActionResult RecuperarItems(string id, bool? spawnable)
        {
            if (id == null)
            {
                return new HttpNotFoundResult();
            }

            using (InventarioEntities db = new InventarioEntities())
            {
                string user = User.Identity.GetUserName();

                var capsulasInterests = ItemsXml.Where(p => p.PaysInterests).Select(s => s.ItemID);

                var capsula = db.Capsulas.SingleOrDefault(p => p.IdCapsula == id && p.IdUsuario == user && (!spawnable.HasValue || capsulasInterests.Contains(p.ItemID)));

                if (capsula == null)
                {
                    return new HttpNotFoundResult();
                }

                CapsulasManageViewModel model = new CapsulasManageViewModel();
                model.IdCapsula = id;
                model.Descripcion = capsula.Descripcion;
                model.Total = capsula.CapsulasItems.Sum(s => s.Cantidad);
                model.Items = capsula.CapsulasItems.Select(p => new ItemInventoryViewModel
                    {
                        CurrentItem = ItemsXml.Single(z => z.ItemID == p.ItemID),
                        Cantidad = p.Cantidad
                    }).OrderBy(x => x.CurrentItem.Order).ToList();

                return View(model);
            }
        }

        [HttpPost]
        public ActionResult ManageItems(CapsulasManageViewModel capsula)
        {
            using (InventarioEntities db = new InventarioEntities())
            {
                string user = User.Identity.GetUserName();
                var capsulaDB = db.Capsulas.SingleOrDefault(p => p.IdCapsula == capsula.IdCapsula && p.IdUsuario == user);

                if (capsulaDB == null)
                {
                    return new HttpNotFoundResult();
                }

                if (capsula.Items != null)
                {
                    foreach (ItemInventoryViewModel item in capsula.Items)
                    {
                        string itemID = item.CurrentItem.ItemID;
                        CapsulasItems capsulaItem = db.CapsulasItems.SingleOrDefault(p => p.IdCapsula == capsula.IdCapsula && p.ItemID == itemID);

                        if (capsulaItem != null)
                        {
                            capsulaItem.Cantidad = item.Cantidad;
                        }
                    }

                    db.SaveChanges();
                }

                return RedirectToAction("List", new { id = capsula.IdCapsula });
            }
        }

        [HttpPost]
        public ActionResult Unload(CapsulaUnloadViewModel capsula)
        {
            if (!ModelState.IsValid)
            {
                return RecuperarItemsUnload(capsula.IdCapsula);
            }

            using (InventarioEntities db = new InventarioEntities())
            {
                string user = User.Identity.GetUserName();
                var capsulaDB = db.Capsulas.SingleOrDefault(p => p.IdCapsula == capsula.IdCapsula && p.IdUsuario == user);

                if (capsulaDB == null)
                {
                    return new HttpNotFoundResult();
                }

                if (capsula.Items != null)
                {
                    foreach (var item in capsula.Items.Where(p => p.CantidadDescargar > 0))
                    {
                        CapsulasItems capsulaItem = db.CapsulasItems.SingleOrDefault(p => p.IdCapsula == capsula.IdCapsula && p.ItemID == item.CurrentItem.ItemID);

                        if (capsulaItem != null)
                        {
                            if (capsulaItem.Cantidad == item.CantidadDescargar)
                            {
                                db.CapsulasItems.Remove(capsulaItem);
                            }
                            else
                            {
                                capsulaItem.Cantidad -= item.CantidadDescargar;
                            }

                            Inventarios inv = db.Inventarios.SingleOrDefault(p => p.IdUsuario == user && p.ItemID == item.CurrentItem.ItemID);

                            if (inv != null)
                            {
                                inv.Cantidad += item.CantidadDescargar;
                            }
                            else
                            {
                                db.Inventarios.Add(new Inventarios { IdUsuario = user, ItemID = item.CurrentItem.ItemID, Cantidad = item.CantidadDescargar });
                            }
                        }
                    }

                    db.SaveChanges();
                }

                return RedirectToAction("List", new { id = capsula.IdCapsula });
            }
        }

        [HttpPost]
        public ActionResult Load(CapsulaLoadViewModel capsula)
        {
            if (!ModelState.IsValid)
            {
                return View(capsula);
            }

            using (InventarioEntities db = new InventarioEntities())
            {
                string user = User.Identity.GetUserName();
                var capsulaDB = db.Capsulas.SingleOrDefault(p => p.IdCapsula == capsula.IdCapsula && p.IdUsuario == user);

                if (capsulaDB == null)
                {
                    return new HttpNotFoundResult();
                }

                if (capsula.Items != null)
                {

                    if (capsula.Items.Any(p => p.CantidadCargar > p.CantidadSuelta))
                    {
                        return View(capsula);
                    }

                    foreach (var item in capsula.Items.Where(p => p.CantidadCargar > 0))
                    {
                        CapsulasItems capsulaItem = db.CapsulasItems.SingleOrDefault(p => p.IdCapsula == capsula.IdCapsula && p.ItemID == item.CurrentItem.ItemID);

                        if (capsulaItem != null)
                        {
                            capsulaItem.Cantidad += item.CantidadCargar;
                        }
                        else
                        {
                            db.CapsulasItems.Add(new CapsulasItems { IdCapsula = capsula.IdCapsula, ItemID = item.CurrentItem.ItemID, Cantidad = item.CantidadCargar });
                        }

                        Inventarios inv = db.Inventarios.SingleOrDefault(p => p.IdUsuario == user && p.ItemID == item.CurrentItem.ItemID);

                        if (inv != null)
                        {
                            if (inv.Cantidad == item.CantidadCargar)
                            {
                                db.Inventarios.Remove(inv);
                            }
                            else
                            {
                                inv.Cantidad -= item.CantidadCargar;
                            }
                        }
                    }

                    db.SaveChanges();
                }

                return RedirectToAction("List", new { id = capsula.IdCapsula });
            }
        }

        [HttpPost]
        public ActionResult LogInterests(CapsulasManageViewModel capsula)
        {
            using (InventarioEntities db = new InventarioEntities())
            {
                string user = User.Identity.GetUserName();

                var capsulaDB = db.Capsulas.SingleOrDefault(p => p.IdCapsula == capsula.IdCapsula && p.IdUsuario == user);

                if (capsulaDB == null)
                {
                    return new HttpNotFoundResult();
                }

                if (capsula.Items == null)
                {
                    return RedirectToAction("List", new { id = capsula.IdCapsula });
                }

                if (capsula.Items.Sum(p => p.Cantidad) > 100)
                {
                    ModelState.AddModelError("Masde100", "Total Quantity on capsule exceeds 100.");
                    return LogInterests(capsula.IdCapsula);
                }

                Dictionary<string, int> v_nuevos = new Dictionary<string, int>();

                foreach (ItemInventoryViewModel item in capsula.Items)
                {
                    CapsulasItems capsulaItem = db.CapsulasItems.SingleOrDefault(p => p.IdCapsula == capsula.IdCapsula && p.ItemID == item.CurrentItem.ItemID);

                    if (capsulaItem != null)
                    {
                        if (capsulaItem.Cantidad > item.Cantidad)
                        {
                            ModelState.AddModelError("Masde100", "Quantity must be equal or greater than quantity on capsule.");
                            return LogInterests(capsula.IdCapsula);
                        }
                        else if (capsulaItem.Cantidad < item.Cantidad)
                        {
                            v_nuevos.Add(item.CurrentItem.ItemID, item.Cantidad - capsulaItem.Cantidad);
                            capsulaItem.Cantidad = item.Cantidad;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                if (v_nuevos.Count > 0)
                {
                    DateTime ahora = DateTime.Now;
                    DateTime diferencia = ahora.AddMinutes(-15);
                    Reproducciones v_ultimaCercana = db.Reproducciones.FirstOrDefault(p => p.IdUsuario == user && p.Fecha >= diferencia);
                    if (v_ultimaCercana != null)
                    {
                        ahora = v_ultimaCercana.Fecha;
                    }

                    Reproducciones v_repro = new Reproducciones { IdCapsula = capsula.IdCapsula, IdUsuario = user, Fecha = ahora };
                    foreach (var kv in v_nuevos)
                    {
                        v_repro.ReproduccionesItems.Add(new ReproduccionesItems { ItemID = kv.Key, Cantidad = kv.Value });
                    }
                    db.Reproducciones.Add(v_repro);

                    db.SaveChanges();

                    return RedirectToAction("List", new { id = capsula.IdCapsula });
                }

                return RedirectToAction("LogInterests", new { id = capsula.IdCapsula });
            }
        }

        [HttpGet]
        public ActionResult DeleteItem(string id, string itemID)
        {
            if (id == null || string.IsNullOrWhiteSpace(itemID))
            {
                return new HttpNotFoundResult();
            }

            using (InventarioEntities db = new InventarioEntities())
            {
                string user = User.Identity.GetUserName();

                var capsulaDB = db.Capsulas.SingleOrDefault(p => p.IdCapsula == id && p.IdUsuario == user);

                if (capsulaDB == null)
                {
                    return new HttpNotFoundResult();
                }

                var item = capsulaDB.CapsulasItems.SingleOrDefault(p => p.ItemID == itemID);

                if (item == null)
                {
                    return new HttpNotFoundResult();
                }

                ItemViewModel summary = new ItemViewModel
                {
                    CurrentItem = ItemsXml.Single(z => z.ItemID == item.ItemID),
                };

                return View(new CapsulasDeleteItemViewModel { IdCapsula = id, Total = capsulaDB.CapsulasItems.Sum(s => s.Cantidad), Item = summary, Cantidad = item.Cantidad });
            }
        }

        [HttpPost]
        public ActionResult DeleteItem(CapsulasDeleteItemViewModel element)
        {
            using (InventarioEntities db = new InventarioEntities())
            {
                string user = User.Identity.GetUserName();

                var capsulaDB = db.Capsulas.SingleOrDefault(p => p.IdCapsula == element.IdCapsula && p.IdUsuario == user);

                if (capsulaDB == null)
                {
                    return new HttpNotFoundResult();
                }

                var item = capsulaDB.CapsulasItems.SingleOrDefault(p => p.ItemID == element.Item.CurrentItem.ItemID);

                if (item == null)
                {
                    return new HttpNotFoundResult();
                }

                db.CapsulasItems.Remove(item);

                db.SaveChanges();

                return RedirectToAction("ManageItems", new { id = element.IdCapsula });
            }
        }

        [HttpGet]
        public ActionResult Delete(string id)
        {
            using (InventarioEntities db = new InventarioEntities())
            {
                string user = User.Identity.GetUserName();

                var capsula = db.Capsulas.SingleOrDefault(p => p.IdCapsula == id && p.IdUsuario == user);

                if (capsula == null)
                {
                    return new HttpNotFoundResult();
                }

                CapsulasDeleteViewModel model = new CapsulasDeleteViewModel();
                model.IdCapsula = id;
                model.Total = capsula.CapsulasItems.Sum(s => s.Cantidad);

                return View(model);
            }
        }

        [HttpPost]
        public ActionResult Delete(CapsulasDeleteViewModel element)
        {
            using (InventarioEntities db = new InventarioEntities())
            {
                string user = User.Identity.GetUserName();

                var capsula = db.Capsulas.SingleOrDefault(p => p.IdUsuario == user && p.IdCapsula == element.IdCapsula);

                if (capsula == null)
                {
                    return new HttpNotFoundResult();
                }

                while (capsula.CapsulasItems.Count > 0)
                {
                    db.CapsulasItems.Remove(capsula.CapsulasItems.First());
                }

                db.Capsulas.Remove(capsula);

                db.SaveChanges();

                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public ActionResult InterestsDateCapsule(DateGrouping grouping, string idCapsula)
        {
            DateTime inicio = DateTime.Now;

            using (InventarioEntities db = new InventarioEntities())
            {
                string user = User.Identity.GetUserName();

                var reproduccionesDB = db.ReproduccionesItems.Where(p => p.Reproducciones.IdUsuario == user && (string.IsNullOrEmpty(idCapsula) || p.Reproducciones.IdCapsula == idCapsula))
                    .GroupBy(r => new { Fecha = DbFunctions.TruncateTime(r.Reproducciones.Fecha).Value, IdCapsula = r.Reproducciones.IdCapsula, ItemID = r.ItemID })
                    .Select(s => new
                    {
                        IdCapsula = s.Key.IdCapsula,
                        Fecha = s.Key.Fecha,
                        ItemID = s.Key.ItemID,
                        Cantidad = s.Sum(m => m.Cantidad),
                    }).ToList();

                var filas = reproduccionesDB
                            .GroupBy(r => new { r.IdCapsula, Fecha = GetResolvedDate(grouping, r.Fecha) })
                            .Select(s => new CapsulaFechaTotalViewModel
                            {
                                IdCapsula = s.Key.IdCapsula,
                                Fecha = s.Key.Fecha,
                                Total = s.Sum(p => p.Cantidad),
                                Items = s.GroupBy(u => u.ItemID).Select(v => new ItemInventoryViewModel { Cantidad = v.Sum(w => w.Cantidad), CurrentItem = ItemsXml.Single(p => p.ItemID == v.First().ItemID) })
                            }).ToList();

                var totales = reproduccionesDB
                    .GroupBy(r => r.IdCapsula)
                    .Select(s => new CapsulaFechaTotalViewModel
                    {
                        IdCapsula = s.Key,
                        Fecha = DateTime.MinValue,
                        Total = s.Sum(p => p.Cantidad),
                        Items = s.GroupBy(u => u.ItemID).Select(v => new ItemInventoryViewModel { Cantidad = v.Sum(w => w.Cantidad), CurrentItem = ItemsXml.Single(p => p.ItemID == v.First().ItemID) })
                    }).ToList();

                var model = new CapsulasInterestsByDateViewModel();
                model.Grouping = grouping;
                model.Capsulas = reproduccionesDB.Select(p => p.IdCapsula).Distinct().ToList();
                model.DateInfo = reproduccionesDB.GroupBy(p => GetResolvedDate(grouping, p.Fecha)).Select(q => new DateInfoModel { Fecha = q.Key, TotalCapsules = q.Select(z => z.IdCapsula).Distinct().Count(), TotalItems = q.Sum(r => r.Cantidad), RealDays = (int)(q.Max(h => h.Fecha) - q.Min(i => i.Fecha)).TotalDays + 1 }).OrderByDescending(b => b.Fecha);
                model.Filas = filas;
                model.Totales = totales;

                TimeSpan tiempo = DateTime.Now.Subtract(inicio);

                return View(model);
            }
        }

        [HttpGet]
        public ActionResult InterestsDateTotal(DateGrouping? grouping)
        {
            if (!grouping.HasValue)
            {
                grouping = DateGrouping.Week;
            }

            return View(grouping.Value);
        }

        [HttpGet]
        public ActionResult _InterestsDateTotal(DateGrouping grouping)
        {
            DateTime inicio = DateTime.Now;

            using (InventarioEntities db = new InventarioEntities())
            {
                string user = User.Identity.GetUserName();

                var reproduccionesDB = db.ReproduccionesItems.Where(p => p.Reproducciones.IdUsuario == user)
                    .GroupBy(r => new { Fecha = DbFunctions.TruncateTime(r.Reproducciones.Fecha).Value, IdCapsula = r.Reproducciones.IdCapsula, ItemID = r.ItemID })
                    .Select(s => new
                    {
                        IdCapsula = s.Key.IdCapsula,
                        Fecha = s.Key.Fecha,
                        ItemID = s.Key.ItemID,
                        Cantidad = s.Sum(m => m.Cantidad),
                    }).ToList();

                var items = reproduccionesDB
                            .GroupBy(r => new { Fecha = GetResolvedDate(grouping, r.Fecha) })
                            .Select(s => new DateInfoTotalModel
                            {
                                Fecha = s.Key.Fecha,
                                TotalCapsules = s.Select(h => h.IdCapsula).Distinct().Count(),
                                TotalItems = s.Sum(p => p.Cantidad),
                                RealDays = (int)(s.Max(h => h.Fecha) - s.Min(i => i.Fecha)).TotalDays + 1,
                                Items = s.GroupBy(u => u.ItemID).Select(v => new { ItemID = v.Key, Cantidad = v.Sum(w => w.Cantidad) }).ToDictionary(m => m.ItemID, n => n.Cantidad)
                            }).ToList();

                var differentItems = reproduccionesDB.Select(h => h.ItemID).Distinct().Select(i => new ItemViewModel { CurrentItem = ItemsXml.Single(j => j.ItemID == i) }).OrderBy(k => k.CurrentItem.Order);

                var totals = reproduccionesDB
                 .GroupBy(q => q.ItemID)
                 .Select(i => new ItemInventoryViewModel { CurrentItem = ItemsXml.Single(j => j.ItemID == i.Key), Cantidad = i.Sum(j => j.Cantidad) }).OrderBy(k => k.CurrentItem.Order);

                var maximos = reproduccionesDB
                            .GroupBy(r => new { Fecha = GetResolvedDate(grouping, r.Fecha), ItemID = r.ItemID })
                            .Select(s => new { ItemID = s.Key.ItemID, Cantidad = s.Sum(p => p.Cantidad) })
                            .GroupBy(h => h.ItemID)
                            .Select(i => new { ItemID = i.Key, Maximo = i.Max(j => j.Cantidad) })
                            .ToDictionary(h => h.ItemID, i => i.Maximo);

                var model = new CapsulasInterestsByDateTotalViewModel();
                model.Grouping = grouping;
                model.DateInfo = items.OrderByDescending(b => b.Fecha);
                model.Totals = totals;
                model.Maximos = maximos;
                model.TotalItems = totals.Sum(p => p.Cantidad);

                TimeSpan tiempo = DateTime.Now.Subtract(inicio);

                return PartialView("_InterestsDateTotal", model);
            }
        }

        [HttpGet]
        public ActionResult InterestsPercentageRarity(DateGrouping? grouping, bool? percentage)
        {
            if (!grouping.HasValue)
            {
                grouping = DateGrouping.Week;
            }
            if (!percentage.HasValue)
            {
                percentage = false;
            }
            return View(new InterestsPercentageRarityViewModel() { Grouping = grouping.Value, Percentage = percentage.Value });
        }

        [HttpGet]
        public FileResult InterestsPercentageRarityChart(DateGrouping grouping, bool percentage)
        {
            DateTime inicio = DateTime.Now;

            var type = SeriesChartType.StackedColumn;
            if (percentage)
            {
                type = SeriesChartType.StackedColumn100;
            }


            using (InventarioEntities db = new InventarioEntities())
            {
                string user = User.Identity.GetUserName();

                string dateFormat = Resources.General.ResourceManager.GetString(string.Format("InterestsChart_DateFormat_{0}", grouping.ToString()));

                var rarityDef = new[] { 
                        new { Rarity = "R1", Name = "Very Common", Color="#b5b2b5" }, 
                        new { Rarity = "R2", Name = "Common", Color="#84f7b5" }, 
                        new { Rarity = "R3", Name = "Rare", Color="#ad8eff" }, 
                        new { Rarity = "R4", Name = "Very Rare", Color="#ff8ef7" } 
                    };

                var dbData = db.ReproduccionesItems.Where(p => p.Reproducciones.IdUsuario == user)
                                    .GroupBy(r => new { Fecha = DbFunctions.TruncateTime(r.Reproducciones.Fecha).Value, ItemID = r.ItemID })
                                    .Select(s => new
                                    {
                                        Fecha = s.Key.Fecha,
                                        ItemID = s.Key.ItemID,
                                        Cantidad = s.Sum(m => m.Cantidad),
                                    }).ToList()
                                    .GroupBy(r => new { Rarity = ItemsXml.Single(j => j.ItemID == r.ItemID).Rarity, Fecha = GetResolvedDate(grouping, r.Fecha) })
                                    .Select(s => new
                                    {
                                        Rarity = s.Key.Rarity,
                                        Date = s.Key.Fecha,
                                        Qty = s.Sum(w => w.Cantidad)
                                    });

                var dates = dbData.Select(p => GetResolvedDate(grouping, p.Date)).Distinct().ToArray();
                var rarityies = dbData.Select(p => p.Rarity).Distinct().ToArray();

                var points = from date in dates
                             from rarity in rarityies
                             select new
                             {
                                 Rarity = rarity,
                                 Date = date
                             };

                var matrix = from point in points
                             join data in dbData on new { point.Rarity, point.Date } equals new { data.Rarity, data.Date } into joineddbData
                             from subData in joineddbData.DefaultIfEmpty()
                             select new
                             {
                                 Rarity = point.Rarity,
                                 Date = point.Date,
                                 Qty = subData == null ? 0 : subData.Qty
                             };

                var itemsQtyDate = matrix
                                .OrderByDescending(a => a.Rarity)
                                .ThenBy(d => d.Date)
                                .GroupBy(b => b.Rarity)
                                .Select(c => new
                                {
                                    Rarity = c.Key,
                                    Name = rarityDef.Single(p => p.Rarity == c.Key).Name,
                                    Color = System.Drawing.ColorTranslator.FromHtml(rarityDef.Single(p => p.Rarity == c.Key).Color),
                                    Fechas = c.Select(o => o.Date.ToString(dateFormat)).ToArray(),
                                    Valores = c.Select(o => o.Qty).ToArray()
                                }
                                );


                PrivateFontCollection pfc = new PrivateFontCollection();
                pfc.AddFontFile(Server.MapPath("~/Content/Coda-Regular.ttf"));

                using (Chart chart = new Chart())
                {
                    chart.Font.Name = pfc.Families[0].Name;
                    chart.Font.Size = FontUnit.Point(8);
                    chart.Width = 1000;
                    chart.Height = 400;
                    chart.BackColor = Color.FromArgb(255, 0x27, 0x2B, 0x30);
                    chart.BorderlineDashStyle = ChartDashStyle.Solid;
                    chart.BorderlineColor = Color.Gray;
                    chart.BorderlineWidth = 1;
                    Legend legend = new Legend();
                    legend.BackColor = Color.Transparent;
                    legend.ForeColor = Color.White;
                    chart.Legends.Add(legend);
                    chart.Palette = ChartColorPalette.None;
                    chart.PaletteCustomColors = itemsQtyDate.Select(p => p.Color).ToArray();

                    using (Font fuente = new Font(pfc.Families[0], 8, GraphicsUnit.Point))
                    {
                        ChartArea area = new ChartArea();
                        area.BackColor = Color.Transparent;
                        area.ShadowColor = Color.Transparent;
                        area.BorderColor = Color.FromArgb(255, 0x88, 0x88, 0x88);
                        area.BorderDashStyle = ChartDashStyle.Solid;

                        var ejeX = area.AxisX;
                        ejeX.LabelStyle.Font = fuente;
                        ejeX.LabelStyle.ForeColor = Color.White;
                        ejeX.LineColor = Color.FromArgb(255, 0x99, 0x99, 0x99);
                        ejeX.IsLabelAutoFit = false;
                        ejeX.IsMarginVisible = true;
                        ejeX.MajorGrid.LineColor = Color.FromArgb(255, 0x99, 0x99, 0x99);
                        ejeX.MajorTickMark.LineColor = Color.FromArgb(255, 0xAA, 0xAA, 0xAA);

                        var interval = Math.Max(dates.Count() / 25, 1);
                        ejeX.Interval = interval;
                        ejeX.LabelStyle.Angle = -90;

                        var ejeY = area.AxisY;
                        ejeY.LabelStyle.Font = fuente;
                        ejeY.LabelStyle.ForeColor = Color.White;
                        ejeY.LineColor = Color.FromArgb(255, 0x99, 0x99, 0x99);
                        ejeY.IsLabelAutoFit = false;
                        ejeY.IsMarginVisible = true;
                        ejeY.MajorGrid.LineColor = Color.FromArgb(255, 0x99, 0x99, 0x99);
                        ejeY.MajorTickMark.LineColor = Color.FromArgb(255, 0xAA, 0xAA, 0xAA);

                        chart.ChartAreas.Add(area);

                        foreach (var item in itemsQtyDate)
                        {
                            Series itemsSerie = new Series(item.Name);
                            itemsSerie.Font = fuente;
                            itemsSerie.ChartType = type;
                            itemsSerie.Points.DataBindXY(item.Fechas, item.Valores);
                            itemsSerie.IsValueShownAsLabel = grouping != DateGrouping.Day;
                            chart.Series.Add(itemsSerie);
                        }

                        MemoryStream ms = new MemoryStream();
                        chart.SaveImage(ms, ChartImageFormat.Png);
                        return File(ms.ToArray(), "image/png");
                    }
                }

            }
        }

        private DateTime GetResolvedDate(DateGrouping grouping, DateTime fecha)
        {
            switch (grouping)
            {
                case DateGrouping.Week:
                    return fecha.AddDays(-(int)fecha.DayOfWeek).Date;
                case DateGrouping.Month:
                    return new DateTime(fecha.Year, fecha.Month, 1);
                case DateGrouping.Year:
                    return new DateTime(fecha.Year, 1, 1);
                case DateGrouping.Day:
                default:
                    return new DateTime(fecha.Year, fecha.Month, fecha.Day);
            }
        }

        [HttpGet]
        public ActionResult LastSpawn()
        {
            using (InventarioEntities db = new InventarioEntities())
            {
                string user = User.Identity.GetUserName();

                DateTime fechaMax = db.Reproducciones.Where(p => p.IdUsuario == user).DefaultIfEmpty().Max(q => q == null ? DateTime.MinValue : q.Fecha);

                CapsulasLastSpawnViewModel model = new CapsulasLastSpawnViewModel();
                model.Total = 0;

                if (fechaMax == DateTime.MinValue)
                {
                    return View(model);
                }

                model.Fecha = fechaMax;
                var reproducciones = db.ReproduccionesItems
                    .Where(p => p.Reproducciones.Fecha == fechaMax && p.Reproducciones.IdUsuario == user)
                    .GroupBy(q => new { q.Reproducciones.IdCapsula, q.ItemID })
                    .Select(s => new
                             {
                                 IdCapsula = s.Key.IdCapsula,
                                 ItemID = s.Key.ItemID,
                                 Cantidad = s.Sum(t => t.Cantidad)
                             }).ToList();
                model.Capsulas = reproducciones
                    .GroupBy(p => p.IdCapsula)
                    .Select(s => new CapsulaContenidoViewModel
                             {
                                 IdCapsula = s.Key,
                                 Cantidad = s.Count(),
                                 Contenidos = s.OrderBy(x => x.ItemID).Select(t => new ItemInventoryViewModel
                                 {
                                     CurrentItem = ItemsXml.Single(z => z.ItemID == t.ItemID),
                                     Cantidad = t.Cantidad
                                 })
                             }).ToList();

                model.Totales = reproducciones
                    .GroupBy(t => t.ItemID)
                    .OrderBy(x => x.Key)
                    .Select(q => new ItemInventoryViewModel
                    {
                        CurrentItem = ItemsXml.Single(z => z.ItemID == q.Key),
                        Cantidad = q.Sum(r => r.Cantidad)
                    });

                model.Total = reproducciones.Sum(p => p.Cantidad);

                return View(model);
            }
        }

        [HttpGet]
        public ActionResult InterestsTotalCapsule(string id)
        {
            using (InventarioEntities db = new InventarioEntities())
            {
                string user = User.Identity.GetUserName();

                var reproducciones = db.ReproduccionesItems.Where(p => (id == null || p.Reproducciones.IdCapsula == id) && p.Reproducciones.IdUsuario == user)
                            .OrderBy(q => q.ItemID)
                             .Select(s => new
                             {
                                 IdCapsula = s.Reproducciones.IdCapsula,
                                 ItemID = s.ItemID,
                                 Cantidad = s.Cantidad,
                             }).ToList();
                var model = reproducciones.GroupBy(r => r.IdCapsula)
                             .Select(s => new CapsulaFechaTotalViewModel
                             {
                                 IdCapsula = s.Key,
                                 Total = s.Sum(p => p.Cantidad),
                                 Items = s.GroupBy(u => u.ItemID).Select(v => new ItemInventoryViewModel { Cantidad = v.Sum(w => w.Cantidad), CurrentItem = ItemsXml.Single(p => p.ItemID == v.First().ItemID) })
                             }).ToList();

                return View(model);
            }
        }

        [HttpGet]
        public ActionResult InterestsTotalByItem()
        {
            using (InventarioEntities db = new InventarioEntities())
            {
                string user = User.Identity.GetUserName();


                var totalesDB = db.Reproducciones.Where(p => p.IdUsuario == user)
                    .SelectMany(p => p.ReproduccionesItems)
                    .GroupBy(p => p.ItemID)
                    .Select(q => new
                    {
                        ItemID = q.Key,
                        Cantidad = q.Sum(r => r.Cantidad)
                    }).ToList();

                var totales = totalesDB.Select(p => new ItemInventoryViewModel
                {
                    CurrentItem = ItemsXml.Single(z => z.ItemID == p.ItemID),
                    Cantidad = p.Cantidad
                }).ToList();

                return View(totales);
            }
        }
    }
}