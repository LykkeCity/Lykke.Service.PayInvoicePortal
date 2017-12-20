var invoices = null;
var sortField = "";
var searchValue = "";
var sortWay = 0;
var pagenumber = 1;
$('.btn_create').on('click', function (e) {
    e.stopPropagation();
    validate(true);
    $('body').addClass('body--menu_opened');
    $('.create.draft').addClass('create--open');
});

$('body').on('click', function () {
    $('body').removeClass('body--menu_opened');
    $('.create.draft').removeClass('create--open');
    $('.create.unpaid').removeClass('create--open');
});
$('.create.draft').on('click', function (e) {
    e.stopPropagation();
});

$(document).ready(function () {

    updateGrid();

    if (generateditem) {
        if (generateditem.Status !== "Draft") {
            showItem(generateditem);
            $('body').addClass('body--menu_opened');
            $('.create.unpaid').addClass('create--open');
        }
    }

    $('#generatebtn').on('click', function (e) {
        $('#Status').val("Unpaid");
        if ($('#Currency').val() == "")
            $('#Currency').val("USD");
        updateGrid();
    });
    $('#draftbtn').on('click', function (e) {
        $('#Status').val("Draft");
        if ($('#Currency').val() == "")
            $('#Currency').val("USD");
        updateGrid();
    });
    $('.showmore').on('click', function (e) {
        pagenumber++;
        updateGrid(null, null, null, true);
    });
    $('#closebtn').on('click', function (e) {
        $('body').removeClass('body--menu_opened');
        $('.create').removeClass('create--open');
    });
    $("#StartDate").datepicker();
    $('.invoices__search').on('click', function () {
        $('.profile_search').toggleClass('vis');
    });
    $('.invoices__row .invoices__cell').on('click', function (sender) {
        var element = sender.target;
        sortfield = "";
        sortWay = (sortWay == 0) ? 1 : 0;
        if (element.className.indexOf("number") !== -1)
            sortfield = "number";
        if (element.className.indexOf("client") !== -1)
            sortfield = "client";
        if (element.className.indexOf("amount") !== -1)
            sortfield = "amount";
        if (element.className.indexOf("currency") !== -1)
            sortfield = "currency";
        if (element.className.indexOf("status") !== -1)
            sortfield = "status";
        if (sortfield !== "")
            updateGrid("", sortfield, sortWay);
    });
    $('.profile_search__button').on('click', function () {
        $('.profile_search').removeClass('vis');
        $('#searchvalue').val("");
        updateGrid();
    });
    $('#searchvalue').on('input', function (e) {
        updateGrid($('#searchvalue').val(), sortField);
    });
    $('#createform').submit(function () {
        var errors = validate();
        return (errors == 0);
    });
});
function validate(clearvalidate) {
    var errors = 0;
    var style = clearvalidate ? "none" : "";
    var inputs = $('#createform input').filter('[require]:visible');
    for (var i = 0; i < inputs.length; i++) {
        if ($(inputs[i]).val() == "") {
            var req = $("[req-for='" + $(inputs[i]).attr("id") + "']");
            $(req).css('display', style);
            if (!clearvalidate)
                errors++;
        }
    }
    return errors;
}
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
        if (keyNames[j] == "StartDate")
            value = value.substr(0, 10);
        if (item)
            item.value = value;
    }
}

function updateGrid(searchValue, sortField, sortway, loadmore) {
    var data = { SearchValue: searchValue, SortField: sortField, Page: pagenumber, SortWay: sortway };
    $.ajax({
        url: "/home/Invoices",
        dataType: 'json',
        type: "POST",
        data: data,
        success: function (gridModel) {
            renderGrid(gridModel, loadmore); //grid need update after deletion item
        }
    });
}
var divtableall = null;
var divtablepaid = null;
var divtableunpaid = null;
var divtabledraft = null;
function renderGrid(gridModel, loadMore) {
    var alllink = document.getElementById("alllink");
    var paidlink = document.getElementById("paidlink");
    var unpaidlink = document.getElementById("unpaidlink");
    var draftlink = document.getElementById("draftlink");

    var paidcnt = gridModel.paidCount;
    var draftcnt = gridModel.draftCount;
    var unpaidcnt = gridModel.unpaidCount;

    var template = document.getElementById("rowtemplate").innerHTML;
    var taball = document.getElementById("all");
    var tabpaid = document.getElementById("paid");
    var tabunpaid = document.getElementById("unpaid");
    var tabdraft = document.getElementById("draft");

    if (!loadMore) {
        taball.innerHTML = "";
        tabpaid.innerHTML = "";
        tabunpaid.innerHTML = "";
        tabdraft.innerHTML = "";

        divtableall = document.createElement("div");
        divtableall.className = "invoices__table";
        taball.appendChild(divtableall);

        divtablepaid = document.createElement("div");
        divtablepaid.className = "invoices__table";
        tabpaid.appendChild(divtablepaid);

        divtableunpaid = document.createElement("div");
        divtableunpaid.className = "invoices__table";
        tabunpaid.appendChild(divtableunpaid);

        divtabledraft = document.createElement("div");
        divtabledraft.className = "invoices__table";
        tabdraft.appendChild(divtabledraft);
    }

    var allstring = "";
    var paidstring = "";
    var unpaidstring = "";
    var draftstring = "";
    invoices = gridModel.data;
    for (var i = 0; i < invoices.length; i++) {
        var tempstr = template;
        var keyNames = Object.keys(invoices[i]);
        for (var j = 0; j < keyNames.length; j++) {
            var value = invoices[i][keyNames[j]];
            if (!value)
                value = "";
            if (keyNames[j] == "Status" && value == "")
                value = "Draft";
            tempstr = tempstr.replace(new RegExp("{{" + keyNames[j] + "}}", 'g'), value);
        }
        switch (invoices[i].Status) {
            case "Paid":
                tempstr = tempstr.replace("{{CssClass}}", "paid");
                tempstr = tempstr.replace("{{DisCssClass}}", "btn--disabled");
                tempstr = tempstr.replace("{{disoption}}", "disabled");
                paidstring += tempstr;
                break;
            case "Unpaid":
                tempstr = tempstr.replace("{{CssClass}}", "unpaid");
                tempstr = tempstr.replace("{{DisCssClass}}", "btn--disabled");
                tempstr = tempstr.replace("{{disoption}}", "disabled");
                unpaidstring += tempstr.replace("{{CssClass}}", "unpaid");
                break;
            case "Draft":
            default:
                tempstr = tempstr.replace("{{disoption}}", "");
                tempstr = tempstr.replace("{{DisCssClass}}", "");
                tempstr = tempstr.replace("{{CssClass}}", "draft");
                draftstring += tempstr.replace("{{CssClass}}", "draft");
                break;
        }
        //tempstr = tempstr.replace("{{DisCssClass}}", "btn--disabled");
        //tempstr = tempstr.replace("{{disoption}}", "disabled");
        allstring += tempstr;
    }
    divtableall.innerHTML += allstring;
    divtablepaid.innerHTML += paidstring;
    divtableunpaid.innerHTML += unpaidstring;
    divtabledraft.innerHTML += draftstring;
    alllink.childNodes[1].innerText = gridModel.allCount;
    draftlink.childNodes[1].innerText = draftcnt;
    paidlink.childNodes[1].innerText = paidcnt;
    unpaidlink.childNodes[1].innerText = unpaidcnt;

    $('.btn.btn--icon').on('click', function (e) {
        e.stopPropagation();
        var invoiceid = $(e.target.parentNode.parentNode.parentNode).attr("invoice");
        $.confirm({
            title: 'Are you sure?',
            content: 'Do you really want to delete this invoice?',
            icon: 'fa fa-question-circle',
            animation: 'scale',
            closeAnimation: 'scale',
            opacity: 0.5,
            buttons: {
                'confirm': {
                    text: 'Yes',
                    btnClass: 'btn-blue',
                    action: function () {
                        $.ajax({
                            url: "/home/deleteinvoice?invoiceId=" + invoiceid,
                            dataType: 'html',
                            success: function (data) {
                                updateGrid(); //grid need update after deletion item
                            }
                        });
                    }
                },
                cancel: function () {

                }
            }
        });
    });

    $('.invoices_item').on('click', function (e) {
        e.stopPropagation();
        var element = e.target;
        if (element.className !== "invoices_item") {
            while (element.className !== "invoices_item") {
                element = element.parentNode;
            }
        }
        var invoiceid = $(element).attr("invoice");
        //if ($(element).has('.invoices_item__status--draft').length !== 0) {
        //    editItem(invoiceid);
        //    $('body').addClass('body--menu_opened');
        //    $('.create.draft').addClass('create--open');
        //}
        //if ($(element).has('.invoices_item__status--unpaid').length !== 0) {
        //    showItem(invoiceid);
        //    $('body').addClass('body--menu_opened');
        //    $('.create.unpaid').addClass('create--open');
        //}

        window.location.href = "/home/invoicedetail/?InvoiceId=" + invoiceid;
    });
}
function showItem(invoice) {
    var currentItem = invoice;
    //for (var i = 0; i < invoices.length; i++) {
    //    if (invoices[i].InvoiceId == invoiceId) {
    //        currentItem = invoices[i];
    //        break;
    //    }
    //}
    var keyNames = Object.keys(currentItem);
    for (var j = 0; j < keyNames.length; j++) {
        var value = currentItem[keyNames[j]];
        if (!value)
            value = "";
        var item = document.getElementById("Unpaid" + keyNames[j]);
        if (keyNames[j] == "StartDate")
            value = value.substr(0, 10);
        if (item)
            item.innerText = value;
        if (keyNames[j] == "InvoiceId") {
            var url = document.getElementById("UnpaidUrl");
            url.innerText = window.location.origin + "/invoice/?invoiceId=" + value;
        }
    }
}