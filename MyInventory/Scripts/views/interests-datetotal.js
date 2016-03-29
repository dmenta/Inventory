$(document).ready(function () {
  $(document).ajaxStart(function () {
    $("#imgLoading").show();
  });

  $(document).ajaxStop(function () {
    $("#imgLoading").hide();
  });

  $("#grouping").change(function () {
    interestsDateTotalActions.updateTable();
    historyManager.setState();
  });

  $('#periods').focus(function () {
    var inp = $(this);
    inp.data('current', inp.val());
  });

  $('#periods').change(function () {
    var inp = $(this);
    var newVal = Number(inp.val());
    if (newVal >= 0 && Math.floor(newVal) == newVal && newVal <= 2000) {
      interestsDateTotalActions.updateTable();
      historyManager.setState();
    }
    else {
      inp.val(inp.data('current'));
    }
  });

  interestsDateTotalActions.updateTable();

  historyManager.init({
    data: function () {
      return { grouping: $("#grouping").val(), periods: $('#periods').val() };
    },
    title: function (data) {
      return 'Interests of last ' + data.periods.toString() + ' ' + data.grouping;
    },
    url: function (data) {
      return '/Interests/DateTotal?grouping=' + data.grouping + '&periods=' + data.periods.toString();
    },
    change: function (data) {
      $("#grouping").val(data.grouping);
      $("#periods").val(data.periods);

      interestsDateTotalActions.updateTable();
    }
  });
});

var interestsDateTotalActions = function () {
  return {
    updateTable: function () {
      var grp = $("#grouping").val();
      var per = $('#periods').val();

      $.ajax({
        url: '/Interests/ByItemDate',
        method: 'GET',
        cache: false,
        dataType: 'json',
        data: { grouping: grp, periods: per },
        success: function (data) {
          $("#tableContainer").html(interestsDateTotalActions.renderInterests(data));
        }
      });
    },
    renderInterests: function (data) {
      var html = '';
      html += '<table class="table table-condensed table-striped table-hover" style="font-size:0.9em">';
      html += '<thead>';
      html += ' <tr>';
      html += '<th class="corner rotate"><div><span></span></div></th>';
      html += '<th class="rotate"><div><span>Caps</span></div></th>';
      html += '<th class="rotate"><div><span>Qty</span></div></th>';
      html += '<th class="rotate"><div><span>Avg</span></div></th>';
      $.each(data.Totals, function (key, item) {
        html += '<th class="text-nowrap rotate"><div><span>' + RenderItemDescription(item.CurrentItem) + '</span></div></th>';
      });
      html += '</tr>';
      html += ' </thead>';
      html += '<tfoot>';
      html += ' <tr>';
      html += '<td class="fecha">Total</td>';
      html += '<td class="totalQuantity"></td>';
      html += '<td class="totalQuantity">' + data.TotalItems + '</td>';
      html += '<td class="totalQuantity"></td>';
      $.each(data.Totals, function (key, total) {
        html += '<td class="itemQuantity">' + total.Quantity + '</td>';
      });
      html += '</tr>';
      html += '</tfoot>';
      html += '<tbody>';
      $.each(data.DateInfo, function (key, info) {
        html += '<tr>';
        html += '<td class="fecha text-nowrap">' + info.FormattedDate + '</td>';
        html += '<td class="totalQuantity">' + info.TotalCapsules + '</td>';
        html += '<td class="totalQuantity">' + info.TotalItems + '</td>';
        html += '<td class="totalQuantity">' + info.Average.toFixed(2) + '</td>';
        $.each(data.Items, function (key, itemId) {
          var quantity = info.Items[itemId];
          if (quantity == null || quantity === 0) {
            html += '<td></td>';
            return;
          }
          if (quantity == data.Highests[itemId]) {
            html += '<td class="itemQuantity highest"><strong>' + quantity + '</strong></td>';
          }
          else {
            html += '<td class="itemQuantity">' + quantity + '</td>';
          }
        });
        html += '</tr>';
      });
      html += '</tbody>';
      html += '</table>';

      return html;
    }
  }
}();


