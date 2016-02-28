function RenderItemDescription(item) {
  var html = '';
  if (item.Level > 0) {
    html += '<span class="nivel' + item.Level + ' itemDetailPopUp" detailpopupitem="' + item.ItemID + '">L' + item.Level + '</span> ';
  }
  html += '<span class="rareza' + item.Rarity + ' itemDetailPopUp" detailPopupItem="' + item.ItemID + '">' + item.Nombre + '</span>';

  return html;
}

function RenderType(tipo) {
  if (tipo.Cantidad == 0 & tipo.CantidadCapsulas == 0) {
    return;
  }

  var h = '<div class="col-lg-3 col-md-4 col-sm-6 col-xs-12">'
  + '<table class="table table-condensed table-striped">'
  + '<colgroup>'
  + '<col class="col-lg-6 col-md-6 col-sm-6 col-xs-6" />'
  + '<col class="col-lg-2 col-md-2 col-sm-2 col-xs-2" />'
  + '<col class="col-lg-2 col-md-2 col-sm-2 col-xs-2" />'
  + '<col class="col-lg-2 col-md-2 col-sm-2 col-xs-2" />'
  + '</colgroup>'
  + '<thead>'
  + '<tr>'
  + '<th>Item</th>'
  + '<th>Qty</th>'
  + '<th>Caps</th>'
  + '<th>Total</th>'
  + '</thead>'
  + '<tbody>';

  h += RenderItems(tipo.Items)

  h += '</tbody>'
  + '</table>'
  + '</div>';

  return h;
}

function RenderTypeEdit(tipo) {
  var h = '<div class="col-lg-5 col-md-4 col-sm-6 col-xs-12">'
  + '<table class="table table-condensed table-striped">'
  + '<colgroup>'
  + '<col class="col-lg-6 col-md-6 col-sm-6 col-xs-6" />'
  + '<col class="col-lg-2 col-md-2 col-sm-2 col-xs-2" />'
  + '<col class="col-lg-2 col-md-2 col-sm-2 col-xs-2" />'
  + '<col class="col-lg-2 col-md-2 col-sm-2 col-xs-2" />'
  + '</colgroup>'
  + '<thead>'
  + '<tr>'
  + '<th>Item</th>'
  + '<th>Qty</th>'
  + '<th>Caps</th>'
  + '<th>Total</th>'
  + '<tbody>';

  h += RenderItemsEdit(tipo.Items)

  h += '</tbody>'
  + '</table>'
  + '</div>';

  return h;
}

function RenderItems(items) {
  var rows = '';

  $.each(items, function (key, item) {
    if (item.Cantidad == 0 & item.CantidadCapsulas == 0) {
      return;
    }

    rows += '<tr>'
    + '<td class="nombre">'
    + RenderItemDescription(item.CurrentItem)
    + '</td>'
    + '<td class="cantidadItem">'
    if (item.Cantidad > 0) {
      rows += item.Cantidad.toString();
    }
    rows += '</td>'
    + '<td class="cantidadItem">'
    if (item.CantidadCapsulas > 0) {
      rows += '(' + item.CantidadCapsulas.toString() + ')';
    }
    rows += '</td>'
    + '<td class="cantidadTotal">'
    + (item.CantidadCapsulas + item.Cantidad).toString()
    + '</td>'
    + '</tr>';
  });

  return rows;
}

function RenderItemsEdit(items) {
  var rows = '';
  $.each(items, function (key, item) {
    rows += '<tr>'
    + '<td class="nombre">'
    + RenderItemDescription(item.CurrentItem)
    + '</td>'
    + '<td class="cantidadItem">'
    + '<input type="text" class="form-control text-right input-sm newQty" min="0" max="2000" data-itemid="' + item.CurrentItem.ItemID + '" data-originalqty="' + item.Cantidad + '" value="' + item.Cantidad + '"/>'
    + '</td>'
    + '<td class="cantidadItem">'
    + '(' + item.CantidadCapsulas.toString() + ')'
    +'</td>'
    + '<td class="cantidadTotal">'
    + (item.CantidadCapsulas + item.Cantidad).toString()
    + '</td>'
    + '</tr>';
  });

  return rows;
}
