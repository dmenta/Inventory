using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyInventory.Models.Inventory {
  public class ManageViewModel {
    public string GroupId { get; set; }
    public bool Editing { get; set; }
    public Dictionary<string, string> Groups { get; set; }
  }

  public class ItemQuantity {
    public string ItemId { get; set; }
    public int Quantity { get; set; }
  }

  public class GroupViewModel {

    public string GroupId { get; set; }

    public string GroupName { get; set; }

    public int TotalQuantity { get; set; }

    public int CapsulesTotal { get; set; }
  }

  public class DifferenceViewModel {
    public string OrigUserId { get; set; }

    public string DestUserId { get; set; }

    public List<GroupDifferenceViewModel> Groups { get; set; }
  }

  public class ItemDifferenceViewModel : ItemViewModelLight {
    public int OrigQuantity { get; set; }
    public int DestQuantity { get; set; }
    public int OrigDifference {
      get {
        if (OrigQuantity >= DestQuantity) {
          return (int)Math.Floor(Convert.ToDouble(OrigQuantity - DestQuantity) / 2f);
        }
        else {
          return 0;
        }
      }
    }

    public int DestDifference {
      get {
        if (DestQuantity >= OrigQuantity) {
          return (int)Math.Floor(Convert.ToDouble(DestQuantity - OrigQuantity) / 2f);
        }
        else {
          return 0;
        }
      }
    }
  }

  public class GroupDifferenceViewModel {
    public string GroupId { get; set; }
    public int OrigDifference { get { return Types.Sum(p => p.OrigDifference); } }
    public int DestDifference { get { return Types.Sum(p => p.DestDifference); } }
    public List<TypeDifferenceViewModel> Types { get; set; }
  }

  public class TypeDifferenceViewModel {
    public string TypeId { get; set; }
    public int OrigDifference { get { return Items.Sum(p => p.OrigDifference); } }
    public int DestDifference { get { return Items.Sum(p => p.DestDifference); } }
    public List<ItemDifferenceViewModel> Items { get; set; }
  }
}