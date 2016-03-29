using MyInventory.Models.Interests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Globalization;
using MyInventory.Code;
using System.Threading.Tasks;
using System.Web.UI.DataVisualization.Charting;
using System.IO;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Data.Entity;
using System.Drawing.Text;
using MyInventory.Models;
using MyInventory.Models.Capsules;

namespace MyInventory.Controllers {
  [Authorize]
  public class InterestsController : BaseController {
    [HttpGet]
    public ActionResult DateCharts(DateGrouping? grouping, string itemId, bool? accumulative) {
      var reproduccionesDB = Database.SpawnsCapsulesItems
          .Where(p => p.SpawnsCapsules.Spawns.UserId == Username)
          .Select(s => s.ItemId)
          .Distinct()
          .ToList();

      ChartsViewModel model = new ChartsViewModel();
      model.Grouping = grouping ?? DateGrouping.Week;
      model.Accumulative = accumulative.GetValueOrDefault();
      model.ItemId = itemId;
      model.ViewableItems = ItemsXml.Where(p => reproduccionesDB.Contains(p.ItemId)).Select(q => new { q.ItemId, Description = q.Description() }).ToDictionary(r => r.ItemId, s => s.Description);
      model.ChartTitle = new ChartTitleViewModel {
        Grouping = model.Grouping,
        Item = ItemViewModelLight.Create(ItemsXml, model.ItemId),
        Accumulative = model.Accumulative
      };
      return View(model);
    }

    [HttpGet]
    public ActionResult _ChartTitle(DateGrouping grouping, string itemId, bool accumulative) {
      var model = new ChartTitleViewModel();
      model.Grouping = grouping;
      model.Item = ItemViewModelLight.Create(ItemsXml, itemId);
      model.Accumulative = accumulative;

      return PartialView("_ChartTitle", model);
    }

    public ActionResult InterestsChart(DateGrouping grouping, string itemId, bool accumulative) {
      DateTime inicio = DateTime.Now;

      var reproduccionesDB = Database.SpawnsCapsules.Where(p => p.Spawns.UserId == Username)
          .Select(s => new {
            Code = s.CapsuleCode,
            Date = s.Spawns.Date,
            Items = s.SpawnsCapsulesItems.Select(y => new { ItemId = y.ItemId, Quantity = y.Quantity }),
          }).ToList();

      var model = new ByDateViewModel();
      model.Grouping = grouping;
      model.DateInfo = reproduccionesDB
                  .GroupBy(r => r.Date.GetResolvedDate(grouping))
                  .Select(s => new DateInfoModel {
                    Date = s.Key,
                    TotalCapsules = s.Where(n => n.Items.Any(q => string.IsNullOrEmpty(itemId) || q.ItemId == itemId)).Select(z => z.Code).Distinct().Count(),
                    TotalItems = s.SelectMany(n => n.Items).Where(b => string.IsNullOrEmpty(itemId) || b.ItemId == itemId).DefaultIfEmpty().Sum(p => p == null ? 0 : p.Quantity)
                  }).OrderBy(i => i.Date).ToList();

      if (accumulative) {
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
          itemsSerie.Points.DataBindXY(model.DateInfo.Select(p => p.Date.ToString(dateFormat)).ToArray(), model.DateInfo.Select(p => p.TotalItems).ToArray());
          itemsSerie.IsValueShownAsLabel = grouping != DateGrouping.Day;
          itemsSerie.LabelForeColor = Color.Orange;
          chart.Series.Add(itemsSerie);

          Series capsulesSerie = new Series("Capsules");
          capsulesSerie.Font = fuente;
          capsulesSerie.ChartType = SeriesChartType.Column;
          capsulesSerie.Points.DataBindXY(model.DateInfo.Select(p => p.Date.ToString(dateFormat)).ToArray(), model.DateInfo.Select(p => p.TotalCapsules).ToArray());
          capsulesSerie.IsValueShownAsLabel = grouping != DateGrouping.Day;
          capsulesSerie.LabelForeColor = Color.LightGray;
          chart.Series.Add(capsulesSerie);

          MemoryStream ms = new MemoryStream();
          chart.SaveImage(ms, ChartImageFormat.Png);
          return File(ms.ToArray(), "image/png");
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
    public ActionResult DateCapsule(DateGrouping grouping, string capsuleCode) {
      DateTime inicio = DateTime.Now;

      var reproduccionesDB = Database.SpawnsCapsulesItems.Where(p => p.SpawnsCapsules.Spawns.UserId == Username && (string.IsNullOrEmpty(capsuleCode) || p.SpawnsCapsules.CapsuleCode == capsuleCode))
          .GroupBy(r => new { Date = DbFunctions.TruncateTime(r.SpawnsCapsules.Spawns.Date).Value, Code = r.SpawnsCapsules.CapsuleCode, ItemId = r.ItemId })
          .Select(s => new {
            Code = s.Key.Code,
            Date = s.Key.Date,
            ItemId = s.Key.ItemId,
            Quantity = s.Sum(m => m.Quantity),
          }).ToList();

      var filas = reproduccionesDB
                  .GroupBy(r => new { Code = r.Code, Date = r.Date.GetResolvedDate(grouping) })
                  .Select(s => new FechaTotalViewModel {
                    Code = s.Key.Code,
                    Date = s.Key.Date,
                    TotalQuantity = s.Sum(p => p.Quantity),
                    Items = s.GroupBy(u => u.ItemId).Select(v => new ItemCapsuleViewModel { Quantity = v.Sum(w => w.Quantity), CurrentItem = ItemBase.Create(ItemsXml, v.First().ItemId) })
                  }).ToList();

      var totales = reproduccionesDB
          .GroupBy(r => r.Code)
          .Select(s => new FechaTotalViewModel {
            Code = s.Key,
            Date = DateTime.MinValue,
            TotalQuantity = s.Sum(p => p.Quantity),
            Items = s.GroupBy(u => u.ItemId).Select(v => new ItemCapsuleViewModel { Quantity = v.Sum(w => w.Quantity), CurrentItem = ItemBase.Create(ItemsXml, v.First().ItemId) })
          }).ToList();

      var model = new ByDateViewModel();
      model.Grouping = grouping;
      model.Capsules = reproduccionesDB.Select(p => p.Code).Distinct().ToList();
      model.DateInfo = reproduccionesDB.GroupBy(p => p.Date.GetResolvedDate(grouping)).Select(q => new DateInfoModel { Date = q.Key, TotalCapsules = q.Select(z => z.Code).Distinct().Count(), TotalItems = q.Sum(r => r.Quantity), RealDays = (int)(q.Max(h => h.Date) - q.Min(i => i.Date)).TotalDays + 1 }).OrderByDescending(b => b.Date);
      model.Rows = filas;
      model.Totals = totales;

      TimeSpan tiempo = DateTime.Now.Subtract(inicio);

      return View(model);
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
          startDate = DateTime.Now.Date.GetResolvedDate(DateGrouping.Week).AddDays((1 - periods) * 7);
          break;
        case DateGrouping.Day:
        default:
          startDate = DateTime.Now.Date.AddDays(1 - periods);
          break;
      }


      var reproduccionesDB = Database.SpawnsCapsulesItems.Where(p => p.SpawnsCapsules.Spawns.UserId == Username && DbFunctions.TruncateTime(p.SpawnsCapsules.Spawns.Date).Value >= startDate)
          .GroupBy(r => new { Date = DbFunctions.TruncateTime(r.SpawnsCapsules.Spawns.Date).Value, Code = r.SpawnsCapsules.CapsuleCode, ItemId = r.ItemId })
          .Select(s => new {
            Code = s.Key.Code,
            Date = s.Key.Date,
            ItemId = s.Key.ItemId,
            Quantity = s.Sum(m => m.Quantity),
          }).ToList();

      var items = reproduccionesDB
                  .GroupBy(r => new { Date = r.Date.GetResolvedDate(grouping) })
                  .Select(s => new DateInfoTotalModel {
                    Date = s.Key.Date,
                    TotalCapsules = s.Select(h => h.Code).Distinct().Count(),
                    TotalItems = s.Sum(p => p.Quantity),
                    RealDays = (int)(s.Max(h => h.Date) - s.Min(i => i.Date)).TotalDays + 1,
                    Items = s.GroupBy(u => u.ItemId).Select(v => new { ItemId = v.Key, Cantidad = v.Sum(w => w.Quantity) }).ToDictionary(m => m.ItemId, n => n.Cantidad)
                  }).ToList();

      var totals = reproduccionesDB
       .GroupBy(q => q.ItemId)
       .Select(i => new ItemCapsuleViewModel { CurrentItem = ItemBase.Create(ItemsXml, i.Key), Quantity = i.Sum(j => j.Quantity) }).OrderBy(k => k.CurrentItem.Order);

      var highests = reproduccionesDB
                  .GroupBy(r => new { Fecha = r.Date.GetResolvedDate(grouping), ItemId = r.ItemId })
                  .Select(s => new { ItemId = s.Key.ItemId, Cantidad = s.Sum(p => p.Quantity) })
                  .GroupBy(h => h.ItemId)
                  .Select(i => new { ItemId = i.Key, Maximo = i.Max(j => j.Cantidad) })
                  .ToDictionary(h => h.ItemId, i => i.Maximo);

      var model = new ByDateTotalViewModel();
      model.Grouping = grouping;
      model.DateInfo = items.OrderByDescending(b => b.Date);
      model.Totals = totals;
      model.Highests = highests;
      model.TotalItems = totals.Sum(p => p.Quantity);

      return PartialView("_DateTotal", model);
    }


    [HttpGet]
    public JsonResult ByItemDate(DateGrouping grouping, int periods) {
      DateTime startDate = new DateTime(2000, 1, 1); // all data

      if (periods < 0) {
        periods = 4;
      }

      if (periods > 0) {
        switch (grouping) {
          case DateGrouping.Year:
            startDate = DateTime.Now.Date.GetResolvedDate(DateGrouping.Year).AddYears(1 - periods);
            break;
          case DateGrouping.Month:
            startDate = DateTime.Now.Date.GetResolvedDate(DateGrouping.Month).AddMonths(1 - periods);
            break;
          case DateGrouping.Week:
            startDate = DateTime.Now.Date.GetResolvedDate(DateGrouping.Week).AddDays((1 - periods) * 7);
            break;
          case DateGrouping.Day:
          default:
            startDate = DateTime.Now.Date.AddDays(1 - periods);
            break;
        }
      }

      string dateFormat = Resources.General.ResourceManager.GetString(string.Format("InterestsByDate_DateFormat_{0}", grouping.ToString()));

      var reproduccionesDB = Database.SpawnsCapsulesItems.Where(p => p.SpawnsCapsules.Spawns.UserId == Username && DbFunctions.TruncateTime(p.SpawnsCapsules.Spawns.Date).Value >= startDate)
          .GroupBy(r => new { Date = DbFunctions.TruncateTime(r.SpawnsCapsules.Spawns.Date).Value, Code = r.SpawnsCapsules.CapsuleCode, ItemId = r.ItemId })
          .Select(s => new {
            Code = s.Key.Code,
            Date = s.Key.Date,
            ItemId = s.Key.ItemId,
            Quantity = s.Sum(m => m.Quantity),
          }).ToList();

      var items = reproduccionesDB
                  .GroupBy(r => new { Date = r.Date.GetResolvedDate(grouping) })
                  .Select(s => new DateInfoTotalModel {
                    Date = s.Key.Date,
                    FormattedDate = s.Key.Date.ToString(dateFormat),
                    TotalCapsules = s.Select(h => h.Code).Distinct().Count(),
                    TotalItems = s.Sum(p => p.Quantity),
                    RealDays = (int)(s.Max(h => h.Date) - s.Min(i => i.Date)).TotalDays + 1,
                    Items = s.GroupBy(u => u.ItemId).Select(v => new { ItemId = v.Key, Cantidad = v.Sum(w => w.Quantity) }).ToDictionary(m => m.ItemId, n => n.Cantidad)
                  }).ToList();

      var totals = reproduccionesDB
       .GroupBy(q => q.ItemId)
       .Select(i => new ItemCapsuleViewModel { CurrentItem = ItemBase.Create(ItemsXml, i.Key), Quantity = i.Sum(j => j.Quantity) }).OrderBy(k => k.CurrentItem.Order);

      var highests = reproduccionesDB
                  .GroupBy(r => new { Date = r.Date.GetResolvedDate(grouping), ItemId = r.ItemId })
                  .Select(s => new { ItemId = s.Key.ItemId, Cantidad = s.Sum(p => p.Quantity) })
                  .GroupBy(h => h.ItemId)
                  .Select(i => new { ItemId = i.Key, Maximo = i.Max(j => j.Cantidad) })
                  .ToDictionary(h => h.ItemId, i => i.Maximo);

      var model = new ByDateTotalViewModel();
      model.Grouping = grouping;
      model.DateInfo = items.OrderByDescending(b => b.Date);
      model.Totals = totals;
      model.Highests = highests;
      model.TotalItems = totals.Sum(p => p.Quantity);
      model.Items = model.Totals.Select(p => p.CurrentItem.ItemId).ToArray();

      return Json(model, JsonRequestBehavior.AllowGet);
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

      string dateFormat = Resources.General.ResourceManager.GetString(string.Format("InterestsChart_DateFormat_{0}", grouping.ToString()));

      var rarityDef = new[] { 
                        new { Rarity = "R1", Name = "Very Common", Color="#b5b2b5" }, 
                        new { Rarity = "R2", Name = "Common", Color="#84f7b5" }, 
                        new { Rarity = "R3", Name = "Rare", Color="#ad8eff" }, 
                        new { Rarity = "R4", Name = "Very Rare", Color="#ff8ef7" } 
                    };

      var dbData = Database.SpawnsCapsulesItems.Where(p => p.SpawnsCapsules.Spawns.UserId == Username)
                          .GroupBy(r => new { Date = DbFunctions.TruncateTime(r.SpawnsCapsules.Spawns.Date).Value, ItemId = r.ItemId })
                          .Select(s => new {
                            Date = s.Key.Date,
                            ItemId = s.Key.ItemId,
                            Quantity = s.Sum(m => m.Quantity),
                          }).ToList()
                          .GroupBy(r => new { Rarity = ItemsXml.Single(j => j.ItemId == r.ItemId).Rarity, Date = r.Date.GetResolvedDate(grouping) })
                          .Select(s => new {
                            Rarity = s.Key.Rarity,
                            Date = s.Key.Date,
                            Quantity = s.Sum(w => w.Quantity)
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
                     Qty = subData == null ? 0 : subData.Quantity
                   };

      var itemsQtyDate = matrix
                      .OrderByDescending(a => a.Rarity)
                      .ThenBy(d => d.Date)
                      .GroupBy(b => b.Rarity)
                      .Select(c => new {
                        Rarity = c.Key,
                        Name = rarityDef.Single(p => p.Rarity == c.Key).Name,
                        Color = System.Drawing.ColorTranslator.FromHtml(rarityDef.Single(p => p.Rarity == c.Key).Color),
                        Dates = c.Select(o => o.Date.ToString(dateFormat)).ToArray(),
                        Values = c.Select(o => o.Qty).ToArray()
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
            itemsSerie.Points.DataBindXY(item.Dates, item.Values);
            itemsSerie.IsValueShownAsLabel = grouping != DateGrouping.Day;
            chart.Series.Add(itemsSerie);
          }

          MemoryStream ms = new MemoryStream();
          chart.SaveImage(ms, ChartImageFormat.Png);
          return File(ms.ToArray(), "image/png");
        }
      }
    }

    [HttpGet]
    public ActionResult LastSpawn() {
      LastSpawnViewModel model = new LastSpawnViewModel();
      model.TotalQuantity = 0;

      var last = Database.Spawns.Where(p => p.UserId == Username).OrderByDescending(q => q.Date).FirstOrDefault();

      if (last == null) {
        return View(model);
      }

      model.SpawnDate = last.Date;
      var reproducciones = last.SpawnsCapsules
          .SelectMany(z => z.SpawnsCapsulesItems)
          .Select(s => new {
            Code = s.CapsuleCode,
            ItemId = s.ItemId,
            Quantity = s.Quantity
          }).ToList();
      model.Capsules = reproducciones
          .GroupBy(p => p.Code)
          .Select(s => new Models.Capsules.ContentsViewModel {
            Code = s.Key,
            ItemsQuantity = s.Count(),
            Items = s.OrderBy(x => x.ItemId).Select(t => new ItemCapsuleViewModel {
              CurrentItem = ItemBase.Create(ItemsXml, t.ItemId),
              Quantity = t.Quantity
            })
          }).ToList();

      model.Totals = reproducciones
          .GroupBy(t => t.ItemId)
          .OrderBy(x => x.Key)
          .Select(q => new ItemCapsuleViewModel {
            CurrentItem = ItemBase.Create(ItemsXml, q.Key),
            Quantity = q.Sum(r => r.Quantity)
          });

      model.TotalQuantity = reproducciones.Sum(p => p.Quantity);

      return View(model);
    }

    [HttpGet]
    public ActionResult TotalCapsule(string id) {
      var reproducciones = Database.SpawnsCapsulesItems.Where(p => p.SpawnsCapsules.Spawns.UserId == Username && (id == null || p.SpawnsCapsules.CapsuleCode == id))
                  .OrderBy(q => q.ItemId)
                   .Select(s => new {
                     Code = s.CapsuleCode,
                     ItemId = s.ItemId,
                     Quantity = s.Quantity,
                   }).ToList();
      var model = reproducciones.GroupBy(r => r.Code)
                   .Select(s => new FechaTotalViewModel {
                     Code = s.Key,
                     TotalQuantity = s.Sum(p => p.Quantity),
                     Items = s.GroupBy(u => u.ItemId).Select(v => new ItemCapsuleViewModel { Quantity = v.Sum(w => w.Quantity), CurrentItem = ItemBase.Create(ItemsXml, v.First().ItemId) })
                   }).ToList();

      return View(model);
    }

    [HttpGet]
    public ActionResult TotalByItem() {
      var totalesDB = Database.SpawnsCapsulesItems.Where(p => p.SpawnsCapsules.Spawns.UserId == Username)
          .GroupBy(p => p.ItemId)
          .Select(q => new {
            ItemId = q.Key,
            Quantity = q.Sum(r => r.Quantity)
          }).ToList();

      var totales = totalesDB.Select(p => new ItemCapsuleViewModel {
        CurrentItem = ItemCapsule.Create(ItemsXml, p.ItemId),
        Quantity = p.Quantity
      }).ToList();

      return View(totales);
    }
  }
}