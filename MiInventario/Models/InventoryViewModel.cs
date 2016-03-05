using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MiInventario.Models {
  public class InventoryManageViewModel {
    public string GroupID { get; set; }
    public bool Editing { get; set; }
    public Dictionary<string, string> Groups { get; set; }
  }

  public class ItemBasico {
    public string ItemID { get; set; }
    public int Cantidad { get; set; }
  }

  public class ItemsQty {
    public string ItemID { get; set; }
    public int Qty { get; set; }
  }

  public class GroupViewModel {

    public string GroupID { get; set; }

    public int Total { get; set; }

    public int TotalCapsulas { get; set; }
  }

  public class DifferenceViewModel {
    public string UsuarioA { get; set; }

    public string UsuarioB { get; set; }

    public List<GroupDifferenceViewModel> Groups { get; set; }
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