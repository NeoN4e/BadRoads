using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Drawing.Imaging;

namespace BadRoads.Models
{
    /// <summary>
    /// This class provides info about GPS Metadata in images
    /// Author: Yuriy Kovalenko (anekosheik@gmail.com)
    /// </summary>
    //public class GPSMetadataReader
    public class ImageHelper
    {

#region Fields
        /// <summary>
        /// Contains the path to the parsed file
        /// </summary>
        private string fileName;

        /// <summary>
        /// Contains the Metadata to the parsed file
        /// </summary>
        private BitmapMetadata bitmapMetadata;
#endregion


#region Constructors
        /// <summary>
        /// Constructs and initializes a new instance 
        /// </summary>
        /// <param name="filename">The path of the file to use in the new instance</param>
        public ImageHelper(string filename)
        {
            try
            {
                FileInfo f = new FileInfo(filename);
                if (f.Extension == ".jpg" || f.Extension == ".jpeg") // only file extensions ".jpg", ".jpeg"
                {
                    this.fileName = filename;
                    ReadGPSMetadata();
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message); 
            }
        }
#endregion


#region Properties
        /// <summary>
        /// Provides info about Date and Time GPS tags
        /// </summary>
        public DateTime? GPSDateTime { get; set; }

        /// <summary>
        /// Provides info about Latitude GPS tags
        /// </summary>
        public double? GPSLatitude { get; set; }

        /// <summary>
        /// Provides info about Longitude GPS tags
        /// </summary>
        public double? GPSLongitude { get; set; }

        /// <summary>
        /// Provides info about the Metadata to the parsed file
        /// </summary>
        private BitmapMetadata BitmapMetadata
        {
            get
            {
                return bitmapMetadata;
            }
        }
#endregion


#region Methods
        /// <summary>
        /// Provides reading Metadata information in the parsed file
        /// </summary>
        private void ReadGPSMetadata()
        {
            using (Stream pictureFileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read)) // open and read file in stream
            {
                JpegBitmapDecoder decoder = new JpegBitmapDecoder(pictureFileStream, BitmapCreateOptions.None, BitmapCacheOption.None); // Disable caching to prevent excessive memory usage.

                bitmapMetadata = (BitmapMetadata)decoder.Frames[0].Metadata; // read Metadata

                if (bitmapMetadata != null)
                {
                    byte[] gpsVersionNumbers = bitmapMetadata.GetQuery(GPSVersionIDQuery) as byte[]; // read info about GPS version
                    bool strangeVersion = (gpsVersionNumbers != null && gpsVersionNumbers[0] == 2); // GPS version should be 2.2.0.0

                    ulong[] latitudes = bitmapMetadata.GetQuery(GPSLatitudeQuery) as ulong[]; // read Latitude info in rational format
                    if (latitudes != null)
                    {
                        double latitude = ConvertCoordinate(latitudes, strangeVersion); // Convert rational in double

                        string northOrSouth = (string)bitmapMetadata.GetQuery(GPSLatitudeRefQuery); // read North or South tag
                        if (northOrSouth == "S")
                        {
                            latitude = -latitude; // South means negative latitude.
                        }
                        this.GPSLatitude = latitude; 
                    }

                    ulong[] longitudes = bitmapMetadata.GetQuery(GPSLongitudeQuery) as ulong[]; // read Longitude info in rational format
                    if (longitudes != null)
                    {
                        double longitude = ConvertCoordinate(longitudes, strangeVersion); // Convert rational in double

                        string eastOrWest = (string)bitmapMetadata.GetQuery(GPSLongitudeRefQuery); // read East or West tag
                        if (eastOrWest == "W")
                        {
                            longitude = -longitude; // West means negative longitude
                        }
                        this.GPSLongitude = longitude;
                    }

                    var gpsTimeStamp = ToRational(bitmapMetadata.GetQuery(GPSTimeStampQuery)); // read Time tag and convert to array
                    var gpsDate = (string)bitmapMetadata.GetQuery(GPSDateStampQuery); // read Date tag and convert to string
                    if (gpsTimeStamp != null && gpsDate != null)
                    {
                        var dateParts = gpsDate.Split(new char[] { ':' });
                        this.GPSDateTime = new DateTime(
                            Int32.Parse(dateParts[0]),
                            Int32.Parse(dateParts[1]),
                            Int32.Parse(dateParts[2])) + TimeSpan.FromMilliseconds(
                                (((gpsTimeStamp[0] * 60) + gpsTimeStamp[1]) * 60 + gpsTimeStamp[2]) * 1000);
                    }
                }
            }
        }

        /// <summary>
        /// Provides converting Time GPS information 
        /// </summary>
        /// <param name="obj">Contains an object, which returned BitmapMetadata</param>
        /// <returns>Array of ulong numbers</returns>
        private static double[] ToRational(object obj)
        {
            ulong[] data = obj as ulong[];
            if (data == null)
            {
                return null;
            }
            else
            {
                return data.Select(x => splitLongAndDivide(x)).ToArray();
            }
        }

        /// <summary>
        /// Provides converting Latitude or Longitude GPS information from rational in double format
        /// </summary>
        /// <param name="coordinates">Array of ulong numbers</param>
        /// <param name="strangeVersion">bool param from info GPS Version tag</param>
        /// <returns>coordinate in double format</returns>
        private double ConvertCoordinate(ulong[] coordinates, bool strangeVersion)
        {
            int degrees;
            int minutes;
            double seconds;

            if (strangeVersion)
            {
                degrees = (int)splitLongAndDivide(coordinates[0]);
                minutes = (int)splitLongAndDivide(coordinates[1]);
                seconds = splitLongAndDivide(coordinates[2]);
            }
            else
            {
                degrees = (int)(coordinates[0] - DEGREES_OFFSET);
                minutes = (int)(coordinates[1] - MINUTES_OFFSET);
                seconds = (double)(coordinates[2] - SECONDS_OFFSET) / 100.0;
            }

            double coordinate = degrees + (minutes / 60.0) + (seconds / 3600);

            double roundedCoordinate = Math.Floor(coordinate * COORDINATE_ROUNDING_FACTOR) / COORDINATE_ROUNDING_FACTOR;

            return roundedCoordinate;
        }

        /// <summary>
        /// Provides converting from ulong in double format
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        private static double splitLongAndDivide(ulong number)
        {
            byte[] bytes = BitConverter.GetBytes(number);
            int int1 = BitConverter.ToInt32(bytes, 0);
            int int2 = BitConverter.ToInt32(bytes, 4);
            return ((double)int1 / (double)int2);
        }

        /// <summary>
        /// Provides info about tag in the Metadata
        /// </summary>
        /// <param name="query">Info about string Photo Metadata Policy</param>
        /// <returns>The metadata at the specified query location</returns>
        private string GetString(string query)
        {
            return (string)bitmapMetadata.GetQuery(query);
        }


        public static void SaveUploadFiles(IEnumerable<HttpPostedFileBase> upload)
        {
            try
            {
                if (upload != null)
                {
                    string directory = "~/Images/FotoRoads/"; // заменить путь
                    List<string> fileList = new List<string>();
                    foreach (var file in upload)
                    {
                        if (file != null)
                        {
                            string fileName = Path.Combine(directory, System.IO.Path.GetFileName(file.FileName));
                            file.SaveAs(fileName);
                            fileList.Add(fileName);
                        }
                    }

                    FileConverter(fileList); // заглушка метод Саши Богуславского
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message); 
            }
        }

        public static void FileConverter(List<string> fileList) // заглушка
        {
            return; 
        }

#endregion

#region Private Constants
        private const long DEGREES_OFFSET = 0x100000000;
        private const long MINUTES_OFFSET = 0x100000000;
        private const long SECONDS_OFFSET = 0x6400000000;
        private const double COORDINATE_ROUNDING_FACTOR = 1000000.0;
#endregion

#region Photo Metadata Policy : GPS
        // GPS tag version
        private const string GPSVersionIDQuery = "/app1/ifd/gps/subifd:{ulong=0}"; // BYTE 4
        // North or South Latitude
        private const string GPSLatitudeRefQuery = "/app1/ifd/gps/subifd:{ulong=1}"; // ASCII 2
        // Latitude        
        private const string GPSLatitudeQuery = "/app1/ifd/gps/subifd:{ulong=2}"; // RATIONAL 3
        // East or West Longitude
        private const string GPSLongitudeRefQuery = "/app1/ifd/gps/subifd:{ulong=3}"; // ASCII 2
        // Longitude
        private const string GPSLongitudeQuery = "/app1/ifd/gps/subifd:{ulong=4}"; // RATIONAL 3
        // Altitude reference
        private const string GPSAltitudeRefQuery = "/app1/ifd/gps/subifd:{ulong=5}"; // BYTE 1
        // Altitude
        private const string GPSAltitudeQuery = "/app1/ifd/gps/subifd:{ulong=6}"; // RATIONAL 1
        // GPS time (atomic clock)
        private const string GPSTimeStampQuery = "/app1/ifd/gps/subifd:{ulong=7}"; // RATIONAL 3
        // GPS satellites used for measurement
        private const string GPSSatellitesQuery = "/app1/ifd/gps/subifd:{ulong=8}"; // ASCII Any
        // GPS receiver status
        private const string GPSStatusQuery = "/app1/ifd/gps/subifd:{ulong=9}"; // ASCII 2
        // GPS measurement mode
        private const string GPSMeasureModeQuery = "/app1/ifd/gps/subifd:{ulong=10}"; // ASCII 2
        // Measurement precision
        private const string GPSDOPQuery = "/app1/ifd/gps/subifd:{ulong=11}"; // RATIONAL 1
        // Speed unit
        private const string GPSSpeedRefQuery = "/app1/ifd/gps/subifd:{ulong=12}"; // ASCII 2
        // Speed of GPS receiver
        private const string GPSSpeedQuery = "/app1/ifd/gps/subifd:{ulong=13}"; // RATIONAL 1
        // Reference for direction of movement
        private const string GPSTrackRefQuery = "/app1/ifd/gps/subifd:{ulong=14}"; // ASCII 2
        // Direction of movement
        private const string GPSTrackQuery = "/app1/ifd/gps/subifd:{ulong=15}"; // RATIONAL 1
        // Reference for direction of image
        private const string GPSImgDirectionRefQuery = "/app1/ifd/gps/subifd:{ulong=16}"; // ASCII 2
        // Direction of image
        private const string GPSImgDirectionQuery = "/app1/ifd/gps/subifd:{ulong=17}"; // RATIONAL 1
        // Geodetic survey data used
        private const string GPSMapDatumQuery = "/app1/ifd/gps/subifd:{ulong=18}"; // ASCII Any
        // Reference for latitude of destination
        private const string GPSDestLatitudeRefQuery = "/app1/ifd/gps/subifd:{ulong=19}"; // ASCII 2
        // Latitude of destination
        private const string GPSDestLatitudeQuery = "/app1/ifd/gps/subifd:{ulong=20}"; // RATIONAL 3
        // Reference for longitude of destination
        private const string GPSDestLongitudeRefQuery = "/app1/ifd/gps/subifd:{ulong=21}"; // ASCII 2
        // Longitude of destination
        private const string GPSDestLongitudeQuery = "/app1/ifd/gps/subifd:{ulong=22}"; // RATIONAL 3
        // Reference for bearing of destination
        private const string GPSDestBearingRefQuery = "/app1/ifd/gps/subifd:{ulong=23}"; // ASCII 2
        // Bearing of destination
        private const string GPSDestBearingQuery = "/app1/ifd/gps/subifd:{ulong=24}"; // RATIONAL 1
        // Reference for distance to destination
        private const string GPSDestDistanceRefQuery = "/app1/ifd/gps/subifd:{ulong=25}"; // ASCII 2
        // Distance to destination
        private const string GPSDestDistanceQuery = "/app1/ifd/gps/subifd:{ulong=26}"; // RATIONAL 1
        // Name of GPS processing method
        private const string GPSProcessingMethodQuery = "/app1/ifd/gps/subifd:{ulong=27}"; // UNDEFINED Any
        // Name of GPS area
        private const string GPSAreaInformationQuery = "/app1/ifd/gps/subifd:{ulong=28}"; // UNDEFINED Any
        // GPS date
        private const string GPSDateStampQuery = "/app1/ifd/gps/subifd:{ulong=29}"; // ASCII 11
        // GPS differential correction
        private const string GPSDifferentialQuery = "/app1/ifd/gps/subifd:{ulong=30}"; // SHORT 1
#endregion


    }
}