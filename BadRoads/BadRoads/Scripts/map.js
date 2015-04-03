$(document).ready(function () { Initialize(); });
var myLatlng = new google.maps.LatLng(48.466601, 35.018155);  // центр карты
var map;                                                  // карта
var geocoder;                                             // объект класса Geocoder
var markers = new Array();

var searchMarker; // Маркер поиска улицы

var imageSearchMarker = "../../Images/newmarker.png";  // картинка для маркера поиска улицы

function SetPoints() {                                     // метод проставления всех точек на карте
    var masPoints = document.getElementsByClassName("points");   // получаем координаты точек с html
    var imageMarker = "../../Images/marker.png";              // картинка для маркеров
    for (var x = 0; x < masPoints.length; x++) {
        var la = $(masPoints[x]).data('latitude');
        la = la.replace(",", ".");
        var ln = $(masPoints[x]).data('longitude');
        ln = ln.replace(",", ".");
        markers[x] = new google.maps.Marker({                    // создаем маркер
            position: new google.maps.LatLng(la, ln),
            map: map,
            icon: imageMarker,
            title: $(masPoints[x]).data('adress')
        });
        markers[x].idPoint = $(masPoints[x]).data('id');                        // присваиваем маркеру свойство с ID точки
        google.maps.event.addListener(markers[x], 'click', function () {       // подписываем маркер на событие click
            window.location.assign("../../Point/PointInfo/"+this.idPoint);   // переход на экшен подробного отображения точки

        });
    }

}
function Initialize() {
    geocoder = new google.maps.Geocoder();      // создание объекта Geocoder
    var mapOptions = {                          // задание настроек для карты
        center: myLatlng,
        zoom: 12,
        mapTypeId: google.maps.MapTypeId.ROADMAP      // тип карты. ROADMAP - дорожная
    };
    map = new google.maps.Map(document.getElementById("map_canvas"), mapOptions);           // создание карты
    SetPoints();                                                                            // вызываем метод проставления всех точек на карте
    var markerClusterer = new MarkerClusterer(map, markers,                                 // создание объекта MarkerClusterer для группировки маркеров на карте
        {                                                                                               // настройки группировки
            maxZoom: 13,
            gridSize: 50,
            styles: null
        });

    //Инициализация маркера поиска без него совсем плохо
    //02 04 2015 Коноваленко А.В.
    searchMarker = new google.maps.Marker({
        map: map,
        icon: imageSearchMarker,
        draggable: true,
    });
    google.maps.event.addListener(searchMarker, 'click', function () {   // подписываем маркер на событие click
        window.location.assign("../../Point/Add/");   // переход в экшен создания точки
    });
}


//Автокомплит плюс позиционирование
//02 04 2015 Коноваленко А.В.
$(function() {
    $("#searchAdress").autocomplete({
        //Определяем значение для адреса при геокодировании
        source: function (request, response) {
            var geocoderRequest = {
            //    'address': request.term,
                'componentRestrictions': {
                    'country': 'ua',
                    'locality': 'Днепропетровск',
                    'route': request.term
                },
            };

            geocoder.geocode(geocoderRequest, function (results, status) {
                response($.map(results, function(item) {
                    return {
                        FullAddress: item.formatted_address,
                        value: item.formatted_address,
                        latitude: item.geometry.location.lat(),
                        longitude: item.geometry.location.lng()
                    }
                }));
            })
        },

        //Выполняется при выборе конкретного адреса
        select: function(event, ui) {
            var location = new google.maps.LatLng(ui.item.latitude, ui.item.longitude);
            map.setCenter(location);

            searchMarker.setPosition(location);
            searchMarker.setTitle(ui.item.FullAddress);
        }
    });
})