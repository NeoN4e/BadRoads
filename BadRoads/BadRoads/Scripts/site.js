$(document).ready(function () {
    Size();
    $(window).resize(Size);
    $('#btnmenu').click(HideShowMenu);
    $('#content').click(function () {
        $('#menu').animate({
            'margin-left': '-250px'
        }, 500);
    });
});

function Size() {
    //присвоение значений для элементов страницы
    var windowWidth = $(window).width();
    var windowHeight = $(window).height();
    $('#content').width(windowWidth);
    $('.fullscreen').height(windowHeight);
    $('.fullscreen').width(windowWidth);
    
    //выделение активного пункта меню
    var link = window.location.pathname;
    $('nav ul li a[href="'+link+'"]').parent().addClass('active');
};

function HideShowMenu() {
    var menuposition = parseInt($('#menu').css('margin-left'));
    if (menuposition == -250) {
        $('#menu').animate({
            'margin-left': '0'
        }, 500);
    } else {
        $('#menu').animate({
            'margin-left': '-250px'
        }, 500);
    }
};