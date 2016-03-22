using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MiInventario.Models.Inventory {
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
    public string OriginUserId { get; set; }

    public string DestinationUserId { get; set; }

    public List<GroupDifferenceViewModel> Groups { get; set; }
  }

  public class ItemDifferenceViewModel : ItemViewModel {
    public int OriginQuantity { get; set; }
    public int DestinationQuantity { get; set; }
    public int OriginDifference {
      get {
        if (OriginQuantity >= DestinationQuantity) {
          return (int)Math.Floor(Convert.ToDouble(OriginQuantity - DestinationQuantity) / 2f);
        }
        else {
          return 0;
        }
      }
    }

    public int DestinationDifference {
      get {
        if (DestinationQuantity >= OriginQuantity) {
          return (int)Math.Floor(Convert.ToDouble(DestinationQuantity - OriginQuantity) / 2f);
        }
        else {
          return 0;
        }
      }
    }
  }

  public class GroupDifferenceViewModel {
    public string GroupId { get; set; }

    public IEnumerable<TypeDifferenceViewModel> Types { get; set; }
  }

  public class TypeDifferenceViewModel {
    public string TypeId { get; set; }

    public IEnumerable<ItemDifferenceViewModel> Items { get; set; }
  }
}