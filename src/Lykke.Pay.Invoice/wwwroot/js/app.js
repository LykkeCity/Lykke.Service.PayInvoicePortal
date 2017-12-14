$('.btn_create').on('click', function(e) {

  e.stopPropagation();

  $('body').addClass('body--menu_opened');
  $('.create').addClass('create--open');
});

$('body').on('click', function() {
  $('body').removeClass('body--menu_opened');
  $('.create').removeClass('create--open');

});
$('.create').on('click', function(e) {
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
});
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
            tempstr = tempstr.replace("{{" + keyNames[j] + "}}", value);

            if (search != null)
                if (!invoicevisible && value.toString().indexOf(search) !== -1 && keyNames[j] !== "InvoiceId")
                    invoicevisible = true;
        }
        if (invoicevisible) {
            allstring += tempstr;
            switch (invoices[i].Status) {
                case "Paid":
                    paidcnt++;
                    paidstring += tempstr;
                    break;
                case "Unpaid":
                    unpaidcnt++;
                    unpaidstring += tempstr;
                    break;
                case "Draft":
                    draftcnt++;
                    draftstring += tempstr;
                    break;
            }
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
}