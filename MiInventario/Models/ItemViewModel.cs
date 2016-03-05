using MiInventario.Code;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MiInventario.Models
{
    public class ItemViewModel
    {
        public Item CurrentItem { get; set; }

    }
    public class ItemInventoryViewModel : ItemViewModel
    {
        public int Cantidad { get; set; }
        public int CantidadCapsulas { get; set; }
    }

    public class ItemUnloadViewModel : ItemViewModel, IValidatableObject
    {
        public int Cantidad { get; set; }
        public int CantidadDescargar { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            if (CantidadDescargar < 0 || Cantidad < CantidadDescargar)
            {
                results.Add(new ValidationResult("Quantity must be a value bewteen 0 and the current quantity in the capsule."));
            }

            return results;
        }
    }
    public class ItemLoadViewModel : ItemViewModel
    {
        public int CantidadSuelta { get; set; }
        public int CantidadEnCapsula { get; set; }
        public int CantidadCargar { get; set; }
    }

    public class ItemDifferenceViewModel : ItemViewModel
    {
        public int CantidadUsuarioA { get; set; }
        public int CantidadUsuarioB { get; set; }
        public int DiferenciaA
        {
            get
            {
                if (CantidadUsuarioA >= CantidadUsuarioB)
                {
                    return (int)Math.Floor(Convert.ToDouble(CantidadUsuarioA - CantidadUsuarioB) / 2f);
                }
                else
                {
                    return 0;
                }
            }
        }
        public int DiferenciaB
        {
            get
            {
                if (CantidadUsuarioB >= CantidadUsuarioA)
                {
                    return (int)Math.Floor(Convert.ToDouble(CantidadUsuarioB - CantidadUsuarioA) / 2f);
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}