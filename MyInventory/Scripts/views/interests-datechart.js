$(document).ready(function () {
  $(document).ajaxStart(function () {
    $("#imgLoading").show();
  });

  $(document).ajaxStop(function () {
    $("#imgLoading").hide();
  });

  $('#grouping,#item,#accumulative').change(function () {
    interestsDateChartActions.updateImage();
    historyManager.setState();
  });

  historyManager.init({
    data: function () {
      return { grouping: $('#grouping').val(), itemId: $('#item').val(), itemName: $('#item option:selected').text(), accumulative: $('#accumulative').is(':checked') };
    },
    title: function (data) {
      return (data.accumulative ? 'Accumulative interests of ' : 'Interests of ') + data.itemName + ' by ' + data.grouping;
    },
    url: function (data) {
      return '/Interests/DateCharts?grouping=' + data.grouping + '&itemID=' + data.itemID + '&accumulative=' + data.accumulative.toString();
    },
    change: function (data) {
      $('#grouping').val(data.grouping);
      $('#item').val(data.itemId);
      $('#accumulative').prop('checked', data.accumulative)

      interestsDateChartActions.updateImage();
    }
  });
});

var interestsDateChartActions = function () {
  return {
    updateImage: function () {
      var grp = $('#grouping').val();
      var item = $('#item').val();
      var acc = $('#accumulative').is(':checked');
      var newImg = '/Interests/InterestsChart?grouping=' + grp + '&itemId=' + item + '&accumulative=' + acc;
      $("img").attr('src', newImg);

      $.ajax({
        url: '/Interests/_ChartTitle',
        method: 'GET',
        cache: false,
        dataType: "html",
        data: { grouping: grp, itemId: item, accumulative: acc },
        success: function (data) {
          $("#chartTitle").html(data);
        }
      });
    }
  }
}();