$(document).ready(function () {
    $(".itemDetailPopUp").click(function (e) {
        e.stopPropagation();
        var posX = e.clientX;
        var posY = e.clientY;
        var dialog = $("#dialog");
        dialog.hide();
        $.ajax({
            dataType: "json",
            url: '/Grupos/ItemDetail',
            data: 'id=' + $(this).attr('detailPopupItem'),
            success: function (result) {
                $("#dialog").css({ top: posY + 'px', left: posX + 'px', position: 'absolute' });
                $("#detailLevel").removeClass()
                $("#detailLevel").addClass('nivel' + result.Level);
                if (result.Level > 0) {
                    $("#detailLevel").text('L' + result.Level);
                }
                else {
                    $("#detailLevel").empty();
                }
                $("#detailName").text(result.Name);
                $("#detailRarity").removeClass()
                $("#detailRarity").addClass('rareza' + result.Rarity);
                $("#detailRarity").text(result.RarityName);
                $("#detailImage").attr("src", "/images/" + result.ItemID+'.png');
                $("#detailTypeDescription").text(result.TypeDescription);
                $("#dialog").show(100, function () { });
            }
        });
    });
    $("#dialog").click(function () {
        $(this).hide();
    })
    $(this).click(function () {
        $('#dialog').hide(); //hide the button
    });
});