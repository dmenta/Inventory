//#region Groups, Types, Items

function RenderItemDescription(item) {
  var html = [];
  if (item.Level > 0) {
    html.push('<span class="nivel');
    html.push(item.Level);
    html.push(' itemDetailPopUp" detailpopupitem="');
    html.push(item.ItemID);
    html.push('">L');
    html.push(item.Level);
    html.push('</span> ');
  }
  html.push('<span class="rareza');
  html.push(item.Rarity);
  html.push(' itemDetailPopUp" detailPopupItem="');
  html.push(item.ItemID);
  html.push('">');
  html.push(item.Nombre);
  html.push('</span>');

  return html.join('');
}

function RenderType(type) {
  if (type.Cantidad == 0 & type.CantidadCapsulas == 0) {
    return;
  }

  var html = [];
  html.push('<div class="col-lg-3 col-md-4 col-sm-6 col-xs-12">');
  html.push('<table class="table table-condensed table-striped">');
  html.push('<colgroup>');
  html.push('<col class="col-lg-6 col-md-6 col-sm-6 col-xs-6" />');
  html.push('<col class="col-lg-2 col-md-2 col-sm-2 col-xs-2" />');
  html.push('<col class="col-lg-2 col-md-2 col-sm-2 col-xs-2" />');
  html.push('<col class="col-lg-2 col-md-2 col-sm-2 col-xs-2" />');
  html.push('</colgroup>');
  html.push('<thead>');
  html.push('<tr>');
  html.push('<th>Item</th>');
  html.push('<th>Qty</th>');
  html.push('<th>Caps</th>');
  html.push('<th>Total</th>');
  html.push('</thead>');
  html.push('<tbody>');

  html.push(RenderItems(type.Items));

  html.push('</tbody>');
  html.push('</table>');
  html.push('</div>');

  return html.join('');
}

function RenderTypeEdit(type) {
  var html = [];
  html.push('<div class="col-lg-5 col-md-4 col-sm-6 col-xs-12">');
  html.push('<table class="table table-condensed table-striped">');
  html.push('<colgroup>');
  html.push('<col class="col-lg-6 col-md-6 col-sm-6 col-xs-6" />');
  html.push('<col class="col-lg-2 col-md-2 col-sm-2 col-xs-2" />');
  html.push('<col class="col-lg-2 col-md-2 col-sm-2 col-xs-2" />');
  html.push('<col class="col-lg-2 col-md-2 col-sm-2 col-xs-2" />');
  html.push('</colgroup>');
  html.push('<thead>');
  html.push('<tr>');
  html.push('<th>Item</th>');
  html.push('<th>Qty</th>');
  html.push('<th>Caps</th>');
  html.push('<th>Total</th>');
  html.push('<tbody>');

  html.push(RenderItemsEdit(type.Items));

  html.push('</tbody>');
  html.push('</table>');
  html.push('</div>');

  return html.join('');
}

function RenderItems(items) {
  var rows = [];

  $.each(items, function (key, item) {
    if (item.Cantidad == 0 & item.CantidadCapsulas == 0) {
      return;
    }

    rows.push('<tr>');
    rows.push('<td class="nombre">');
    rows.push(RenderItemDescription(item.CurrentItem));
    rows.push('</td>');
    rows.push('<td class="cantidadItem">');
    if (item.Cantidad > 0) {
      rows.push(item.Cantidad.toString());
    }
    rows.push('</td>');
    rows.push('<td class="cantidadItem">');
    if (item.CantidadCapsulas > 0) {
      rows.push('(');
      rows.push(item.CantidadCapsulas.toString());
      rows.push(')');
    }
    rows.push('</td>');
    rows.push('<td class="cantidadTotal">');
    rows.push((item.CantidadCapsulas + item.Cantidad).toString());
    rows.push('</td>');
    rows.push('</tr>');
  });

  return rows.join('');
}

function RenderItemsEdit(items) {
  var rows = [];
  $.each(items, function (key, item) {
    rows.push('<tr>');
    rows.push('<td class="nombre">');
    rows.push(RenderItemDescription(item.CurrentItem));
    rows.push('</td>');
    rows.push('<td class="cantidadItem">');
    rows.push('<input type="text" class="form-control text-right input-sm newQty" min="0" max="2000" data-itemid="');
    rows.push(item.CurrentItem.ItemID);
    rows.push('" data-originalqty="');
    rows.push(item.Cantidad);
    rows.push('" value="');
    rows.push(item.Cantidad);
    rows.push('"/>');
    rows.push('</td>');
    rows.push('<td class="cantidadItem">');
    rows.push('(');
    rows.push(item.CantidadCapsulas.toString());
    rows.push(')');
    rows.push('</td>');
    rows.push('<td class="cantidadTotal">');
    rows.push((item.CantidadCapsulas + item.Cantidad).toString());
    rows.push('</td>');
    rows.push('</tr>');
  });

  return rows.join('');
}

//#endregion 
