﻿var pagenumber = 1;
var filter = {};
filter.sortField = "";
filter.searchValue = "";
filter.sortWay = 0;
filter.period = 0;
var updatedStatusCnt = 0;
var statuses = ["All", "Paid", "Unpaid", "Draft", "Removed", "InProgress", "Overpaid", "Underpaid", "LatePaid"];

function updateBalance() {
    $.ajax({
        url: "/home/Balance",
        dataType: 'json',
        type: "POST",
        success: function (balance) {
            $('.balance__quantity').text(balance);
        }
    });
}

function validate(clearvalidate) {
    var errors = 0;
    var style = clearvalidate ? "none" : "";
    var inputs = $('#createform input').filter('[require]:visible');
    for (var i = 0; i < inputs.length; i++) {
        if (clearvalidate)
            $(inputs[i]).val("");
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
        if (keyNames[j] === "createdDate" || keyNames[j] === "dueDate")
            value = value.substr(0, 10);
        if (item)
            item.value = value;
    }
}

function updateGrid(loadmore) {
    var serverlink = "/home/Invoices";
    var data = { Filter: filter, Page: pagenumber };
    for (var i = 0; i < statuses.length; i++) {
        filter.status = statuses[i];
        $.ajax({
            url: serverlink,
            dataType: 'json',
            type: "POST",
            data: data,
            success: function (model) {

                if (model.filter.status === "All")
                    renderGridHeader(model);
                renderStatus(model, loadmore);
                updatedStatusCnt++;
                if (updatedStatusCnt === statuses.length)
                    setEvents();
            }
        });
    }
}

function setEvents() {
    $('.btn.btn--icon.delete').on('click', function (e) {
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
                                updateGrid();
                            }
                        });
                    }
                },
                cancel: function () {

                }
            }
        });
    });
    updatedStatusCnt = 0;
}

function renderGridHeader(model) {
    var alllink = document.getElementById("alllink");
    var paidlink = document.getElementById("paidlink");
    var unpaidlink = document.getElementById("unpaidlink");
    var draftlink = document.getElementById("draftlink");
    var removedlink = document.getElementById("removedlink");
    var latepaidlink = document.getElementById("latepaidlink");
    var overpaidlink = document.getElementById("overpaidlink");
    var inprogresslink = document.getElementById("inprogresslink");
    var underpaidlink = document.getElementById("underpaidlink");

    alllink.childNodes[1].innerText = model.header.allCount;
    draftlink.childNodes[1].innerText = model.header.draftCount;
    paidlink.childNodes[1].innerText = model.header.paidCount;
    unpaidlink.childNodes[1].innerText = model.header.unpaidCount;
    removedlink.childNodes[1].innerText = model.header.removedCount;
    inprogresslink.childNodes[1].innerText = model.header.inProgressCount;
    underpaidlink.childNodes[1].innerText = model.header.underpaidCount;
    latepaidlink.childNodes[1].innerText = model.header.latePaidCount;
    overpaidlink.childNodes[1].innerText = model.header.overpaidCount;
}

function renderStatus(model, loadmore) {
    var tabdiv = null;
    if (model.filter.status === null)
        model.filter.status = "all";
    var allstring = "";
    var template = document.getElementById("rowtemplate").innerHTML;
    var tabstatus = document.getElementById(model.filter.status.toLowerCase());
    if (tabstatus.getElementsByClassName("invoices__table").length !== 1) {
        tabdiv = document.createElement("div");
        tabdiv.className = "invoices__table";
        tabstatus.appendChild(tabdiv);
        tabstatus.childNodes[0].appendChild(tabdiv);
    } else {
        tabdiv = tabstatus.getElementsByClassName("invoices__table")[0];
    }
    var invoices = model.data;
    for (var i = 0; i < invoices.length; i++) {
        var tempstr = template;
        var keyNames = Object.keys(invoices[i]);
        for (var j = 0; j < keyNames.length; j++) {
            var value = invoices[i][keyNames[j]];
            if (value == null)
                value = "";
            if (keyNames[j] == "status" && value == "")
                value = "Draft";
            if (keyNames[j] === "createdDate" || keyNames[j] === "dueDate")
                value = value.substr(0, 10);
            tempstr = tempstr.replace(new RegExp("{{" + keyNames[j] + "}}", 'g'), value);
        }
        switch (invoices[i].status) {
            case "Paid":
                tempstr = tempstr.replace("{{cssClass}}", "paid");
                tempstr = tempstr.replace("{{disCssClass}}", "btn--disabled");
                tempstr = tempstr.replace("{{disoption}}", "disabled");
                break;
            case "Unpaid":
                tempstr = tempstr.replace("{{cssClass}}", "unpaid");
                tempstr = tempstr.replace("{{disCssClass}}", "");
                tempstr = tempstr.replace("{{disoption}}", "");
                break;
            case "Draft":
                tempstr = tempstr.replace("{{disoption}}", "");
                tempstr = tempstr.replace("{{disCssClass}}", "");
                tempstr = tempstr.replace("{{cssClass}}", "draft");
                break;
            case "Removed":
                tempstr = tempstr.replace("{{cssClass}}", "draft");
                tempstr = tempstr.replace("{{disCssClass}}", "btn--disabled");
                tempstr = tempstr.replace("{{disoption}}", "disabled");
            case "LatePaid":
            case "OverPaid":
            case "UnderPaid":
            default:
                tempstr = tempstr.replace("{{cssClass}}", "red");
                tempstr = tempstr.replace("{{disCssClass}}", "btn--disabled");
                tempstr = tempstr.replace("{{disoption}}", "disabled");
                break;
        }
        allstring += tempstr;
    }
    if (loadmore)
        tabdiv.innerHTML += allstring;
    else
        tabdiv.innerHTML = allstring;

    $('.invoices_item').on('click', function (e) {
        e.stopPropagation();
        var element = e.target;
        if (element.className !== "invoices_item") {
            while (element.className !== "invoices_item") {
                element = element.parentNode;
            }
        }
        var invoiceid = $(element).attr("invoice");
        window.location.href = "/home/invoicedetail/?InvoiceId=" + invoiceid;
    });
    var showmore = document.getElementsByClassName("showmore " + model.filter.status)[0];
    showmore.style.display = (model.pageCount !== 0 && model.pageCount > pagenumber) ? "" : "none";
}

function showItem(invoice) {
    var currentItem = invoice;
    var keyNames = Object.keys(currentItem);
    for (var j = 0; j < keyNames.length; j++) {
        var value = currentItem[keyNames[j]];
        if (!value)
            value = "";
        var item = document.getElementById("Unpaid" + keyNames[j]);
        if (keyNames[j] === "Amount") {
            value = currentItem[keyNames[j]] + " " + currentItem["AssetId"];
        }
        if (keyNames[j] === "CreatedDate" || keyNames[j] === "DueDate")
            value = value.substr(0, 10);
        if (item)
            item.innerText = value;
        if (keyNames[j] == "Id") {
            var url = document.getElementById("UnpaidUrl");
            url.innerText = window.location.origin + "/invoice/" + value;
        }

    }
    //$('#UnpaidInvoiceNumberHeader').text(" Invoice #" + invoice.InvoiceNumber);
}
function setTooltip(message) {
    $('.create__item-copy')
        .attr('title', message);
    $('.create__item-copy').tooltip("open");
}

var _validFileExtensions = [".pdf", ".doc", ".docx", ".xls", ".xlsx"];
function setActiveTab() {
    var currentTab = "#all";
    if (window.location.href.indexOf("#") !== -1)
        currentTab = window.location.href.substr(window.location.href.indexOf("#"));
    else {
        currentTab = getCookie("currenttab");
    }
    if (currentTab) {
        $('li[role="presentation"]').removeClass("active");
        $('li[role="presentation"] a[href="' + currentTab + '"]').tab('show');;
        window.location.href = currentTab;
    }
}
function getCookie(name) {
    var matches = document.cookie.match(new RegExp(
        "(?:^|; )" + name.replace(/([\.$?*|{}\(\)\[\]\\\/\+^])/g, '\\$1') + "=([^;]*)"
    ));
    return matches ? decodeURIComponent(matches[1]) : undefined;
}
$(document).ready(function (e) {
    updateGrid();
    updateBalance();
    setActiveTab();

    if (generateditem) {
        if (generateditem.Status !== "Draft") {

            showItem(generateditem);
            $('body').addClass('body--menu_opened');
            $('.create.unpaid').addClass('create--open');
            generateditem = null;
        }
    }

    $('#generatebtn').on('click', function (e) {
        $('#Status').val("Unpaid");
        if ($('#Currency').val() == "")
            $('#Currency').val("CHF");
        updateGrid();
    });
    $('a[data-toggle="tab"]').on('click', function (e) {
        window.location = e.target.getAttribute("href");
        if (e.target.getAttribute("href"))
            document.cookie = "currenttab=" + e.target.getAttribute("href");
    });
    $("#upload").change(function () {
        var fileUpload = $("#upload").get(0);
        var files = fileUpload.files;

        for (var i = 0; i < files.length; i++) {
            var oInput = files[i];
            var sFileName = oInput.name;
            if (sFileName.length > 0) {
                var blnValid = false;
                for (var j = 0; j < _validFileExtensions.length; j++) {
                    var sCurExtension = _validFileExtensions[j];
                    if (sFileName.substr(sFileName.length - sCurExtension.length, sCurExtension.length).toLowerCase() == sCurExtension.toLowerCase()) {
                        blnValid = true;
                        break;
                    }
                }

                if (!blnValid) {
                    alert("Sorry, " + sFileName + " is invalid, allowed extensions are: " + _validFileExtensions.join(", "));
                    fileUpload.value = '';
                    return false;
                }
            }
        }

        var divfiles = document.createElement("div");
        divfiles.className = "invoice_files__row";
        var filesdiv = $(".invoice_files")[0];
        filesdiv.appendChild(divfiles);

        var filetype = document.createElement("div");
        filetype.className = "invoice_files__doc";
        filetype.innerText = files[0].name.split('.').pop();
        divfiles.appendChild(filetype);

        var nameblock = document.createElement("div");
        nameblock.className = "invoice_files__block";
        divfiles.appendChild(nameblock);

        var namefile = document.createElement("div");
        namefile.className = "invoice_files__name";
        namefile.innerText = files[0].name;
        nameblock.appendChild(namefile);

        var filesize = document.createElement("div");
        filesize.className = "invoice_files__size";
        filesize.innerText = (files[0].size / 1024).toFixed(0) + " KB";
        divfiles.appendChild(filesize);
    });

    $('.icon.icon--copy').on('click', function (e) {
        e.stopPropagation();
        var $temp = $("<input>");
        $("body").append($temp);
        $temp.val($('#UnpaidUrl').text()).select();
        document.execCommand("copy");
        $temp.remove();
        setTooltip('Copied!');
    });

    $('#draftbtn').on('click', function (e) {
        $('#Amount').attr("min", "0.00");
        $('#Status').val("Draft");
        if ($('#Currency').val() == "")
            $('#Currency').val("CHF");
        updateGrid();
    });

    $('#closebtn').on('click', function (e) {
        $('body').removeClass('body--menu_opened');
        $('.create').removeClass('create--open');
    });
    $('#closeunpaidbtn').on('click', function (e) {
        $('body').removeClass('body--menu_opened');
        $('.create').removeClass('create--open');
    });

    $('.invoices__search').on('click', function () {
        $('.profile_search').toggleClass('vis');
    });
    $('.invoices__row .invoices__cell').on('click', function (sender) {
        var element = sender.target;
        filter.sortField = "";
        pagenumber = 1;
        filter.sortWay = (filter.sortWay == 0) ? 1 : 0;
        if (element.className.indexOf("number") !== -1)
            filter.sortField = "number";
        if (element.className.indexOf("client") !== -1)
            filter.sortField = "client";
        if (element.className.indexOf("amount") !== -1)
            filter.sortField = "amount";
        if (element.className.indexOf("currency") !== -1)
            filter.sortField = "currency";
        if (element.className.indexOf("status") !== -1)
            filter.sortField = "status";
        if (element.className.indexOf("date") !== -1)
            filter.sortField = "duedate";
        if (filter.sortField !== "")
            updateGrid();
    });

    $('.profile_search__button').on('click', function () {
        $('.profile_search').removeClass('vis');
        $('#searchvalue').val("");
        filter.searchValue = "";
        filter.period = 0;
        updateGrid();
    });

    $('.profile_search__button_close').
        on('click', function () {
            $('#searchvalue').val("");
            filter.searchValue = "";
            filter.period = 0;
            updateGrid();
        });

    $('#searchvalue').on('input', function (e) {
        filter.searchValue = $('#searchvalue').val();
        updateGrid();
    });

    $('#createform').submit(function () {
        var errors = validate();
        if ($('#Status').val() === "Draft") {
            if ($('#ClientName').val() !== "")
                return true;
        }
        return (errors == 0);
    });

    $('.create__item-copy').tooltip({
        show: {
            effect: "slideDown",
            delay: 250
        }
    });

    $('.btn_create').on('click', function (e) {
        e.stopPropagation();
        validate(true);
        $("#StartDate").datepicker({
            beforeShow: function (input, inst) {
                setTimeout(function () {
                    var left = parseInt(inst.dpDiv.css('left'));
                    inst.dpDiv.css({
                        left: left - 30
                    });
                },
                    0);
            }
        }).datepicker("setDate", new Date());
        $('body').addClass('body--menu_opened');
        $('.create.draft').addClass('create--open');
    });

    $('.icon--cal').on('click',
        function (e) {
            e.stopPropagation();
            $("#StartDate").datepicker('show');
        });

    $('body').on('click', function (e) {
        if (e.target.className === "menu_overlay") {
            $('body').removeClass('body--menu_opened');
            $('.create.draft').removeClass('create--open');
            $('.create.unpaid').removeClass('create--open');
        }
        else if (e.target.className === "ui-icon ui-icon-circle-triangle-w" ||
            e.target.className === "ui-icon ui-icon-circle-triangle-e" ||
            e.target.className === "ui-datepicker-prev ui-corner-all ui-state-hover ui-datepicker-prev-hover" ||
            e.target.className === "ui-datepicker-next ui-corner-all ui-state-hover ui-datepicker-next-hover") {
            $('body').addClass('body--menu_opened');
        }
    });

    $('.create.draft').on('click', function (e) {
        e.stopPropagation();
    });

    $('.create.unpaid').on('click', function (e) {
        e.stopPropagation();
    });
    $('#selectperiod').on('change', function (e) {
        $('._value').text($("#selectperiod option:selected").text());
        filter.period = $("#selectperiod option:selected").val();
        updateGrid();
    });
    $('.showmore').on('click', function (e) {
        pagenumber++;
        updateGrid(true);
    });

});