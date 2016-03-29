$(document).ready(function () {
  $.ajax({
    url: '/Inventory/GroupsTotals',
    method: 'GET',
    dataType: "json",
    cache: false
  })
  .done(function (data) {
    var capsulesGrandTotal = 0;
    var grandTotal = 0;
    var html = '';

    $.each(data, function (key, group) {
      // to footer (only one)
      if (group.GroupID === 'G_ALL') {
        capsulesGrandTotal = group.CapsulesTotal;
        grandTotal = group.TotalQuantity;

        $('#groupTotals tfoot').append(inventoryIndexActions.renderGroupSummary(group));
      }
      else {
        html += inventoryIndexActions.renderGroupSummary(group);
      }
    });

    $('#groupTotals tbody').append(html);

    if ((capsulesGrandTotal + grandTotal) > 2000) {
      $('#alertas').append('<div class="alert alert-dismissible alert-danger"><button type="button" class="close" data-dismiss="alert">×</button>Total Items is greater than the maximum allowed (2000).</div>');
    }
  });
});

// ------------- end $(document).ready
var inventoryIndexActions = function () {
  return {
    renderGroupSummary: function (group) {
      var html = '<tr>';
      html += '<td><a href="/Inventory/Manage/' + group.GroupId + '?editing=false">' + group.GroupName + '</a></td>';
      html += '<td class="itemQuantity">' + group.TotalQuantity + '</td>';
      html += '<td class="capsuleQuantity">' + group.CapsulesTotal + '</td>';
      html += '<td class="text-center"><a href="/Inventory/Manage/' + group.GroupId + '?editing=true">Edit</a></td>';
      html += '</tr>';

      return html;
    }
  };
}();