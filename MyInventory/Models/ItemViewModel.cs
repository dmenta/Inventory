using MyInventory.Code;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyInventory.Models {
  public class ItemViewModel {
    public Item CurrentItem { get; set; }

  }
  public class ItemViewModelLight {
    public ItemBase CurrentItem { get; set; }
    public static ItemViewModelLight Create(IReadOnlyList<Item> itemsXml, string itemId) {
      ItemViewModelLight instance = new ItemViewModelLight();

      if (!string.IsNullOrEmpty(itemId)) {
        instance.CurrentItem = ItemBase.Create(itemsXml, itemId);
      }

      return instance;
    }
  }
}