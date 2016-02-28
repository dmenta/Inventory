using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MiInventario.Code
{
  public class ItemGroup
  {
    public string GroupID { get; set; }
    public IEnumerable<ItemType> Types { get; set; }
  }

  public class ItemType
  {
    public string TypeID { get; set; }
    public IEnumerable<Item> Items { get; set; }
  }
  public class Item
  {
    public string ItemID { get; set; }
    public int Order { get; set; }
    public string GroupID { get; set; }
    public string GroupName()
    {
      return Resources.Groups.ResourceManager.GetString(GroupID);
    }
    public string TypeID { get; set; }
    public int Level { get; set; }
    public string Rarity { get; set; }
    public string RarityName()
    {
      return Resources.Rarities.ResourceManager.GetString(Rarity);
    }
    public bool IsCapsule { get; set; }
    public bool PaysInterests { get; set; }
    public string Nombre
    {
      get { return Resources.ItemsNames.ResourceManager.GetString(ItemID) ?? Resources.Types.ResourceManager.GetString(TypeID); }
    }
    public string Description()
    {
      string descripcionItem = string.Empty;

      if (Level > 0)
      {
        descripcionItem += string.Format("L{0} ", Level);
      }
      descripcionItem += Nombre;
      if (Level == 0)
      {
        descripcionItem += " " + RarityName();
      }
      return descripcionItem;
    }
  }
}