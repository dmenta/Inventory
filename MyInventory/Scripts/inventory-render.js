//#region Groups, Types, Items

function RenderItemDescription(item) {
  var html = '';
  if (item.Level > 0) {
    html += '<span class="level' + item.Level.toString() + '">L' + item.Level.toString() + '</span> ';
  }
  html += '<span class="rarity' + item.Rarity.toString() + '">' + item.Name + '</span>';

  return html;
}

function RenderType(type) {
  if (type.QuantityCantidad === 0 && type.CapsulesQuantity === 0) {
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
    if (item.Quantity === 0 && item.CapsulesQuantity === 0) {
      return;
    }

    rows += '<tr>';
    rows += '<td class="name">';
    rows += RenderItemDescription(item.CurrentItem);
    rows += '</td>';
    rows += '<td class="itemQuantity">' + (item.Quantity > 0 ? item.Quantity.toString() : '') + '</td>';
    rows += '<td class="itemQuantity">' + (item.Quantity > 0 ? '(' + item.CapsulesQuantity.toString() + ')' : '') + '</td>';
    rows += '<td class="totalQuantity">' + (item.CapsulesQuantity + item.Quantity).toString() + '</td>';
    rows += '</tr>';
});

  return rows;
}

function RenderItemsEdit(items) {
  var rows = '';
  $.each(items, function (key, item) {
    rows += '<tr>';
    rows += '<td class="name">';
    rows += RenderItemDescription(item.CurrentItem);
    rows += '</td>';
    rows += '<td class="itemQuantity">';
    rows += '<input type="number" class="form-control text-right input-sm newQty" min="0" max="' + (item.CurrentItem.UniqueId === null || item.CurrentItem.UniqueId.length === 0 ? '2000' : '1') + '" data-itemid="';
    rows += item.CurrentItem.ItemId;
    rows += '" data-originalqty="';
    rows += item.Quantity;
    rows += '" value="';
    rows += item.Quantity;
    rows += '"/>';
    rows += '</td>';
    rows += '<td class="itemQuantity">';
    rows += '(';
    rows += item.CapsulesQuantity.toString();
    rows += ')';
    rows += '</td>';
    rows += '<td class="totalQuantity">';
    rows += (item.CapsulesQuantity + item.Quantity).toString();
    rows += '</td>';
    rows += '</tr>';
});

  return rows;
}

//#endregion 
