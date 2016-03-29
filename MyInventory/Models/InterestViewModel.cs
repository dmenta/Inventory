using MyInventory.Code;
using MyInventory.Models.Capsules;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace MyInventory.Models.Interests {
  //public class ItemInterestsViewModel {
  //  public ItemInterests CurrentItem { get; set; }
  //  public static ItemViewModelLight Create(IReadOnlyList<Item> itemsXml, string itemId) {
  //    ItemViewModelLight instance = new ItemViewModelLight();
  //    instance.CurrentItem = ItemBase.Create(itemsXml, itemId);

  //    return instance;
  //  }

  //  public int Quantity { get; set; }
  //}

  public class ItemInterests {
    public string ItemId { get; set; }
    public int Order { get; set; }
    public string GroupId { get; set; }
    public string TypeId { get; set; }
    public int Level { get; set; }
    public string Rarity { get; set; }
    public string Name {
      get { return Resources.ItemsNames.ResourceManager.GetString(ItemId) ?? Resources.Types.ResourceManager.GetString(TypeId); }
    }

    private ItemInterests() {

    }

    public static ItemInterests Create(IReadOnlyList<Item> itemsXml, string itemId) {
      return Create(itemsXml.Single(s => s.ItemId == itemId));
    }

    public static ItemInterests Create(Item model) {
      var instance = new ItemInterests();
      instance.ItemId = model.ItemId;
      instance.Order = model.Order;
      instance.GroupId = model.GroupId;
      instance.ItemId = model.ItemId;
      instance.Level = model.Level;
      instance.Rarity = model.Rarity;

      return instance;
    }

    public override bool Equals(Object obj) {
      if (obj == null || GetType() != obj.GetType()) {
        return false;
      }

      Item itemB = (Item)obj;

      // Use Equals to compare instance variables.
      return ItemId.Equals(itemB.ItemId);
    }
    public override int GetHashCode() {
      return ItemId.GetHashCode();
    }
  }
  public class ChartTitleViewModel {
    public DateGrouping Grouping { get; set; }
    public ItemViewModelLight Item { get; set; }
    public bool Accumulative { get; set; }
  }
  public class LastSpawnViewModel {
    public DateTime? SpawnDate { get; set; }
    public int TotalQuantity { get; set; }
    public IEnumerable<Capsules.ContentsViewModel> Capsules { get; set; }

    public IEnumerable<ItemCapsuleViewModel> Totals { get; set; }
  }

  public class ByDateViewModel {
    public string Title {
      get {
        if (TotalCapsules == 1) {
          return Capsules.Single() + ": " + Resources.General.ResourceManager.GetString(string.Format("InterestsByDate_Title_{0}", Grouping.ToString()));
        }
        else {
          return Resources.General.ResourceManager.GetString(string.Format("InterestsByDate_Title_{0}", Grouping.ToString()));
        }
      }
    }
    public IEnumerable<string> Capsules { get; set; }
    public int TotalCapsules { get { return Capsules.Count(); } }
    public int TotalItems { get { return DateInfo.Sum(p => p.TotalItems); } }
    public DateGrouping Grouping { get; set; }
    public IEnumerable<DateInfoModel> DateInfo { get; set; }
    public IEnumerable<FechaTotalViewModel> Rows { get; set; }
    public IEnumerable<FechaTotalViewModel> Totals { get; set; }
  }


  public class DateInfoModel {

    [ScriptIgnoreAttribute]
    public DateTime Date { get; set; }

    public int TotalCapsules { get; set; }

    public int TotalItems { get; set; }

    [ScriptIgnoreAttribute]
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
    public IEnumerable<ItemCapsuleViewModel> Totals { get; set; }
    public Dictionary<string, int> Highests { get; set; }

    public string[] Items { get; set; }

    public Dictionary<string, string> DateFormats { get; set; }
  }
  public class DateInfoTotalModel : DateInfoModel {
    public Dictionary<string, int> Items { get; set; }

    public string FormattedDate { get; set; }
  }
  public class FechaTotalViewModel {
    public string Code { get; set; }
    public DateTime Date { get; set; }
    public int TotalQuantity { get; set; }
    public IEnumerable<ItemCapsuleViewModel> Items { get; set; }
  }

  public class ChartsViewModel {
    public Dictionary<string, string> ViewableItems { get; set; }
    public DateGrouping Grouping { get; set; }
    public string ItemId { get; set; }
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