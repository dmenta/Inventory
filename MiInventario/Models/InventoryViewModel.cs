using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MiInventario.Models.Inventory {
  public class ManageViewModel {
    public string GroupID { get; set; }
    public bool Editing { get; set; }
    public Dictionary<string, string> Groups { get; set; }
  }

  public class ItemQuantity {
    public string ItemID { get; set; }
    public int Quantity { get; set; }
  }

  public class GroupViewModel {

    public string GroupID { get; set; }

    public string GroupName { get; set; }

    public int Total { get; set; }

    public int CapsulesTotal { get; set; }
  }

  public class DifferenceViewModel {
    public string UsuarioA { get; set; }

    public string UsuarioB { get; set; }

    public List<GroupDifferenceViewModel> Groups { get; set; }
  }

  public class ItemDifferenceViewModel : ItemViewModel {
    public int CantidadUsuarioA { get; set; }
    public int CantidadUsuarioB { get; set; }
    public int DiferenciaA {
      get {
        if (CantidadUsuarioA >= CantidadUsuarioB) {
          return (int)Math.Floor(Convert.ToDouble(CantidadUsuarioA - CantidadUsuarioB) / 2f);
        }
        else {
          return 0;
        }
      }
    }

    public int DiferenciaB {
      get {
        if (CantidadUsuarioB >= CantidadUsuarioA) {
          return (int)Math.Floor(Convert.ToDouble(CantidadUsuarioB - CantidadUsuarioA) / 2f);
        }
        else {
          return 0;
        }
      }
    }
  }

  public class GroupDifferenceViewModel {
    public string GroupID { get; set; }

    public IEnumerable<TypeDifferenceViewModel> Types { get; set; }
  }

  public class TypeDifferenceViewModel {
    public string TypeID { get; set; }

    public IEnumerable<ItemDifferenceViewModel> Items { get; set; }
  }
}