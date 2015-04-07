$(document).ready(function () { Initialize(); $("#textcomment").change(CanSend);  });
var latitude;                                // широта точки
var longitude;                               // долгота точки
var map;                                     // карта
var marker;                                  // маркер точки

function Initialize() {
    latitude = $("#stringforMap").data("latitude");
    longitude = $("#stringforMap").data("longitude");
    latitude = latitude.replace(",", ".");
    longitude = longitude.replace(",", ".");
    var myLatlng = new google.maps.LatLng(latitude, longitude);
    geocoder = new google.maps.Geocoder();      // создание объекта Geocoder
    var mapOptions = {                          // задание настроек для карты
        center: myLatlng,
        zoom: 14,
        mapTypeId: google.maps.MapTypeId.ROADMAP      // тип карты. ROADMAP - дорожная
    };
    map = new google.maps.Map(document.getElementById("mapinfo"), mapOptions);           // создание карты
    var imageMarker = "../../Images/marker.png";              // картинка для маркерa
    marker = new google.maps.Marker({                    // создаем маркер
        position: myLatlng,
        map: map,
        icon: imageMarker,
        title: $("#stringforMap").data('adress')
    });
    google.maps.event.addListener(map, 'click', function (data) {          // при клике на карте
        var stringForMap = latitude + "-" + longitude;
        window.location.assign("../../Home/Map?stringForMap=" + stringForMap);   // переход на подробную карту
    });
    google.maps.event.addListener(marker, 'click', function (data) {          // при клике на маркере
        var stringForMap = latitude + "-" + longitude;
        window.location.assign("../../Home/Map?stringForMap=" + stringForMap);   // переход на подробную карту
    });
}
function CanSend()   // функция, если добавили комментарий в форму
{
    if ($("#textcomment").val() == "")
        $("#subm").attr("disabled", true);    // разблокировать кнопку Отправить
    else
        $("#subm").attr("disabled", false);
}
