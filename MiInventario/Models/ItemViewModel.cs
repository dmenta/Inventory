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

    public class ItemEditViewModel : ItemViewModel
    {

        [Range(0, 2000, ErrorMessage = "Cantidad no puede ser mayor a 2000")]
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

            if (Cantidad < CantidadDescargar)
                results.Add(new ValidationResult("La cantidad a descargar no puede ser mayor que la cantidad en la capsula"));

            return results;
        }
    }
    public class ItemLoadViewModel : ItemViewModel, IValidatableObject
    {
        public int CantidadSuelta { get; set; }
        public int CantidadEnCapsula { get; set; }
        public int CantidadCargar { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            if (CantidadSuelta < CantidadCargar)
                results.Add(new ValidationResult("La cantidad a cargar no puede ser mayor que la cantidad en el inventario"));

            return results;
        }
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