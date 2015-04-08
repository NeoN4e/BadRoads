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
    /// This class provides info about GPS Metadata in images and other
    /// Author: Yuriy Kovalenko (anekosheik@gmail.com). Last modified 07/04/2015 23:40
    /// </summary>
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
                if (filename != null)
                {
                    if (!File.Exists(Path.GetFullPath(filename)))
                    {
                        filename = HttpContext.Current.Server.MapPath("~" + filename); // find file in server path
                    }
                    FileInfo f = new FileInfo(filename);
                    if (f.Extension == ".jpg" || f.Extension == ".jpeg") // only file extensions ".jpg", ".jpeg"
                    {
                        this.fileName = filename;
                        ReadGPSMetadata(); // call method for take metadata
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException("Только файлы с расширением \".jpg\" или \".jpeg\" могут быть добавлены");
                    }
                }
                else
                {
                    throw new NullReferenceException();
                }
                
            }
            catch (ArgumentOutOfRangeException aorex)
            {
                throw aorex;
            }
            catch (NullReferenceException nrex)
            {
                throw nrex;
            }
            catch (Exception ex)
            {
                throw ex;
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

        /// <summary>
        /// Method for save uploads files whith Extension ".jpg" or ".jpeg"
        /// </summary>
        /// <param name="idPoint">id point</param>
        /// <param name="upload">files, where be upload from html-form</param>
        /// <returns></returns>
        public static List<string> SaveUploadFiles(int idPoint, IEnumerable<HttpPostedFileBase> upload)
        {
            List<string> fileList = new List<string>();
            try
            {
                if (upload != null)
                {
                    string bPath = "/Images/Gallery/"; // path for save references from "src"-attribute view image
                    string basePath = HttpContext.Current.Server.MapPath("~" + bPath); // physical path
                    string directory = "point_" + idPoint.ToString(); // name for directory
                    if (!System.IO.Directory.Exists(basePath + directory))
                    {
                        Directory.CreateDirectory(basePath + directory); // create new directory
                    }

                    int countFile = 1;
                    string fileName;
                    foreach (var file in upload)
                    {
                        if (file != null)
                        {
                            FileInfo f = new FileInfo(file.FileName);
                            if (f.Extension == ".jpg" || f.Extension == ".jpeg") // only file extensions ".jpg", ".jpeg"
                            {
                                bool stop = true;
                                do
                                {
                                    fileName = Path.Combine(basePath + directory, "img_" + idPoint.ToString() + "_" + countFile + f.Extension); // create file name
                                    if (!File.Exists(fileName))
                                    {
                                        file.SaveAs(fileName); // save image on the server
                                        fileList.Add(bPath + directory + "/" + Path.GetFileName(fileName)); // add in collection, path for save references from "src"-attribute view image
                                        stop = false;
                                    }
                                    else
                                    {
                                        countFile++;
                                    }
                                } while (stop);
                            }
                            else
                            {
                                throw new ArgumentOutOfRangeException("Только файлы с расширением \".jpg\" или \".jpeg\" могут быть добавлены");
                            }
                        }
                        else
                        {
                            throw new NullReferenceException();
                        }
                    }
                }
                else
                {
                    throw new NullReferenceException();
                }
                return fileList;
            }
            catch (ArgumentOutOfRangeException aorex)
            {
                throw aorex;
            }
            catch (NullReferenceException nrex)
            {
                throw nrex;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        
        /// <summary>
        /// a method which calculates the distance between points
        /// </summary>
        /// <param name="lat1">GPS Latitude point 1</param>
        /// <param name="long1">GPS Longitude 1</param>
        /// <param name="lat2">GPS Latitude point 2</param>
        /// <param name="long2">GPS Longitude 2</param>
        /// <returns></returns>
        public static double LatLngToDistance(double lat1, double long1, double lat2, double long2)
        {
            double R = 6372795.0; // Earth's radius in meters

            // conversion degrees to radians
            lat1 *= Math.PI / 180;
            lat2 *= Math.PI / 180;
            long1 *= Math.PI / 180;
            long2 *= Math.PI / 180;

            // calculation of cosine and sine latitude and longitude difference
            double cl1 = Math.Cos(lat1);
            double cl2 = Math.Cos(lat2);
            double sl1 = Math.Sin(lat1);
            double sl2 = Math.Sin(lat2);
            double delta = long2 - long1;
            double cdelta = Math.Cos(delta);
            double sdelta = Math.Sin(delta);

            //calculating the length of the great circle
            double y = Math.Sqrt(Math.Pow(cl2 * sdelta, 2) + Math.Pow(cl1 * sl2 - sl1 * cl2 * cdelta, 2));
            double x = sl1 * sl2 + cl1 * cl2 * cdelta;
            double ad = Math.Atan2(y, x);
            double dist = ad * R; //the distance between the two coordinates in meters

            return Math.Round(dist, 2); // Returns the distance in meters, rounded to 0.01 m
        }

        /// <summary>
        /// Provides check Metadata and distance beetween GPS tag in Point adn GPS tag in image
        /// </summary>
        /// <param name="fileList">List whith path to image</param>
        /// <param name="p">Point for check</param>
        /// <param name="radius">for chek distance</param>
        /// <returns></returns>
        public static bool CheckPointMetaDataAndDistance(List<string> fileList, Point p, double radius=100.00)
        {
            bool check = false;
            foreach (var item in fileList)
            {
                ImageHelper m = new ImageHelper(item); // item fof read GPS Metadata
                if (m.GPSLatitude != null || m.GPSLongitude != null)
                {
                    double distance = ImageHelper.LatLngToDistance(Convert.ToDouble(p.GeoData.Latitude), Convert.ToDouble(p.GeoData.Longitude),
                                                Convert.ToDouble(m.GPSLatitude), Convert.ToDouble(m.GPSLongitude)); // calculation the distance beetween GPS tag in Point adn GPS tag in image
                    if (distance <= radius) // check distance from radius
                    {
                        check = true;
                    }
                    else
                    {
                        check = false;
                        break;
                    }
                }
                else
                {
                    check = false;
                    break;
                }
            }
            return check;
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
        // GPS time (atomic clock)
        private const string GPSTimeStampQuery = "/app1/ifd/gps/subifd:{ulong=7}"; // RATIONAL 3
        // GPS date
        private const string GPSDateStampQuery = "/app1/ifd/gps/subifd:{ulong=29}"; // ASCII 11
        #endregion

    }
}