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
