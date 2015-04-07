$(document).ready(function () { AlreadySetLocation();  Initialize(); });
var myLatlng;                                              // центр карты
var map;                                                   // карта
var geocoder;                                              // объект класса Geocoder
var markers = new Array();                                 // массив с маркерами для всех точек на карте
var searchMarker;                                          // Маркер поиска улицы
var imageMarker = "../../Images/marker.png";               // картинка для маркеров
var imageSearchMarker = "../../Images/newmarker.png";      // картинка для маркера поиска улицы
var zoom;                                                  // масштаб карты


function AlreadySetLocation() {                                          // функция, если для карты заданы конкретные координаты центра. При переходе из экшена PointInfo
    var stringLocation = $("#stringforMap").data("str");
    if (stringLocation != "") {                                            // выполняется, только если передавались координаты из экшена
        var arr = stringLocation.split("-");
        myLatlng = new google.maps.LatLng(arr[0], arr[1]);                // задаем конкретные координаты
        zoom = 14;                                                        // задаем приближенный масштаб для карты
    }
    else
    {
        myLatlng = new google.maps.LatLng(48.466601, 35.018155);             // задаем координаты центра Днепропетровска
        zoom = 13;                                                           // задаем обычный масштаб для карты
    }
        
}


function SetPoints() {                                     // метод проставления всех точек на карте
    var masPoints = document.getElementsByClassName("points");   // получаем координаты точек с html
    if (masPoints.length > 0) {
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
                window.location.assign("../../Point/PointInfo/" + this.idPoint);   // переход на экшен подробного отображения точки
            });
        }

        var markerClusterer = new MarkerClusterer(map, markers,                                 // создание объекта MarkerClusterer для группировки маркеров на карте
        {                                                                                               // настройки группировки
            maxZoom: 13,
            gridSize: 50,
            styles: null
        });

    }

}


function Initialize() {
    geocoder = new google.maps.Geocoder();      // создание объекта Geocoder
    var mapOptions = {                          // задание настроек для карты
        center: myLatlng,
        zoom: zoom,
        mapTypeId: google.maps.MapTypeId.ROADMAP      // тип карты. ROADMAP - дорожная
    };
    map = new google.maps.Map(document.getElementById("map_canvas"), mapOptions);           // создание карты
    SetPoints();                                                                            // вызываем метод проставления всех точек на карте


    //Инициализация маркера поиска без него совсем плохо
    //02 04 2015 Коноваленко А.В.
    searchMarker = new google.maps.Marker({
        map: map,
        icon: imageSearchMarker,
        draggable: true,
    });
    google.maps.event.addListener(searchMarker, 'click', function () {   // подписываем маркер на событие click
        var stringForMap = this.latOk + "-" + this.longOk;
        window.location.assign("../../Point/Add?stringForMap=" + stringForMap);   // переход в экшен создания точки
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
            searchMarker.latOk = ui.item.latitude;
            searchMarker.longOk = ui.item.longitude;
            searchMarker.setTitle(ui.item.FullAddress);
        }
    });
})