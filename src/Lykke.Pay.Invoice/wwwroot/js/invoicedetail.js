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
$('.icon.icon--copy').on('click', function (e) {
    e.stopPropagation();
    var $temp = $("<input>");
    $("body").append($temp);
    $temp.val($('.invoice_paid__link').text()).select();
    document.execCommand("copy");
    $temp.remove();
    setTooltip('Copied!');
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
$(document).ready(function(e) {
    var duedate = $("#Data_DueDate").val();
    var startdate = $('#Data_StartDate').text();
    $("#Data_DueDate").datepicker('option', 'minDate', new Date(startdate)).datepicker("setDate", new Date(duedate));
    $('.create__item-copy').tooltip({
        show: {
            effect: "slideDown",
            delay: 250
        }
    });
});
function setTooltip(message) {
    $('.create__item-copy')
        .attr('title', message);
    $('.create__item-copy').tooltip("open");
}