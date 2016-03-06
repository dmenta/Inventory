using MiInventario.Code;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;

namespace MiInventario.Models.Capsules {
  public class CapsulesViewModel {
    public string IdCapsula { get; set; }
    public string Descripcion { get; set; }
    public ItemViewModel ItemEncapsulado { get; set; }
    public int Total { get; set; }
    public bool Spawnable { get; set; }
  }

  public class SendToViewModel {
    public string IdCapsula { get; set; }
    public string Descripcion { get; set; }
    public string UserID { get; set; }
    public ItemViewModel ItemEncapsulado { get; set; }
  }
  public class CreateViewModel {
    [Required]
    [MaxLength(8, ErrorMessage = "El nombre de la cápsula debe tener 8 caracteres.")]
    [MinLength(8, ErrorMessage = "El nombre de la cápsula debe tener 8 caracteres.")]
    public string IdCapsula { get; set; }

    [MaxLength(40, ErrorMessage = "La descripcion no puede superar los 40 caracteres.")]
    public string Descripcion { get; set; }

    [Required]
    public string ItemID { get; set; }

    public Dictionary<string, string> Capsulas { get; set; }
  }
  public class EditViewModel {
    [Required]
    public string IdCapsula { get; set; }

    [MaxLength(40, ErrorMessage = "La descripcion no puede superar los 40 caracteres.")]
    public string Descripcion { get; set; }

    [Required]
    public string ItemID { get; set; }

    public Dictionary<string, string> Capsulas { get; set; }
  }
  public class ManageViewModel {
    public string IdCapsula { get; set; }
    public string Descripcion { get; set; }
    public int Total { get; set; }
    public bool Spawnable { get; set; }
    public List<ItemInventoryViewModel> Items { get; set; }
  }
  public class UnloadViewModel {
    public string IdCapsula { get; set; }
    public string Descripcion { get; set; }
    public int Total { get; set; }
    public bool Spawnable { get; set; }
    public bool UnloadAll { get; set; }
    public List<ItemUnloadViewModel> Items { get; set; }
  }
  public class LoadViewModel : IValidatableObject {
    public string IdCapsula { get; set; }
    public string Descripcion { get; set; }
    public int Total { get; set; }
    public bool Spawnable { get; set; }
    public List<ItemLoadViewModel> Items { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
      var results = new List<ValidationResult>();

      if (Items.Sum(p => p.CantidadCargar + p.CantidadEnCapsula) > 100) {
        results.Add(new ValidationResult("Total Quantity on capsule exceeds 100."));
      }
      else {
        if (Items.Any(p => p.CantidadCargar < 0 || p.CantidadSuelta < p.CantidadCargar)) {
          results.Add(new ValidationResult("Quantity must be a value bewteen 0 and the current quantity in inventory."));
        }
      }
      return results;
    }
  }

  public class DeleteViewModel {
    public string IdCapsula { get; set; }
    public int Total { get; set; }
  }

  public class DeleteItemViewModel {
    public string IdCapsula { get; set; }
    public int Total { get; set; }
    public ItemViewModel Item { get; set; }
    public int Cantidad { get; set; }
  }

  public class AddItemViewModel {
    public string IdCapsula { get; set; }
    public int Total { get; set; }

    [Required]
    public string ItemID { get; set; }

    [Required]
    [Range(1, 100, ErrorMessage = "Quantity must be a value between 1 and 100.")]
    public int Cantidad { get; set; }
    public List<ItemViewModel> AddeableItems { get; set; }
  }
  public class ContenidoViewModel {
    public string IdCapsula { get; set; }
    public int Total { get; set; }
    public IEnumerable<ItemInventoryViewModel> Contenidos { get; set; }
    public int Cantidad { get; set; }
  }

  public class ItemUnloadViewModel : ItemViewModel, IValidatableObject {
    public int Cantidad { get; set; }
    public int CantidadDescargar { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
      var results = new List<ValidationResult>();

      if (CantidadDescargar < 0 || Cantidad < CantidadDescargar) {
        results.Add(new ValidationResult("Quantity must be a value bewteen 0 and the current quantity in the capsule."));
      }

      return results;
    }
  }
  public class ItemLoadViewModel : ItemViewModel {
    public int CantidadSuelta { get; set; }
    public int CantidadEnCapsula { get; set; }
    public int CantidadCargar { get; set; }
  }
}