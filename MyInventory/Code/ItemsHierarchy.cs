using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyInventory.Code
{
  public class ItemGroup
  {
    public string GroupId { get; set; }
    public IEnumerable<ItemType> Types { get; set; }
  }

  public class ItemType
  {
    public string TypeId { get; set; }
    public IEnumerable<Item> Items { get; set; }
  }


  public class ItemBase {
    public string ItemId { get; set; }
    public int Order { get; set; }
    public string GroupId { get; set; }
    public string TypeId { get; set; }
    public int Level { get; set; }
    public string Rarity { get; set; }
    public string UniqueId { get; set; }
    public string Name {
      get { return Resources.ItemsNames.ResourceManager.GetString(ItemId) ?? Resources.Types.ResourceManager.GetString(TypeId); }
    }

    protected ItemBase() {

    }

    public static ItemBase Create(IReadOnlyList<Item> itemsXml, string itemId) {
      return Create(itemsXml.Single(s => s.ItemId == itemId));
    }

    public static ItemBase Create(Item model) {
      var instance = new ItemBase();
      instance.ItemId = model.ItemId;
      instance.Order = model.Order;
      instance.GroupId = model.GroupId;
      instance.TypeId = model.TypeId;
      instance.Level = model.Level;
      instance.Rarity = model.Rarity;
      instance.UniqueId = model is ItemCapsule ? ((ItemCapsule)model).UniqueId : null;

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

  public class Item: ItemBase
  {
    public string GroupName()
    {
      return Resources.Groups.ResourceManager.GetString(GroupId);
    }
    public string RarityName()
    {
      return Resources.Rarities.ResourceManager.GetString(Rarity);
    }
    public string Description()
    {
      string descriptionItem = string.Empty;

      if (Level > 0)
      {
        descriptionItem += string.Format("L{0} ", Level);
      }
      descriptionItem += Name;
      if (Level == 0)
      {
        descriptionItem += " " + RarityName();
      }
      return descriptionItem;
    }
    public bool IsKey { get; set; }
  }

  public class ItemCapsule : Item {
    public bool PaysInterests { get; set; }
    public bool IsKeyLocker { get; set; }
    public bool Transfer { get; set; }
  }
}