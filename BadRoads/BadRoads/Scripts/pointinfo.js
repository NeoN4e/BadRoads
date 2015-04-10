$(document).ready(function () { Initialize(); $("#textcomment").change(CanSend);  });
var latitude;                                // latitude of point
var longitude;                               // longitude of point
var map;                                     // map
var marker;                                  // point's marker

function Initialize() {
    latitude = $("#stringforMap").data("latitude");
    longitude = $("#stringforMap").data("longitude");
    latitude = latitude.replace(",", ".");
    longitude = longitude.replace(",", ".");
    var myLatlng = new google.maps.LatLng(latitude, longitude);
    geocoder = new google.maps.Geocoder();      // create object of class Geocoder
    var mapOptions = {                          //  set options for map 
        center: myLatlng,
        zoom: 14,
        mapTypeId: google.maps.MapTypeId.ROADMAP      // type of map. ROADMAP - for roads
    };
    map = new google.maps.Map(document.getElementById("mapinfo"), mapOptions);           // create map
    var imageMarker = "../../Images/marker.png";              // image for marker
    marker = new google.maps.Marker({                    // create marker
        position: myLatlng,
        map: map,
        icon: imageMarker,
        title: $("#stringforMap").data('adress')
    });
    google.maps.event.addListener(map, 'click', function (data) {          // click on map
        var stringForMap = latitude + "-" + longitude;
        window.location.assign("../../Home/Map?stringForMap=" + stringForMap);   // path to the action of main map
    });
    google.maps.event.addListener(marker, 'click', function (data) {          // click on marker
        var stringForMap = latitude + "-" + longitude;
        window.location.assign("../../Home/Map?stringForMap=" + stringForMap);   // path to the action of main map
    });
}
function CanSend()   // function if description for point has been added
{
    if ($("#textcomment").val() == "")
        $("#subm").attr("disabled", true);    // set enabled button submit
    else
        $("#subm").attr("disabled", false);
}
