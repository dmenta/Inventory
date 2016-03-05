using MiInventario.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MiInventario.Models {
  public class CapsuleModel {

    private string resolvedDescription;
    public string IdCapsula { get; set; }

    public int Total {
      get { return Items == null ? 0 : Items.Sum(p => p.Quantity); }
    }

    public Item CapsuleItem { get; set; }

    public string Descripction { get; set; }

    public string ResolvedDescription {
      get {
        if (resolvedDescription == null) {
          resolvedDescription = SolveDescription();
        }

        return resolvedDescription;
      }
    }

    public string SolveDescription() {
      if (string.IsNullOrEmpty(Descripction)) {
        if (Items == null) {
          return Resources.General.ResourceManager.GetString("Capsule_Empty");
        }

        int itemQty = Items.Count();

        if (itemQty == 1) {
          return Items.Single().CurrentItem.Description();
        }

        return string.Format("({0} items)", itemQty);
      }
      else {
        return Descripction;
      }
    }

    public IEnumerable<CapsuleItemModel> Items { get; set; }
  }

  public class CapsuleItemModel {

    public Item CurrentItem { get; set; }

    public int Quantity { get; set; }
  }

}
