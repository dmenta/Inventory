//#region Groups, Types, Items

function RenderItemDescription(item) {
  var html = '';
  if (item.Level > 0) {
    html += '<span class="nivel' + item.Level.toString() + '">L' + item.Level.toString() + '</span> ';
  }
  html += '<span class="rareza' + item.Rarity.toString() + '">' + item.Nombre + '</span>';

  return html;
}

function RenderType(type) {
  if (type.Cantidad === 0 && type.CantidadCapsulas === 0) {
    return;
  }

  var html = '';
  html += '<div class="col-lg-3 col-md-4 col-sm-6 col-xs-12">';
  html += '<table class="table table-condensed table-striped">';
  html += '<colgroup>';
  html += '<col class="col-lg-6 col-md-6 col-sm-6 col-xs-6" />';
  html += '<col class="col-lg-2 col-md-2 col-sm-2 col-xs-2" />';
  html += '<col class="col-lg-2 col-md-2 col-sm-2 col-xs-2" />';
  html += '<col class="col-lg-2 col-md-2 col-sm-2 col-xs-2" />';
  html += '</colgroup>';
  html += '<thead>';
  html += '<tr>';
  html += '<th>Item</th>';
  html += '<th>Qty</th>';
  html += '<th>Caps</th>';
  html += '<th>Total</th>';
  html += '</thead>';
  html += '<tbody>';

  html += RenderItems(type.Items);

  html += '</tbody>';
  html += '</table>';
  html += '</div>';

  return html;
}

function RenderTypeEdit(type) {
  var html = '';
  html += '<div class="col-lg-5 col-md-4 col-sm-6 col-xs-12">';
  html += '<table class="table table-condensed table-striped">';
  html += '<colgroup>';
  html += '<col class="col-lg-6 col-md-6 col-sm-6 col-xs-6" />';
  html += '<col class="col-lg-2 col-md-2 col-sm-2 col-xs-2" />';
  html += '<col class="col-lg-2 col-md-2 col-sm-2 col-xs-2" />';
  html += '<col class="col-lg-2 col-md-2 col-sm-2 col-xs-2" />';
  html += '</colgroup>';
  html += '<thead>';
  html += '<tr>';
  html += '<th>Item</th>';
  html += '<th>Qty</th>';
  html += '<th>Caps</th>';
  html += '<th>Total</th>';
  html += '<tbody>';

  html += RenderItemsEdit(type.Items);

  html += '</tbody>';
  html += '</table>';
  html += '</div>';

  return html;
}

function RenderItems(items) {
  var rows = '';

  $.each(items, function (key, item) {
    if (item.Cantidad === 0 && item.CantidadCapsulas === 0) {
      return;
    }

    rows += '<tr>';
    rows += '<td class="nombre">';
    rows += RenderItemDescription(item.CurrentItem);
    rows += '</td>';
    rows += '<td class="cantidadItem">' + (item.Cantidad > 0 ? item.Cantidad.toString() : '') + '</td>';
    rows += '<td class="cantidadItem">' + (item.Cantidad > 0 ? '(' + item.CantidadCapsulas.toString() + ')' : '') + '</td>';
    rows += '<td class="cantidadTotal">' + (item.CantidadCapsulas + item.Cantidad).toString() + '</td>';
    rows += '</tr>';
});

  return rows;
}

function RenderItemsEdit(items) {
  var rows = '';
  $.each(items, function (key, item) {
    rows += '<tr>';
    rows += '<td class="nombre">';
    rows += RenderItemDescription(item.CurrentItem);
    rows += '</td>';
    rows += '<td class="cantidadItem">';
    rows += '<input type="text" class="form-control text-right input-sm newQty" min="0" max="2000" data-itemid="';
    rows += item.CurrentItem.ItemID;
    rows += '" data-originalqty="';
    rows += item.Cantidad;
    rows += '" value="';
    rows += item.Cantidad;
    rows += '"/>';
    rows += '</td>';
    rows += '<td class="cantidadItem">';
    rows += '(';
    rows += item.CantidadCapsulas.toString();
    rows += ')';
    rows += '</td>';
    rows += '<td class="cantidadTotal">';
    rows += (item.CantidadCapsulas +item.Cantidad).toString();
    rows += '</td>';
    rows += '</tr>';
});

  return rows;
}

//#endregion 
