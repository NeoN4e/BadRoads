$(document).ready(function () {    
    Slider();
    $(window).resize(Size);
    $('#btnmenu').click(HideShowMenu);
    $('.content').click(function () {
        $('#menu').animate({
            'margin-left': '-250px'
        }, 500);
    });
    Size();
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

function Slider() {
    var elWrap = $('.slider'),
		el = elWrap.find('img'),      
		indexImg = 1,
		indexMax = el.length;

    function change() {
        el.fadeOut(200);
        el.filter(':nth-child(' + indexImg + ')').fadeIn(200);
    }

    $('.next').click(function () {
        indexImg++;
        if (indexImg > indexMax) {
            indexImg = 1;
        }
        change();
    });
    $('.prev').click(function () {
        indexImg--;
        if (indexImg < 1) {
            indexImg = indexMax;
        }
        change();
    });
}