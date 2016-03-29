$(document).ready(function () {
  $('#grouping,#percentage').change(function () {
    interestsPercentageRarityActions.updateImage();
    historyManager.setState();
  });

  historyManager.init({
    data: function () {
      return { grouping: $("#grouping").val(), percentage: $("#percentage").is(':checked') };
    },
    title: function (data) {
      return "Interest per Rarity per " + data.grouping + " by " + (data.percentage ? "Percentage" : "Quantity");
    },
    url: function (data) {
      return '/Interests/PercentageRarity?grouping=' + data.grouping + '&percentage=' + data.percentage;
    },
    change: function (data) {
      $("#grouping").val(data.grouping);
      $("#percentage").prop('checked', data.percentage);

      interestsPercentageRarityActions.updateImage();
    }
  });
});

var interestsPercentageRarityActions = function () {
  return {
    updateImage: function () {
      var grp = $("#grouping").val();
      var per = $("#percentage").is(':checked');
      var newImg = '/Interests/PercentageRarityChart?grouping=' + grp + '&percentage=' + per;
      $("img").attr('src', newImg);

      $("#chartTitle").text("Interest per Rarity per " + grp + " by " + (per ? "Percentage" : "Quantity"));
    }
  }
}();