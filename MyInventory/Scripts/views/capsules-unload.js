$(document).ready(function () {
  $("#unloadAll")
    .click(function () {
      var $this = $(this);
      if ($this.is(':checked')) {
        $(".form-control").each(function (index) {
          var valor = $(this).prev().val();
          $(this).val(valor);
          $(this).attr("readonly", true);
        });
      }
      else {
        $(".form-control").each(function (index) {
          $(this).attr("readonly", false);
        });
      }
    });
});