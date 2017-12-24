$('#generatebtn').on('click', function (e) {
    $('#Data_Status').val("Unpaid");
});
$('.invoice_paid__back').on('click', function (e) {
    window.location.href = "/home/profile";
});
$('#draftbtn').on('click', function (e) {
    $('#Data_Status').val("Draft");
});
$('.icon.icon--copy').on('click', function (e) {
    e.stopPropagation();
    var $temp = $("<input>");
    $("body").append($temp);
    $temp.val($('.invoice_paid__link').text()).select();
    document.execCommand("copy");
    $temp.remove();
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
                    $('#Data_Status').val("Deleted");
                    $('#createform').submit();
                }
            },
            cancel: function () {

            }
        }
    });
    
});