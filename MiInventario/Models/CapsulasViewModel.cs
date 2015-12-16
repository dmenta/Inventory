using MiInventario.Code;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;

namespace MiInventario.Models
{
    public class ChartTitleViewModel
    {
        public DateGrouping Grouping { get; set; }
        public ItemViewModel Item { get; set; }
        public bool Accumulative { get; set; }
    }
    public class CapsulasViewModel
    {
        public string IdCapsula { get; set; }
        public string Descripcion { get; set; }
        public ItemViewModel ItemEncapsulado { get; set; }
        public int Total { get; set; }
        public bool Spawnable { get; set; }
    }

    public class CapsulasSendToViewModel
    {
        public string IdCapsula { get; set; }
        public string Descripcion { get; set; }
        public string UserID { get; set; }
        public ItemViewModel ItemEncapsulado { get; set; }
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
        public string ItemID { get; set; }

        public Dictionary<string, string> Capsulas { get; set; }
    }
    public class CapsulasEditViewModel
    {
        [Required]
        public string IdCapsula { get; set; }

        [MaxLength(40, ErrorMessage = "La descripcion no puede superar los 40 caracteres.")]
        public string Descripcion { get; set; }

        [Required]
        public string ItemID { get; set; }

        public Dictionary<string, string> Capsulas { get; set; }
    }
    public class CapsulasManageViewModel
    {
        public string IdCapsula { get; set; }
        public string Descripcion { get; set; }
        public int Total { get; set; }
        public bool Spawnable { get; set; }
        public List<ItemInventoryViewModel> Items { get; set; }
    }
    public class CapsulaUnloadViewModel
    {
        public string IdCapsula { get; set; }
        public string Descripcion { get; set; }
        public int Total { get; set; }
        public bool Spawnable { get; set; }
        public bool UnloadAll { get; set; }
        public List<ItemUnloadViewModel> Items { get; set; }
    }
    public class CapsulaLoadViewModel : IValidatableObject
    {
        public string IdCapsula { get; set; }
        public string Descripcion { get; set; }
        public int Total { get; set; }
        public bool Spawnable { get; set; }
        public List<ItemLoadViewModel> Items { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            if (Items.Sum(p => p.CantidadCargar + p.CantidadEnCapsula) > 100)
            {
                results.Add(new ValidationResult("Total Quantity on capsule exceeds 100."));
            }
            else
            {
                if (Items.Any(p => p.CantidadCargar < 0 || p.CantidadSuelta < p.CantidadCargar))
                {
                    results.Add(new ValidationResult("Quantity must be a value bewteen 0 and the current quantity in inventory."));
                }
            }
            return results;
        }
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
        public ItemViewModel Item { get; set; }
        public int Cantidad { get; set; }
    }

    public class CapsulasAddItemViewModel
    {
        public string IdCapsula { get; set; }
        public int Total { get; set; }

        [Required]
        public string ItemID { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Quantity must be a value between 1 and 100.")]
        public int Cantidad { get; set; }
        public List<ItemViewModel> AddeableItems { get; set; }
    }

    public class CapsulasLastSpawnViewModel
    {
        public DateTime? Fecha { get; set; }
        public int Total { get; set; }
        public IEnumerable<CapsulaContenidoViewModel> Capsulas { get; set; }

        public IEnumerable<ItemInventoryViewModel> Totales { get; set; }
    }

    public class CapsulasInterestsByDateViewModel
    {
        public string Title
        {
            get
            {
                if (TotalCapsules == 1)
                {
                    return Capsulas.Single() + ": " + Resources.General.ResourceManager.GetString(string.Format("InterestsByDate_Title_{0}", Grouping.ToString()));
                }
                else
                {
                    return Resources.General.ResourceManager.GetString(string.Format("InterestsByDate_Title_{0}", Grouping.ToString()));
                }
            }
        }
        public IEnumerable<string> Capsulas { get; set; }
        public int TotalCapsules { get { return Capsulas.Count(); } }
        public int TotalItems { get { return DateInfo.Sum(p => p.TotalItems); } }
        public DateGrouping Grouping { get; set; }
        public IEnumerable<DateInfoModel> DateInfo { get; set; }
        public IEnumerable<CapsulaFechaTotalViewModel> Filas { get; set; }
        public IEnumerable<CapsulaFechaTotalViewModel> Totales { get; set; }
    }


    public class DateInfoModel
    {
        public DateTime Fecha { get; set; }
        public int TotalCapsules { get; set; }
        public int DifferentItems { get; set; }
        public int TotalItems { get; set; }
        public int RealDays { get; set; }
        public double Average
        {
            get { return (double)TotalItems / TotalCapsules / RealDays; }
        }

    }

    public class CapsulasInterestsByDateTotalViewModel
    {
        public string Title
        {
            get
            {
                return Resources.General.ResourceManager.GetString(string.Format("InterestsByDate_Title_{0}", Grouping.ToString()));
            }
        }
        public DateGrouping Grouping { get; set; }
        public IEnumerable<DateInfoTotalModel> DateInfo { get; set; }
        public int TotalItems { get; set; }
        public IEnumerable<ItemInventoryViewModel> Totals { get; set; }
        public Dictionary<string, int> Maximos { get; set; }
    }
    public class DateInfoTotalModel : DateInfoModel
    {
        public Dictionary<string, int> Items { get; set; }
    }
    public class CapsulaFechaTotalViewModel
    {
        public string IdCapsula { get; set; }
        public DateTime Fecha { get; set; }
        public int Total { get; set; }
        public IEnumerable<ItemInventoryViewModel> Items { get; set; }
    }
    public class CapsulaContenidoViewModel
    {
        public string IdCapsula { get; set; }
        public int Total { get; set; }
        public IEnumerable<ItemInventoryViewModel> Contenidos { get; set; }
        public int Cantidad { get; set; }
    }

    public class CapsulesInterestsChartsViewModel
    {
        public Dictionary<string, string> ViewableItems { get; set; }
        public DateGrouping Grouping { get; set; }
        public string ItemID { get; set; }
        public bool Accumulate { get; set; }

        public string ChartTitle { get; set; }
    }
}