using System.Web;
using System.Web.Optimization;

namespace MyInventory {
  public class BundleConfig {
    // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
    public static void RegisterBundles(BundleCollection bundles) {
      BundleFileSetOrdering fileOrder = new BundleFileSetOrdering("externalScripts");

      bundles.Add(new ScriptBundle("~/bundles/jquery").Include("~/Scripts/jquery-2.2.1.js*"));

      bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include("~/Scripts/jquery.validate*"));

      bundles.Add(new ScriptBundle("~/bundles/inventory-common").Include(
        "~/Scripts/history-manager.js*",
        "~/Scripts/inventory-render.js*"));

      bundles.Add(new StyleBundle("~/bundles/bootstrap").Include(
          "~/Scripts/bootstrap.js",
          "~/Scripts/respond.js"));

      bundles.Add(new ScriptBundle("~/bundlesview/inventory-difference").Include("~/Scripts/views/inventory-difference.js*"));
      bundles.Add(new ScriptBundle("~/bundlesview/inventory-index").Include("~/Scripts/views/inventory-index.js*"));
      bundles.Add(new ScriptBundle("~/bundlesview/inventory-manage").Include("~/Scripts/views/inventory-manage.js*"));

      bundles.Add(new ScriptBundle("~/bundlesview/capsules-index").Include("~/Scripts/views/capsules-index.js*"));

      // Use the development version of Modernizr to develop with and learn from. Then, when you're
      // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
      //bundles.Add(new ScriptBundle("~/bundles/modernizr").Include("~/Scripts/modernizr-*"));

      bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/bootstrap/bootstrap.css",
                "~/Content/Site.css"));

      // Set EnableOptimizations to false for debugging. For more information,
      // visit http://go.microsoft.com/fwlink/?LinkId=301862
      BundleTable.EnableOptimizations = true;
    }
  }
}
