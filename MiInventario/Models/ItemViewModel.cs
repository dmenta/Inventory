using MiInventario.Code;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MiInventario.Models {
  public class ItemViewModel {
    public Item CurrentItem { get; set; }

  }
  public class ItemInventoryViewModel : ItemViewModel {
    public int Cantidad { get; set; }
    public int CantidadCapsulas { get; set; }
  }

}