using MiInventario.Models.Interests;
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

namespace MiInventario.Controllers {
  [Authorize]
  public class InterestsController : BaseController {
    [HttpGet]
    public ActionResult DateCharts() {
      using (InventarioEntities db = new InventarioEntities()) {
        string user = User.Identity.GetUserName();

        var reproduccionesDB = db.ReproduccionesItems
            .Where(p => p.Reproducciones.IdUsuario == user)
            .Select(s => s.ItemID)
            .Distinct()
            .ToList();

        ChartsViewModel model = new ChartsViewModel();
        model.Grouping = DateGrouping.Week;
        model.Accumulate = true;
        model.ViewableItems = ItemsXml.Where(p => reproduccionesDB.Contains(p.ItemID)).Select(q => new { q.ItemID, Description = q.Description() }).ToDictionary(r => r.ItemID, s => s.Description);

        return View(model);
      }
    }

    [HttpGet]
    public ActionResult _ChartTitle(DateGrouping grouping, string itemID, bool accumulative) {
      var model = new ChartTitleViewModel();
      model.Grouping = grouping;
      model.Item = new Models.ItemViewModel() { CurrentItem = ItemsXml.SingleOrDefault(p => p.ItemID == itemID) };
      model.Accumulative = accumulative;

      return PartialView("_ChartTitle", model);
    }

    [HttpGet]
    public ActionResult DateItemChart(DateGrouping grouping, string itemID, bool accumulate) {
      DateTime inicio = DateTime.Now;

      using (InventarioEntities db = new InventarioEntities()) {
        string user = User.Identity.GetUserName();

        var reproduccionesDB = db.ReproduccionesItems.Where(p => p.Reproducciones.IdUsuario == user)
            .GroupBy(z => new { z.Reproducciones.Fecha, z.Reproducciones.IdCapsula })
            .Select(s => new {
              IdCapsula = s.Key.IdCapsula,
              Fecha = s.Key.Fecha,
              Items = s.GroupBy(x => x.ItemID).Select(y => new { ItemID = y.Key, Cantidad = y.Sum(t => t.Cantidad) }),
            }).ToList();

        var model = new ByDateViewModel();
        model.Grouping = grouping;
        model.DateInfo = reproduccionesDB
                    .GroupBy(r => r.Fecha.GetResolvedDate(grouping))
                    .Select(s => new DateInfoModel {
                      Fecha = s.Key,
                      TotalCapsules = s.Where(n => n.Items.Any(q => string.IsNullOrEmpty(itemID) || q.ItemID == itemID)).Select(z => z.IdCapsula).Distinct().Count(),
                      DifferentItems = s.SelectMany(n => n.Items).Where(b => string.IsNullOrEmpty(itemID) || b.ItemID == itemID).Select(h => h.ItemID).Distinct().Count(),
                      TotalItems = s.SelectMany(n => n.Items).Where(b => string.IsNullOrEmpty(itemID) || b.ItemID == itemID).DefaultIfEmpty().Sum(p => p == null ? 0 : p.Cantidad)
                    }).OrderBy(i => i.Fecha).ToList();

        if (accumulate) {
          int actualItemQty = 0;
          int actualCapsQty = 0;
          foreach (DateInfoModel info in model.DateInfo) {
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
    public ActionResult InterestsChart(DateGrouping grouping, string itemID, bool accumulate) {
      DateTime inicio = DateTime.Now;

      using (InventarioEntities db = new InventarioEntities()) {
        string user = User.Identity.GetUserName();

        var reproduccionesDB = db.ReproduccionesItems.Where(p => p.Reproducciones.IdUsuario == user)
            .GroupBy(z => new { z.Reproducciones.Fecha, z.Reproducciones.IdCapsula })
            .Select(s => new {
              IdCapsula = s.Key.IdCapsula,
              Fecha = s.Key.Fecha,
              Items = s.GroupBy(x => x.ItemID).Select(y => new { ItemID = y.Key, Cantidad = y.Sum(t => t.Cantidad) }),
            }).ToList();

        var model = new ByDateViewModel();
        model.Grouping = grouping;
        model.DateInfo = reproduccionesDB
                    .GroupBy(r => r.Fecha.GetResolvedDate(grouping))
                    .Select(s => new DateInfoModel {
                      Fecha = s.Key,
                      TotalCapsules = s.Where(n => n.Items.Any(q => string.IsNullOrEmpty(itemID) || q.ItemID == itemID)).Select(z => z.IdCapsula).Distinct().Count(),
                      DifferentItems = s.SelectMany(n => n.Items).Where(b => string.IsNullOrEmpty(itemID) || b.ItemID == itemID).Select(h => h.ItemID).Distinct().Count(),
                      TotalItems = s.SelectMany(n => n.Items).Where(b => string.IsNullOrEmpty(itemID) || b.ItemID == itemID).DefaultIfEmpty().Sum(p => p == null ? 0 : p.Cantidad)
                    }).OrderBy(i => i.Fecha).ToList();

        if (accumulate) {
          int actualItemQty = 0;
          int actualCapsQty = 0;
          foreach (DateInfoModel info in model.DateInfo) {
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

        using (Chart chart = new Chart()) {
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

          using (Font fuente = new Font(pfc.Families[0], 8, GraphicsUnit.Point)) {
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
    public ActionResult DateCapsule(DateGrouping grouping, string idCapsula) {
      DateTime inicio = DateTime.Now;

      using (InventarioEntities db = new InventarioEntities()) {
        string user = User.Identity.GetUserName();

        var reproduccionesDB = db.ReproduccionesItems.Where(p => p.Reproducciones.IdUsuario == user && (string.IsNullOrEmpty(idCapsula) || p.Reproducciones.IdCapsula == idCapsula))
            .GroupBy(r => new { Fecha = DbFunctions.TruncateTime(r.Reproducciones.Fecha).Value, IdCapsula = r.Reproducciones.IdCapsula, ItemID = r.ItemID })
            .Select(s => new {
              IdCapsula = s.Key.IdCapsula,
              Fecha = s.Key.Fecha,
              ItemID = s.Key.ItemID,
              Cantidad = s.Sum(m => m.Cantidad),
            }).ToList();

        var filas = reproduccionesDB
                    .GroupBy(r => new { r.IdCapsula, Fecha = r.Fecha.GetResolvedDate(grouping) })
                    .Select(s => new FechaTotalViewModel {
                      IdCapsula = s.Key.IdCapsula,
                      Fecha = s.Key.Fecha,
                      Total = s.Sum(p => p.Cantidad),
                      Items = s.GroupBy(u => u.ItemID).Select(v => new Models.ItemInventoryViewModel { Cantidad = v.Sum(w => w.Cantidad), CurrentItem = ItemsXml.Single(p => p.ItemID == v.First().ItemID) })
                    }).ToList();

        var totales = reproduccionesDB
            .GroupBy(r => r.IdCapsula)
            .Select(s => new FechaTotalViewModel {
              IdCapsula = s.Key,
              Fecha = DateTime.MinValue,
              Total = s.Sum(p => p.Cantidad),
              Items = s.GroupBy(u => u.ItemID).Select(v => new Models.ItemInventoryViewModel { Cantidad = v.Sum(w => w.Cantidad), CurrentItem = ItemsXml.Single(p => p.ItemID == v.First().ItemID) })
            }).ToList();

        var model = new ByDateViewModel();
        model.Grouping = grouping;
        model.Capsulas = reproduccionesDB.Select(p => p.IdCapsula).Distinct().ToList();
        model.DateInfo = reproduccionesDB.GroupBy(p => p.Fecha.GetResolvedDate(grouping)).Select(q => new DateInfoModel { Fecha = q.Key, TotalCapsules = q.Select(z => z.IdCapsula).Distinct().Count(), TotalItems = q.Sum(r => r.Cantidad), RealDays = (int)(q.Max(h => h.Fecha) - q.Min(i => i.Fecha)).TotalDays + 1 }).OrderByDescending(b => b.Fecha);
        model.Filas = filas;
        model.Totales = totales;

        TimeSpan tiempo = DateTime.Now.Subtract(inicio);

        return View(model);
      }
    }

    [HttpGet]
    public ActionResult DateTotal(DateGrouping? grouping, int? periods) {
      if (!grouping.HasValue) {
        grouping = DateGrouping.Week;
      }
      if (!periods.HasValue || periods.Value < 1) {
        periods = 4;
      }

      return View(new DateTotalViewModel { Grouping = grouping.Value, Periods = periods.Value });
    }

    [HttpGet]
    public ActionResult _DateTotal(DateGrouping grouping, int periods) {
      DateTime startDate;

      if (periods < 1) {
        periods = 4;
      }

      switch (grouping) {
        case DateGrouping.Year:
          startDate = DateTime.Now.Date.GetResolvedDate(DateGrouping.Year).AddYears(1 - periods);
          break;
        case DateGrouping.Month:
          startDate = DateTime.Now.Date.GetResolvedDate(DateGrouping.Month).AddMonths(1 - periods);
          break;
        case DateGrouping.Week:
          startDate = DateTime.Now.Date.GetResolvedDate(DateGrouping.Week).AddDays((1-periods)*7);
          break;
        case DateGrouping.Day:
        default:
          startDate = DateTime.Now.Date.AddDays(1 - periods);
          break;
      }

      using (InventarioEntities db = new InventarioEntities()) {
        string user = User.Identity.GetUserName();

        var reproduccionesDB = db.ReproduccionesItems.Where(p => p.Reproducciones.IdUsuario == user && DbFunctions.TruncateTime(p.Reproducciones.Fecha).Value>=startDate)
            .GroupBy(r => new { Fecha = DbFunctions.TruncateTime(r.Reproducciones.Fecha).Value, IdCapsula = r.Reproducciones.IdCapsula, ItemID = r.ItemID })
            .Select(s => new {
              IdCapsula = s.Key.IdCapsula,
              Fecha = s.Key.Fecha,
              ItemID = s.Key.ItemID,
              Cantidad = s.Sum(m => m.Cantidad),
            }).ToList();

        var items = reproduccionesDB
                    .GroupBy(r => new { Fecha = r.Fecha.GetResolvedDate(grouping) })
                    .Select(s => new DateInfoTotalModel {
                      Fecha = s.Key.Fecha,
                      TotalCapsules = s.Select(h => h.IdCapsula).Distinct().Count(),
                      TotalItems = s.Sum(p => p.Cantidad),
                      RealDays = (int)(s.Max(h => h.Fecha) - s.Min(i => i.Fecha)).TotalDays + 1,
                      Items = s.GroupBy(u => u.ItemID).Select(v => new { ItemID = v.Key, Cantidad = v.Sum(w => w.Cantidad) }).ToDictionary(m => m.ItemID, n => n.Cantidad)
                    }).ToList();

        var differentItems = reproduccionesDB.Select(h => h.ItemID).Distinct().Select(i => new Models.ItemViewModel { CurrentItem = ItemsXml.Single(j => j.ItemID == i) }).OrderBy(k => k.CurrentItem.Order);

        var totals = reproduccionesDB
         .GroupBy(q => q.ItemID)
         .Select(i => new Models.ItemInventoryViewModel { CurrentItem = ItemsXml.Single(j => j.ItemID == i.Key), Cantidad = i.Sum(j => j.Cantidad) }).OrderBy(k => k.CurrentItem.Order);

        var maximos = reproduccionesDB
                    .GroupBy(r => new { Fecha = r.Fecha.GetResolvedDate(grouping), ItemID = r.ItemID })
                    .Select(s => new { ItemID = s.Key.ItemID, Cantidad = s.Sum(p => p.Cantidad) })
                    .GroupBy(h => h.ItemID)
                    .Select(i => new { ItemID = i.Key, Maximo = i.Max(j => j.Cantidad) })
                    .ToDictionary(h => h.ItemID, i => i.Maximo);

        var model = new ByDateTotalViewModel();
        model.Grouping = grouping;
        model.DateInfo = items.OrderByDescending(b => b.Fecha);
        model.Totals = totals;
        model.Maximos = maximos;
        model.TotalItems = totals.Sum(p => p.Cantidad);

        return PartialView("_DateTotal", model);
      }
    }

    [HttpGet]
    public ActionResult PercentageRarity(DateGrouping? grouping, bool? percentage) {
      if (!grouping.HasValue) {
        grouping = DateGrouping.Week;
      }
      if (!percentage.HasValue) {
        percentage = false;
      }
      return View(new PercentageRarityViewModel() { Grouping = grouping.Value, Percentage = percentage.Value });
    }

    [HttpGet]
    public FileResult PercentageRarityChart(DateGrouping grouping, bool percentage) {
      DateTime inicio = DateTime.Now;

      var type = SeriesChartType.StackedColumn;
      if (percentage) {
        type = SeriesChartType.StackedColumn100;
      }


      using (InventarioEntities db = new InventarioEntities()) {
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
                            .Select(s => new {
                              Fecha = s.Key.Fecha,
                              ItemID = s.Key.ItemID,
                              Cantidad = s.Sum(m => m.Cantidad),
                            }).ToList()
                            .GroupBy(r => new { Rarity = ItemsXml.Single(j => j.ItemID == r.ItemID).Rarity, Fecha = r.Fecha.GetResolvedDate(grouping) })
                            .Select(s => new {
                              Rarity = s.Key.Rarity,
                              Date = s.Key.Fecha,
                              Qty = s.Sum(w => w.Cantidad)
                            });

        var dates = dbData.Select(p => p.Date.GetResolvedDate(grouping)).Distinct().ToArray();
        var rarityies = dbData.Select(p => p.Rarity).Distinct().ToArray();

        var points = from date in dates
                     from rarity in rarityies
                     select new {
                       Rarity = rarity,
                       Date = date
                     };

        var matrix = from point in points
                     join data in dbData on new { point.Rarity, point.Date } equals new { data.Rarity, data.Date } into joineddbData
                     from subData in joineddbData.DefaultIfEmpty()
                     select new {
                       Rarity = point.Rarity,
                       Date = point.Date,
                       Qty = subData == null ? 0 : subData.Qty
                     };

        var itemsQtyDate = matrix
                        .OrderByDescending(a => a.Rarity)
                        .ThenBy(d => d.Date)
                        .GroupBy(b => b.Rarity)
                        .Select(c => new {
                          Rarity = c.Key,
                          Name = rarityDef.Single(p => p.Rarity == c.Key).Name,
                          Color = System.Drawing.ColorTranslator.FromHtml(rarityDef.Single(p => p.Rarity == c.Key).Color),
                          Fechas = c.Select(o => o.Date.ToString(dateFormat)).ToArray(),
                          Valores = c.Select(o => o.Qty).ToArray()
                        }
                        );


        PrivateFontCollection pfc = new PrivateFontCollection();
        pfc.AddFontFile(Server.MapPath("~/Content/Coda-Regular.ttf"));

        using (Chart chart = new Chart()) {
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

          using (Font fuente = new Font(pfc.Families[0], 8, GraphicsUnit.Point)) {
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

            foreach (var item in itemsQtyDate) {
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

    [HttpGet]
    public ActionResult LastSpawn() {
      using (InventarioEntities db = new InventarioEntities()) {
        string user = User.Identity.GetUserName();

        DateTime fechaMax = db.Reproducciones.Where(p => p.IdUsuario == user).DefaultIfEmpty().Max(q => q == null ? DateTime.MinValue : q.Fecha);

        LastSpawnViewModel model = new LastSpawnViewModel();
        model.Total = 0;

        if (fechaMax == DateTime.MinValue) {
          return View(model);
        }

        model.Fecha = fechaMax;
        var reproducciones = db.ReproduccionesItems
            .Where(p => p.Reproducciones.Fecha == fechaMax && p.Reproducciones.IdUsuario == user)
            .GroupBy(q => new { q.Reproducciones.IdCapsula, q.ItemID })
            .Select(s => new {
              IdCapsula = s.Key.IdCapsula,
              ItemID = s.Key.ItemID,
              Cantidad = s.Sum(t => t.Cantidad)
            }).ToList();
        model.Capsulas = reproducciones
            .GroupBy(p => p.IdCapsula)
            .Select(s => new Models.Capsules.ContenidoViewModel {
              IdCapsula = s.Key,
              Cantidad = s.Count(),
              Contenidos = s.OrderBy(x => x.ItemID).Select(t => new Models.ItemInventoryViewModel {
                CurrentItem = ItemsXml.Single(z => z.ItemID == t.ItemID),
                Cantidad = t.Cantidad
              })
            }).ToList();

        model.Totales = reproducciones
            .GroupBy(t => t.ItemID)
            .OrderBy(x => x.Key)
            .Select(q => new Models.ItemInventoryViewModel {
              CurrentItem = ItemsXml.Single(z => z.ItemID == q.Key),
              Cantidad = q.Sum(r => r.Cantidad)
            });

        model.Total = reproducciones.Sum(p => p.Cantidad);

        return View(model);
      }
    }

    [HttpGet]
    public ActionResult TotalCapsule(string id) {
      using (InventarioEntities db = new InventarioEntities()) {
        string user = User.Identity.GetUserName();

        var reproducciones = db.ReproduccionesItems.Where(p => (id == null || p.Reproducciones.IdCapsula == id) && p.Reproducciones.IdUsuario == user)
                    .OrderBy(q => q.ItemID)
                     .Select(s => new {
                       IdCapsula = s.Reproducciones.IdCapsula,
                       ItemID = s.ItemID,
                       Cantidad = s.Cantidad,
                     }).ToList();
        var model = reproducciones.GroupBy(r => r.IdCapsula)
                     .Select(s => new FechaTotalViewModel {
                       IdCapsula = s.Key,
                       Total = s.Sum(p => p.Cantidad),
                       Items = s.GroupBy(u => u.ItemID).Select(v => new Models.ItemInventoryViewModel { Cantidad = v.Sum(w => w.Cantidad), CurrentItem = ItemsXml.Single(p => p.ItemID == v.First().ItemID) })
                     }).ToList();

        return View(model);
      }
    }

    [HttpGet]
    public ActionResult TotalByItem() {
      using (InventarioEntities db = new InventarioEntities()) {
        string user = User.Identity.GetUserName();


        var totalesDB = db.Reproducciones.Where(p => p.IdUsuario == user)
            .SelectMany(p => p.ReproduccionesItems)
            .GroupBy(p => p.ItemID)
            .Select(q => new {
              ItemID = q.Key,
              Cantidad = q.Sum(r => r.Cantidad)
            }).ToList();

        var totales = totalesDB.Select(p => new Models.ItemInventoryViewModel {
          CurrentItem = ItemsXml.Single(z => z.ItemID == p.ItemID),
          Cantidad = p.Cantidad
        }).ToList();

        return View(totales);
      }
    }

  }
}