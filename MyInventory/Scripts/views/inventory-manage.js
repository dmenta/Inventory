$(document).ready(function () {
  $(document).ajaxStart(function () {
    $("#imgLoading").show();
  });

  $(document).ajaxStop(function () {
    $("#imgLoading").hide();
  });

  $('.edit').click(function () {
    inventoryManageActions.editing = true;
    inventoryManageActions.changeState();
  });
  $('.cancel').click(function () {
    inventoryManageActions.editing = false;
    inventoryManageActions.changeState();
  });

  $('.save').click(function () {
    inventoryManageActions.setDisabled();
    inventoryManageActions.save();
  });

  $('#Groups').change(function () {
    inventoryManageActions.editing = false;
    inventoryManageActions.changeState();
  });

  $('#Groups').val(inventoryManageActions.groupId);
  inventoryManageActions.showGroups();

  historyManager.init({
    data: function () {
      var groupSel = $('#Groups');
      return { groupID: groupSel.val(), groupText: $("option:selected", groupSel).text(), editing: inventoryManageActions.editing };
    },
    title: function (data) {
      return '@ViewBag.Title ' + data.groupText + (data.editing ? ' [editing]' : '');
    },
    url: function (data) {
      return '/Inventory/Manage/' + data.groupID + '?editing=' + data.editing.toString();
    },
    change: function (data) {
      $('#Groups').val(data.groupID);
      inventoryManageActions.editing = data.editing;
      inventoryManageActions.showGroups();
    }
  });
});

// ------------- end $(document).ready

var inventoryManageActions = function(){
  return {
    editing: false,
    groupId: null,
    showGroups: function () {
      inventoryManageActions.setDisabled();

      $('#resultado').on('blur', '.newQty', function () {
        var inp = $(this);
        var min = Number(inp.attr('min'));
        var max = Number(inp.attr('max'));
        var newValue = Number(inp.val());
        if (newValue<min){
          newValue = min;
        }
        else if (newValue>max){
          newValue = max;
        }
        else {
          return;
        }

        inp.val(newValue);
      });

      $.ajax({
        url: inventoryManageActions.editing ? '/Inventory/ItemsEdit' : '/Inventory/Items',
        method: 'POST',
        dataType: "json",
        data: { groupId: $('#Groups').val() },
        success: function (data) {
          var nuevosItems = ''

          if (!data.Result) {
            $('#resultado').html('<div class="col-lg-6 alert alert-dismissible alert-success"><button type="button" class="close" data-dismiss="alert">×</button>There are no items in this group</div>');
          }
          else {
            $.each(data.Groups, function (key, group) {
              $.each(group.Types, function (key, type) {
                if (inventoryManageActions.editing) {
                  nuevosItems += RenderTypeEdit(type);
                }
                else {
                  nuevosItems += RenderType(type);
                }
              });
            });

            $('#resultado').html(nuevosItems);
          }

          if (inventoryManageActions.editing) {
            inventoryManageActions.setEditing();
          }
          else {
            inventoryManageActions.setViewing();
            $('#Groups').focus();
          }
        },
        error: function (requestObject, error, errorThrown) {
          alert(errorThrown);
        },
      });
    },
    save: function (){
      var items = [];
      $('.newQty').each(function (key, info) {
        var inp = $(info);
        var id = inp.data("itemid");
        var oldQty = inp.data("originalqty");
        var newQty = inp.val();

        if (parseInt(oldQty) != newQty) {
          item = { "ItemId": id, "Quantity": newQty };
          items.push(item);
        }
      });

      if (items.length == 0) {
        inventoryManageActions.editing = false;
        inventoryManageActions.changeState(false);
      }
      else {
        $.ajax({
          url: '/Inventory/SaveNewQty',
          method: 'POST',
          data: JSON.stringify({ "items": items }),
          contentType: 'application/json',
          traditional: true,
          success: function (data) {
            inventoryManageActions.editing = false;
            inventoryManageActions.changeState(false);
          },
          error: function (requestObject, error, errorThrown) {
            alert(errorThrown);
          },
        });
      }
    },
    setDisabled: function (){
      $('#Groups').prop('disabled', true);
      $('.edit').prop('disabled', true);
      $('.edition').prop('disabled', true);
    },
    setViewing: function (){
      $('#Groups').prop('disabled', false);
      $('.edit').prop('disabled', false).show();
      $('.edition').hide();
    },
    setEditing: function (){
      $('#Groups').prop('disabled', true);
      $('.edit').prop('disabled', true).hide();
      $('.edition').prop('disabled', false).show();
    },
    changeState: function (){
      inventoryManageActions.showGroups();
      historyManager.setState();
    }
  }
}();
