using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MiInventario2.Models
{
    public class GrupoViewModel
    {
        public int IdGrupo { get; set; }
        public string Descripcion { get; set; }
        public int Total { get; set; }
        public int TotalCapsulas { get; set; }
        public IEnumerable<TipoViewModel> Tipos { get; set; }
    }

    public class TipoViewModel
    {
        public int IdTipo { get; set; }
        public IEnumerable<ItemViewModel> Items { get; set; }
    }

    public class GrupoEditViewModel
    {
        public int IdGrupo { get; set; }
        public string Descripcion { get; set; }
        public int Total { get; set; }
        public int TotalCapsulas { get; set; }
        public List<ItemViewModel> Items { get; set; }
    }
}
