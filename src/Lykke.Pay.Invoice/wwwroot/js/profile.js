$('.btn_create').on('click', function (e) {

    e.stopPropagation();

    $('body').addClass('body--menu_opened');
    $('.create').addClass('create--open');
});

$('body').on('click', function () {
    $('body').removeClass('body--menu_opened');
    $('.create').removeClass('create--open');

});
$('.create').on('click', function (e) {
    e.stopPropagation();
});
$(document).ready(function () {
    renderGrid();
    $('#searchfield').on('input', function (e) {
        renderGrid($('#searchfield').val());
    });
    $('#generatebtn').on('click', function (e) {
        $('#Status').val("Unpaid");
    });
    $('#draftbtn').on('click', function (e) {
        $('#Status').val("Draft");
    });
    $('#closebtn').on('click', function (e) {
        $('body').removeClass('body--menu_opened');
        $('.create').removeClass('create--open');

    });
});
function editItem(invoiceId) {
    var currentItem = null;
    for (var i = 0; i < invoices.length; i++) {
        if (invoices[i].InvoiceId == invoiceId) {
            currentItem = invoices[i];
            break;
        }
    }
    var keyNames = Object.keys(currentItem);
    for (var j = 0; j < keyNames.length; j++) {
        var value = currentItem[keyNames[j]];
        if (!value)
            value = "";
        var item = document.getElementById(keyNames[j]);
        if (item)
            item.value = value;
    }
}
function renderGrid(search) {
    var alllink = document.getElementById("alllink");
    var paidlink = document.getElementById("paidlink");
    var unpaidlink = document.getElementById("unpaidlink");
    var draftlink = document.getElementById("draftlink");

    var paidcnt = 0, draftcnt = 0, unpaidcnt = 0;

    var template = document.getElementById("rowtemplate").innerHTML;
    var taball = document.getElementById("all");
    taball.innerHTML = "";
    var divtableall = document.createElement("div");
    divtableall.className = "invoices__table";
    taball.appendChild(divtableall);

    var tabpaid = document.getElementById("paid");
    tabpaid.innerHTML = "";
    var divtablepaid = document.createElement("div");
    divtablepaid.className = "invoices__table";
    tabpaid.appendChild(divtablepaid);

    var tabunpaid = document.getElementById("unpaid");
    tabunpaid.innerHTML = "";
    var divtableunpaid = document.createElement("div");
    divtableunpaid.className = "invoices__table";
    tabunpaid.appendChild(divtableunpaid);

    var tabdraft = document.getElementById("draft");
    tabdraft.innerHTML = "";
    var divtabledraft = document.createElement("div");
    divtabledraft.className = "invoices__table";
    tabdraft.appendChild(divtabledraft);
    var invoicevisible = (search != null) ? false : true;

    var allstring = "";
    var paidstring = "";
    var unpaidstring = "";
    var draftstring = "";
    for (var i = 0; i < invoices.length; i++) {
        invoicevisible = (search != null) ? false : true;
        var tempstr = template;
        var keyNames = Object.keys(invoices[i]);
        for (var j = 0; j < keyNames.length; j++) {
            var value = invoices[i][keyNames[j]];
            if (!value)
                value = "";
            if (keyNames[j] == "Status" && value == "")
                value = "Draft";
            tempstr = tempstr.replace("{{" + keyNames[j] + "}}", value);

            if (search != null)
                if (!invoicevisible && value.toString().indexOf(search) !== -1 && keyNames[j] !== "InvoiceId")
                    invoicevisible = true;
        }
        if (invoicevisible) {
            switch (invoices[i].Status) {
                case "Paid":
                    paidcnt++;
                    tempstr = tempstr.replace("{{CssClass}}", "paid");
                    paidstring += tempstr;
                    break;
                case "Unpaid":
                    unpaidcnt++;

                    tempstr = tempstr.replace("{{CssClass}}", "unpaid");
                    unpaidstring += tempstr.replace("{{CssClass}}", "unpaid");
                    break;
                case "Draft":
                default:
                    draftcnt++;
                    tempstr = tempstr.replace("{{disoption}}", "");
                    tempstr = tempstr.replace("{{DisCssClass}}", "");
                    tempstr = tempstr.replace("{{CssClass}}", "draft");
                    draftstring += tempstr.replace("{{CssClass}}", "draft");
                    break;
            }
            tempstr = tempstr.replace("{{DisCssClass}}", "btn--disabled");
            tempstr = tempstr.replace("{{disoption}}", "disabled");
            allstring += tempstr;
        }
    }
    divtableall.innerHTML = allstring;
    divtablepaid.innerHTML = paidstring;
    divtableunpaid.innerHTML = unpaidstring;
    divtabledraft.innerHTML = draftstring;
    alllink.childNodes[1].innerText = invoices.length;
    draftlink.childNodes[1].innerText = draftcnt;
    paidlink.childNodes[1].innerText = paidcnt;
    unpaidlink.childNodes[1].innerText = unpaidcnt;

    $('.btn.btn--icon').on('click', function (e) {
        e.stopPropagation();
        var invoiceid = $(e.target.parentNode.parentNode.parentNode).attr("invoice");
        $.ajax({
            url: "/home/deleteinvoice?invoiceId=" + invoiceid,
            dataType: 'html',
            success: function (data) {
                renderGrid(); //grid need update after deletion item
            }
        });
    });

    $('.invoices_item').on('click', function (e) {
        e.stopPropagation();
        var element = e.target;
        if (element.className != "invoices_item") {
            while (element.className != "invoices_item") {
                element = element.parentNode;
            }
        }
        var invoiceid = $(element).attr("invoice");
        if ($(element).has('.invoices_item__status--draft').length !== 0) {
            editItem(invoiceid);
            $('body').addClass('body--menu_opened');
            $('.create').addClass('create--open');
        }
    });
}