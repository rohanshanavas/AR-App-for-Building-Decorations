using UnityEngine;
using UnityEditor;
using System;

// This class provides methods to calculate distances between geographical coordinates.
public static class GeoCodeCalc
{
    // Constants defining Earth's radius in different units of measurement.
    public const double EarthRadiusInMiles = 3956.0;
    public const double EarthRadiusInKilometers = 6367.0;
    public const double EarthRadiusInMetres = 6371000;

    // Converts degrees to radians.
    public static double ToRadian(double val) { return val * (Math.PI / 180); }

    // Calculates the difference in radians between two values.
    public static double DiffRadian(double val1, double val2) { return ToRadian(val2) - ToRadian(val1); }

    // Calculates the distance between two sets of geographical coordinates.
    public static double CalcDistance(double lat1, double lng1, double lat2, double lng2)
    {
        return CalcDistance(lat1, lng1, lat2, lng2, GeoCodeCalcMeasurement.Miles);
    }

    // Calculates the distance between two sets of geographical coordinates in the specified unit of measurement.
    public static double CalcDistance(double lat1, double lng1, double lat2, double lng2, GeoCodeCalcMeasurement m)
    {

        // Selects the appropriate Earth's radius based on the chosen unit of measurement.
        double radius = GeoCodeCalc.EarthRadiusInMiles;

        if (m == GeoCodeCalcMeasurement.Kilometers) 
        { 
            radius = GeoCodeCalc.EarthRadiusInKilometers;
        }
        if (m == GeoCodeCalcMeasurement.Metre) 
        { 
            radius = GeoCodeCalc.EarthRadiusInMetres; 
        }

        // Calculates the distance using the Haversine formula.
        return radius * 2 * Math.Asin(Math.Min(1, Math.Sqrt((Math.Pow(Math.Sin((DiffRadian(lat1, lat2)) / 2.0), 2.0) + Math.Cos(ToRadian(lat1)) * Math.Cos(ToRadian(lat2)) * Math.Pow(Math.Sin((DiffRadian(lng1, lng2)) / 2.0), 2.0)))));
    }
}

// Enumeration defining different units of measurement for geographical distances.
public enum GeoCodeCalcMeasurement : int
{
    Miles = 0,
    Kilometers = 1,
    Metre = 2
}
