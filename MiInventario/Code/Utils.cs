using MiInventario.Code;
using MiInventario.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MiInventario {
  public static class Utils {
    public static string GetUsernamFromEmail(string email) {
      if (string.IsNullOrWhiteSpace(email)) {
        return "(no user)";
      }
      if (email.IndexOf("@") == -1 || (email.IndexOf("@") != email.LastIndexOf("@"))) {
        return email;
      }

      return email.Split('@')[0];
    }

    public static int DaysInYear(this DateTime fecha) {
      var thisYear = new DateTime(fecha.Year, 1, 1);
      var nextYear = new DateTime(fecha.Year + 1, 1, 1);

      return (nextYear - thisYear).Days;
    }

    public static int DaysInMonth(this DateTime fecha) {
      return DateTime.DaysInMonth(fecha.Year, fecha.Month);
    }

    public static DateTime GetResolvedDate(this DateTime fecha, DateGrouping grouping) {
      switch (grouping) {
        case DateGrouping.Week:
          return fecha.AddDays(-(int)fecha.DayOfWeek).Date;
        case DateGrouping.Month:
          return new DateTime(fecha.Year, fecha.Month, 1);
        case DateGrouping.Year:
          return new DateTime(fecha.Year, 1, 1);
        case DateGrouping.Day:
        default:
          return new DateTime(fecha.Year, fecha.Month, fecha.Day);
      }
    }
  }
}