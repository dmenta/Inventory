using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MiInventario2.Models
{
    public class ItemViewModel
    {
        public int IdItem { get; set; }
        public int IdGrupo { get; set; }
        public int IdTipo { get; set; }
        public string Descripcion { get; set; }
        public bool MostrarNivel { get; set; }
        public int Nivel { get; set; }
        public int IdRareza { get; set; }
        public string Rareza { get; set; }
        public bool MostrarRareza { get; set; }
        public int Cantidad { get; set; }
        public int CantidadCapsulas { get; set; }
    }
}