var pagenumber = 1;
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
            $(inputs[i]).removeClass("error");
        if ($(inputs[i]).val() == "") {
            $(inputs[i]).addClass("error");
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
                tempstr = tempstr.replace("{{cssClass}}", "green");
                break;
            case "Unpaid":
                tempstr = tempstr.replace("{{cssClass}}", "yellow");
                tempstr = tempstr.replace("{{disCssClass}}", "");
                tempstr = tempstr.replace("{{disoption}}", "");
                break;
            case "Draft":
                tempstr = tempstr.replace("{{disoption}}", "");
                tempstr = tempstr.replace("{{disCssClass}}", "");
                tempstr = tempstr.replace("{{cssClass}}", "gray");
                break;
            case "Removed":
                tempstr = tempstr.replace("{{cssClass}}", "");
                break;
            case "UnderPaid":
            case "OverPaid":
                tempstr = tempstr.replace("{{cssClass}}", "violet");
                break;
        }

        tempstr = tempstr.replace("{{cssClass}}", "red");
        tempstr = tempstr.replace("{{disCssClass}}", "btn--disabled");
        tempstr = tempstr.replace("{{disoption}}", "disabled");

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
            value = currentItem[keyNames[j]];
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
            $('.create.unpaid').addClass('sidebar_menu--open');
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
        var href = e.currentTarget.getAttribute("href");
        if (href) {
            document.cookie = "currenttab=" + href;
            window.location = href;
        }
    });
    $("#upload").change(function () {
        $('.new_file').remove();

        var allowedFiles = /(\.pdf|\.doc|\.docx|\.xls|\.xlsx)$/i;

        for (var i = 0; i < this.files.length; i++) {
            if (!allowedFiles.exec(this.files[i].name)) {
                alert("The file '" + this.files[i].name + "' is invalid, allowed extensions are: .pdf; .doc; .docx; .xls; .xlsx.");
                this.value = '';
                return false;
            }
        }

        for (var j = 0; j < this.files.length; j++) {
            $('.invoice_files_hint').hide();
            var assetLink = $('.invoice_files');
            assetLink.show();
            $('.asset_icon', assetLink).text(this.files[j].name.split('.').pop());
            $('.asset_link__title', assetLink).text(this.files[j].name);
            $('.asset_link__desc', assetLink).text(getFileSize(this.files[j].size));
        }

        return true;
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
        $('.create').removeClass('sidebar_menu--open');
    });
    $('#closeunpaidbtn').on('click', function (e) {
        $('body').removeClass('body--menu_opened');
        $('.create').removeClass('sidebar_menu--open');
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

        if (errors === 0) {
            $('#createform').submit(function () {
                return false;
            });
            disableButtons();
            return true;
        }

        enableButtons();

        return false;
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
        $("#StartDate").val(moment().format("DD.MM.YYYY"));
        $('body').addClass('body--menu_opened');
        $('.create.draft').addClass('sidebar_menu--open');
        enableButtons();
    });
    

    $('body').on('click', function (e) {
        if (e.target.className === "menu_overlay") {
            $('body').removeClass('body--menu_opened');
            $('.create.draft').removeClass('sidebar_menu--open');
            $('.create.unpaid').removeClass('sidebar_menu--open');
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

function disableButtons() {
    $('#generatebtn').attr('disabled', 'disabled');
    $('#draftbtn').attr('disabled', 'disabled');
}

function enableButtons() {
    $('#generatebtn').removeAttr('disabled');
    $('#draftbtn').removeAttr('disabled');
}

function getFileSize(value) {
    if (value < 1024) {
        return value + ' bytes';
    } else if (value > 1024 && value < 1048576) {
        return (value / 1024).toFixed(0) + ' KB';
    } else {
        return (value / 1048576).toFixed(0) + ' MB';
    }
}

$('.datetimepicker').datetimepicker({
    //    debug: true,
    format: 'DD.MM.YYYY',
    icons: {
        time: "icon--clock",
        date: "icon--cal",
        up: "icon--chevron-thin-up",
        down: "icon--chevron-thin-down",
        previous: "icon--chevron-thin-left",
        next: "icon--chevron-thin-right"
    }
});

var searchBlock = $('.search_filter'),
    searchActiveClass = 'search_filter--show';

function showSearch(e) {
    e.preventDefault();
    searchBlock.toggleClass(searchActiveClass);
}

function hideSearch(e) {
    e.preventDefault();
    searchBlock.removeClass(searchActiveClass);
}

$('._show_search_block').on('click', showSearch);
$('._hide_search_block').on('click', hideSearch);