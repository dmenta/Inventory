using MyInventory.Code;
using MyInventory.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyInventory {
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

    public static int DaysInYear(this DateTime date) {
      var thisYear = new DateTime(date.Year, 1, 1);
      var nextYear = new DateTime(date.Year + 1, 1, 1);

      return (nextYear - thisYear).Days;
    }

    public static int DaysInMonth(this DateTime date) {
      return DateTime.DaysInMonth(date.Year, date.Month);
    }

    public static DateTime GetResolvedDate(this DateTime date, DateGrouping grouping) {
      switch (grouping) {
        case DateGrouping.Week:
          return date.AddDays(-(int)date.DayOfWeek).Date;
        case DateGrouping.Month:
          return new DateTime(date.Year, date.Month, 1);
        case DateGrouping.Year:
          return new DateTime(date.Year, 1, 1);
        case DateGrouping.Day:
        default:
          return new DateTime(date.Year, date.Month, date.Day);
      }
    }

    public static IEnumerable<string> GetConnectionData() {
      // Data Source=(local)\Desarrollo;Initial Catalog=Inventory;User ID=sa;Password=superi6644;
      IEnumerable<string> data = null;
      if (HttpContext.Current.IsDebuggingEnabled) {
        data = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
          .Where(p => p.IndexOf("user", StringComparison.OrdinalIgnoreCase) != 0 && p.IndexOf("password", StringComparison.OrdinalIgnoreCase) != 0);
      }

      return data;
    }
  }
}