using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AugmentedScript : MonoBehaviour
{


    private float startLatitude = 0; //Initialize to 0
    private float startLongitude = 0; //Initialize to 0
    private float currentLongitude;
    private float currentLatitude;


    private GameObject InfoTextObject;
    private GameObject LatTextObject;
    private GameObject LongTextObject;
    private GameObject MarkerTextObject;
    //private float distanceTraveled;

    private bool setOriginalValues = true;

    private Vector3 markerTargetPosition;
    private Vector3 markerOriginalPosition;

    private float speed = .1f;
    private bool locationServiceStarted = false;

    private bool gpsIsStabile = false;


    IEnumerator StartLocationService()
    {
        print("In StartlocationService");
        if (!Input.location.isEnabledByUser)
            yield break;

        // Start service before querying location
        Input.location.Start(1f, .1f);


        // Wait until service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Service didn't initialize in 20 seconds
        if (maxWait < 1)
        {
            InfoTextObject.GetComponent<Text>().text = "Info: Locationservice timed out!";
            yield break;
        }

        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            print("Unable to determine device location");
            yield break;
        }
        else
        {
            // Access granted and location value could be retrieved
            print("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);
        }
        
    }

    //calculates distanceTraveled between two sets of coordinates, taking into account the curvature of the earth.
    public float Calc(float lat1, float lon1, float lat2, float lon2)
    {

        var R = 6378.137; // Radius of earth in KM
        var dLat = lat2 * Mathf.PI / 180 - lat1 * Mathf.PI / 180;
        var dLon = lon2 * Mathf.PI / 180 - lon1 * Mathf.PI / 180;
        float a = Mathf.Sin(dLat / 2) * Mathf.Sin(dLat / 2) +
          Mathf.Cos(lat1 * Mathf.PI / 180) * Mathf.Cos(lat2 * Mathf.PI / 180) *
          Mathf.Sin(dLon / 2) * Mathf.Sin(dLon / 2);
        var c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));
        var distance = R * c;
        distance = distance * 1000f; // meters
                                     //set the distanceTraveled text on the canvas
        //InfoTextObject.GetComponent<Text>().text = "Distance: " + distanceTraveled;
        //convert distanceTraveled from double to float
        float distanceFloat = (float)distance;
        //set the target position of the ufo, this is where we lerp to in the update function
        //targetPosition = originalPosition - new Vector3(0, 0, distanceFloat * 12);
        //distanceTraveled was multiplied by 12 so I didn't have to walk that far to get the UFO to show up closer
        return distanceFloat;
    }


    void Start()
    {
        StartCoroutine(StartLocationService());
        //Input.location.Start(1f, .1f);
        ////get distanceTraveled text reference
        InfoTextObject = GameObject.FindGameObjectWithTag("infoText");
        LatTextObject = GameObject.FindGameObjectWithTag("latText");
        LongTextObject = GameObject.FindGameObjectWithTag("longText");
        MarkerTextObject = GameObject.FindGameObjectWithTag("markerText");

        //Instanciate with the position of the marker in Unity at start
        markerTargetPosition = transform.position;
        markerOriginalPosition = transform.position;

        //InfoTextObject.GetComponent<Text>().text = "Info: App Started!";
        ////start GetCoordinate() function 
        ////StartCoroutine("GetCoordinates");

        //StartLocationService();

        //initialize target and original position
        //targetPosition = transform.position;
        //originalPosition = transform.position;

    }

    void Update()
    {
        //linearly interpolate from current position to target position
        //this.transform.position = Vector3.Lerp(transform.position, targetPosition, speed);
        ////rotate by 1 degree about the y axis every frame
        //this.transform.eulerAngles += new Vector3(0, 1f, 0);

        if (Input.location.status == LocationServiceStatus.Running)
        {
            currentLatitude = Input.location.lastData.latitude;
            currentLongitude = Input.location.lastData.longitude;

            if (!gpsIsStabile)
            {
                startLatitude = Input.location.lastData.latitude;
                startLongitude = Input.location.lastData.longitude;
                //Check if GPS has stabilized and if it has, set the current location
                if (startLongitude == currentLongitude && startLatitude == currentLatitude)
                {
                    startLatitude = currentLatitude;
                    startLongitude = currentLongitude;
                    gpsIsStabile = true;
                }
            }
           

           float distanceTraveled = Calc(startLatitude, startLongitude, currentLatitude, currentLongitude);
            //markerTargetPosition = markerOriginalPosition - new Vector3(0, 0, distanceTraveled * 12);
            markerTargetPosition = markerOriginalPosition - new Vector3(0, 0, distanceTraveled * 10);
            transform.position = Vector3.Lerp(transform.position, markerTargetPosition, speed);

            

            Debug.Log("Long: " + Input.location.lastData.longitude);
            Debug.Log("Lat: " + Input.location.lastData.latitude);
            InfoTextObject.GetComponent<Text>().text = "Distance traveled: " + distanceTraveled.ToString() + " m, GPS ready: " + gpsIsStabile.ToString();
            LongTextObject.GetComponent<Text>().text = "StartLong: " + startLongitude + " StartLat: " + startLatitude;
            LatTextObject.GetComponent<Text>().text = "CurrentLong: " + currentLongitude + " Currentlat: " + currentLatitude;
            MarkerTextObject.GetComponent<Text>().text = "CurrentPos(x,y,z): (" + transform.position.x + "," + transform.position.y + "," + transform.position.z+")"
                + " TargetPos(x,y,z):("+ markerTargetPosition.x + ","+ markerTargetPosition.y+","+ markerTargetPosition.z+")";
        }



        //If hitting back key on phone
        if (Input.GetKey("escape"))
        {
            Input.location.Stop();
            print("Quitting");
            Application.Quit();
        }

    }
}

