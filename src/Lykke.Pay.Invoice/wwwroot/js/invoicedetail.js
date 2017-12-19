$('#generatebtn').on('click', function (e) {
    $('#Data_Status').val("Unpaid");
});
$('#draftbtn').on('click', function (e) {
    $('#Data_Status').val("Draft");
});
$('#deletebtn').on('click', function (e) {
    $('#Data_Status').val("Deleted");
});