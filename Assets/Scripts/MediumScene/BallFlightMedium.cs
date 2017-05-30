using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class BallFlightMedium : MonoBehaviour {

    // Public game objects
    public Slider SpeedSlider;

    // Class with coordinates
    class Coordinates {
        public List<double> x;
        public List<double> y;
        public List<double> z;

        public Coordinates() {
            x = new List<double>();
            y = new List<double>();
            z = new List<double>();
        }

        //public List<double> X { get { return x; } set { x = value; } }
        //public List<double> Y { get { return y; } set { y = value; } }
        //public List<double> Z { get { return z; } set { z = value; } }
    }

    // Variables
    const string FILENAME = @"C:\Andrey\Dev\Repositories\Unity\TestTask\Assets\Resources\ball_path.json";
    float speed;             // Current flight speed
    float curspeed;          // Counter for flight speed
    float firstClickTime;    // Time of first click
    int curposition;         // Current ball position
    int direction;           // Direction of ball moving
    bool isMoving;           // Is object moving
    Coordinates coordinates; // List of coordinates from ball_path.json
    List<Vector3> positions; // Ball positions

    void Start () {
        coordinates= new Coordinates();
        positions = new List<Vector3>();
        curspeed = 0;
        speed = 0.5f;
        curposition = 0;
        direction = 1;
        isMoving = false;
        firstClickTime = 0;

        ParseJsonCoordinates(ReadFileToString(FILENAME), out coordinates);
        CoordinatesToPositions();
        SpeedSlider.onValueChanged.AddListener(delegate { ChangeSpeed(); });

        gameObject.transform.position = positions[0];
    }

    void Update () {
        // Check condition coordinates is empty
        if (coordinates == null || coordinates.x.Capacity == 0 ||
            coordinates.y.Capacity == 0 || coordinates.z.Capacity == 0)
            return;

        if (isMoving) {
            SpeedSlider.gameObject.SetActive(true);
            MoveBall();
        }
        else {
            SpeedSlider.gameObject.SetActive(false);
        }
	}

    // Read json file to Array of Lists
    bool ParseJsonCoordinates(string json_data, out Coordinates coordinates) {
        
        // Check if read string is empty
        if (json_data.Length == 0) {
            Debug.Log("BallFlight.ParseJsonCoordinates(): Json data is empty!");
            coordinates = null;
            return false;
        }

        coordinates = JsonUtility.FromJson<Coordinates>(json_data);
        return true;
    }

    // Read file to string
    string ReadFileToString(string filename) {
        string json_data = string.Empty;

        // Reading file
        try {
            using (StreamReader sr = new StreamReader(filename)) {
                json_data = sr.ReadToEnd();
            }
        }
        catch (Exception e) {
            Debug.Log("BallFlight.ReadFileToString(): Something went wrong in READING FILE");
            Debug.Log(e.Message);
        }

        return json_data;
    }

    /* Move ball with direction
     * direction == 1 by default
     * 
     * direction == 1 ball moves forward
     * direction == -1 ball moves backward
     */
    void MoveBall() {

        curspeed += speed;

        for (int i = 0; i < (int)curspeed; ++i) {

            curposition += direction;
            gameObject.transform.position = positions[curposition];
            curspeed = 0;

            // Check if ball reach the end point
            if (curposition >= positions.Count - 1 || curposition <= 0) {
                isMoving = false;
                direction *= -1;
                break;
            }
        }
    }

    // If mouse clicked on object
    void OnMouseDown() {

        if (isMoving == false) {
            gameObject.GetComponent<TrailRenderer>().Clear();
            isMoving = true;
        }

        // Double click realisation
        if (Time.time - firstClickTime <= 0.3f) {
            if (direction == 1) {
                gameObject.transform.position = positions[0];
                curposition = 0;
                isMoving = false;
            }
            else {
                gameObject.transform.position = positions[positions.Count - 1];
                curposition = positions.Count - 1;
                isMoving = false;
            }

            gameObject.GetComponent<TrailRenderer>().Clear();
        }
        else
            firstClickTime = Time.time;
    }

    // Calculate coordinates to positions
    void CoordinatesToPositions() {
        for (int i = 0; i < coordinates.x.Capacity; ++i) {
            positions.Add(new Vector3((float)coordinates.x[i], (float)coordinates.y[i], (float)coordinates.z[i]));
        }
    }

    // On Slider's value changed
    void ChangeSpeed() {
        speed = SpeedSlider.value;
    }
}
