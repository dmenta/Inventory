using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MiInventario2.Models
{
    public class CapsulasViewModel
    {
        public string IdCapsula { get; set; }
        public string ItemEncapsulado { get; set; }
        public int Total { get; set; }
        public bool Spawnable { get; set; }
    }
    public class CapsulasCreateViewModel
    {
        [Required]
        [MaxLength(8, ErrorMessage = "El nombre de la cápsula debe tener 8 caracteres.")]
        [MinLength(8, ErrorMessage = "El nombre de la cápsula debe tener 8 caracteres.")]
        public string IdCapsula { get; set; }

        [MaxLength(40, ErrorMessage = "La descripcion no puede superar los 40 caracteres.")]
        public string Descripcion { get; set; }

        [Required]
        public int IdItem { get; set; }

        public Dictionary<int, string> Capsulas { get; set; }
    }
    public class CapsulasEditViewModel
    {
        [Required]
        public string IdCapsula { get; set; }

        [MaxLength(40, ErrorMessage = "La descripcion no puede superar los 40 caracteres.")]
        public string Descripcion { get; set; }

        [Required]
        public int IdItem { get; set; }

        public Dictionary<int, string> Capsulas { get; set; }
    }
    public class CapsulasManageViewModel
    {
        public string IdCapsula { get; set; }
        public string Descripcion { get; set; }
        public int Total { get; set; }
        public List<ItemViewModel> Items { get; private set; }

        public CapsulasManageViewModel()
        {
            Items = new List<ItemViewModel>();
        }

        public bool Spawnable { get; set; }
    }

    public class CapsulasDeleteViewModel
    {
        public string IdCapsula { get; set; }
        public int Total { get; set; }
    }

    public class CapsulasDeleteItemViewModel
    {
        public string IdCapsula { get; set; }
        public int Total { get; set; }
        public ItemSummaryViewModel Item { get; set; }
        public int Cantidad { get; set; }
    }

    public class CapsulasAddItemViewModel
    {
        public string IdCapsula { get; set; }
        public int Total { get; set; }
        public int IdItem { get; set; }
        public int Cantidad { get; set; }
        public List<ItemSummaryViewModel> AddeableItems { get; private set; }

        public CapsulasAddItemViewModel()
        {
            AddeableItems = new List<ItemSummaryViewModel>();
        }
    }

    public class CapsulasInterestsViewModel
    {
        public List<string> Capsulas { get; set; }
        public Dictionary<string, List<string>> Filas { get; set; }
        public List<string> Totales { get; set; }
    }

    public class CapsulasLastSpawnViewModel
    {
        public DateTime? Fecha { get; set; }
        public int Total { get; set; }
        public IEnumerable<CapsulaFechaTotalViewModel> Capsulas { get; set; }

        public IEnumerable<string> Totales { get; set; }
    }

    public class CapsulasInterests2ViewModel
    {
        public IEnumerable<string> Capsulas { get; set; }
        public IEnumerable<DateTime> Fechas{ get; set; }
        public IEnumerable<CapsulaFechaTotalViewModel> Filas { get; set; }
        public IEnumerable<CapsulaFechaTotalViewModel> Totales { get; set; }
    }
    public class CapsulaFechaTotalViewModel
    {
        public string IdCapsula { get; set; }
        public DateTime Fecha { get; set; }
        public int Total { get; set; }
        public IEnumerable<string> Elementos { get; set; }
    }
}