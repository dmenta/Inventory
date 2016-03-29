$(document).ready(function () {
  $("#btnAcciones,#btnAccionesDropdown").prop('disabled', true);

  $('a.replacedLink').click(function (e) {
    var selected = $(".selection:checked");
    if (selected.length == 0) {
      e.preventDefault();
      alert("Please select the capsule on which you want to perform the action.");
      return;
    }
    if ($(this).attr("data-needspawn") == "true" && selected.attr("data-spawn") == "false") {
      e.preventDefault();
      alert("This action is not valid for this type of capsule.");
      return;
    }

    $(this).attr('href', $(this).attr("data-url").replace("IdCapsula", selected.attr("name")));
  });

  $.ajax({
    url: '/Capsules/CapsulesSummary',
    method: 'GET',
    dataType: "json",
    cache: false,
    success: function (data) {
      $('h2:first').text('Capsules (' + data.length + ')');

      var html = '';

      $.each(data, function (key, capsule) {
        html += '<tr>';
        html += '<td class="text-center"><input name="' + capsule.CapsuleId + '" data-spawn="' + capsule.Properties.PaysInterests + '" type="checkbox" class="selection" /></td>';

        if (capsule.Properties.IsKeyLocker) {
          html += '<td class="capsuleCode ' + capsule.Properties.UniqueId + '"><a href="/Capsules/List/' + capsule.CapsuleId + '">Key Locker</a></td>';
        }
        else {
          html += '<td class="capsuleCode ' + (capsule.Properties.PaysInterests ? 'spawnable' : 'noSpawn') + '"><a href="/Capsules/List/' + capsule.CapsuleId + '">' + capsule.Code + '</a></td>';
        }

        html += ' <td class=" text-nowrap">' + ((capsule.Name != null && capsule.Name.length > 0) ? capsule.Name : RenderItemDescription(capsule.ItemInside.CurrentItem)) + '</td>';
        html += '<td class="totalQuantity">' + (capsule.TotalQuantity > 100 ? '<strong class="text-danger">' + capsule.TotalQuantity + '</strong>' : capsule.TotalQuantity) + '</td>';
        html += '</tr>';
      });

      $('#capsules tbody').append(html);
    }
  });

  $('#capsules tbody').on('click', 'input.selection', function (e) {
    e.stopPropagation();

    $("input.selection").not(this).prop('checked', false);

    if ($(this).is(':checked')) {
      $("#btnAcciones,#btnAccionesDropdown").prop('disabled', false);
    }
    else {
      $("#btnAcciones,#btnAccionesDropdown").prop('disabled', true);
    }
  });

  $('#capsules tbody').on('click', 'tr', function (e) {
    if (e.target.tagName != 'A') {
      $(':checkbox', this).trigger('click');
    }
  });
});