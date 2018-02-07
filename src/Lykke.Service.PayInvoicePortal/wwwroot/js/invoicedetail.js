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
            cancel: function () {}
        }
    });
});

$(document).ready(function(e) {
    var duedate = $("#Data_DueDate").val();
    var startdate = $('#Data_StartDate').text();
    $("#Data_DueDate").datepicker('option', 'minDate', new Date(startdate)).datepicker("setDate", new Date(duedate));
    initClipboard();

    $("#upload").change(function () {
        $('.new_file').remove();

        if ($('.invoice_paid_files__doc').length === 0)
            $(".invoice_paid_files__row span").show();

        if (this.files.length === 0)
            return true;

        var allowedFiles = /(\.pdf|\.doc|\.docx|\.xls|\.xlsx)$/i;

        for (var i = 0; i < this.files.length; i++) {
            if (!allowedFiles.exec(this.files[i].name)) {
                alert("The file '" + this.files[i].name + "' is invalid, allowed extensions are: .pdf; .doc; .docx; .xls; .xlsx.");
                this.value = '';
                return false;
            }
        }

        $(".invoice_paid_files__row span").hide();

        for (var j = 0; j < this.files.length; j++) {
            var fileRow = $('<div>').addClass('invoice_files__row').addClass('new_file');
            $('<div>').addClass('invoice_paid_files__doc').text(this.files[j].name.split('.').pop()).appendTo(fileRow);
            $('<div>').addClass('invoice_paid_files__name').text(this.files[j].name).appendTo(fileRow);
            $('<div>').addClass('invoice_paid_files__size').text(getFileSize(this.files[j].size)).appendTo(fileRow);
            $("#files_container").append(fileRow);
        }

        return true;
    });
});
function initClipboard() {
    var clipboard = new Clipboard('.create__item-copy', {
        text: function (trigger) {
            return trigger.previousElementSibling.innerHTML;
        }
    });

    clipboard.on('success', function (e) {
        e.trigger.innerHTML = '<i class="icon icon--check_thin"></i> Copied';
        e.clearSelection();
    });
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