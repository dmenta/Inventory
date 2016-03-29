$(document).ready(function () {
  $(document).ajaxStart(function () {
    $('.refresh').prop('disabled', true);
    $("#imgLoading").show();
  });

  $(document).ajaxStop(function () {
    $("#imgLoading").hide();
    $('.refresh').prop('disabled', false);
  });

  $('.refresh').click(function () {
    inventoryDifferenceActions.getDifference();
  });

  inventoryDifferenceActions.getDifference();
});

// ------------- end $(document).ready

var inventoryDifferenceActions = function () {
  return {
    getDifference: function () {
      $.ajax({
        url: '/Inventory/DifferenceData',
        method: 'GET',
        cache: false,
        dataType: "json"
      })
      .done(function (data) {
        $('#title').text(data.OrigUserId + ' - ' + data.DestUserId);

        if (!data) {
          $('#differenceDetail').html('<div class="col-lg-6 alert alert-dismissible alert-success"><button type="button" class="close" data-dismiss="alert">×</button>There are no differences.</div>');
        }
        else {
          var html = '';
          $.each(data.Groups, function (key, group) {
            $.each(group.Types, function (key, type) {
              html += inventoryDifferenceActions.renderTypeDifference(type);
            });
          });

          $('#differenceDetail').html(html);
        }
      })
      .fail(function (requestObject, error, errorThrown) {
        $('#title').empty();
        var message = errorThrown;
        if (requestObject.status === 0) {
          message = 'Possible lost of connection with server';
        }
        $('#differenceDetail').html('<div class="col-lg-6 alert alert-dismissible alert-success"><button type="button" class="close" data-dismiss="alert">×</button>' + message + '</div>');
      });
    },

    renderTypeDifference: function (type) {
      var html = '';

      html += '<div class="col-lg-4 col-md-4 col-sm-6 col-xs-12">';
      html += '<table class="table table-condensed table-striped">';
      html += '<colgroup>';
      html += '<col class="col-lg-4 col-md-4 col-sm-4 col-xs-4" />';
      html += '<col class="col-lg-2 col-md-2 col-sm-2 col-xs-2" />';
      html += '<col class="col-lg-2 col-md-2 col-sm-2 col-xs-2" />';
      html += '<col class="col-lg-2 col-md-2 col-sm-2 col-xs-2" />';
      html += '<col class="col-lg-2 col-md-2 col-sm-2 col-xs-2" />';
      html += '</colgroup>';
      html += '<thead>';
      html += '<tr class="small">';
      html += '<th>Item</th>';
      html += '<th>Me</th>';
      html += '<th>Other</th>';
      html += '<th>D Me</th>';
      html += '<th>D Other</th>';
      html += '</tr>';
      html += '</thead>';
      html += '<tbody>';

      // rows
      $.each(type.Items, function (key, item) {
        html += '<tr>';
        html += '<td class="name">' + RenderItemDescription(item.CurrentItem) + '</td>';
        html += '<td class="itemQuantity">' + item.OrigQuantity.toString() + '</td>';
        html += '<td class="itemQuantity">' + item.DestQuantity.toString() + '</td>';
        html += '<td class="itemQuantity">' + (item.OrigDifference > 0 ? item.OrigDifference.toString() : '') + '</td>';
        html += '<td class="itemQuantity">' + (item.DestDifference > 0 ? item.DestDifference.toString() : '') + '</td>';
        html += '</tr>';
      });

      html += '</tbody>';
      html += '</table>';
      html += '</div>';

      return html;
    }
  };
}();