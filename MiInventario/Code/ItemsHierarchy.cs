using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MiInventario.Code
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
  public class Item
  {
    public string ItemId { get; set; }
    public int Order { get; set; }
    public string GroupId { get; set; }
    public string GroupName()
    {
      return Resources.Groups.ResourceManager.GetString(GroupId);
    }
    public string TypeId { get; set; }
    public int Level { get; set; }
    public string Rarity { get; set; }
    public string RarityName()
    {
      return Resources.Rarities.ResourceManager.GetString(Rarity);
    }
    public bool IsCapsule { get; set; }
    public bool PaysInterests { get; set; }
    public string Name
    {
      get { return Resources.ItemsNames.ResourceManager.GetString(ItemId) ?? Resources.Types.ResourceManager.GetString(TypeId); }
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
    public bool IsKeyLocker { get; set; }
    public string UniqueId { get; set; }
    public bool Transfer { get; set; }

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

    public bool IsKey { get; set; }
  }
}