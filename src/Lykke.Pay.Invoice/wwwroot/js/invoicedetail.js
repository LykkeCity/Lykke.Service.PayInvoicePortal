$('#generatebtn').on('click', function (e) {
    $('#Data_Status').val("Unpaid");
});
$('.invoice_paid__back').on('click', function (e) {
    window.location.href = "/home/profile";
});
$('#draftbtn').on('click', function (e) {
    $('#Data_Amount').attr("min", "0.00");
    $('#Data_Status').val("Draft");
});

$('#deletebtn').on('click', function (e) {
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
                        url: "/home/deleteinvoice" + window.location.search,
                        type: "GET",
                        success: function (data) {
                            window.location.href = "/home/profile";
                        }
                    });
                }
            },
            cancel: function () {

            }
        }
    });
    
});
var _validFileExtensions = [".pdf", ".doc", ".docx", ".xls", ".xlsx"];
$(document).ready(function(e) {
    var duedate = $("#Data_DueDate").val();
    var startdate = $('#Data_StartDate').text();
    $("#Data_DueDate").datepicker('option', 'minDate', new Date(startdate)).datepicker("setDate", new Date(duedate));
    initClipboard();

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
                    return false;
                }
            }
        }

        var divfiles = document.createElement("div");
        divfiles.className = "invoice_files__row";
        if ($(".invoice_paid_files__row span")[0])
            $(".invoice_paid_files__row span")[0].style.display = "none";
        var filesdiv = $(".invoice_paid_files__row")[0];
        filesdiv.appendChild(divfiles);

        var filetype = document.createElement("div");
        filetype.className = "invoice_paid_files__doc";
        filetype.innerText = files[0].name.split('.').pop();
        divfiles.appendChild(filetype);

        var namefile = document.createElement("div");
        namefile.className = "invoice_paid_files__name";
        namefile.innerText = files[0].name;
        divfiles.appendChild(namefile);

        var filesize = document.createElement("div");
        filesize.className = "invoice_paid_files__size";
        filesize.innerText = (files[0].size / 1024).toFixed(0) + " KB";
        divfiles.appendChild(filesize);
    });
});
function initClipboard() {
    var clipboard = new Clipboard('.create__item-copy', {
        text: function (trigger) {
            debugger;
            return trigger.previousElementSibling.innerHTML;
        }
    });

    clipboard.on('success', function (e) {
        e.trigger.innerHTML = '<i class="icon icon--check_thin"></i> Copied';
        e.clearSelection();
    });
}