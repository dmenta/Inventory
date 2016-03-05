/// Ejemplo

/*<body>
<p>Estas en la página<span id="coord">5</span>.</p>
<p>
 <a href="?x=6" onclick="return go(1);">Ir a página 6</a> o
 <a href="?x=4" onclick="return go(-1);">Volver a página 4</a>?
</p>
</body>
*/

var currentPage = 5; // Arrancamos desde la pagina 5

function go(d) {
  event.preventDefault();
  setupPage(currentPage + d);
  history.pushState(currentPage, 'Página' + currentPage.toString(), '?x=' + currentPage);

  return false;
}
onpopstate = function (event) {
  setupPage(event.state);
}
function setupPage(page) {
  currentPage = page;
  document.title = 'Página ' + currentPage;
  document.getElementById('coord').textContent = currentPage;
  document.links[0].href = '?x=' + (currentPage + 1);
  document.links[0].textContent = 'Ir a página ' + (currentPage + 1);
  document.links[1].href = '?x=' + (currentPage - 1);
  document.links[1].textContent = 'Volver a página ' + (currentPage - 1);
}