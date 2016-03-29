$(document).ready(function () {
  $('#ItemId').change(function () {
    var uniqueId = $('#ItemId option:selected').data('uniqueid');
    if (uniqueId.length > 0) {
      $('#Code').val(uniqueId).prop('readonly', true);
    }
    else {
      $('#Code').prop('readonly', false);
    }
  });
});