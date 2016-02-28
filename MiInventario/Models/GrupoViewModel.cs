using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MiInventario.Models
{
    public class GrupoViewModel
    {
        public string GroupID { get; set; }
        public int Total { get; set; }
        public int TotalCapsulas { get; set; }
        public IEnumerable<TipoViewModel> Tipos { get; set; }
    }

    public class TipoViewModel
    {
        public string TypeID { get; set; }
        public IEnumerable<ItemInventoryViewModel> Items { get; set; }

        public int Cantidad { get; set; }

        public int CantidadEnCapsulas { get; set; }
    }

    public class GrupoEditViewModel
    {
        public string GroupID { get; set; }
        public int Total { get; set; }
        public int TotalCapsulas { get; set; }
        public List<ItemEditViewModel> Items { get; set; }
    }

    public class DifferenceViewModel
    {
        public string UsuarioA { get; set; }
        public string UsuarioB { get; set; }
        public List<GrupoDifferenceViewModel> Grupos { get; set; }
    }

    public class GrupoDifferenceViewModel
    {
        public string GroupID { get; set; }
        public IEnumerable<TipoDifferenceViewModel> Tipos { get; set; }
    }

    public class TipoDifferenceViewModel
    {
        public string TypeID { get; set; }
        public IEnumerable<ItemDifferenceViewModel> Items { get; set; }
    }

}
