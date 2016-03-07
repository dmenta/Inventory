using MiInventario.Code;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;

namespace MiInventario.Models.Interests {
  public class ChartTitleViewModel {
    public DateGrouping Grouping { get; set; }
    public ItemViewModel Item { get; set; }
    public bool Accumulative { get; set; }
  }
  public class LastSpawnViewModel {
    public DateTime? Fecha { get; set; }
    public int Total { get; set; }
    public IEnumerable<Capsules.ContenidoViewModel> Capsulas { get; set; }

    public IEnumerable<ItemInventoryViewModel> Totales { get; set; }
  }

  public class ByDateViewModel {
    public string Title {
      get {
        if (TotalCapsules == 1) {
          return Capsulas.Single() + ": " + Resources.General.ResourceManager.GetString(string.Format("InterestsByDate_Title_{0}", Grouping.ToString()));
        }
        else {
          return Resources.General.ResourceManager.GetString(string.Format("InterestsByDate_Title_{0}", Grouping.ToString()));
        }
      }
    }
    public IEnumerable<string> Capsulas { get; set; }
    public int TotalCapsules { get { return Capsulas.Count(); } }
    public int TotalItems { get { return DateInfo.Sum(p => p.TotalItems); } }
    public DateGrouping Grouping { get; set; }
    public IEnumerable<DateInfoModel> DateInfo { get; set; }
    public IEnumerable<FechaTotalViewModel> Filas { get; set; }
    public IEnumerable<FechaTotalViewModel> Totales { get; set; }
  }


  public class DateInfoModel {
    public DateTime Fecha { get; set; }
    public int TotalCapsules { get; set; }
    public int DifferentItems { get; set; }
    public int TotalItems { get; set; }
    public int RealDays { get; set; }
    public double Average {
      get { return (double)TotalItems / TotalCapsules / RealDays; }
    }

  }

  public class ByDateTotalViewModel {
    public string Title {
      get {
        return Resources.General.ResourceManager.GetString(string.Format("InterestsByDate_Title_{0}", Grouping.ToString()));
      }
    }
    public DateGrouping Grouping { get; set; }
    public IEnumerable<DateInfoTotalModel> DateInfo { get; set; }
    public int TotalItems { get; set; }
    public IEnumerable<ItemInventoryViewModel> Totals { get; set; }
    public Dictionary<string, int> Maximos { get; set; }
  }
  public class DateInfoTotalModel : DateInfoModel {
    public Dictionary<string, int> Items { get; set; }
  }
  public class FechaTotalViewModel {
    public string IdCapsula { get; set; }
    public DateTime Fecha { get; set; }
    public int Total { get; set; }
    public IEnumerable<ItemInventoryViewModel> Items { get; set; }
  }

  public class ChartsViewModel {
    public Dictionary<string, string> ViewableItems { get; set; }
    public DateGrouping Grouping { get; set; }
    public string ItemID { get; set; }
    public bool Accumulative { get; set; }

    public ChartTitleViewModel ChartTitle { get; set; }
  }

  public class PercentageRarityViewModel {
    public Code.DateGrouping Grouping { get; set; }
    public bool Percentage { get; set; }
  }

  public class DateTotalViewModel {
    public DateGrouping Grouping { get; set; }
    public int Periods { get; set; }
  }
}