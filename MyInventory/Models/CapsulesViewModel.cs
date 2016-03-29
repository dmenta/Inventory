using MyInventory.Code;
using MyInventory.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;

namespace MyInventory.Models.Capsules {
  public class ItemCapsuleViewModel : ItemViewModelLight {
    public int CapsulesQuantity { get; set; }
    public int Quantity { get; set; }
  }

  public class CapsuleViewModel {
    public int CapsuleId { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public int TotalQuantity { get; set; }
    public ItemViewModelLight ItemInside { get; set; }
    public CapsuleProperties Properties { get; set; }
  }

  public class CapsuleProperties {
    public bool PaysInterests { get; set; }
    public bool IsKeyLocker { get; set; }
    public string UniqueId { get; set; }
    public string UniqueName { get; set; }
    public bool IsTransferable { get; set; }
    public int Order { get; set; }
  }

  public class SendToViewModel : CapsuleViewModel {
    public string UserId { get; set; }
  }

  public class AddEditViewModel : CapsuleViewModel {
    [Required]
    public string ItemId { get; set; }
    public IEnumerable<ItemCapsule> CapsuleTypes { get; set; }
  }

  public class ManageViewModel : CapsuleViewModel {
    public List<ItemCapsuleViewModel> Items { get; set; }
  }

  public class UnloadViewModel : CapsuleViewModel {
    public bool UnloadAll { get; set; }
    public List<ItemUnloadViewModel> Items { get; set; }
  }

  public class LoadViewModel : CapsuleViewModel, IValidatableObject {
    public List<ItemLoadViewModel> Items { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
      var results = new List<ValidationResult>();

      if (Items.Sum(p => p.LoadQuantity + p.CapsuleQuantity) > 100) {
        results.Add(new ValidationResult("Total Quantity on capsule exceeds 100."));
      }
      else {
        if (Items.Any(p => p.LoadQuantity < 0 || p.ItemQuantity < p.LoadQuantity)) {
          results.Add(new ValidationResult("Quantity must be a value bewteen 0 and the current quantity in inventory."));
        }
      }
      return results;
    }
  }

  public class DeleteItemViewModel : CapsuleViewModel {
    public ItemViewModelLight Item { get; set; }
    public int Quantity { get; set; }
  }

  public class AddItemViewModel : CapsuleViewModel {
    [Required]
    public string ItemId { get; set; }

    [Required]
    [Range(1, 100, ErrorMessage = "Quantity must be a value between 1 and 100.")]
    public int Quantity { get; set; }
    public List<ItemViewModel> AddeableItems { get; set; }
  }

  public class ContentsViewModel {
    public string Code { get; set; }
    public int TotalQuantity { get; set; }
    public IEnumerable<ItemCapsuleViewModel> Items { get; set; }
    public int ItemsQuantity { get; set; }
  }

  public class ItemUnloadViewModel : ItemViewModelLight, IValidatableObject {
    public int CapsuleQuantity { get; set; }
    public int UnloadQuantity { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
      var results = new List<ValidationResult>();

      if (UnloadQuantity < 0 || CapsuleQuantity < UnloadQuantity) {
        results.Add(new ValidationResult("Quantity must be a value bewteen 0 and the current quantity in the capsule."));
      }

      return results;
    }
  }

  public class ItemLoadViewModel : ItemViewModelLight {
    public int ItemQuantity { get; set; }
    public int CapsuleQuantity { get; set; }
    public int LoadQuantity { get; set; }
  }
}